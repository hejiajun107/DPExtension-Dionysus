using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.AE
{
    [Serializable]
    public class DamageDeepenAttachEffectScript : AttachEffectScriptable
    {
        public DamageDeepenAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private int damage = 20;

        private int delay = 20;

        private Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MirageEXWH");

        public override void OnUpdate()
        {
            if (delay > 0) {
                delay--;
            }
            base.OnUpdate();
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (pAttacker.IsNull)
                return;

            if (pAttackingHouse.IsNull)
                return;

            if (pAttackingHouse.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                return;

            if (!pWH.IsNull)
            {
                if(pAttacker.CastToTechno(out var pAttackTechno))
                {
                    if (pWH.Ref.Base.ID == "MirageWH")
                    {
                        var bullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pAttackTechno, damage, warhead, 100, true);
                        bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    }
                    else
                    {
                        if (delay <= 0)
                        {
                            delay = 20;
                            //附加额外伤害
                            var bullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pAttackTechno, damage / 2, warhead, 100, true);
                            bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                        }
                    }
                }
            }
            
            base.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
        }

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
        }

        //重置duration
        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if (damage <= 100)
            {
                damage += 20;
            }
            Duration = duration;
        }

        public override void OnRemove()
        {
            Duration = 0;
            base.OnRemove();
        }

    }
}
