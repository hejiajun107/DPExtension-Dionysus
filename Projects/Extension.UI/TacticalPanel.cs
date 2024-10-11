using DynamicPatcher;
using Extension.UI.Core;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.UI
{
    public class TacticalPanel : Panel
    {
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Draw()
        {
            base.Draw();
        }
    }

    public class TacticalHooks
    {
        [Hook(HookType.AresHook, Address = 0x693268, Size = 5)]
        public static unsafe UInt32 MouseClass_UpdateCursor_LeftRelease(REGISTERS* R)
        {
            //if (CCForm.LeftRelease(WWMouseClass.Instance.GetCoords()))
            //{
            //    R->Stack(0x28 + 0x8, 0u);
            //    R->EAX = 0u;
            //    return 0x693276;
            //}
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6931A5, Size = 6)]
        public static unsafe UInt32 MouseClass_UpdateCursor_LeftPress(REGISTERS* R)
        {
            //if (CCForm.LeftPress(WWMouseClass.Instance.GetCoords()))
            //{
            //    R->EAX = 0u;
            //    return 0x6931B4;
            //}

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x69300B, Size = 6)]
        public static unsafe UInt32 MouseClass_UpdateCursor(REGISTERS* R)
        {
            //if (CCForm.UpdateCursor(WWMouseClass.Instance.GetCoords()))
            //{
            //    R->Stack(0x30 + -0x24, 0u);
            //    R->EAX = 0u;
            //    return 0x69301A;
            //}
            return 0;
        }

    }
}
