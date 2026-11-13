using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//from kratos
namespace Extension.EventSystems
{
    public class GScreenRenderEvent : EventBase
    {
        public override string Name => "GScreenClass_Render";
        public override string Description => "Raised when GScreen is Render";
    }
    public class SidebarRenderEvent : EventBase
    {
        public override string Name => "SidebarClass_Draw_It";
        public override string Description => "Raised when Sidebar is Render";
    }
    /// <summary>
    /// Composite 已经按缩放倍率拉伸上屏到 Primary 之后触发。
    /// 监听者画在 Primary 上，坐标与尺寸都是 1:1 的屏幕坐标，不参与战场缩放。
    /// </summary>
    public class ScreenSpaceRenderEvent : EventBase
    {
        public override string Name => "GScreenClass_Render_ScreenSpace";
        public override string Description => "Raised after the tactical surface is blitted to Primary";
    }
    public class GScreenEventArgs : EventArgs
    {
        public GScreenEventArgs(bool isBeginRender)
        {
            IsBeginRender = isBeginRender;
        }

        public bool IsBeginRender { get; }
        public bool IsLateRender => !IsBeginRender;
    }
    public class GScreenEventSystem : EventSystem
    {

        public GScreenEventSystem()
        {
            GScreenRenderEvent = new GScreenRenderEvent();
            SidebarRenderEvent = new SidebarRenderEvent();
            ScreenSpaceRenderEvent = new ScreenSpaceRenderEvent();
        }

        public GScreenRenderEvent GScreenRenderEvent { get; }
        public SidebarRenderEvent SidebarRenderEvent { get; }
        public ScreenSpaceRenderEvent ScreenSpaceRenderEvent { get; }
    }
}
