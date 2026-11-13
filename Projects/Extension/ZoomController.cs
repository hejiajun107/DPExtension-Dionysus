using System;

namespace Extension
{
    /// <summary>
    /// 战场缩放状态机。只维护倍率数值，不涉及渲染。
    /// 渲染侧（表面尺寸、ViewBounds、上屏 blit）在 M2 接入。
    /// </summary>
    public static class ZoomController
    {
        /// <summary>缩放下限。运行时扩大 ViewBounds 不会让引擎重画扩大的区域（只会拖出旧帧残影），
        /// 因此暂时只支持放大；待接入视口重建入口后再放开。</summary>
        public const double MinZoom = 1.0;

        /// <summary>缩放上限。</summary>
        public const double MaxZoom = 2.0;

        /// <summary>每格滚轮的倍率增量。</summary>
        public const double Step = 0.1;

        /// <summary>默认倍率（原版视野）。</summary>
        public const double DefaultZoom = 1.0;

        /// <summary>目标倍率。滚轮直接改这个值。</summary>
        public static double TargetZoom { get; private set; } = DefaultZoom;

        /// <summary>当前显示倍率。每帧向 TargetZoom 缓动。</summary>
        public static double CurrentZoom { get; private set; } = DefaultZoom;

        /// <summary>最近一次倍率变化的时间戳（Environment.TickCount），用于刻度盘淡出。</summary>
        public static int LastChangeTick { get; private set; }

        /// <summary>是否处于默认倍率。</summary>
        public static bool IsDefault => Math.Abs(TargetZoom - DefaultZoom) < 1e-6;

        /// <summary>
        /// 处理一次滚轮。
        /// </summary>
        /// <param name="delta">WM_MOUSEWHEEL 的 wParam 高字，正数向前（放大）。</param>
        public static void OnWheel(int delta)
        {
            if (delta == 0)
            {
                return;
            }

            int steps = delta / 120;
            if (steps == 0)
            {
                steps = delta > 0 ? 1 : -1;
            }

            // 滚轮向前 = 放大，与主流 RTS 一致
            double next = Math.Round(TargetZoom + steps * Step, 2);
            next = Clamp(next);

            if (Math.Abs(next - TargetZoom) < 1e-6)
            {
                return;
            }

            TargetZoom = next;
            LastChangeTick = Environment.TickCount;
        }

        /// <summary>复位到默认倍率。</summary>
        public static void Reset()
        {
            if (IsDefault)
            {
                return;
            }

            TargetZoom = DefaultZoom;
            LastChangeTick = Environment.TickCount;
        }

        /// <summary>每帧缓动，返回是否仍在过渡中。</summary>
        public static bool Update()
        {
            double diff = TargetZoom - CurrentZoom;
            if (Math.Abs(diff) < 0.001)
            {
                CurrentZoom = TargetZoom;
                return false;
            }

            CurrentZoom += diff * 0.25;
            return true;
        }

        /// <summary>场景切换/退出时调用，清掉残留状态。</summary>
        public static void Clear()
        {
            TargetZoom = DefaultZoom;
            CurrentZoom = DefaultZoom;
            LastChangeTick = 0;
        }

        private static double Clamp(double value)
        {
            if (value < MinZoom)
            {
                return MinZoom;
            }
            if (value > MaxZoom)
            {
                return MaxZoom;
            }
            return value;
        }
    }
}
