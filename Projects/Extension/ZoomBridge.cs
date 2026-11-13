using DynamicPatcher;
using PatcherYRpp;
using System;

namespace Extension
{
    /// <summary>
    /// 战场缩放核心：直接驱动引擎自带的"上屏缩放"开关。
    ///
    /// 原理（IDA 已确认，sub_4F4780 = 2D 上屏路径）：
    ///   Composite -> Primary 这次 Blt 里，若 *(double*)(pPStm + 0xC0) != 1.0，
    ///   引擎会把源矩形改成居中的 (W/scale, H/scale)，再拉伸到整块 Composite 尺寸上屏。
    ///   也就是说这是引擎原生的缩放分支，不需要我们改任何矩形、也不需要重建表面。
    ///
    /// 因此这里只做一件事：把倍率写进 pPStm + 0xC0。
    /// 视野（DSurface::ViewBounds）始终保持进入战场时的基准值，不触发 RefreshViewBounds，
    /// 也就不会碰到 ZBuffer/ABuffer/侧栏，之前的闪烁、花屏、闪退路径全部绕开。
    /// </summary>
    public static class ZoomBridge
    {
        // DSurface::ViewBounds
        private const int ViewBounds = 0x886FA0;
        // pPStm（引擎的上屏缩放因子 owner），+0xC0 = double Zoom
        private const int PStm = 0x887324;
        // DSurface::Primary（最终上屏表面，缩放拉伸后的结果就在这上面）
        private const int Primary = 0x887308;
        // DSurface::Temp（Surface.Current，当前绘制目标）
        private const int Temp = 0x887314;
        // WWMouseClass 实例指针的存放地址（解一次引用就是实例地址）
        private const int WWMousePP = 0x887640;
        // WWMouseClass 内 +0x24 = 当前软件光标所在表面
        private const int WWMouseSurface = 0x24;
        // WWMouseClass vtable +0x3C：从指定表面保存背景并画光标
        private const int WWMouseDrawCursor = 0x7B90C0;

        /// <summary>+0xC0 处的上屏缩放因子。</summary>
        private const int ZOOM_OFFSET = 0xC0;

        /// <summary>当前是否处于缩放状态（倍率不是 1.0）。</summary>
        public static bool Active { get; private set; }

        /// <summary>进入战场时的视野宽度（缩放前的基准值）。侧栏位置需要它。</summary>
        public static int BaseViewWidth => _captured ? _base.Width : 0;

        private static bool _enabled;
        private static bool _captured;
        private static double _zoom = 1.0;
        private static double _appliedZoom = 1.0;
        private static RectangleStruct _base;

        /// <summary>进入战场时调用：记录原始视野，并确保引擎缩放因子是 1.0。</summary>
        public static unsafe void Activate()
        {
            _base = Surface.ViewBound;
            _zoom = 1.0;
            _appliedZoom = 1.0;
            _captured = true;
            _enabled = true;
            Active = false;

            int pstm = *(int*)PStm;
            if (pstm != 0)
            {
                *(double*)(pstm + ZOOM_OFFSET) = 1.0;

                int composite = *(int*)(0x88731C);
                Logger.Log("[ZoomBridge] base view {0}x{1}, composite {2}x{3}, zoom={4:0.###}",
                    _base.Width, _base.Height,
                    composite != 0 ? *(int*)(composite + 4) : 0,
                    composite != 0 ? *(int*)(composite + 8) : 0,
                    *(double*)(pstm + ZOOM_OFFSET));
            }
        }

        /// <summary>退出战场时调用：把缩放因子还原成 1.0。</summary>
        public static unsafe void Deactivate()
        {
            _enabled = false;
            _zoom = 1.0;
            _appliedZoom = 1.0;
            Active = false;

            int pstm = *(int*)PStm;
            if (pstm != 0)
            {
                *(double*)(pstm + ZOOM_OFFSET) = 1.0;
            }
        }

        /// <summary>
        /// 每帧在世界渲染之前调用。只写引擎的缩放因子，写值没变化时不做任何事。
        /// </summary>
        public static unsafe void Apply(double zoom)
        {
            if (!_enabled || !_captured)
            {
                return;
            }

            // 只做放大：下限 1.0（缩小会让引擎采样到没被重画的区域，产生花屏/闪退）
            if (zoom < 1.0) zoom = 1.0;
            if (zoom > 4.0) zoom = 4.0;

            _zoom = zoom;
            Active = Math.Abs(zoom - 1.0) > 0.001;

            if (Math.Abs(zoom - _appliedZoom) < 0.0005)
            {
                return;
            }

            int pstm = *(int*)PStm;
            if (pstm == 0)
            {
                return;
            }

            *(double*)(pstm + ZOOM_OFFSET) = zoom;
            _appliedZoom = zoom;

            Logger.Log("[ZoomBridge] zoom={0:0.###}", zoom);
        }

        /// <summary>清空记录的基准视野（离开战场时由 UI 侧调用）。</summary>
        public static void Reset()
        {
            _captured = false;
            _enabled = false;
            Active = false;
            _zoom = 1.0;
            _appliedZoom = 1.0;
        }

        /// <summary>
        /// 把当前绘制目标切到 Composite，画完恢复。返回切换前的表面，供 RestoreCurrent 使用。
        /// </summary>
        public static unsafe int SwitchToComposite()
        {
            int old = *(int*)Temp;
            int composite = *(int*)(0x88731C);
            if (composite != 0)
            {
                *(int*)Temp = composite;
            }
            return old;
        }

        /// <summary>
        /// 把当前绘制目标切到 Primary，画完恢复。返回切换前的表面，供 RestoreCurrent 使用。
        /// Primary 上是已经按倍率拉伸过的最终画面，画在这里等于 1:1 屏幕坐标，不参与缩放。
        /// </summary>
        public static unsafe int SwitchToPrimary()
        {
            int old = *(int*)Temp;
            int primary = *(int*)Primary;
            if (primary != 0)
            {
                *(int*)Temp = primary;
            }
            return old;
        }

        /// <summary>
        /// 缩放状态下把软件光标重画到 Primary 上。
        ///
        /// 引擎原本在 Composite→Primary 上屏之前，把光标存背景 + 画进 Composite
        /// （GScreenClass::DrawOnTop 0x4F4583 那次调用），这样光标会随 Composite 一起被放大，
        /// 而 WWMouse 的 16ms 线程又会把光标画在 Primary 上，结果就是"两个鼠标"。
        ///
        /// 这里在上屏之后直接对 Primary 调同一个函数：存的是上屏后的新鲜背景，
        /// 光标也是 1:1 的原始大小、真实位置。上屏前的 Composite 光标由 TacticalZoom 跳过。
        /// </summary>
        public static unsafe void RedrawCursorOnPrimary()
        {
            if (!Active)
            {
                return;
            }

            int mouse = *(int*)WWMousePP;
            if (mouse == 0)
            {
                return;
            }

            int primary = *(int*)Primary;
            if (primary == 0)
            {
                return;
            }

            // 该函数在 pSurface == WWMouse.Surface 时会直接返回，临时换掉以绕过这个判断
            int saved = *(int*)(mouse + WWMouseSurface);
            *(int*)(mouse + WWMouseSurface) = 0;
            try
            {
                ((delegate* unmanaged[Thiscall]<int, int, int, void>)WWMouseDrawCursor)(mouse, primary, 0);
            }
            finally
            {
                *(int*)(mouse + WWMouseSurface) = saved;
            }
        }

        /// <summary>恢复 SwitchToComposite / SwitchToPrimary 之前的绘制目标。</summary>
        public static unsafe void RestoreCurrent(int old)
        {
            *(int*)Temp = old;
        }
    }
}
