using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.AE
{
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
            if(burst>0)
            {
                if (count++ >= 30)
                {
                    count = 0;
                    burst--;

                    if (attacker != null && !attacker.Expired)
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
                attacker.Set(pAttacker);
            }
            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
        }

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            return;
        }


    }
}
