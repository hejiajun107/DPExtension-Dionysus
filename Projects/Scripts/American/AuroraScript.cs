using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.American
{
    [Serializable]
    public class AuroraScript : TechnoScriptable
    {
        public AuroraScript(TechnoExt owner) : base(owner) { }

        private int checkRof = 0;

        TechnoExt pTargetRef;

        private int duration = 0;

        private int coolDown = 500;

        static Pointer<WarheadTypeClass> immnueWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("AuroraImmnueWh");

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (checkRof-- <= 0)
            {
                checkRof = 20;

                if (Owner.OwnerObject.Ref.Ammo > 0 && duration <= 1000)
                {
                    pTargetRef=(TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Target.Convert<TechnoClass>()));
                    if (!pTargetRef.IsNullOrExpired())
                    {
                        var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                        var targetLocation = pTargetRef.OwnerObject.Ref.Base.Base.GetCoords();
                        var distance = currentLocation.DistanceFrom(targetLocation);
                        if (distance <= 30 * 256)
                        {
                            //施加免疫效果
                            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, immnueWarhead, 100, false);
                            pBullet.Ref.DetonateAndUnInit(currentLocation);
                            duration += 20;
                        }
                    }
                }
                else
                {
                    coolDown -= 20;
                    if (coolDown <= 0)
                    {
                        coolDown = 500;
                        duration = 0;
                    }

                }
            }
        }



    }
}
