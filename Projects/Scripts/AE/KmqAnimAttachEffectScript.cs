using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(KmqAnimAttachEffectScript))]
    public class KmqAnimAttachEffectScript : AttachEffectScriptable
    {
        public KmqAnimAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private int burst = 3;

        private int count = 0;

        private TechnoExt attacker;

        public override void OnUpdate()
        {
            if (burst > 0)
            {
                if (count++ >= 30)
                {
                    count = 0;
                    burst--;

                    if (attacker != null && !attacker.IsNullOrExpired())
                    {
                        attacker.OwnerObject.Ref.Fire_NotVirtual(Owner.OwnerObject.Convert<AbstractClass>(), 1);
                    }
                }
            }

            base.OnUpdate();
        }

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            //获取发射者
            if (!pAttacker.IsNull)
            {
                if (pAttacker.CastToTechno(out var ptAttacker))
                {
                    attacker = (TechnoExt.ExtMap.Find(ptAttacker));
                }
            }
            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
        }

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            return;
        }


    }
}
