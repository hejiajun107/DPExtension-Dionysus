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

    [ScriptAlias(nameof(NanoReparingAttachEffectScript))]
    [Serializable]
    public class NanoReparingAttachEffectScript : AttachEffectScriptable
    {
        public NanoReparingAttachEffectScript(TechnoExt owner) : base(owner)
        {
            pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }

        private SwizzleablePointer<AnimClass> pAnim;

        static Pointer<AnimTypeClass> repairAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("NanoRepairE");

        static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<WarheadTypeClass> pRobRepair => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("NanoRepairWh");


        private int delay = 100;

        private static HashSet<string> whs = new HashSet<string>()
        {
            "ScudStrikeWH","SRBomb4WH","JALCbombBombWH","Minerdeath","ToxinShotSWH1","MiniSubExpWh"
        };

        public override void OnUpdate()
        {
            Duration = 2000;

            if (delay > 0)
            {
                delay--;
            }

            base.OnUpdate();

            if (pAnim.IsNull)
            {
                CreateAnim();
            }

            pAnim.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords());

            var visible = false;

            if (Owner.OwnerObject.Ref.Base.Health < Owner.OwnerRef.Type.Ref.Base.Strength)
            {
                visible = true;
                if (delay <= 0)
                {
                    delay = 100;
                    var pbullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Pointer<TechnoClass>.Zero, 60, pRobRepair, 100, false);
                    pbullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }

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

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (pAttackingHouse.IsNull)
                return;

            if (!pAttackingHouse.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                return;

            if(whs.Contains(pWH.Ref.Base.ID))
            {
                pDamage.Ref = (int)(pDamage.Ref * 0.4);
            }
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
