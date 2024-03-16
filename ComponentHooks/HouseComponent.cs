using DynamicPatcher;
using Extension.Ext;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP4CW.DPExtension_Dionysus.ComponentHooks
{
    public class HouseComponent
    {
        [Hook(HookType.AresHook, Address = 0x4F8440, Size = 5)]
        static public unsafe UInt32 HouseClass_Update_Components(REGISTERS* R)
        {
            try
            {
                Pointer<HouseClass> pHouse = (IntPtr)R->ECX;

                HouseExt ext = HouseExt.ExtMap.Find(pHouse);
                ext.GameObject.Foreach(c => c.OnUpdate());

                return 0;
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return 0;
            }
        }
    }
}
