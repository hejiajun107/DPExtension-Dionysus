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
    [Serializable]
    [ScriptAlias(nameof(NanoSightAttachEffectScript))]
    public class NanoSightAttachEffectScript : AttachEffectScriptable
    {
        public NanoSightAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<SuperWeaponTypeClass> revealSW => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("PsychicReveal4Jp");

        Pointer<HouseClass> house;

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            house = pAttackingHouse;
        }

        public override void OnUpdate()
        {
            if(Duration % 50 == 0)
            {
                RevealAt(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }
            base.OnUpdate();
        }



        private void RevealAt(CoordStruct location)
        {
            if(house.IsNull)
            {
                Duration = 0;
                return;
            }
         
            Pointer<SuperClass> pSuper = house.Ref.FindSuperWeapon(revealSW);
            CellStruct targetCell = CellClass.Coord2Cell(location);
            pSuper.Ref.IsCharged = true;
            pSuper.Ref.Launch(targetCell, true);
            pSuper.Ref.IsCharged = false;
        }

    }

}
