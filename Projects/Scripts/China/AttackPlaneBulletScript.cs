using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.China
{
    [Serializable]
    public class AttackPlaneBulletScript : BulletScriptable
    {
        public AttackPlaneBulletScript(BulletExt owner) : base(owner) { }

        public bool isActived = false;

        ExtensionReference<TechnoExt> pTargetRef;

        private static ColorStruct innerColor = new ColorStruct(200, 200, 160);
        private static ColorStruct outerColor = new ColorStruct(0, 0, 0);
        private static ColorStruct outerSpread = new ColorStruct(0, 0, 0);

        private int damage = 8;

        private CoordStruct StartOwner;

        private CoordStruct StartFire;

        static Pointer<BulletTypeClass> bullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> scanWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("A5ScanWh");
        static Pointer<WarheadTypeClass> animWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("A5SAnimWh");

        public override void OnUpdate()
        {
            if (isActived == false)
            {
                pTargetRef.Set(Owner.OwnerObject.Ref.Owner);

                if (pTargetRef.TryGet(out TechnoExt pTargetExt))
                {
                    //单位起始位置
                    StartOwner = pTargetExt.OwnerObject.Ref.Base.Base.GetCoords();
                    //起始开火点
                    StartFire = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                    isActived = true;
                }
            }

            if(isActived)
            {
                if (pTargetRef.TryGet(out TechnoExt pTargetExt))
                {
                    var moveLocation = pTargetExt.OwnerObject.Ref.Base.Base.GetCoords() - StartOwner;
                    var fireStart = StartFire + moveLocation;
                    var height = Owner.OwnerObject.Ref.Base.GetHeight();
                    var fireEnd = Owner.OwnerObject.Ref.Base.Base.GetCoords() - new CoordStruct(0, 0, height);
                    Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(fireStart, fireEnd, innerColor, outerColor, outerSpread, 2);

                    Pointer<BulletClass> pBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pTargetExt.OwnerObject, damage, scanWarhead, 100, true);
                    pBullet.Ref.DetonateAndUnInit(fireEnd);

                    Pointer<BulletClass> pBulletAnim = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pTargetExt.OwnerObject, 1, animWarhead, 100, false);
                    pBulletAnim.Ref.DetonateAndUnInit(fireStart);

                    if(damage<18)
                    {
                        damage++;
                    }

                }
            }

        }

    }
}
