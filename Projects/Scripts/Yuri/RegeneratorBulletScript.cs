using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;

namespace DpLib.Scripts.Yuri
{
    [Serializable]
    [ScriptAlias(nameof(RegeneratorBulletScript))]
    public class RegeneratorBulletScript : BulletScriptable
    {
        public RegeneratorBulletScript(BulletExt owner) : base(owner) { }

        private bool isActived = false;

        static Pointer<WarheadTypeClass> expWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenExplodeWh");

        static Pointer<BulletTypeClass> expBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenMissile");
        static Pointer<WarheadTypeClass> virusWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenExplodeWh3");
        static Pointer<WarheadTypeClass> nukeWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenExplodeWh4");
        static Pointer<WarheadTypeClass> repairWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenExplodeWh2");

        static Pointer<WarheadTypeClass> mindWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenExplodeWh5");

        static Pointer<WarheadTypeClass> genicWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenExplodeWh6");



        //static Pointer<SuperWeaponTypeClass> psychicDominator => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("PsychicDominator4YP");
        static Pointer<BulletTypeClass> mindBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("CtrlToGround");
        static Pointer<WarheadTypeClass> mindCtrlWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("GenCtrlAirWh");




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
            { "YURIGAI",6 }
        };

        public override void OnUpdate()
        {
            if (isActived) return;

            isActived = true;
            var ptechno = Owner.OwnerObject.Ref.Owner;

            if (ptechno.IsNull) return;

            var targetLocation = Owner.OwnerObject.Ref.TargetCoords;

            Random random = new Random(targetLocation.X + targetLocation.Y);

            //var passengerCount = ptechno.Ref.Passengers.NumPassengers;

            FireExplodeAt(ptechno, targetLocation, 200, 0);

            int i = 0;

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

                }
            }
        }
    }
}
