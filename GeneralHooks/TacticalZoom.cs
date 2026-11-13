using DynamicPatcher;
using Extension;
using PatcherYRpp;
using System;

namespace DP4CW.DPExtension_Dionysus.GeneralHooks
{
    /// <summary>
    /// 战场缩放（暂时停用，代码保留以便恢复）。
    ///
    /// 原做法：0x4F44CB 处缓动倍率并写入 pPStm+0xC0，交给引擎自带的"上屏缩放"分支
    /// （sub_4F4780 居中裁剪 + 拉伸，Composite → Primary）整体放大世界。
    ///
    /// 停用原因：自绘 UI 和 Phobos 的超武侧栏都画在 Composite 上，会跟着一起被放大，
    /// 目前没有既能缩放战场、又让这些 Composite 上的界面保持 1:1 的方案，故先整体关掉。
    ///
    /// 恢复时需同步打开：UIRoot 的 ZoomController.OnWheel 与 ZoomIndicator、
    /// MouseWheel 的滚轮 hook、GScreenExt 的上屏后 hook，以及 SurfaceScale 的表面扩容。
    /// </summary>
    public static class TacticalZoom
    {
        // ---- 缩放入口（已停用）----
        /*
        private const int DRAW_ON_TOP_WORLD = 0x4F44CB;

        [Hook(HookType.AresHook, Address = DRAW_ON_TOP_WORLD, Size = 0x5)]
        public static unsafe UInt32 DrawOnTop_BeforeWorld(REGISTERS* R)
        {
            try
            {
                ZoomController.Update();
                ZoomBridge.Apply(ZoomController.CurrentZoom);
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        */

        // ---- 光标：不要把软件光标画进被放大的 Composite（仅缩放时需要，已停用）----
        // 0x4F4583 起是 `mov ecx, WWMouseClass` + SetCursorSurface(Composite, 0) + call [edx+3Ch]
        // （存背景 + 把光标画进 Composite）。Composite 被整体放大时光标也会跟着放大，
        // 而 WWMouse 的 16ms 线程又把光标画在 Primary 上，于是出现"两个鼠标"。
        //
        // 注意：0x4F4583 本身已被 Phobos 占用（GScreenClass_NewMessageListDraw），
        // 所以原来挂在下一条指令 0x4F4589（mov eax, DSurface::Composite）。
        /*
        private const int COMPOSITE_CURSOR = 0x4F4589;   // 8B 05 ?? ?? ?? ??  mov eax, DSurface::Composite
        private const int AFTER_COMPOSITE_CURSOR = 0x4F4596;

        [Hook(HookType.AresHook, Address = COMPOSITE_CURSOR, Size = 0x5)]
        public static unsafe UInt32 SkipCompositeCursor(REGISTERS* R)
        {
            try
            {
                if (ZoomBridge.Active)
                {
                    // 跳过 SetCursorSurface(Composite, 0) 与画光标，直接继续后续逻辑
                    return AFTER_COMPOSITE_CURSOR;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        */

        // ---- 侧栏位置修正（仅缩放时需要，已停用）----
        // 缩放时视野恒为基准值，曾用这 5 处 hook 把基准宽度写回寄存器，保证侧栏贴在屏幕右边。
        // 现在不缩放，ViewBounds 就是真实宽度，引擎原指令即可。
        /*
        private const int SIDEBAR_X_EDX_1 = 0x6A71D0; // 8B 15 A8 6F 88 00  mov edx, ds:886FA8h
        private const int SIDEBAR_X_EDX_2 = 0x6A72B6; // 8B 15 A8 6F 88 00  mov edx, ds:886FA8h
        private const int SIDEBAR_X_EBP_1 = 0x6A732C; // 8B 2D A8 6F 88 00  mov ebp, ds:886FA8h
        private const int SIDEBAR_X_EBP_2 = 0x6A73C3; // 8B 2D A8 6F 88 00  mov ebp, ds:886FA8h
        private const int SIDEBAR_X_EDI = 0x6A7439;   // 8B 3D A8 6F 88 00  mov edi, ds:886FA8h

        [Hook(HookType.AresHook, Address = SIDEBAR_X_EDX_1, Size = 0x6)]
        public static unsafe UInt32 SidebarX_Edx1(REGISTERS* R) => SetSidebarWidth(R, 0x6A71D6, 0);

        [Hook(HookType.AresHook, Address = SIDEBAR_X_EDX_2, Size = 0x6)]
        public static unsafe UInt32 SidebarX_Edx2(REGISTERS* R) => SetSidebarWidth(R, 0x6A72BC, 0);

        [Hook(HookType.AresHook, Address = SIDEBAR_X_EBP_1, Size = 0x6)]
        public static unsafe UInt32 SidebarX_Ebp1(REGISTERS* R) => SetSidebarWidth(R, 0x6A7332, 1);

        [Hook(HookType.AresHook, Address = SIDEBAR_X_EBP_2, Size = 0x6)]
        public static unsafe UInt32 SidebarX_Ebp2(REGISTERS* R) => SetSidebarWidth(R, 0x6A73C9, 1);

        [Hook(HookType.AresHook, Address = SIDEBAR_X_EDI, Size = 0x6)]
        public static unsafe UInt32 SidebarX_Edi(REGISTERS* R) => SetSidebarWidth(R, 0x6A743F, 2);

        /// <summary>
        /// 把基准视野宽度写进寄存器并跳过原始 mov；进入战场前（宽度未知）返回 0 让原指令执行。
        /// </summary>
        /// <param name="next">原始 mov 之后那条指令的地址</param>
        /// <param name="reg">0 = edx、1 = ebp、2 = edi</param>
        private static unsafe UInt32 SetSidebarWidth(REGISTERS* R, int next, int reg)
        {
            try
            {
                int width = ZoomBridge.BaseViewWidth;
                if (width <= 0)
                {
                    return 0;
                }

                uint value = (uint)width;
                if (reg == 0)
                {
                    R->EDX = value;
                }
                else if (reg == 1)
                {
                    R->EBP = value;
                }
                else
                {
                    R->EDI = value;
                }

                return (uint)next;
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
