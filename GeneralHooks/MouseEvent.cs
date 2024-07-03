using DynamicPatcher;
using Extension.EventSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP4CW.DPExtension_Dionysus.GeneralHooks
{
    public static class MouseEvent
    {
        //[Hook(HookType.AresHook, Address = 0x69300B, Size = 0x6)]
        //public static unsafe UInt32 Mouse_Move(REGISTERS* R)
        //{
        //    EventSystem.General.Broadcast(EventSystem.MouseEventSystem.MouseMoveEvent, EventArgs.Empty);
        //    return 0;
        //}

        //[Hook(HookType.AresHook, Address = 0x6931A5, Size = 0x6)]
        //public static unsafe UInt32 Mouse_Down(REGISTERS* R)
        //{
        //    EventSystem.General.Broadcast(EventSystem.MouseEventSystem.MouseDownEvent, EventArgs.Empty);
        //    return 0;
        //}

        //[Hook(HookType.AresHook, Address = 0x693268, Size = 0x5)]
        //public static unsafe UInt32 Mouse_Up(REGISTERS* R)
        //{
        //    EventSystem.General.Broadcast(EventSystem.MouseEventSystem.MouseUpEvent, EventArgs.Empty);
        //    return 0;
        //}
    }
}
