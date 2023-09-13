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
    [ScriptAlias(nameof(IceWingFreezingAttachEffectScript))]
    public class IceWingFreezingAttachEffectScript : AttachEffectScriptable
    {
        public IceWingFreezingAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private TechnoExt attacker;

        private static Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        private static Pointer<WarheadTypeClass> freezeWhAG => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("FreezingAEWH");
        private static Pointer<WarheadTypeClass> freezeWhAA => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("FreezingAAEWH");


        private int type = 1;

        private int delay = 20;

        public override void OnUpdate()
        {
            if (delay-- <= 0)
            {
                if (Owner.OwnerObject.Ref.Base.InLimbo)
                    return;

                var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                var bullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), getAttacker(), 1, type == 1 ? freezeWhAG : freezeWhAA, 100, true);
                bullet.Ref.DetonateAndUnInit(coord);
                delay = 20;
            }
        }


        private Pointer<TechnoClass> getAttacker()
        {
            Pointer<TechnoClass> launcher;

            //引爆
            //设定发射者
            if (!attacker.IsNullOrExpired())
            {
                launcher = attacker.OwnerObject;
            }
            else
            {
                launcher = Pointer<TechnoClass>.Zero;
            }
            return launcher;
        }

        public override void OnRemove()
        {
            Duration = 0;
            base.OnRemove();
            return;
        }

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {

            //获取发射者
            if (!pAttacker.IsNull)
            {
                if (pAttacker.CastToTechno(out var ptAttcker))
                {
                    attacker = TechnoExt.ExtMap.Find(ptAttcker);
                }
            }

            if (!pWH.IsNull)
            {
                if (pWH.Ref.Base.ID == "FreezingLaserWH")
                {
                    type = 1;
                }
                else
                {
                    type = 2;
                }
            }

            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
        }


        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            return;
        }

        public override void OnAttachEffectRemove()
        {
            base.OnAttachEffectRemove();
        }
    }
}
