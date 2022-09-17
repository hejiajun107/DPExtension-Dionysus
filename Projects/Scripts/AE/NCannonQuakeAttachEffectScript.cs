using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using Scripts.AE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(NCannonQuakeAttachEffectScript))]

    public class NCannonQuakeAttachEffectScript : AttachEffectScriptable
    {
        public NCannonQuakeAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private int damage = 0;

        private int delay = 50;

        private bool exploded = false;

        private static Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisble");

        private static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("NCDERSWH");

        public override void OnUpdate()
        {
            if(!exploded)
            {
                if (delay-- <= 0)
                {
                    //引爆
                    var bullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, warhead, 100, true);
                    bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    Duration = 0;
                }
            }
           
            base.OnUpdate();
        }

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            damage = 15;
            delay = 100;
            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
        }

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if (damage <= 150)
            {
                damage += 15;
                delay += 10;
                Duration += 10;
            }
        }

        public override void OnRemove()
        {
            Duration = 0;
            base.OnRemove();
        }

    }
}
