using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace Scripts.American
{
    [ScriptAlias(nameof(FreezingWingBulletScript))]
    [Serializable]
    public class FreezingWingBulletScript : BulletScriptable
    {
        public FreezingWingBulletScript(BulletExt owner) : base(owner)
        {
        }

        private static Pointer<WeaponTypeClass> weapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ICWGGunWp");

        private bool inited = false;
        public override void OnUpdate()
        {
            if(!inited)
            {
                inited = true;
                var ptechno = Owner.OwnerObject.Ref.Owner;
                if(ptechno.IsNotNull)
                {
                    var rd = new Random(Owner.OwnerObject.Ref.SourceCoords.X + Owner.OwnerObject.Ref.SourceCoords.Y - Owner.OwnerObject.Ref.SourceCoords.Z);

                    for (var i = 0; i <= 1; i++)
                    {
                        if (Owner.OwnerRef.Target.IsNotNull)
                        {
                            var bullet = weapon.Ref.Projectile.Ref.CreateBullet(Owner.OwnerObject.Ref.Target, ptechno, weapon.Ref.Damage, weapon.Ref.Warhead, weapon.Ref.Speed, true);
                            bullet.Ref.MoveTo(Owner.OwnerObject.Ref.SourceCoords, new BulletVelocity(0, 0, 0));
                            if (rd.Next(0, 100) >= 20)
                            {
                                bullet.Ref.TargetCoords = bullet.Ref.TargetCoords + new CoordStruct(rd.Next(-300, 300), rd.Next(-300, 300), 0);
                            }
                        }
                    }
                }
            }
        }
    }
}
