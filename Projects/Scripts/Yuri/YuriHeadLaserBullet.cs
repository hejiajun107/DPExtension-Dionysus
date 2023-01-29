using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.Yuri
{
    [Serializable]
    [ScriptAlias(nameof(YuriHeadLaserBullet))]
    public class YuriHeadLaserBullet : BulletScriptable
    {
        public YuriHeadLaserBullet(BulletExt owner) : base(owner) { }

        private bool IsActive = false;

        TechnoExt pTargetRef;


        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("HuimieWH");

        static Pointer<WeaponTypeClass> wp => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("HuimieLaserRED");

        private CoordStruct start;

        public override void OnUpdate()
        {
            if (IsActive == false)
            {
                IsActive = true;
                start = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                pTargetRef = TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
                return;
            }

            var height = Owner.OwnerObject.Ref.Base.GetHeight();
            var target = Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, -height);

            if (!pTargetRef.IsNullOrExpired())
            {
                var pTechno = pTargetRef.OwnerObject;
                //Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(start, target, innerColor, outerColor, outerSpread, 20);
                //pLaser.Ref.IsHouseColor = true;
                //pLaser.Ref.Thickness = 3;


                Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), pTechno, 25, warhead, 100, true);
                pBullet.Ref.Base.SetLocation(target);

                pTargetRef.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, wp, start);

                pBullet.Ref.DetonateAndUnInit(target);
            }




        }
    }
}
