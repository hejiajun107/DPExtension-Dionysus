﻿using Extension.CWUtilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;
using PatcherYRpp;
using System.Collections.Generic;
using System.Linq;

namespace Extension.CW
{




    public partial class TechnoGlobalExtension
    {
        private bool isAreaProtecting = false;

        private CoordStruct areaProtectTo;

        private static List<CoordStruct> areaGuardCoords = new List<CoordStruct>()
        {
            new CoordStruct(-300,-300,0),
            new CoordStruct(-300,0,0),
            new CoordStruct(0,0,0),
            new CoordStruct(300,0,0),
            new CoordStruct(300,300,0),
            new CoordStruct(0,300,0),
        };

        private int currentAreaProtectedIndex = 0;

        private bool isAreaGuardReloading = false;

        private int areaGuardTargetCheckRof = 20;


        [UpdateAction]
        public void TechnoClass_Update_Fighter_Area_Guard()
        {

        
            if (!Data.FighterAreaGuard)
            {
                return;
            }

            var mission = Owner.OwnerObject.Convert<MissionClass>();

            if (mission.Ref.CurrentMission == Mission.Move)
            {
                isAreaProtecting = false;
                isAreaGuardReloading = false;
                return;
            }

            if (Owner.OwnerObject.CastToFoot(out Pointer<FootClass> pfoot))
            {
                if (!isAreaProtecting)
                {
                    if (mission.Ref.CurrentMission == Mission.Area_Guard)
                    {
                        isAreaProtecting = true;

                        CoordStruct dest = pfoot.Ref.Locomotor.Destination();
                        areaProtectTo = dest;
                    }
                }


                if (isAreaProtecting)
                {
                    //没弹药的情况下返回机场
                    if(Owner.OwnerObject.Ref.Ammo==0 && !isAreaGuardReloading)
                    {
                        Owner.OwnerObject.Ref.SetTarget(default);
                        Owner.OwnerObject.Ref.SetDestination(default, false);
                        mission.Ref.ForceMission(Mission.Stop);
                        isAreaGuardReloading = true;
                        return;
                    }

                    //填弹完毕后继续巡航
                    if (isAreaGuardReloading)
                    {
                        if(Owner.OwnerObject.Ref.Ammo >= Data.FighterMaxAmmo)
                        {
                            isAreaGuardReloading = false;
                            mission.Ref.ForceMission(Mission.Area_Guard);
                        }
                        else
                        {
                            if (mission.Ref.CurrentMission != Mission.Sleep && mission.Ref.CurrentMission != Mission.Enter)
                            {
                                if (mission.Ref.CurrentMission == Mission.Guard)
                                {
                                    mission.Ref.ForceMission(Mission.Sleep);
                                }
                                else
                                {
                                    mission.Ref.ForceMission(Mission.Enter);
                                }
                                return;
                            }
                        }
                    }
                   

                    if (mission.Ref.CurrentMission == Mission.Move)
                    {
                        isAreaProtecting = false;
                        return;
                    }
                    else if (mission.Ref.CurrentMission == Mission.Attack)
                    {
                        return;
                    }
                    else if (mission.Ref.CurrentMission == Mission.Enter)
                    {
                        if(isAreaGuardReloading)
                        {
                            return;
                        }
                        else
                        {
                            mission.Ref.ForceMission(Mission.Stop);
                        }
                    }else if(mission.Ref.CurrentMission == Mission.Sleep)
                    {
                        if (isAreaGuardReloading)
                        {
                            return;
                        }
                    }


                    if (areaProtectTo != null)
                    {
                        var dest = areaProtectTo;

                        var house = Owner.OwnerObject.Ref.Owner;

                        if(Data.FighterAutoFire)
                        {
                            if (areaProtectTo.DistanceFrom(Owner.OwnerObject.Ref.Base.Base.GetCoords()) <= 2000)
                            {
                                if (areaGuardTargetCheckRof-- <= 0)
                                {
                                    areaGuardTargetCheckRof = 20;

                                    var target = Finder.FineOneTechno(house, x =>
                                    {
                                        var coords = x.Ref.Base.Base.GetCoords();
                                        var height = x.Ref.Base.GetHeight();
                                        var type = x.Ref.Base.Base.WhatAmI();

                                        if (x.Ref.Base.InLimbo)
                                        {
                                            return false;
                                        }

                                        var bounsRange = 0;
                                        if (x.Ref.Base.GetHeight() > 10)
                                        {
                                            bounsRange = Data.FighterGuardRange;
                                        }

                                        if ((coords - new CoordStruct(0, 0, height)).DistanceFrom(dest) <= (Data.FighterGuardRange * 256 + bounsRange) && type != AbstractType.Building)
                                        {
                                            return true;
                                        }
                                        return false;
                                    }, FindRange.Enermy);

                                    if (!target.IsNullOrExpired())
                                    {
                                        Owner.OwnerObject.Ref.SetTarget(target.OwnerObject.Convert<AbstractClass>());
                                        mission.Ref.ForceMission(Mission.Stop);
                                        mission.Ref.ForceMission(Mission.Attack);
                                        return;
                                    }
                                }

                            }
                        }
                       


                        if (areaProtectTo.DistanceFrom(Owner.OwnerObject.Ref.Base.Base.GetCoords()) <= 2000)
                        {
                            if (currentAreaProtectedIndex > areaGuardCoords.Count() - 1)
                            {
                                currentAreaProtectedIndex = 0;
                            }
                            dest += areaGuardCoords[currentAreaProtectedIndex];
                            currentAreaProtectedIndex++;
                        }

                        pfoot.Ref.Locomotor.Move_To(dest);
                        var cell = CellClass.Coord2Cell(dest);
                        if (MapClass.Instance.TryGetCellAt(cell, out Pointer<CellClass> pcell))
                        {
                            Owner.OwnerObject.Ref.SetDestination(pcell.Convert<AbstractClass>(), false);
                        }
                    }
                }
            }

        }

    }

    public partial class TechnoGlobalTypeExt
    {

        [INIField(Key = "Fighter.AreaGuard")]
        public bool FighterAreaGuard = false;
        [INIField(Key = "Fighter.GuardRange")]
        public int FighterGuardRange = 5;
        [INIField(Key = "Fighter.AutoFire")]
        public bool FighterAutoFire = false;
        [INIField(Key = "Ammo")]
        public int FighterMaxAmmo = 0;
    }

}