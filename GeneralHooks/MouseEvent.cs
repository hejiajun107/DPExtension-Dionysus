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
        // MouseCursorType.Default = 0（箭头指针）
        // 参考 Phobos SWButtonClass::OnMouseEnter:
        //   MouseClass::Instance.UpdateCursor(MouseCursorType::Default, false);
        private const int MOUSE_CURSOR_DEFAULT = 0;

        // 鼠标路径中两个“世界处理”函数 call 点的下一条指令地址：
        //   0x69302F call sub_4AAE90（sub_692F30，鼠标移动路径）→ 0x693034
        //   0x6931C9 call sub_4AAE90（case WM_LBUTTONDOWN）      → 0x6931CE
        //   0x69328B call sub_4AB9B0（case WM_LBUTTONUP）        → 0x693290
        // 只在返回地址等于这些值时抑制，避免影响其它调用者
        // （0x653CFF / 0x653D58，来自 AI/联机指令路径，不能拦）。
        private const int RET_WORLD_MOVE = 0x693034;
        private const int RET_WORLD_DOWN = 0x6931CE;
        private const int RET_WORLD_UP = 0x693290;

        // sub_4AAE90 / sub_4AB9B0 自身的 retn 14h 指令地址（机器码 C2 14 00）：
        // 被调方清栈 = 弹出返回地址 + 丢弃 5 个 dword 参数。
        // 只能跳到 retn 指令本身，不能跳到它前面的 pop edi/pop esi/pop ebp/pop ebx +
        // add esp,10h(或 7Ch)——那些需要函数 prologue 已执行才会平衡。
        // 在函数入口处跳 retn 时，ESP 仍指向返回地址，等价于该函数立即正常返回，
        // 未执行的 prologue（sub esp/push）与未执行的尾声（pop/add esp）正好抵消。
        // 另：AresHook 桩用 PUSHAD/POPAD 保存恢复寄存器，而 x86 POPAD 不会恢复 ESP，
        // 所以托管侧写 R->ESP 是无效的（曾按“改 ESP 模拟 retn”实现，结果栈未清理而崩溃）。
        private const int RET_WORLD_ACTION = 0x4AB77E; // sub_4AAE90 的 retn 14h
        private const int RET_WORLD_CLICK = 0x4AC2A7;  // sub_4AB9B0 的 retn 14h

        // 说明：三个鼠标消息 hook 只负责“广播 + 记录是否被 UI 消费”，一律返回 0 继续执行原指令。
        // 绝不能在这里跳转跳过引擎代码：中途跳转会跳过尚未被清理的 push（sub_692610 只清 3 个
        // dword，多出的 1 个由后续被调方的 retn 吸收），造成栈错位与泄漏，
        // 表现为 ret 到非法地址而崩溃。
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
                    SidebarClass.UpdateCursor(MOUSE_CURSOR_DEFAULT, false);
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
                    SidebarClass.UpdateCursor(MOUSE_CURSOR_DEFAULT, false);
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
                    SidebarClass.UpdateCursor(MOUSE_CURSOR_DEFAULT, false);
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        // sub_4AAE90：MapClass 鼠标动作 / 世界交互处理。UI 已消费鼠标时，直接跳到它自身的 retn 14h，
        // 整段跳过函数体，同时由 retn 完成返回地址弹出与 5 个参数清理（等价 ret 0x14），栈完美平衡。
        // 覆盖鼠标移动路径（0x69302F）与左键按下路径（0x6931C9）两处调用。
        [Hook(HookType.AresHook, Address = 0x4AAE90, Size = 0x8)]
        public static unsafe UInt32 Mouse_WorldAction(REGISTERS* R)
        {
            try
            {
                int retAddr = (int)R->Stack32(0);
                if ((retAddr == RET_WORLD_MOVE || retAddr == RET_WORLD_DOWN)
                    && UIRoot.Instance.IsMouseEventConsumed)
                {
                    return (UInt32)RET_WORLD_ACTION;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        // sub_4AB9B0：世界点击 → 构造并下发 NetworkEvent 命令（崩溃点 0x4AC21C 所在函数）。
        // 只在左键抬起路径（0x69328B）被调用，UI 已消费时跳到它自身的 retn 14h 整段跳过。
        [Hook(HookType.AresHook, Address = 0x4AB9B0, Size = 0x5)]
        public static unsafe UInt32 Mouse_WorldClick(REGISTERS* R)
        {
            try
            {
                int retAddr = (int)R->Stack32(0);
                if (retAddr == RET_WORLD_UP && UIRoot.Instance.IsMouseEventConsumed)
                {
                    return (UInt32)RET_WORLD_CLICK;
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
