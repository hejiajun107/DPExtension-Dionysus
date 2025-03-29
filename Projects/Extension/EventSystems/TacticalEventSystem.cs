using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.EventSystems
{
  
    public class TacticalRenderEvent : EventBase
    {
        public override string Name => "Tactical_Render";
        public override string Description => "Raised when Tactical is Render";
    }

    public class TacticalEventArgs : EventArgs
    {
        public TacticalEventArgs(bool isBeginRender)
        {
            IsBeginRender = isBeginRender;
        }

        public bool IsBeginRender { get; }
        public bool IsLateRender => !IsBeginRender;
    }
    public class TacticalEventSystem : EventSystem
    {

        public TacticalEventSystem()
        {
            TactcialRenderEvent = new TacticalRenderEvent();
        }

        public TacticalRenderEvent TactcialRenderEvent { get; }
    }
}
