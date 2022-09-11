using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{
    [Serializable]
    public class YuriHeadScript : TechnoScriptable
    {
        public YuriHeadScript(TechnoExt owner) : base(owner) { }
        static ColorStruct innerColor = new ColorStruct(255, 0, 0);
        static ColorStruct outerColor = new ColorStruct(255, 0, 0);
        static ColorStruct outerSpread = new ColorStruct(255, 0, 0);

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("HuimieInvisibleAll");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("HuimieWH");

        private bool IsBursting = false;

        private CoordStruct _target;

        private double k;
        private double b;
        private double distance;
        private double delataX;
        private double delataY;
        //private double delataZ;
        private int burstCount = 0;
        private int currentBurst = 0;

        private int burstDelay = 0;

        private int currentFrame = 0;

        public override void OnUpdate()
        {
            if(IsBursting && currentBurst <= burstCount)
            {
                if (currentFrame < burstDelay)
                {
                    currentFrame++;
                }
                else
                {
                    var pTechno = Owner.OwnerObject;
                    var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                    var target = new CoordStruct((int)(_target.X + delataX * currentBurst), (int)(_target.Y + delataY * currentBurst), (int)(_target.Z));
                    //var target = new CoordStruct((int)(_target.X + delataX * currentBurst), (int)((_target.X + delataX * currentBurst) * k  + b), (int)(_target.Z));

                    Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(currentLocation, target, innerColor, outerColor, outerSpread, 20);
                    pLaser.Ref.IsHouseColor = false;
                    pLaser.Ref.Thickness = 30;

                    Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 25, warhead, 100, true);
                    pBullet.Ref.DetonateAndUnInit(target);

                    currentBurst++;
                    currentFrame = 0;
                }
            }
            else
            {
                IsBursting = false;
                burstCount = 0;
                currentBurst = 0;
            }
           

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(IsBursting == false)
            {
                var target = pTarget.Ref.GetCoords();
                var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                k = (double)(currentLocation.Y - target.Y) / (double)(currentLocation.X - currentLocation.Y);
                b = currentLocation.Y - k * currentLocation.X;
                _target = new CoordStruct(currentLocation.X, currentLocation.Y, target.Z);
                distance = _target.DistanceFrom(target) + 900;
                burstCount = (int)Math.Round(distance) / 35;
                delataX = (target.X - currentLocation.X) / burstCount;
                delataY = (target.Y - currentLocation.Y) / burstCount;
                //delataZ = (target.Z - currentLocation.Z) / burstCount;


                //Logger.Log($"距离{distance}，次数{burstCount},每次x变化{delataX},起始点({_target.X},{_target.Y},{_target.Z})");
                //Logger.Log($"目标点({target.X},{target.Y},{target.Z})");


                IsBursting = true;
                currentFrame = 0;
                currentBurst = 0;
            }
        }

    }
}
