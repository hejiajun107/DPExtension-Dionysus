using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.Yuri
{
    [Serializable]
    public class FutureTankLaserBullet : BulletScriptable
    {
        public FutureTankLaserBullet(BulletExt owner) : base(owner) { }

        private bool IsActive = false;

        TechnoExt pTargetRef;

        static ColorStruct innerColor = new ColorStruct(0, 180, 255);
        static ColorStruct outerColor = new ColorStruct(0, 128, 255);
        static ColorStruct outerSpread = new ColorStruct(0, 180, 255);

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("FutureFreezingWH");

        private CoordStruct start;

        private CoordStruct end;

        public override void OnUpdate()
        {
            if (IsActive == false)
            {
                IsActive = true;
                start = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                pTargetRef=TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
                end = Owner.OwnerObject.Ref.TargetCoords;
                return;
            }

            var height = Owner.OwnerObject.Ref.Base.GetHeight();
            var target = Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, -height);

            if (target.DistanceFrom(end) > 256 * 2)
            {
                return;
            }


            if (!pTargetRef.Expired)
            {
                var pTechno = pTargetRef.OwnerObject;
                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(start, target, innerColor, outerColor, outerSpread, 20);
                pLaser.Ref.IsHouseColor = true;
                pLaser.Ref.Thickness = 1;

                Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), pTechno, 25, warhead, 100, true);
                pBullet.Ref.DetonateAndUnInit(target);
            }




        }
    }
}
