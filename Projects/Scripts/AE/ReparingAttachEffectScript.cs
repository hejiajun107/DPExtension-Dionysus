using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.AE
{
    [ScriptAlias(nameof(ReparingAttachEffectScript))]
    [Serializable]
    public class ReparingAttachEffectScript : AttachEffectScriptable
    {
        public ReparingAttachEffectScript(TechnoExt owner) : base(owner)
        {
            pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }

        private SwizzleablePointer<AnimClass> pAnim;

        static Pointer<AnimTypeClass> repairAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("RepairTank");


        public override void OnUpdate()
        {
            base.OnUpdate();
   
            if (pAnim.IsNull)
            {
                CreateAnim();
            }

            pAnim.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords());

            var visible = false;

            if (Owner.OwnerObject.Ref.Base.Health < Owner.OwnerRef.Type.Ref.Base.Strength)
                visible = true;

            if (Owner.OwnerObject.Ref.Owner != HouseClass.Player)
                visible = false;

            pAnim.Ref.Invisible = !visible;
        
        }

        public override void OnAttachEffectRemove()
        {
            KillAnim();
        }

        public override void OnRemove()
        {
            KillAnim();
        }

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            base.OnAttachEffectRecieveNew(duration, pDamage, pWH, pAttacker, pAttackingHouse);
        }

        private void CreateAnim()
        {
            if (!pAnim.IsNull)
            {
                KillAnim();
            }

            var anim = YRMemory.Create<AnimClass>(repairAnim, Owner.OwnerObject.Ref.Base.Base.GetCoords());
            pAnim.Pointer = anim;
        }

        private void KillAnim()
        {
            if (!pAnim.IsNull)
            {
                pAnim.Ref.TimeToDie = true;
                pAnim.Ref.Base.UnInit();
                pAnim.Pointer = IntPtr.Zero;
            }
        }
    }
}
