using Extension;
using PatcherYRpp;

namespace Extension.UI
{
    /// <summary>
    /// UI 坐标基准视口。战场缩放时 Surface.ViewBound 会随倍率变化，
    /// 但侧栏/面板/刻度盘的布局必须固定在进入战场时的原始视口上。
    /// </summary>
    public static class UiViewport
    {
        private static RectangleStruct _view;
        private static bool _captured;

        public static RectangleStruct View
        {
            get
            {
                if (!_captured)
                {
                    Capture();
                }
                return _view;
            }
        }

        public static void Capture()
        {
            _view = Surface.ViewBound;
            _captured = true;
        }

        /// <summary>未缩放时每帧刷新基准，兼顾窗口尺寸变化。</summary>
        public static void RefreshIfUnzoomed()
        {
            if (!ZoomBridge.Active)
            {
                Capture();
            }
        }

        public static void Reset()
        {
            _captured = false;
        }
    }
}
