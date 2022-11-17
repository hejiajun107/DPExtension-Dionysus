using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(FreezingSnapAttachEffectScript))]
    public class FreezingSnapAttachEffectScript : AttachEffectScriptable
    {
        public FreezingSnapAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private int delay = 40;

        private Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("FutureSnapWH");

        public override void OnUpdate()
        {
            if (delay > 0)
            {
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
                if (pAttacker.CastToTechno(out var pAttackTechno))
                {
                    if (delay <= 0)
                    {
                        delay = 40;
                        var bullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pAttackTechno, 1, warhead, 100, true);
                        bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
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
            Duration = duration;
        }

        public override void OnRemove()
        {
            Duration = 0;
            base.OnRemove();
        }

    }
}
