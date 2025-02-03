using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DpLib.Scripts.Heros
{

    [Serializable]
    [ScriptAlias(nameof(ColdBladeScript))]
    public class ColdBladeScript : TechnoScriptable
    {
        public ColdBladeScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner,10);
        }

        private ManaCounter _manaCounter;


        private bool isActived = false;



        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("DFMissleSeeker");

        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DFMissleHE");


        Random random = new Random(142543);


        //中心点位置
        private CoordStruct center;

        private int maxAttackCount = 30;

        private int delay = 8;

        private int currentFrame = 0;

        private static Pointer<WarheadTypeClass> wCharger => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChargeWeaponWh");
        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private Queue<CoordStruct> targetCoords = new Queue<CoordStruct>();


        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);
                if (_manaCounter.Cost(20))
                {
                    var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Cast<AbstractClass>(), Owner.OwnerObject, 1, wCharger, 100, false);
                    bullet.Ref.DetonateAndUnInit(Owner.OwnerRef.Base.Base.GetCoords());
                }
            }

            if (isActived)
            {
                if (currentFrame >= delay)
                {
                    if (targetCoords.Count() > 0)
                    {
                        var tcenter = targetCoords.Dequeue();

                        for (int i = 0; i < 2; i++)
                        {
                            //导弹轰炸
                            var ntargetGround = new CoordStruct(tcenter.X + random.Next(-Game.CellSize * 2, Game.CellSize), tcenter.Y + random.Next(-Game.CellSize * 2, Game.CellSize), tcenter.Z);
                            var ntarget = new CoordStruct(ntargetGround.X, ntargetGround.Y, ntargetGround.Z + 3800);

                            if (MapClass.Instance.TryGetCellAt(ntargetGround, out Pointer<CellClass> pCell))
                            {
                                var damage = Owner.OwnerObject.Ref.Veterancy.IsElite() ? 70 : 50;
                                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pCell.Convert<AbstractClass>(), Owner.OwnerObject, damage, warhead, 150, true);
                                BulletVelocity velocity = new BulletVelocity(0, 0, 0);
                                //var cellCoord = pCell.Ref.GetCenterCoords();
                                //var t = ntargetGround.BigDistanceForm(cellCoord) / 120;
                                //velocity.X = (ntarget.X - cellCoord.X) / t;
                                //velocity.Y = (ntarget.Y - cellCoord.Y) / t;
                                //velocity.Z = (ntarget.Z - cellCoord.Z) / t;
                                pBullet.Ref.MoveTo(ntarget, velocity);
                                pBullet.Ref.SetTarget(pCell.Convert<AbstractClass>());
                            }
                        }
                    }
                    else
                    {
                        isActived = false;
                        currentFrame = 0;
                    }
                    currentFrame = 0;
                }
                else
                {
                    currentFrame++;
                }


            }
        }



        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 0)
            {
                if (_manaCounter.Cost(80))
                {
                    center = pTarget.Ref.GetCoords();
                    targetCoords.Clear();

                    var picked = ObjectFinder.FindTechnosNear(center, 5 * Game.CellSize).Select(x => x.Convert<TechnoClass>()).Where(x => !x.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner) && !x.Ref.Base.Base.IsInAir() && !x.Ref.Base.InLimbo).OrderBy(x => x.Ref.Base.Base.GetCoords().BigDistanceForm(center))
                        .Take(30);

                    foreach(var pick in picked)
                    {
                        targetCoords.Enqueue(pick.Ref.Base.Base.GetCoords());
                    }

                    while (targetCoords.Count() < 30)
                    {
                        targetCoords.Enqueue(new CoordStruct(center.X + random.Next(-4 * Game.CellSize, 4 * Game.CellSize), center.Y + random.Next(-4 * Game.CellSize, 4 * Game.CellSize), center.Z));
                    }

                    isActived = true;
                    currentFrame = 0;
                }
            }


        }
    }


    [Serializable]
    [ScriptAlias(nameof(TeslaAmmoAttachEffectScript))]
    public class TeslaAmmoAttachEffectScript : AttachEffectScriptable
    {
        public TeslaAmmoAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }


        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
           
        }

        public override void OnUpdate()
        {
            Duration = 100;
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(weaponIndex == 0)
            {
                var location = pTarget.Ref.GetCoords();
                var technos = ObjectFinder.FindTechnosNear(location, 5 * Game.CellSize)
                    .OrderByDescending(x => x.Ref.Base.GetCoords()
                    .DistanceFrom(location))
                    .Select(x => x.Convert<TechnoClass>())
                    .Where(x => !x.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner)).Take(3)
                    .ToList();

                var count = technos.Count();

                CoordStruct last = pTarget.Ref.GetCoords();
                
                var warhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Electric");
                var warheadM = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("TankElectric");
                var bullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

                var pfBullet = bullet.Ref.CreateBullet(pTarget.Convert<AbstractClass>(), Owner.OwnerObject, 20, warheadM, 100, true);
                pfBullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());

                for (var i = 0; i < count; i++)
                {
                    var techno = technos[i];
                    if(techno.Ref.Base.Base.GetCoords().DistanceFrom(pTarget.Ref.GetCoords())<100)
                    {
                        continue;
                    }
                    var pBullet = bullet.Ref.CreateBullet(techno.Convert<AbstractClass>(), Owner.OwnerObject, 30, warhead, 100, true);
                    pBullet.Ref.DetonateAndUnInit(techno.Ref.Base.Base.GetCoords());
                    if (i > 0)
                    {
                        Pointer<EBolt> pBolt = YRMemory.Create<EBolt>();
                        if (!pBolt.IsNull)
                        {
                            pBolt.Ref.Fire(last, techno.Ref.Base.Base.GetCoords(), 0);
                            pBolt.Ref.AlternateColor = false;
                        }
                        last = techno.Ref.Base.Base.GetCoords();
                    }
                }

                if (count < 3)
                {
                    for (var j = 0; j < 3 - count; j++)
                    {
                        var target = location + new CoordStruct(MathEx.Random.Next(-5 * Game.CellSize, 5 * Game.CellSize), MathEx.Random.Next(-5 * Game.CellSize, 5 * Game.CellSize), 0);
                        var pBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 30, warhead, 100, true);
                        pBullet.Ref.DetonateAndUnInit(target);

                        Pointer<EBolt> pBolt = YRMemory.Create<EBolt>();
                        if (!pBolt.IsNull)
                        {
                            pBolt.Ref.Fire(last, target, 0);
                            pBolt.Ref.AlternateColor = false;
                        }
                        last = target;
                    }
                }


            }
        }




    }



    [Serializable]
    [ScriptAlias(nameof(LaserPointerAttachEffectScript))]
    public class LaserPointerAttachEffectScript : AttachEffectScriptable
    {
        public LaserPointerAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private int rof = 80;


        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {

        }

        public override void OnUpdate()
        {
            Duration = 100;
            if(rof>0)
            {
                rof--;
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(weaponIndex==0 && rof <= 0)
            {
                rof = 80;

                var laserColor = new ColorStruct(255, 0, 0);
                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0,0,100), pTarget.Ref.GetCoords(), laserColor, laserColor, laserColor, 10);
                pLaser.Ref.Thickness = 5;
                pLaser.Ref.IsHouseColor = true;

                var bullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                var pBullet = bullet.Ref.CreateBullet(pTarget, Owner.OwnerObject, 1,WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("LaserPointerWh") , 100, true);
                pBullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
            }
        }

    }

    [Serializable]
    [ScriptAlias(nameof(MedicalPackageAttchEffectScript))]
    public class MedicalPackageAttchEffectScript : AttachEffectScriptable
    {
        public MedicalPackageAttchEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private int delay = 500;
        private int rof = 30;

        public override void OnUpdate()
        {
            Duration = 100;

            if (Owner.OwnerObject.Ref.Base.InLimbo)
                return;

            if (delay > 0)
            {
                delay--;
            }

            if(delay>0)
                return;

            if (rof > 0)
            {
                rof--;
                return;
            }

            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var techno = ObjectFinder.FindTechnosNear(location, 3 * Game.CellSize).Where(x => x.Ref.Base.WhatAmI() == AbstractType.Infantry)
                .Select(x => x.Convert<TechnoClass>()).
                Where(x => x.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                .Where(x => x.Ref.Base.Health < x.Ref.Type.Ref.Base.Strength).OrderBy(x => x.Ref.Base.Base.GetCoords().DistanceFrom(location))
                .FirstOrDefault();
                ;

            if(techno != null && techno.IsNotNull)
            {
                rof = 30;
                var health = techno.Ref.Base.Health + 30;
                techno.Ref.Base.Health = techno.Ref.Type.Ref.Base.Strength > health ? health : techno.Ref.Type.Ref.Base.Strength;
                YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("VOLHEAL"), techno.Ref.Base.Base.GetCoords());
            }

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            delay = 500;
            base.OnFire(pTarget, weaponIndex);
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            delay = 500;
            base.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
        }
    }

}
