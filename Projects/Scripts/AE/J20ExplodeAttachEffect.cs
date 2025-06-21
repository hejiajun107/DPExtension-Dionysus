using DynamicPatcher;
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
    [Serializable]
    [ScriptAlias(nameof(J20ExplodeAttachEffect))]
    public class J20ExplodeAttachEffect : AttachEffectScriptable
    {
        public J20ExplodeAttachEffect(TechnoExt owner) : base(owner)
        {
            pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }

        private int Count = 1;

        private Pointer<AnimTypeClass> counterAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("J10Counter");

        private SwizzleablePointer<AnimClass> pAnim;

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (pAnim.IsNull)
            {
                CreateAnim();
            }

            pAnim.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            if (Count <= 0)
            {
                pAnim.Ref.Invisible = true;
            }
            else
            {

                pAnim.Ref.Invisible = false;
            }

            var frame = Count == 0 ? 1 : Count;
            pAnim.Ref.Animation.Value = frame > 3 ? 3 : frame;
            pAnim.Ref.Pause();
        }

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
                        var bullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisble").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), attacker, (int)(50 * attacker.Ref.FirepowerMultiplier), wh, 100, false);
                        bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    }
                }
            }
        }

        public override void OnRemove()
        {
            Duration = 0;
            KillAnim();
            base.OnRemove();
        }

        public override void OnAttachEffectRemove()
        {
            KillAnim();
            base.OnAttachEffectRemove();
        }

        private void CreateAnim()
        {
            if (!pAnim.IsNull)
            {
                KillAnim();
            }

            var anim = YRMemory.Create<AnimClass>(counterAnim, Owner.OwnerObject.Ref.Base.Base.GetCoords());
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
