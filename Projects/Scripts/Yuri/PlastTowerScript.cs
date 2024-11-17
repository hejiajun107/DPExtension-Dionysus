using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Yuri
{
    [ScriptAlias(nameof(PlastTowerScript))]
    [Serializable]
    public class PlastTowerScript : TechnoScriptable
    {
        public PlastTowerScript(TechnoExt owner) : base(owner)
        {
        }

        private int level = 0;

        public int clearDelay = 100;

        private static Pointer<WarheadTypeClass> pWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("PlastWaveWh");

        public override void OnUpdate()
        {
            if(clearDelay--<=0)
            {
                level = 0;
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(pTarget.CastToTechno(out var ptechno))
            {
                if(ptechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                {
                    var mission = Owner.OwnerObject.Convert<MissionClass>();
                    mission.Ref.ForceMission(Mission.Stop);
                    return;
                }
            }
            else
            {
                var mission = Owner.OwnerObject.Convert<MissionClass>();
                mission.Ref.ForceMission(Mission.Stop);
                return;
            }
         
            clearDelay = 100;
            var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
            var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 5 + level, pWh, 100, false);
            pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            level++;
            if (level > 30)
                level = 30;
        }
    }

    [ScriptAlias(nameof(PlastTowerAttachEffect))]
    [Serializable]
    public class PlastTowerAttachEffect : AttachEffectScriptable
    {
        public PlastTowerAttachEffect(TechnoExt owner) : base(owner)
        {
        }

        TechnoExt attacker;

        private static Pointer<AnimTypeClass> pTowerAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("EnergyPlst");

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (attacker.IsNullOrExpired())
                return;


            if (attacker.OwnerObject.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                return;

            if (Owner.OwnerObject.Ref.Base.InLimbo)
                return;

            YRMemory.Create<AnimClass>(pTowerAnim, attacker.OwnerRef.Base.Base.GetCoords() + new CoordStruct(0, 0, 888));

            var damage = (int)(Owner.OwnerObject.Ref.Type.Ref.Base.Strength / 10);

            if (Owner.OwnerObject.Ref.Type.Ref.ImmuneToPsionics)
                damage = (int)(damage * 0.3);

             var inviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
            var pBullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), attacker.OwnerObject, damage, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("PlastFBWh"), 100, true);
            pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
        }

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if (pAttacker.IsNotNull)
            {
                if (pAttacker.CastToTechno(out var ptechno))
                {
                    attacker = TechnoExt.ExtMap.Find(ptechno);
                }
            }
        }

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            Duration = duration;

            if(pAttacker.IsNotNull)
            {
                if(pAttacker.CastToTechno(out var ptechno))
                {
                    attacker = TechnoExt.ExtMap.Find(ptechno);
                }
            }
        }
    }
}
