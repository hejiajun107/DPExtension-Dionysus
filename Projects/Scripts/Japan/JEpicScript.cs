using Extension.Coroutines;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections;

namespace DpLib.Scripts.Japan
{
    [Serializable]
    [ScriptAlias(nameof(JEpicScript))]
    public class JEpicScript : TechnoScriptable
    {
        public JEpicScript(TechnoExt owner) : base(owner) { }

        private int delay = 40;

        private Random random = new Random(11123);

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> expWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EpicPoExpWh");

        static Pointer<SuperWeaponTypeClass> swVirus => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("UnitDeliveryForVirus");

        static Pointer<AnimTypeClass> anim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("PIFFPIFF");

        private bool IsExploding = false;

        private int explodeDelay = 200;

        private static Pointer<BulletTypeClass> pLaserBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("JEpicLaser");

        private static Pointer<WarheadTypeClass> chargingWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SRChargingWh");

        private bool IsDead = false;


        private static Pointer<WarheadTypeClass> slowWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("JEPRippleWh");


        private static Pointer<WarheadTypeClass> toGroundWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("JEPBombWH");


        public override void OnUpdate()
        {
            base.OnUpdate();

            if(IsDead)
            {
                Owner.OwnerObject.Ref.Base.TakeDamage(10000, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Super"), false);
            }

            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                bool canMissionExplode = true;
                var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                if(MapClass.Instance.TryGetCellAt(coord,out var pcell))
                {
                    var pBuilding = pcell.Ref.GetBuilding();
                    
                    if(pBuilding.IsNotNull)
                    {
                        if (pBuilding.Ref.Base.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                        {
                            canMissionExplode = false;
                        }
                    }
                    
                }

                if(canMissionExplode)
                {
                    IsExploding = !IsExploding;
                    explodeDelay = 200;
                    mission.Ref.ForceMission(Mission.Stop);
                }
            }


            if (IsExploding)
            {
                if (explodeDelay > 0)
                {
                    if (explodeDelay % 5 == 0 && explodeDelay > 0 && explodeDelay != 0)
                    {
                        var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                        for (var i = 0; i < 5; i++)
                        {
                            var bullet = pLaserBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 200, chargingWh, 80, true);
                            bullet.Ref.MoveTo(location + new CoordStruct(random.Next(-1000, 1000), random.Next(-1000, 1000), Owner.OwnerObject.Ref.Base.GetHeight() + 500), new BulletVelocity(random.Next(0, 90), random.Next(0, 90), random.Next(0, 90)));
                            bullet.Ref.SetTarget(Owner.OwnerObject.Convert<AbstractClass>());
                        }
                    }

                    explodeDelay--;
                }
                else
                {
                    if (explodeDelay == 0)
                    {
                        var yellowAmount = Owner.OwnerObject.Ref.Tiberium.GetAmount(0);
                        var colorfulAmount = Owner.OwnerObject.Ref.Tiberium.GetAmount(1);
                        Owner.OwnerObject.Ref.Tiberium.RemoveAmount(colorfulAmount, 1);
                        Owner.OwnerObject.Ref.Tiberium.RemoveAmount(yellowAmount, 0);
                        Owner.GameObject.StartCoroutine(Boom((int)yellowAmount, (int)colorfulAmount, Owner.OwnerObject.Ref.Base.Base.GetCoords()));
                        explodeDelay--;
                    }
                    else
                    {
                        if (Owner.OwnerObject.CastToFoot(out var pfoot))
                        {
                            pfoot.Ref.SpeedMultiplier = 0;
                        }
                    }
                }
            }



            if (delay-- > 0) return;

            delay = 40;

            var currentAmount = Owner.OwnerObject.Ref.Tiberium.GetTotalAmount();

            if (currentAmount < 120)
            {
                //获取脚下的矿
                var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var currentCell = CellClass.Coord2Cell(coord);

                var enumerator = new CellSpreadEnumerator(1);

                foreach (CellStruct offset in enumerator)
                {
                    currentAmount = (int)Owner.OwnerObject.Ref.Tiberium.GetTotalAmount();

                    if (currentAmount >= 120) return;

                    CoordStruct where = CellClass.Cell2Coord(currentCell + offset, coord.Z);

                    if (MapClass.Instance.TryGetCellAt(where, out var pCell))
                    {
                        var value = pCell.Ref.GetContainedTiberiumValue();
                        if (value > 0)
                        {

                            var index = pCell.Ref.GetContainedTiberiumIndex();
                            var amount = value / ((index == 0 || index == 3) ? 25f : 50f);

                            amount = (currentAmount + amount > 120) ? (120 - currentAmount) : amount;


                            Owner.OwnerObject.Ref.Tiberium.AddAmount(amount, index);
                            pCell.Ref.ReduceTiberium((int)amount);
                            YRMemory.Create<AnimClass>(anim, where);
                        }
                    }
                }
            }


            if (Owner.OwnerObject.Ref.Tiberium.GetTotalAmount() >= 120)
            {
                var pWarhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EPICMONEYWH");
                var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 0, pWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

            }
            else
            {
                if (random.Next(100) >= 70)
                {
                    Owner.OwnerObject.Ref.Tiberium.AddAmount(1, 1);
                }
                else
                {
                    Owner.OwnerObject.Ref.Tiberium.AddAmount(1, 0);
                }
            }



        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);

            if (weaponIndex == 0)
            {
                var total = Owner.OwnerObject.Ref.Tiberium.GetTotalAmount();

                if (total >= 10)
                {
                    var yellowAmount = Owner.OwnerObject.Ref.Tiberium.GetAmount(0);
                    var colorfulAmount = Owner.OwnerObject.Ref.Tiberium.GetAmount(1);
                    var yellow2Amount = Owner.OwnerObject.Ref.Tiberium.GetAmount(2);


                    var colorful = colorfulAmount > 10 ? 10 : colorfulAmount;
                    var yellow = (int)((10 - colorful) > yellowAmount ? yellowAmount : 10 - colorful);
                    var yellow2 = 0;
                    if(colorful - yellow < 10)
                    {
                        yellow2 = 10 - (int)colorful - (int)yellow;
                    }


                    //每次消耗5个矿
                    Owner.OwnerObject.Ref.Tiberium.RemoveAmount(colorful, 1);
                    Owner.OwnerObject.Ref.Tiberium.RemoveAmount(yellow, 0);

                    if(yellow2 > 0)
                    {
                        Owner.OwnerObject.Ref.Tiberium.RemoveAmount(yellow2, 2);
                    }

                    //todo
                    var damage = (int)(yellowAmount * 15 + colorful * 30);
                    Pointer<BulletClass> expBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, expWarhead, 50, false);
                    expBullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());

                    //放置毒云
                    Pointer<HouseClass> pOwner = Owner.OwnerObject.Ref.Owner;
                    Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(swVirus);
                    CellStruct targetCell = CellClass.Coord2Cell(pTarget.Ref.GetCoords());
                    pSuper.Ref.IsCharged = true;
                    pSuper.Ref.Launch(targetCell, true);
                    pSuper.Ref.IsCharged = false;

                }
            }



        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (explodeDelay < 0)
            {
                if (pWH.IsNotNull)
                {
                    if(pWH.Ref.Base.ID != "Super")
                    {
                        pDamage.Ref = 0;
                        return;
                    }
                }
            }
        }



        IEnumerator Boom(int yellow, int colorful, CoordStruct center)
        {
            var count = yellow + colorful;

            var max = count / 4 + 5;


            var waveCount = 6;

            for (var radius = 1; radius <= 15; radius++)
            {
                var r = radius * Game.CellSize;

                for (var angle = 0; angle < 360; angle += 360 / waveCount)
                {
                    var pos = new CoordStruct(center.X + (int)(r * Math.Round(Math.Cos(angle * Math.PI / 180), 2)), center.Y + (int)(r * Math.Round(Math.Sin(angle * Math.PI / 180), 2)), center.Z);
                    var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                    int damage = 10;
                    Pointer<BulletClass> pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, slowWh, 100, true);
                    pBullet.Ref.DetonateAndUnInit(pos);
                }

                waveCount++;

                yield return new WaitForFrames(5);
            }

            for (var radius = 15; radius > 0; radius--)
            {
                var burstCount = 6 + radius * 2 + max * radius / 15;
                var r = radius * Game.CellSize;

                for (var angle = 0; angle < 360; angle += 360 / burstCount)
                {
                    var pos = new CoordStruct(center.X + (int)(r * Math.Round(Math.Cos(angle * Math.PI / 180), 2)), center.Y + (int)(r * Math.Round(Math.Sin(angle * Math.PI / 180), 2)), center.Z);
                    var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                    int damage = 100;
                    Pointer<BulletClass> pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, toGroundWh, 100, true);
                    pBullet.Ref.DetonateAndUnInit(pos);
                }

                yield return new WaitForFrames(10);
            }

            yield return new WaitForFrames(5);


            IsDead = true;
        }


    }
}
