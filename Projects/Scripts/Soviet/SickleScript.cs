using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.Soviet
{

    [Serializable]
    [ScriptAlias(nameof(SickleScript))]
    public class SickleScript : TechnoScriptable
    {
        public SickleScript(TechnoExt owner) : base(owner) 
        {
        
        }

        private static Pointer<WeaponTypeClass> jumpWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("JumpWeapon");

        private int jumpCoodDown = 0;


        public override void OnUpdate()
        {
            if (jumpCoodDown > 0)
            {
                jumpCoodDown--;
                return;
            }

            var target = Owner.OwnerObject.Ref.Target;

            if (target.IsNull)
            {
                return;
            }

            if (target.Ref.WhatAmI() != AbstractType.Building && target.Ref.IsInAir() == false)
            {
                var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var targetLocation = target.Ref.GetCoords();
                var distance = currentLocation.DistanceFrom(targetLocation);
                if (distance >= 6 * 256 && distance <= 4500)
                {
                    JumpTo(target);
                    jumpCoodDown = 500;
                }

            }
        }


        private void JumpTo(Pointer<AbstractClass> target)
        {
            Pointer<BulletClass> bullet = jumpWeapon.Ref.Projectile.Ref.CreateBullet(target, Owner.OwnerObject, jumpWeapon.Ref.Damage, jumpWeapon.Ref.Warhead, 60, true);
            bullet.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0,0,50), new BulletVelocity(0, 0, 0));
            bullet.Ref.SetTarget(target);
        }

    }
}
