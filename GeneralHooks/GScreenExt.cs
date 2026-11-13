
using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension;
using Extension.Ext;
using Extension.EventSystems;

namespace ExtensionHooks
{
    public class GScreenExtHook
    {

        static GScreenExtHook()
        {

        }

        [Hook(HookType.AresHook, Address = 0x4F4497, Size = 6)] // GScreenClass_Render
        public static unsafe UInt32 GScreenClass_Render(REGISTERS* R)
        {
            try
            {
                // 只有进入战场后（ZoomBridge 已记录基准视野）才参与绘制。
                // 主菜单/加载界面不跑我们的 UI，避免加载阶段多余的绘制开销。
                if (ZoomBridge.BaseViewWidth <= 0) return 0;
                EventSystem.GScreen.Broadcast(EventSystem.GScreen.GScreenRenderEvent, new GScreenEventArgs(true));
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        [Hook(HookType.AresHook, Address = 0x4F4583, Size = 6)] // 世界/UI 绘制完成、引擎画光标之前（mov ecx, WWMouseClass）
        public static unsafe UInt32 GScreenClass_Render_Late(REGISTERS* R)
        {
            try
            {
                // 仅在战场内生效：加载界面/主菜单不切换绘制目标、不广播 UI。
                if (ZoomBridge.BaseViewWidth <= 0) return 0;

                // 世界系绘制（单位脚本的血条/图标等，坐标来自 CoordsToClient）和自绘 UI
                // 都画进 Composite；缩放功能停用后这里就是 1:1 的屏幕坐标。
                //
                // 注意必须挂在画光标之前：引擎紧接着会把软件光标画进 Composite，
                // 挂到 0x4F4757（画光标之后）会让 UI 压住鼠标。
                int old = ZoomBridge.SwitchToComposite();
                try
                {
                    EventSystem.GScreen.Broadcast(EventSystem.GScreen.GScreenRenderEvent, new GScreenEventArgs(false));
                }
                finally
                {
                    ZoomBridge.RestoreCurrent(old);
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        // ---- 上屏后的屏幕空间渲染（缩放方案的配套，已随缩放功能一起停用）----
        // 不缩放时 UI 画在 Composite 上就是 1:1，不需要再画一次到 Primary，
        // 更不需要把被 blit 抹掉的光标重画到 Primary。
        /*
        [Hook(HookType.AresHook, Address = 0x4F4766, Size = 7)] // 上屏（Composite→Primary）返回之后：mov byte_A9FA94, 0
        public static unsafe UInt32 GScreenClass_Render_ScreenSpace(REGISTERS* R)
        {
            try
            {
                // 只在战场内绘制；主菜单/加载界面不参与。
                if (ZoomBridge.BaseViewWidth <= 0) return 0;

                // 上屏已经完成：此时画到 Primary 就是 1:1 的屏幕坐标，
                // 既不会被倍率放大，也不会被那次 blit 覆盖（本帧内 Primary 不会再被写）。
                int old = ZoomBridge.SwitchToPrimary();
                try
                {
                    EventSystem.GScreen.Broadcast(EventSystem.GScreen.ScreenSpaceRenderEvent, EventArgs.Empty);
                }
                finally
                {
                    ZoomBridge.RestoreCurrent(old);
                }

                // 上屏那次 blit 会把软件光标抹掉，缩放时把它重新画到 Primary 上（1:1）。
                ZoomBridge.RedrawCursorOnPrimary();
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        */

        [Hook(HookType.AresHook, Address = 0x6A70AE, Size = 5)] // SidebarClass_Draw_It
        public static unsafe UInt32 SidebarClass_Draw_It(REGISTERS* R)
        {
            try
            {
                // Logger.Log($"{Game.CurrentFrame} SidebarClass_Draw_It call"); // before GScreen
                EventSystem.GScreen.Broadcast(EventSystem.GScreen.SidebarRenderEvent, EventArgs.Empty);
            }
            catch (Exception e)
            {

                Logger.PrintException(e);
            }
            return 0;
        }

    }
}

