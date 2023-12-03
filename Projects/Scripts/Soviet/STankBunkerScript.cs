using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Soviet
{
    [ScriptAlias(nameof(STankBunkerScript))]
    [Serializable]
    public class STankBunkerScript : TechnoScriptable
    {
        public STankBunkerScript(TechnoExt owner) : base(owner)
        {
        }

        private int rof = 100;

        private static List<CoordStruct> coordOffets = new List<CoordStruct>()
        {
            new CoordStruct(256,256,30),
            new CoordStruct(256,-256,30),
            new CoordStruct(-256,256,30),
            new CoordStruct(-256,-256,30),
        };

        private int currentChargeIndex = 0;

        private static Pointer<WeaponTypeClass> chargeWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("AssaultBolt");

        public override void OnUpdate()
        {
            if (rof-- > 0)
                return;

            rof = 100;

            if (Owner.OwnerObject.Ref.BunkerLinkedItem.IsNull)
                return;

            var strength = Owner.OwnerObject.Ref.BunkerLinkedItem.Ref.Type.Ref.Base.Strength;
            if(Owner.OwnerObject.Ref.BunkerLinkedItem.Ref.Base.Health < strength)
            {
                Owner.OwnerObject.Ref.BunkerLinkedItem.Ref.Base.Health = Owner.OwnerObject.Ref.BunkerLinkedItem.Ref.Base.Health + 5 > strength ? strength : Owner.OwnerObject.Ref.BunkerLinkedItem.Ref.Base.Health + 5;
            }

            Owner.OwnerObject.Ref.Electric_Zap(Owner.OwnerObject.Ref.BunkerLinkedItem.Convert<AbstractClass>(), chargeWeapon, Owner.OwnerObject.Ref.Base.Base.GetCoords() + coordOffets[currentChargeIndex]);

            currentChargeIndex++;
            if (currentChargeIndex > 3)
                currentChargeIndex = 0;

            var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
            var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Ref.BunkerLinkedItem.Convert<AbstractClass>(), Owner.OwnerObject, 0, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("TKBEffectWh"), 100, true);
            pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            int trueDamage = MapClass.GetTotalDamage(pDamage.Ref, pWH, Owner.OwnerObject.Ref.Type.Ref.Base.Armor, DistanceFromEpicenter);

            if (trueDamage > 1)
            {
                if (DistanceFromEpicenter > Game.CellSize)
                {
                    pDamage.Ref =(int)(pDamage.Ref * 0.2);
                }
            }
        }
    }
}
