using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(J20ExplodeAttachEffect))]
    public class J20ExplodeAttachEffect : AttachEffectScriptable
    {
        public J20ExplodeAttachEffect(TechnoExt owner) : base(owner)
        {
        }

        private int Count = 1;

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            Duration = duration;
            Count++;
            if (Count >= 4)
            {
                Count = 0;

                if(!pAttacker.IsNull)
                {
                    if(pAttacker.CastToTechno(out var attacker))
                    {
                        var wh = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("J20FireExpWH");
                        var bullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisble").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), attacker, 80, wh, 100, false);
                        bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    }
                }
            }
        }
    }
}
