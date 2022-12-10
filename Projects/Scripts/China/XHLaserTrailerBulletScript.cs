using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts
{

    [Serializable]
    [ScriptAlias(nameof(XHLaserTrailerBulletScript))]

    public class XHLaserTrailerBulletScript : BulletScriptable
    {
        public XHLaserTrailerBulletScript(BulletExt owner) : base(owner)
        {
        }

        static ColorStruct innerColor = new ColorStruct(0, 223, 223);
        static ColorStruct outerColor = new ColorStruct(0, 255, 255);
        static ColorStruct outerSpread = new ColorStruct(10, 10, 10);

        CoordStruct lastLocation;

        bool started = false;

        public bool IsActive { get; private set; }

        private CoordStruct start;

        TechnoExt pTargetRef;

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHLaserExpWH");

        static Pointer<WarheadTypeClass> warheadA => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHLaserAllyExpWH");

        private int counter = 0;


        public override void OnUpdate()
        {

            if (IsActive == false)
            {
                IsActive = true;
                start = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                pTargetRef = (TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner));
                counter = 0;
                return;
            }

            var height = Owner.OwnerObject.Ref.Base.GetHeight();
            var target = Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, -height);

            if (!pTargetRef.IsNullOrExpired())
            {
                var pTechno = pTargetRef.OwnerObject;
                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(start, target, innerColor, outerColor, outerSpread, 20);
                pLaser.Ref.IsHouseColor = true;

                var thickness = counter / 5;

                if (thickness > 6)
                {
                    thickness = 6;
                }

                pLaser.Ref.Thickness = thickness;

                counter++;

                Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), pTechno, 38, warhead, 100, true);
                pBullet.Ref.DetonateAndUnInit(target);

                Pointer<BulletClass> pBulletA = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), pTechno, 19, warhead, 100, false);
                pBulletA.Ref.DetonateAndUnInit(target);
            }


        }

    }
}
