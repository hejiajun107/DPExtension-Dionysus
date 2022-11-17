using Extension.Ext;
using Extension.Script;
using PatcherYRpp;

namespace DpLib.Scripts.AE
{
    [ScriptAlias(nameof(InsuranceAttachEffectScript))]
    public class InsuranceAttachEffectScript : AttachEffectScriptable
    {
        public InsuranceAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private Pointer<AnimTypeClass> anim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("MONEY2");

        private int lastAttackIndex = -1;

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (!pAttackingHouse.IsNull)
            {
                lastAttackIndex = pAttackingHouse.Ref.ArrayIndex;
            }
        }

        public override void OnRemove()
        {
            if (Owner.OwnerObject.Ref.Base.Health <= 0)
            {
                if (lastAttackIndex == -1 || lastAttackIndex != Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                {
                    int money = Owner.OwnerObject.Ref.Type.Ref.Cost;

                    if (!Owner.OwnerObject.Ref.Owner.IsNull)
                    {
                        Owner.OwnerObject.Ref.Owner.Ref.TransactMoney(money);
                        YRMemory.Create<AnimClass>(anim, Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 200));
                    }
                }
            }
        }

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            return;
        }
    }
}
