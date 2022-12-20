﻿using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.AE
{
    [ScriptAlias(nameof(JWindTowerAttachEffect))]
    [Serializable]
    public class JWindTowerAttachEffect : AttachEffectScriptable
    {
        public JWindTowerAttachEffect(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<WeaponTypeClass> oWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("WINDERBOLT");
        private static Pointer<WeaponTypeClass> lWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("WaveLargeShot");
        private static Pointer<WeaponTypeClass> sWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("WaveSmallShot");
        private static Pointer<WeaponTypeClass> aWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("WaveAirShot");




        TechnoExt attacker;
        bool large = false;
        bool isAir = false;

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if (pAttacker.IsNull)
            {
                return;
            }

            if (pAttacker.CastToTechno(out var ptechno))
            {
                attacker = TechnoExt.ExtMap.Find(ptechno);
                large = pWH.Ref.Base.ID == "WINDERAGWHE";
                isAir = pWH.Ref.Base.ID == "WindEyeWH";
            }
        }

        public override void OnAttachEffectRemove()
        {
            if (!attacker.IsNullOrExpired())
            {

                attacker.OwnerObject.Ref.GetWeapon(0).Ref.WeaponType = isAir ? aWeapon : (large ? lWeapon : sWeapon);
                attacker.OwnerRef.Fire_NotVirtual(Owner.OwnerObject.Convert<AbstractClass>(), 0);
                attacker.OwnerObject.Ref.GetWeapon(0).Ref.WeaponType = oWeapon;
            }
        }
    }
}
