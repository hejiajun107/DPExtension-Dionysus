using DynamicPatcher;
using Extension.EventSystems;
using Extension.UI;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP4CW.DPExtension_Dionysus.GeneralHooks
{
    public static class MouseEvent
    {
        [Hook(HookType.AresHook, Address = 0x69300B, Size = 0x6)]
        public static unsafe UInt32 Mouse_Move(REGISTERS* R)
        {
            try
            {
                Point2D position = WWMouseClass.Instance.GetCoords();
                var args = new MouseEventArgs(position, MouseButtons.None);
                EventSystem.MouseEventSystem.Broadcast(EventSystem.MouseEventSystem.MouseMoveEvent, args);
                if (UIRoot.Instance.IsMouseEventConsumed)
                {
                    // 三层保险写入手型：MouseClass(Index+Frame) + WWMouseClass.ImageFrameIndex
                    WWMouseClass.SetPointerFrameSelectHand();
                    return 0x69301A;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6931A5, Size = 0x6)]
        public static unsafe UInt32 Mouse_Down(REGISTERS* R)
        {
            try
            {
                Point2D position = WWMouseClass.Instance.GetCoords();
                var args = new MouseEventArgs(position, MouseButtons.Left);
                EventSystem.MouseEventSystem.Broadcast(EventSystem.MouseEventSystem.MouseDownEvent, args);
                if (UIRoot.Instance.IsMouseEventConsumed)
                {
                    WWMouseClass.SetPointerFrameSelectHand();
                    return 0x6931B4;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x693268, Size = 0x5)]
        public static unsafe UInt32 Mouse_Up(REGISTERS* R)
        {
            try
            {
                Point2D position = WWMouseClass.Instance.GetCoords();
                var args = new MouseEventArgs(position, MouseButtons.Left);
                EventSystem.MouseEventSystem.Broadcast(EventSystem.MouseEventSystem.MouseUpEvent, args);
                if (UIRoot.Instance.IsMouseEventConsumed)
                {
                    WWMouseClass.SetPointerFrameSelectHand();
                    return 0x693276;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
    }
}
