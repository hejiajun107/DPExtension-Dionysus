﻿using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(DTowerLaserBullet))]
    public class DTowerLaserBullet : BulletScriptable
    {
        public DTowerLaserBullet(BulletExt owner) : base(owner) { }

        private bool IsActive = false;

        TechnoExt pTargetRef;

        static ColorStruct color = new ColorStruct(255, 0, 255);



        static ColorStruct exColor1 = new ColorStruct(255, 0, 0);

        static ColorStruct exColor2 = new ColorStruct(255, 128, 0);

        static ColorStruct exColor3 = new ColorStruct(255, 255, 0);

        static ColorStruct exColor4 = new ColorStruct(0, 255, 0);

        static ColorStruct exColor5 = new ColorStruct(0, 0, 255);


        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DeathRayTowWH");

        static Pointer<WeaponTypeClass> rayWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("DrRayFakeWeapon");


        public override void OnUpdate()
        {
            if (IsActive == false)
            {
                IsActive = true;
                pTargetRef = TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
                return;
            }

            var height = Owner.OwnerObject.Ref.Base.GetHeight();
            var target = Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, -height);

            if (!pTargetRef.IsNullOrExpired())
            {
                var start = Owner.OwnerObject.Ref.Owner.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 710);

                var pTechno = pTargetRef.OwnerObject;

                var usedColor = GetColor(Owner.OwnerObject.Ref.Speed);

                //Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(start, target, usedColor, usedColor, usedColor, 10);
                //pLaser.Ref.IsHouseColor = false;
                var damage = 15;
                if (Owner.OwnerObject.Ref.Owner.IsNotNull)
                {
                    if (Owner.OwnerObject.Ref.Owner.Ref.Veterancy.IsElite())
                    {
                        damage = 23;
                    }
                }

                damage = (int)Math.Floor(damage * Owner.OwnerObject.Ref.Owner.Ref.FirepowerMultiplier);

                Pointer <BulletClass> pBullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), pTechno, damage, warhead, 100, true);
                pBullet.Ref.Base.SetLocation(target);
                pTargetRef.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, rayWeapon, start);
                pBullet.Ref.DetonateAndUnInit(target);
            }




        }

        private ColorStruct GetColor(int speed)
        {
            switch (speed)
            {
                case 90:
                    return color;
                case 95:
                    return exColor1;
                case 96:
                    return exColor2;
                case 97:
                    return exColor3;
                case 98:
                    return exColor4;
                case 99:
                    return exColor5;
                default:
                    return color;
            }
        }
    }
}
