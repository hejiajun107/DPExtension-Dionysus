using DynamicPatcher;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(JALCScript))]
    [Serializable]
    public class JALCScript : TechnoScriptable
    {
        public JALCScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
            var pBullet = pInviso.Ref.CreateBullet(pTarget, Owner.OwnerObject,1,WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("JALCAttachWH"),100, true);
            pBullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());

            base.OnFire(pTarget, weaponIndex);
        }
    }

    [ScriptAlias(nameof(NanoDeconstructionAttachEffectScript))]
    [Serializable]
    public class NanoDeconstructionAttachEffectScript : AttachEffectScriptable
    {
        public NanoDeconstructionAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        public TechnoExt Attacker;

        private int delay = 20;

        public int level = 0;

        public bool removed = false;

        public override void OnUpdate()
        {
            if (delay-- <= 0)
            {
                delay = 20;

                var mult = 1.0 + (level * 0.01);
                var damage = (int)(15.0 * mult);

                var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Attacker.IsNullOrExpired() ? Pointer<TechnoClass>.Zero : Attacker.OwnerObject,
                    damage,
                    WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("JALCDestWH"),
                    100, true);

                var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                pBullet.Ref.DetonateAndUnInit(coord);
                for(var i = 0; i < 2; i++)
                {
                    YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("JALCDestExp"), coord + new CoordStruct(MathEx.Random.Next(-50, 50), MathEx.Random.Next(-50, 50), MathEx.Random.Next(0, 50)));
                }

            }
            base.OnUpdate();
        }

        public override void OnAttachEffectRemove()
        {
            if (!removed)
            {
                removed = true;
                if (Attacker.IsNullOrExpired())
                {
                    return;
                }

                var houseExt = Attacker.GetHouseGlobalExtension();
                var typeId = Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID.ToString();
                int levelUp = 3;
                int max = 300;
                if (!houseExt.DeconstructionLevels.ContainsKey(typeId))
                {
                    houseExt.DeconstructionLevels.Add(typeId, levelUp);
                }
                else
                {
                    var lastLevel = houseExt.DeconstructionLevels[typeId] + levelUp;
                    houseExt.DeconstructionLevels[typeId] = lastLevel < max ? lastLevel : max;
                }
            }
            base.OnAttachEffectRemove();
        }

        public override void OnRemove()
        {
            OnAttachEffectRemove();
            base.OnRemove();
        }

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            Duration = Duration;
            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
            if (pAttacker.IsNotNull)
            {
                Attacker = TechnoExt.ExtMap.Find(pAttacker.Convert<TechnoClass>());
                if (Attacker.IsNullOrExpired())
                    return;
                var houseExt = Attacker.GetHouseGlobalExtension();
                if (houseExt.DeconstructionLevels.ContainsKey(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID))
                {
                    level = houseExt.DeconstructionLevels[Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID];
                }
                else
                {
                    level = 0;
                }
            }
        }
    }

}
