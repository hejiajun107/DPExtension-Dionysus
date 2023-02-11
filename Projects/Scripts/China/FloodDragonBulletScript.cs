using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(FloodDragonBulletScript))]
    public class FloodDragonBulletScript : BulletScriptable
    {
        public FloodDragonBulletScript(BulletExt owner) : base(owner)
        {
        }

        private int delay = 0;

        private static Pointer<WeaponTypeClass> pWeap => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("C094TorpedoSP");

        public override void OnUpdate()
        {
            if (delay++ >= 5)
            {
                var distance = GameUtil.BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords(), Owner.OwnerObject.Ref.Target.IsNull ? Owner.OwnerObject.Ref.TargetCoords : Owner.OwnerObject.Ref.Target.Ref.GetCoords());
                if (distance <= 5 * Game.CellSize)
                {
                    var bullet = pWeap.Ref.Projectile.Ref.CreateBullet(Owner.OwnerObject.Ref.Target, Owner.OwnerObject.Ref.Owner, pWeap.Ref.Damage, pWeap.Ref.Warhead, pWeap.Ref.Speed, pWeap.Ref.Warhead.Ref.Bright);
                    var velocity = Owner.OwnerObject.Ref.Velocity;
                    bullet.Ref.DamageMultiplier = Owner.OwnerRef.DamageMultiplier;
                    bullet.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords(), new BulletVelocity(velocity.X, velocity.Y, 100));
                    bullet.Ref.SetTarget(Owner.OwnerObject.Ref.Target);
                    Owner.OwnerRef.Base.UnInit();
                }
            }
        }

    }
}
