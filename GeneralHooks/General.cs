using DynamicPatcher;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Components;
using Extension.EventSystems;
using Extension.CW;

namespace GeneralHooks
{
    public class General
    {
        static General()
        {
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioStartEvent, MathExHandler);
        }

        private static void MathExHandler(object sender, EventArgs e)
        {
            // ensure network synchronization
            MathEx.SetRandomSeed(0);
            //Logger.Log("set random seed!");
        }

        [Hook(HookType.AresHook, Address = 0x52BA60, Size = 5)]
        public static unsafe UInt32 YR_Boot(REGISTERS* R)
        {

            return 0;
        }

        // in progress: Initializing Tactical display
        [Hook(HookType.AresHook, Address = 0x6875F3, Size = 6)]
        public static unsafe UInt32 Scenario_Start1(REGISTERS* R)
        {
            EventSystem.General.Broadcast(EventSystem.General.ScenarioStartEvent, EventArgs.Empty);
            var uiManager = new UIManager();
            uiManager.Init();
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6D3D10, Size = 6)]
        public static unsafe UInt32 Tactical_Render(REGISTERS* R)
        {
            EventSystem.Tactical.Broadcast(EventSystem.Tactical.TactcialRenderEvent, new TacticalEventArgs(true));
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6D471A, Size = 6)]
        public static unsafe UInt32 Tactical_LateRender(REGISTERS* R)
        {
            EventSystem.Tactical.Broadcast(EventSystem.Tactical.TactcialRenderEvent,new TacticalEventArgs(false));
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x55AFB3, Size = 6)]
        public static unsafe UInt32 LogicClass_Update(REGISTERS* R)
        {
            EventSystem.General.Broadcast(EventSystem.General.LogicClassUpdateEvent, new LogicClassUpdateEventArgs(true));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x55B719, Size = 5)]
        public static unsafe UInt32 LogicClass_Update_Late(REGISTERS* R)
        {
            EventSystem.General.Broadcast(EventSystem.General.LogicClassUpdateEvent, new LogicClassUpdateEventArgs(false));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x685659, Size = 0xA)]
        public static unsafe UInt32 Scenario_ClearClasses(REGISTERS* R)
        {
            EventSystem.General.Broadcast(EventSystem.General.ScenarioClearClassesEvent, EventArgs.Empty);

            return 0;
        }


        [Hook(HookType.AresHook, Address = 0x7CD8EF, Size = 9)]
        public static unsafe UInt32 ExeTerminate(REGISTERS* R)
        {

            return 0;
        }
    }
}
