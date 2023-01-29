using DynamicPatcher;
using Extension.Ext;
using PatcherYRpp;
using System;
using Extension.CW;
using Extension.Encryption;
using System.Runtime.Remoting.Messaging;
using PatcherYRpp.Utilities;
using Extension.Utilities;

namespace ComponentHooks
{
    public class Hook4CwComponent
    {
        [Hook(HookType.AresHook, Address = 0x702E9D, Size = 6)]
        public static unsafe UInt32 TechnoClass_RegisterDestruction(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                Pointer<TechnoClass> pKiller = (IntPtr)R->EDI;

                double giveExpMultiple = 1.0;
                double gainExpMultiple = 1.0;

                if (pTechno != null)
                {
                    var technoExt = TechnoExt.ExtMap.Find(pTechno);
                    var globalExt = technoExt.GameObject.GetComponent<TechnoGlobalExtension>();
                    giveExpMultiple = globalExt.Data.GiveExperienceMultiple;
                }

                if (pKiller != null)
                {
                    var killerExt = TechnoExt.ExtMap.Find(pKiller);
                    var globalExt = killerExt.GameObject.GetComponent<TechnoGlobalExtension>();
                    gainExpMultiple = globalExt.Data.GainExperienceMultiple;
                }

                int cost = (int)R->EBP;
                R->EBP = (uint)(cost * gainExpMultiple * giveExpMultiple);
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x4DB7F7, Size = 6)]
        public static unsafe UInt32 FootClass_In_Which_Layer(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
            TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
            var gscript = ext.GameObject.GetComponent<TechnoGlobalExtension>();
            if (gscript != null)
            {
                var layer = gscript.Data.RenderLayer;

                if (!string.IsNullOrEmpty(layer))
                {
                    if (layer == "air")
                    {
                        R->EAX = (uint)Layer.Air;
                    }
                    else if (layer == "top")
                    {
                        R->EAX = (uint)Layer.Top;
                    }
                    else if (layer == "ground")
                    {
                        R->EAX = (uint)Layer.Ground;
                    }
                    return 0x4DB803;
                }
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6AB64F, Size = 6)]
        public static unsafe UInt32 Cameo_Clicked_Action(REGISTERS* R)
        {
            Pointer<TechnoTypeClass> pType = (IntPtr)R->EAX;

            var pHouse = HouseClass.Player;
            if(!pType.IsNull)
            {
                if (pType.Ref.BuildLimit == 1)
                {
                    if(pHouse.Ref.CanBuild(pType, true, true) == CanBuildResult.TemporarilyUnbuildable)
                    {
                        Logger.Log("can build");
                        var ext = Finder.FineOneTechno(pHouse, x => x.Ref.Type == pType, FindRange.Owner);
                        if(ext!=null)
                        {
                            if(!ext.IsNullOrExpired())
                            {
                                var gext = ext.GameObject.GetComponent<TechnoGlobalExtension>();
                                if (gext != null)
                                {
                                    if(gext.Data.IsEpicUnit || gext.Data.IsHero)
                                    {
                                        MapClass.UnselectAll();
                                          Logger.Log("UnselectAll");
                                        ext.OwnerObject.Ref.Base.Select();
                                            Logger.Log("Select");
                                        MapClass.Instance.CenterMap();
                                            Logger.Log("CenterMap");

                                        MapClass.Instance.MarkNeedsRedraw(1);
                                            Logger.Log("Redraw");
                                    }
                                }

                            }
                        }
                    }
                }
                
            }

            return 0;
        }

    }
}
