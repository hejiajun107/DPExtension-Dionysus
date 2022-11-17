using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(HonorRetiredAttachEffect))]
    public class HonorRetiredAttachEffect : AttachEffectScriptable
    {
        public HonorRetiredAttachEffect(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        private static Pointer<WarheadTypeClass> wh1 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("HonorExpWh1");
        private static Pointer<WarheadTypeClass> wh2 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("HonorExpWh2");

        public override void OnUpdate()
        {
            Duration = 2000;
            base.OnUpdate();
        }

        public override void OnRemove()
        {
            base.OnRemove();
            if (Owner.OwnerObject.Ref.Base.Health <= 0)
            {
                int strength = Owner.OwnerObject.Ref.Type.Ref.Base.Strength;
                var damage = 50 + strength / 5;

                if (strength > 500)
                {
                    ExplodeAt(damage, wh2, Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
                else
                {
                    ExplodeAt(damage, wh1, Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }

            }
        }

        private void ExplodeAt(int damage, Pointer<WarheadTypeClass> warhead, CoordStruct location)
        {
            var bullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, warhead, 100, true);
            bullet.Ref.DetonateAndUnInit(location);
        }
    }
}
