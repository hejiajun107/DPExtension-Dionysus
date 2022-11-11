using DpLib.Scripts.Japan;
using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(SunRiseFortressScript))]
    [Serializable]
    public class SunRiseFortressScript : TechnoScriptable
    {
        public SunRiseFortressScript(TechnoExt owner) : base(owner)
        {
        }

        private bool startExplode = false;
        private bool blasted = false;
        private int delay = 120;

        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("pInviso");

        private static Pointer<BulletTypeClass> pLaserBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("SunRiseLaser");

        private static Pointer<WarheadTypeClass> chargingWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SRChargingWh");

        private static Pointer<BulletTypeClass> pToGroundBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("SunRiseToGround");
        private static Pointer<WarheadTypeClass> toGroundWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SRBomb4WH");

        private static Pointer<WarheadTypeClass> selfDestructWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChaosDamageWh");

        private static Pointer<AnimTypeClass> pshock => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("ElecBurst");


        private Random random = new Random(114514);

        public override void OnUpdate()
        {
            if(!Owner.OwnerObject.Ref.IsHumanControlled)
            {
                if (Owner.OwnerObject.Ref.Base.Health < 800 && !startExplode)
                    startExplode = true;
            }

            if (!startExplode)
                return;

            if (delay % 5 == 0 && delay > 0)
            {
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                for (var i = 0; i < 5; i++)
                {
                    var bullet = pLaserBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 200, chargingWh, 80, true);
                    bullet.Ref.MoveTo(location + new CoordStruct(random.Next(-1000, 1000), random.Next(-1000, 1000), 0),new BulletVelocity(0,0,0));
                    bullet.Ref.SetTarget(Owner.OwnerObject.Convert<AbstractClass>());
                }
            }
            delay--;

            if (delay <= 0 && !blasted)
            {
                blasted = true;
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var height = Owner.OwnerObject.Ref.Base.GetHeight();

                //爆炸
                for (var i = 0; i < 20; i++)
                {
                    FireBombTo(500, location, height);
                }

                for (var i = 0; i < 15; i++)
                {
                    FireBombTo(1000, location, height);
                }

                for (var i = 0; i < 15; i++)
                {
                    FireBombTo(2000, location, height);
                }

                for (var i = 0; i < 10; i++)
                {
                    FireBombTo(2500, location, height);
                }

                YRMemory.Create<AnimClass>(pshock, location);
                Owner.OwnerObject.Ref.Base.TakeDamage(10000, selfDestructWh, false);
            }
        }

        private void FireBombTo(int radius,CoordStruct location,int height)
        {
            var health = Owner.OwnerObject.Ref.Base.Health;
            var damage = (int)(80 + Math.Round((double)(health / 5200d),2) * 50);
            
            var targetLocation = location + new CoordStruct(random.Next(-radius, radius), random.Next(-radius, radius), -height);
            if(MapClass.Instance.TryGetCellAt(targetLocation,out var pCell))
            {
                var bullet = pToGroundBullet.Ref.CreateBullet(pCell.Convert<AbstractClass>(), Owner.OwnerObject, damage, toGroundWh, 30, true);
                bullet.Ref.MoveTo(location, new BulletVelocity(random.Next(-50,50), random.Next(-50, 50), random.Next(-50, 50)));
                bullet.Ref.SetTarget(pCell.Convert<AbstractClass>());
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 1 && startExplode == false)
            {
                //自爆
                startExplode = true;
                var mission = Owner.OwnerObject.Convert<MissionClass>();
                mission.Ref.ForceMission(Mission.Stop);
            }
        }
    }
}
