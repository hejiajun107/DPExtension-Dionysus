using DynamicPatcher;
using Extension.Ext;
using PatcherYRpp;
using System;
using Extension.CW;
using Extension.Utilities;
using Extension.Ext4CW;
using static System.Net.Mime.MediaTypeNames;
using Extension.Script;

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
                    var globalExt = technoExt.GameObject.GetTechnoGlobalComponent();
                    giveExpMultiple = globalExt.Data.GiveExperienceMultiple;
                    //technoExt.GameObject.Foreach(c => (c as ITechnoScriptable)?.OnKilledBy(pKiller));
                }

                if (pKiller != null)
                {
                    var killerExt = TechnoExt.ExtMap.Find(pKiller);
                    var globalExt = killerExt.GameObject.GetTechnoGlobalComponent();
                    gainExpMultiple = globalExt.Data.GainExperienceMultiple;
                    //killerExt.GameObject.Foreach(c => (c as ITechnoScriptable)?.OnKill(pTechno));
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
            var gscript = ext.GameObject.GetTechnoGlobalComponent();
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
                        var exts = Finder.FindTechno(pHouse, x => x.Ref.Type == pType || (pType.Ref.Base.Base.ID == "SFZS" && x.Ref.Type.Ref.Base.Base.ID == "Executioner"), FindRange.Owner);
                        if (exts!=null && exts.Count>0)
                        {
                            MapClass.UnselectAll();
                        }
                        foreach (var ext in exts)
                        {
                            if (ext != null)
                            {
                                if (!ext.IsNullOrExpired())
                                {
                                    var gext = ext.GameObject.GetTechnoGlobalComponent();
                                    if (gext != null)
                                    {
                                        if (gext.Data.IsEpicUnit || gext.Data.IsHero)
                                        {
                                            if(!gext.Owner.OwnerObject.Ref.Base.InLimbo)
                                            {
                                                ext.OwnerObject.Ref.Base.Select();
                                            }
                                            else
                                            {
                                                if(!gext.Owner.OwnerObject.Ref.Transporter.IsNull)
                                                {
                                                    gext.Owner.OwnerObject.Ref.Transporter.Ref.Base.Select();
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                        }

                        if (exts != null && exts.Count > 0)
                        {
                            MapClass.Instance.CenterMap();
                            MapClass.Instance.MarkNeedsRedraw(1);
                        }

                 
                    }
                }
                
            }

            return 0;
        }


        [Hook(HookType.AresHook, Address = 0x44A03C, Size = 6)]
        [Hook(HookType.AresHook, Address = 0x739956, Size = 6)]
        public static unsafe UInt32 Techno_Deploy_Or_UnDeploy_To_Another(REGISTERS* R)
        {
            Pointer<TechnoClass> pFrom = (IntPtr)R->EBP;
            Pointer<TechnoClass> pTo = (IntPtr)R->EBX;

            if(pFrom.IsNotNull && pTo.IsNotNull)
            {
                var extFrom = TechnoExt.ExtMap.Find(pFrom);
                var extTo = TechnoExt.ExtMap.Find(pTo);
                if(!extFrom.IsNullOrExpired() && !extTo.IsNullOrExpired())
                {
                    var gextFrom = extFrom.GameObject.GetTechnoGlobalComponent();
                    var gextTo = extTo.GameObject.GetTechnoGlobalComponent();

                    if (gextFrom != null)
                    {
                        gextFrom.ConvertSyncStatus(pFrom, pTo);
                    }

                    if (gextTo != null)
                    {
                        gextTo.IsDeployedFrom = true;
                    }
                }
            }

            
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x4A8FCC, Size = 5)]
        public static unsafe UInt32 MapClass_Can_Building_Type_Placed_Here(REGISTERS* R)
        {
            //uint goon = 0x4A8FD1;
            //uint canPlaceHere = 0x4A902C;

            Pointer<CellClass> pCell = (IntPtr)R->ECX;

            var building = pCell.Ref.GetBuilding();
            if(building.IsNotNull)
            {
                var techno = building.Convert<TechnoClass>();
                var technoExt = TechnoExt.ExtMap.Find(techno);
                if(!technoExt.IsNullOrExpired())
                {
                    var gext = technoExt.GameObject.GetTechnoGlobalComponent();
                    if(gext.IgnoreBaseNormal)
                    {
                        R->EAX = (uint)Pointer<BuildingClass>.Zero;
                        return 0x4A8FD1;
                    }
                }
            }

            R->EAX = (uint)building;

            return 0x4A8FD1;
        }

        [Hook(HookType.AresHook, Address = 0x7019D8, Size = 5)]
        public static unsafe UInt32 TechnoClass_ReceiveDamage_SkipLowDamageCheck(REGISTERS* R)
        {
            Pointer<int> pDamage = (IntPtr)R->lea_Stack(0xC4 - 0x4);
            if (pDamage.Ref == 0)
            {
                return 0x7019E3;
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x4571E0, Size = 5)]
        public static unsafe UInt32 BuildingClass_Infiltrate(REGISTERS* R)
        {
            Pointer<BuildingClass> pBuilding = (IntPtr)R->ECX;
            Pointer<HouseClass> pEnter = R->Stack<Pointer<HouseClass>>(0x4);


            if(pBuilding.IsNotNull && pEnter.IsNotNull)
            {
                var technoExt = TechnoExt.ExtMap.Find(pBuilding.Convert<TechnoClass>());
                if(!technoExt.IsNullOrExpired())
                {
                    var gext = technoExt.GameObject.GetTechnoGlobalComponent();
                    if(gext!=null)
                    {
                        gext.InfiltratedBy(pEnter);
                    }
                }
            }
            return 0;
        }



        //Prevent_7BAEA1
        [Hook(HookType.AresHook, Address = 0x7BAE60, Size = 5)]
        public static unsafe UInt32 BSurface_GetPixel(REGISTERS* R)
        {
            Pointer<BSurface> pSurface = (IntPtr)R->ECX;
            Pointer<Point2D> pPoint = R->Stack<Pointer<Point2D>>(0x4);

            if(pPoint.Ref.X > pSurface.Ref.BaseSurface.Width || pPoint.Ref.Y > pSurface.Ref.BaseSurface.Height)
            {
                pPoint.Ref.X = 0;
                pPoint.Ref.Y = 0;
            }

            return 0;
        }



        /// <summary>
        /// Prevent 004C2C19
        /// From Modenc
        /// 	A vehicle that did not achieve complete "independent existence" launched a weapon with EBolt special effects, the game crashes due to the loss of the shooter's pointer.
        ///        Examples:
        ///A vehicle, as a passenger with a vehicle with OpenTopped=true(like Battle Fortress), fires outwards.
        ///At this point, the Battle Fortress is suddenly destroyed, which has a high chance of causing a crash.
        ///Replacing IsElectricBolt with IsLaser can partially reduce the probability of crashing, but it still IE(0046650D), albeit with a slightly lower probability.
        ///After a huge number of tests, it seems very stable if the payload is an Infantry instead of a Vehicle.
        ///If we don't use any conventional weapon effects (including lasers) and instead use the Railgun particle system's laser and other particle effects, it seems to be quite stable
        /// </summary>
        /// <param name="R"></param>
        /// <returns></returns>
        [Hook(HookType.AresHook, Address = 0x6FD5FC, Size = 10)]
        public static unsafe UInt32 Set_EBolt_Owner(REGISTERS* R)
        {
            Pointer<UnitClass> pUnit = (IntPtr)R->ECX;
            if(!pUnit.Ref.Base.Base.Base.InLimbo)
            {
                return 0;
            }
            return 0X6FD60B;
        }



        //[Hook(HookType.AresHook, Address = 0x6FC339, Size = 6)]
        //public static unsafe UInt32 TechnoClass_CanFire(REGISTERS* R)
        //{
        //    Pointer<TechnoClass> pTechno = (IntPtr)(void*)R->ESI;
        //    Pointer<WeaponTypeClass> pWeapon = (IntPtr)(void*)R->EDI;
        //    Pointer<AbstractClass> pTarget = R->Stack<Pointer<AbstractClass>>(0x20 - (-0x4));
        //    TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
        //    var gscript = ext.GameObject.GetTechnoGlobalComponent();
        //    UInt32 cannotFire = 0x6FCB7E;

        //    if (gscript != null)
        //    {
        //        if (!gscript.CanFire(pTarget, pWeapon))
        //        {
        //            return cannotFire;
        //        }
        //    }
        //    return 0;
        //}

        //[Hook(HookType.AresHook, Address = 0x739450, Size = 5)]
        //public static unsafe UInt32 Unit_Class_Deploy_LocationFix(REGISTERS* R)
        //{
        //    Pointer<UnitClass> pThis = (IntPtr)R->EBP;
        //    var deploysInto = pThis.Ref.Base.Base.Type.Ref.DeploysInto;
        //    CellStruct mapCoords = pThis.Ref.Base.Base.Base.GetMapCoords();
        //    R->Stack(0x28 - 0x10, mapCoords);
        //    var width = deploysInto.Ref.GetFoundationWidth();
        //    var height = deploysInto.Ref.GetFoundationHeight(false);

        //    var x = mapCoords.X;
        //    var y = mapCoords.Y;

        //    if (width > 2)
        //        x = (short)(mapCoords.X - (short)(Math.Ceiling(width / 2.0) - 1));
        //    if (height > 2)
        //        y = (short)(mapCoords.Y - (short)(Math.Ceiling(height / 2.0) - 1));

        //    R->Stack(0x28 - 0x14, new CellStruct(x, y));
        //    return 0x7394BE;
        //}

    }
}
