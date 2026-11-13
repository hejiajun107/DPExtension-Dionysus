using DynamicPatcher;
using Extension.EventSystems;
using Extension.UI;
using PatcherYRpp;
using System;

namespace DP4CW.DPExtension_Dionysus.GeneralHooks
{
    /// <summary>
    /// 鼠标滚轮缩放（已随缩放功能一起停用）。
    /// 原来会拦截 WndProc 的 WM_MOUSEWHEEL 分支，把战场内的滚轮吞掉改成缩放；
    /// 现在不缩放，滚轮完全交回引擎，避免无谓地拦消息。
    /// </summary>
    public static class MouseWheel
    {
        // sub_7775C0 (WndProc) 的滚轮分支：
        //   0x777985  cmp byte_B73561, bl   （6 字节，绝对寻址，可安全复制到 trampoline）
        //   0x77798B  jnz short 0x7779B9
        // 只覆盖 0x777985 这一条完整指令，Size = 6，避免切断 0x77798B 的 rel8 相对跳转。
        private const int WHEEL_BRANCH = 0x777985;

        // 0x777949: pop edi; pop esi; xor eax,eax; pop ebx; pop ecx; retn 10h
        // 与入口的 push ecx/ebx/esi/edi 完全对应。从 0x777985 跳到这里栈是平衡的
        // （prologue 已执行完，路径上没有未清理的 push）。
        private const int WNDPROC_EPILOGUE = 0x777949;

        /*
        [Hook(HookType.AresHook, Address = WHEEL_BRANCH, Size = 0x6)]
        public static unsafe UInt32 WndProc_Wheel(REGISTERS* R)
        {
            try
            {
                if (!UIRoot.Instance.IsActive)
                {
                    // 未进入战场（主菜单等）：不干预原版滚轮
                    return 0;
                }

                // prologue 已压入 ecx/ebx/esi/edi 共 0x10 字节，
                // 故 wParam 位于 [esp+0x1C]（Msg 在 0x18、hWnd 在 0x14）。
                // 高 16 位是滚轮增量（有符号），低 16 位是按键状态。
                int wParam = (int)R->Stack32(0x1C);
                int delta = (short)(wParam >> 16);
                if (delta == 0)
                {
                    return 0;
                }

                Point2D position = WWMouseClass.Instance.GetCoords();
                RectangleStruct view = Surface.ViewBound;
                bool inView = position.X >= view.X && position.X < view.X + view.Width
                           && position.Y >= view.Y && position.Y < view.Y + view.Height;
                if (!inView)
                {
                    // 侧栏 / 视口外：保持原版 SidebarUp/SidebarDown 滚动
                    return 0;
                }

                EventSystem.MouseEventSystem.Broadcast(
                    EventSystem.MouseEventSystem.MouseWheelEvent,
                    new MouseWheelEventArgs(delta, position));

                // 战场内一律吞掉消息，避免同时滚动侧栏。
                // 是否真的缩放由 UIRoot 根据命中测试决定。
                return (UInt32)WNDPROC_EPILOGUE;
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return 0;
            }
        }
        */
    }
}
