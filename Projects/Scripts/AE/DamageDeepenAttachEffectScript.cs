﻿using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(DamageDeepenAttachEffectScript))]
    public class DamageDeepenAttachEffectScript : AttachEffectScriptable
    {
        public DamageDeepenAttachEffectScript(TechnoExt owner) : base(owner)
        {
            pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }

        private int damage => count <= 5 ? 16 * count : (count - 5) * 10 + 80;

        private int count = 1;

        private int delay = 20;

        private Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MirageEXWH");

        private Pointer<AnimTypeClass> counterAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("MgtkLock");

        private SwizzleablePointer<AnimClass> pAnim;


        public override void OnUpdate()
        {
            if (pAnim.IsNull)
            {
                CreateAnim();
            }

            pAnim.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            var scount = count <= 0 ? 0 : count - 1;
            pAnim.Ref.Animation.Value = scount > 9 ? 9 : scount;
            pAnim.Ref.Pause();

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
                if (pWH.Ref.Base.ID == "MirageEXWH")
                    return;

                if (pAttacker.CastToTechno(out var pAttackTechno))
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

        }

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
        }

        //重置duration
        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if (count < 10)
            {
                count += 1;
            }

            Duration = duration;
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
