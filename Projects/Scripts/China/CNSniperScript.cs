﻿using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using Scripts.Japan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.China
{
    [ScriptAlias(nameof(CNSniperScript))]
    [Serializable]
    public class CNSniperScript : TechnoScriptable
    {
        public CNSniperScript(TechnoExt owner) : base(owner)
        {
        }

        private string weaponName;
        private string eliteWeaponName;

        static Pointer<AnimTypeClass> pAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SniperReloadSound");

        public override void Awake()
        {
            weaponName = Owner.OwnerObject.Ref.Type.Ref.Weapon.Ref.WeaponType.Ref.Base.ID;
            eliteWeaponName = Owner.OwnerObject.Ref.Type.Ref.EliteWeapon.Ref.WeaponType.Ref.Base.ID;
            Owner.OwnerObject.Ref.Ammo = 0;
        }

        private int rof = 0;

        public override void OnUpdate()
        {
            if (Owner.OwnerObject.Ref.Ammo == 0 && weaponChanged)
            {
                weaponChanged = false;
                if (Owner.OwnerObject.Ref.Veterancy.IsElite())
                {
                    Owner.OwnerObject.Ref.GetWeapon(0).Ref.WeaponType = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(eliteWeaponName);
                }
                else
                {
                    Owner.OwnerObject.Ref.GetWeapon(0).Ref.WeaponType = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponName);
                }
                return;
            }

            //进入载具后无论如何换回主武器
            if (!Owner.OwnerObject.Ref.Base.IsOnMap && weaponChanged)
            {
                Owner.OwnerObject.Ref.Ammo = 0;
                weaponChanged = false;
                if (Owner.OwnerObject.Ref.Veterancy.IsElite())
                {
                    Owner.OwnerObject.Ref.GetWeapon(0).Ref.WeaponType = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(eliteWeaponName);
                }
                else
                {
                    Owner.OwnerObject.Ref.GetWeapon(0).Ref.WeaponType = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponName);
                }
            }

            if (rof > 0)
            {
                rof--;
            }
            
        }

        private bool weaponChanged = false;
        private bool toRecover = false;

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 0)
            {
                if (Owner.OwnerObject.Ref.Ammo == 1)
                {
                    if (Owner.OwnerObject.Ref.Veterancy.IsElite())
                    {
                        Owner.OwnerObject.Ref.GetWeapon(0).Ref.WeaponType = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ExplosiveAwpE");
                    }
                    else
                    {
                        Owner.OwnerObject.Ref.GetWeapon(0).Ref.WeaponType = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ExplosiveAwp");
                    }
                }
                //if (weaponChanged)
                //{
                //    weaponChanged = false;
                //    toRecover = true;
                //}
            }
            else
            {
                if (rof <= 0)
                {
                    rof = 500;
                    Owner.OwnerObject.Ref.Ammo = 1;
                    weaponChanged = true;
                    toRecover = false;
                    YRMemory.Create<AnimClass>(pAnim,Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    if (Owner.OwnerObject.Ref.Veterancy.IsElite())
                    {
                        Owner.OwnerObject.Ref.GetWeapon(0).Ref.WeaponType = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ExplosiveAwpE");
                    }
                    else
                    {
                        Owner.OwnerObject.Ref.GetWeapon(0).Ref.WeaponType = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ExplosiveAwp");
                    }
                }
            }
        }
    }
}