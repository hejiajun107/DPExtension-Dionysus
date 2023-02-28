using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using System;

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

        private int currentAttackCount = 0;

        private int delay = 8;

        private int currentFrame = 0;

        private static Pointer<WarheadTypeClass> wCharger => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChargeWeaponWh");
        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");


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
                    if (currentAttackCount < maxAttackCount)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            //导弹轰炸
                            var ntarget = new CoordStruct(center.X + random.Next(-1000, 1000), center.Y + random.Next(-1000, 1000), 2000);
                            var ntargetGround = new CoordStruct(center.X + random.Next(-1000, 1000), center.Y + random.Next(-1000, 1000), -center.Z);

                            if (MapClass.Instance.TryGetCellAt(ntargetGround, out Pointer<CellClass> pCell))
                            {
                                var damage = Owner.OwnerObject.Ref.Veterancy.IsElite() ? 70 : 50;
                                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, warhead, 70, true);
                                BulletVelocity velocity = new BulletVelocity(0, 0, 0);
                                pBullet.Ref.MoveTo(ntarget, velocity);
                                pBullet.Ref.SetTarget(pCell.Convert<AbstractClass>());
                            }
                        }

                        currentAttackCount++;
                    }
                    else
                    {
                        isActived = false;
                        currentFrame = 0;
                        currentAttackCount = 0;
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
                if (_manaCounter.Cost(100))
                {
                    center = pTarget.Ref.GetCoords();
                    isActived = true;
                    currentAttackCount = 0;
                    currentFrame = 0;
                }
            }


        }
    }
}
