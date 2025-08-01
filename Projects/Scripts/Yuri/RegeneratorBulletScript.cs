﻿using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DpLib.Scripts.Yuri
{
    [Serializable]
    [ScriptAlias(nameof(RegeneratorBulletScript))]
    public class RegeneratorBulletScript : BulletScriptable
    {
        public RegeneratorBulletScript(BulletExt owner) : base(owner) { }

        private bool isActived = false;

        static Pointer<WarheadTypeClass> expWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenExplodeWh");

        static Pointer<BulletTypeClass> seekerBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenSeeker");



        static Pointer<BulletTypeClass> expBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenMissile");
        static Pointer<WarheadTypeClass> virusWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenExplodeWh3");
        static Pointer<WarheadTypeClass> nukeWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenExplodeWh4");
        static Pointer<WarheadTypeClass> repairWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenExplodeWh2");

        static Pointer<WarheadTypeClass> mindWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenExplodeWh5");

        static Pointer<WarheadTypeClass> genicWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenExplodeWh6");



        //static Pointer<SuperWeaponTypeClass> psychicDominator => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("PsychicDominator4YP");
        static Pointer<BulletTypeClass> mindBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("CtrlToGround");
        static Pointer<WarheadTypeClass> mindCtrlWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("GenCtrlAirWh");

        static Pointer<BulletTypeClass> mindBulletTypeCL => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("CtrlToGround2");

        static Pointer<WarheadTypeClass> mindCtrlWarheadCL => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SplitControllerCL");




        private static Dictionary<string, int> specialWarheadList = new Dictionary<string, int>()
        {
            { "ENGINEER",2},
            { "CNENGINEER",2},
            { "SENGINEER",2},
            { "YENGINEER",2},
            { "VIRUS",3},
            { "Laserman",3},
            { "PRES",4},
            { "YURIPR",5 },
            { "YURIGAI",6 },
            { "YURI",7 }
        };

        public override void OnUpdate()
        {
            if (isActived) return;

            isActived = true;
            var ptechno = Owner.OwnerObject.Ref.Owner;

            if (ptechno.IsNull) return;

            var targetLocation = Owner.OwnerObject.Ref.TargetCoords;

            var random = MathEx.Random;

            //var passengerCount = ptechno.Ref.Passengers.NumPassengers;

            FireExplodeAt(ptechno, targetLocation, 200, 0);

            int i = 0;

            if (ptechno.Ref.Passengers.GetFirstPassenger().IsNull)
            {
                var alliesInf = ObjectFinder.FindObjectsNear(ptechno.Ref.Base.Base.GetCoords(), 3 * Game.CellSize).Select(x => x.Convert<TechnoClass>()).Where(x => x.Ref.Owner == ptechno.Ref.Owner && x.Ref.Base.Base.WhatAmI() == AbstractType.Infantry)
                    .OrderBy(x => x.Ref.Base.Base.GetCoords().DistanceFrom(ptechno.Ref.Base.Base.GetCoords())).Take(20).ToList();

                var spWarhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Special");

                foreach (var item in alliesInf)
                {
                    var strength = item.Ref.Type.Ref.Base.Strength;
                    var id = item.Ref.Type.Ref.Base.Base.ID;
                    FireExplodeAt(ptechno, targetLocation + new CoordStruct(random.Next(-1200, 1200), random.Next(-1200, 1200), 0), 60 + (int)(1.2 * strength), 200 * i, specialWarheadList.ContainsKey(id) ? specialWarheadList[id] : 0);

                    Pointer<BulletClass> bullet = seekerBullet.Ref.CreateBullet(ptechno.Convert<AbstractClass>(), ptechno, 0, spWarhead, 60, true);
                    bullet.Ref.MoveTo(item.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 150), new BulletVelocity(random.Next(-100, 100), random.Next(-100, 100), random.Next(-100, 100)));
                    bullet.Ref.SetTarget(ptechno.Convert<AbstractClass>());

                    item.Ref.Base.TakeDamage(10000, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenKillWh"), false);

                }
            }
            else
            {
                while (!ptechno.Ref.Passengers.GetFirstPassenger().IsNull)
                {
                    i++;
                    var passenger = ptechno.Ref.Passengers.GetFirstPassenger();
                    if (passenger.Convert<AbstractClass>().CastToTechno(out Pointer<TechnoClass> pPassenger))
                    {
                        var strength = pPassenger.Ref.Base.Health;

                        var id = passenger.Ref.Base.Type.Ref.Base.Base.ID.ToString();

                        FireExplodeAt(ptechno, targetLocation + new CoordStruct(random.Next(-1200, 1200), random.Next(-1200, 1200), 0), 60 + (int)(1.2 * strength), 200 * i, specialWarheadList.ContainsKey(id) ? specialWarheadList[id] : 0);
                        pPassenger.Ref.Base.UnInit();
                    }
                }

            }



            base.OnUpdate();
        }

        private void FireExplodeAt(Pointer<TechnoClass> ptechno, CoordStruct location, int damage, int delay = 0, int warheadType = 0)
        {

            Pointer<WarheadTypeClass> warhead;
            switch (warheadType)
            {
                case 0:
                    {
                        warhead = expWarhead;
                        break;
                    }
                case 2:
                    {
                        warhead = repairWarhead;
                        break;
                    }
                case 3:
                    {
                        warhead = virusWarhead;
                        break;
                    }
                case 4:
                    {
                        warhead = nukeWarhead;
                        break;
                    }
                case 5:
                    {
                        warhead = mindWarhead;
                        //Pointer<HouseClass> pOwner = ptechno.Ref.Owner;
                        //Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(psychicDominator);
                        //CellStruct targetCell = CellClass.Coord2Cell(location);
                        //pSuper.Ref.IsCharged = true;
                        //pSuper.Ref.Launch(targetCell, true);
                        //pSuper.Ref.IsCharged = false;
                        break;
                    }
                case 6:
                    {
                        warhead = genicWarhead;
                        break;
                    }
                case 7:
                    {
                        warhead = mindWarhead;
                        break;
                    }
                default:
                    {
                        warhead = expWarhead;
                        break;
                    }
            }


            var cell = CellClass.Coord2Cell(location);
            var bullet = expBulletType.Ref.CreateBullet(ptechno.Convert<AbstractClass>(), ptechno, damage, warhead, 50, true);

            if (delay == 0)
            {
                bullet.Ref.DetonateAndUnInit(location);
            }
            else
            {
                if (MapClass.Instance.TryGetCellAt(cell, out Pointer<CellClass> pcell))
                {
                    bullet.Ref.SetTarget(pcell.Convert<AbstractClass>());
                    bullet.Ref.MoveTo(location + new CoordStruct(0, 0, delay), new BulletVelocity(0, 0, 0));

                    if (warheadType == 5)
                    {
                        var mindBullet = mindBulletType.Ref.CreateBullet(ptechno.Convert<AbstractClass>(), ptechno, 1, mindCtrlWarhead, 50, false);
                        mindBullet.Ref.SetTarget(pcell.Convert<AbstractClass>());
                        mindBullet.Ref.MoveTo(location + new CoordStruct(0, 0, delay + 2000), new BulletVelocity(0, 0, 0));
                    }
                    else if (warheadType == 7)
                    {
                        var clocation = location + new CoordStruct(MathEx.Random.Next(-3 * Game.CellSize, 3 * Game.CellSize), MathEx.Random.Next(-3 * Game.CellSize, 3 * Game.CellSize), 0);
                        var technos = ObjectFinder.FindTechnosNear(clocation, 4 * Game.CellSize).Where(x => (x.Ref.Base.WhatAmI() == AbstractType.Unit || x.Ref.Base.WhatAmI() == AbstractType.Infantry)).OrderBy(x => x.Ref.Base.GetCoords().DistanceFrom(location)).Take(2).ToList();
                        if (technos != null && technos.Count() > 0)
                        {
                            foreach (var techno in technos)
                            {
                                var mindBulletCL = mindBulletTypeCL.Ref.CreateBullet(techno.Convert<AbstractClass>(), ptechno, 1, mindCtrlWarheadCL, 50, false);
                                mindBulletCL.Ref.SetTarget(techno.Convert<AbstractClass>());
                                mindBulletCL.Ref.MoveTo(location + new CoordStruct(0, 0, delay + 2000), new BulletVelocity(0, 0, 0));
                            }
                        }

                    }
                }
            }
        }
    }
}
