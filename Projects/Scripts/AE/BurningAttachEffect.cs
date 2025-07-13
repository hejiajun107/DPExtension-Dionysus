using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.AE
{
    [ScriptAlias(nameof(BurningAttachEffect))]
    [Serializable]
    public class BurningAttachEffect : AttachEffectScriptable
    {
        public BurningAttachEffect(TechnoExt owner) : base(owner)
        {
        }

        private int Count = 1;

        public TechnoExt Attacker;

        private int damageDelay = 20;

        private const int BurningNeedCount = 40;

        private static Dictionary<string, int> BurningLevel = new Dictionary<string, int>()
        {
            { "ZAFKTR4AI" ,2 },
            { "ZAFKTR" , 2 },
            { "HWDYHWH" , 5 },
            { "FireGunFlame" , 4 },
            { "J20FireExpWH", 41 }

        };

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (Count >= BurningNeedCount)
            {
                if (damageDelay-- <= 0)
                {
                    damageDelay = 20;
                    var burningMultipler = 0.02;
                    if (Count >= BurningNeedCount * 2)
                    {
                        burningMultipler = 0.03;
                    }
                    
                    var damage = (int)(Owner.OwnerObject.Ref.Type.Ref.Base.Strength * burningMultipler);
                    if (damage < 2 )
                    {
                        damage = 2;
                    }

                    var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                    var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), !Attacker.IsNullOrExpired() ? Attacker.OwnerObject : Pointer<TechnoClass>.Zero, damage,
                        WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DYHBurningWH"), 100, false);
                    pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }

        }

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if (Count < BurningNeedCount * 3)
            {
                Count += BurningLevel.ContainsKey(pWH.Ref.Base.ID) ? BurningLevel[pWH.Ref.Base.ID] : 1;
            }

            if (pAttacker.CastToTechno(out var pTechno))
            {
                Attacker = TechnoExt.ExtMap.Find(pTechno);
            }
        }

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            Duration = duration;
            if (Count < BurningNeedCount * 3)
            {
                Count += BurningLevel.ContainsKey(pWH.Ref.Base.ID) ? BurningLevel[pWH.Ref.Base.ID] : 1;
            }
            
            if(pAttacker.CastToTechno(out var pTechno))
            {
                Attacker = TechnoExt.ExtMap.Find(pTechno);
            }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (Count >= BurningNeedCount)
            {
                if (MapClass.GetTotalDamage(pDamage.Ref, pWH, Owner.OwnerObject.Ref.Type.Ref.Base.Armor, DistanceFromEpicenter) < 0)
                {
                    pDamage.Ref = (int)(pDamage.Ref * 0.2);
                }
            }
        }
    }
}
