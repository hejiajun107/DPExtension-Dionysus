﻿using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

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

        HouseExt houseExt;

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if(!pAttackingHouse.IsNull)
            {
                houseExt = HouseExt.ExtMap.Find(pAttackingHouse);
            }
        }

        public override void OnUpdate()
        {
            if (Duration % 50 == 0)
            {
                RevealAt(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }
            base.OnUpdate();
        }



        private void RevealAt(CoordStruct location)
        {
            if (houseExt.IsNullOrExpired())
            {
                Duration = 0;
                return;
            }

            Pointer<SuperClass> pSuper = houseExt.OwnerObject.Ref.FindSuperWeapon(revealSW);
            CellStruct targetCell = CellClass.Coord2Cell(location);
            pSuper.Ref.IsCharged = true;
            pSuper.Ref.Launch(targetCell, true);
            pSuper.Ref.IsCharged = false;
        }

    }

}
