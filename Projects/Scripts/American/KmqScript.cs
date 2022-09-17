using DpLib.Scripts.Yuri;
using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.American
{
    [Serializable]
    [ScriptAlias(nameof(KmqScript))]
    public class KmqScript : TechnoScriptable
    {
        public KmqScript(TechnoExt owner) : base(owner)
        {
        }

        private Pointer<WeaponTypeClass> weapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("KmqGuideWeapon");

        private int delay = 180;
        public override void OnUpdate()
        {
            if (delay > 0)
            {
                delay--;
            }
            base.OnUpdate();
        }


        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 0)
            {
                if (delay <= 0)
                {
                    delay = 180;

                    var bullet = weapon.Ref.Projectile.Ref.CreateBullet(pTarget, Owner.OwnerObject, 1, weapon.Ref.Warhead, 100, false);
                    bullet.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 100), new BulletVelocity(0, 0, 0));
                    bullet.Ref.SetTarget(pTarget);
                }
            }
        }
    }
}
