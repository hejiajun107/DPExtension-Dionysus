using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.Heros
{
    [Serializable]
    [ScriptAlias(nameof(ScrinEggScript))]

    public class ScrinEggScript : TechnoScriptable
    {
        public ScrinEggScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter();
        }

        private ManaCounter _manaCounter;

        private Random random = new Random(12352);

        private static Pointer<WeaponTypeClass> MeotorWeapon1 => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("Meotor1Weapon");
        private static Pointer<WeaponTypeClass> MeotorWeapon2 => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("Meotor2Weapon");
        private static Pointer<WeaponTypeClass> MeotorWeapon3 => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("Meotor3Weapon");

        private static Pointer<WarheadTypeClass> blastWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EggBlastWh");
        private static Pointer<BulletTypeClass> pBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisoMissile");


        //部署标记仅AI使用
        private bool depolyed = false;

        private int delay = 150;


        public override void OnUpdate()
        {
            //这段仅供AI解除部署
            if (depolyed)
            {
                var mission = Owner.OwnerObject.Convert<MissionClass>();

                if (delay <= 0)
                {
                    depolyed = false;
                    Owner.OwnerObject.Ref.SetTarget(default);
                    mission.Ref.QueueMission(Mission.Guard, false);
                    mission.Ref.NextMission();
                }
                else
                {
                    if (delay % 10 == 0)
                    {
                        FireMeotor();
                    }
                    mission.Ref.ForceMission(Mission.Stop);
                }

                delay--;
            }

            _manaCounter.OnUpdate(Owner);
            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);

            if (weaponIndex == 0)
            {
                var blastCount = 6;
                var blastDamage = 25;
                var targetLocation = pTarget.Ref.GetCoords();
                if (Owner.OwnerObject.Ref.Veterancy.IsElite())
                {
                    blastCount = 10;
                    blastDamage = 40;
                }

                for (var i = 0; i < blastCount; i++)
                {
                    var tlocation = targetLocation + new CoordStruct(random.Next(-400, 400), random.Next(-400, 400), 0);
                    if (MapClass.Instance.TryGetCellAt(tlocation, out var pcell))
                    {
                        var bullet = pBullet.Ref.CreateBullet(pcell.Convert<AbstractClass>(), Owner.OwnerObject, blastDamage, blastWh, 60, false);
                        bullet.Ref.MoveTo(tlocation + new CoordStruct(0, 0, 150 * i), new BulletVelocity(0, 0, 0));
                        bullet.Ref.SetTarget(pcell.Convert<AbstractClass>());
                    }
                }
            }
            if (weaponIndex == 1)
            {
                FireMeotor();
            }
        }

        private void FireMeotor()
        {
            var damageMuliteple = 1d;

            var costResult = false;

            if (Owner.OwnerObject.Ref.Veterancy.IsElite())
            {
                damageMuliteple = 1.2d;
                costResult = _manaCounter.Cost(3);
            }
            else
            {
                costResult = _manaCounter.Cost(5);
            }

            if (costResult)
            {
                var rd = random.Next(0, 3);
                var weapon = GetWeapon(rd);

                var target = Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(random.Next(-1400, 1400), random.Next(-1400, 1400), 0);
                if (MapClass.Instance.TryGetCellAt(target, out var pCell))
                {
                    var cellTarget = pCell.Convert<AbstractClass>();
                    var startLocation = target + new CoordStruct(0, 0, 3000);

                    var anim = YRMemory.Create<AnimClass>(weapon.Ref.Anim.Get(0), startLocation);

                    var bullet = weapon.Ref.Projectile.Ref.CreateBullet(cellTarget, Owner.OwnerObject, (int)(weapon.Ref.Damage * damageMuliteple), weapon.Ref.Warhead, weapon.Ref.Speed, false);
                    bullet.Ref.MoveTo(startLocation, new BulletVelocity(0, 0, 0));
                    bullet.Ref.SetTarget(cellTarget);
                }
            }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            base.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
            if (depolyed)
                return;
            if (pAttacker.IsNull)
                return;
            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;
            if (Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                return;

            if (pAttacker.CastToTechno(out var pTechno))
            {
                if (pTechno.Ref.Owner.IsNull)
                    return;
                if (pTechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                    return;

                depolyed = true;


                var mission = Owner.OwnerObject.Convert<MissionClass>();
                if (mission != null)
                {
                    delay = 150;
                    mission.Ref.ForceMission(Mission.Stop);
                    //var target = Owner.OwnerObject.Ref.Base.Base.GetDestination();
                    //if (MapClass.Instance.TryGetCellAt(target, out var pcell))
                    //{
                    //    Owner.OwnerObject.Ref.SetTarget(pcell.Convert<AbstractClass>());
                    //    mission.Ref.ForceMission(Mission.Guard);
                    //}
                }
            }
        }

        private Pointer<WeaponTypeClass> GetWeapon(int index)
        {
            switch (index)
            {
                case 0:
                    return MeotorWeapon1;
                case 1:
                    return MeotorWeapon2;
                case 2:
                    return MeotorWeapon3;
                default:
                    return MeotorWeapon1;
            }
        }


    }
}
