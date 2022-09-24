
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using System.Threading.Tasks;
using System.Linq;
using Extension.Shared;
using DpLib.Scripts.China;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(BadunScript))]
    public class BadunScript : TechnoScriptable
    {
        public BadunScript(TechnoExt owner) : base(owner) {
            _manaCounter = new ManaCounter();
        }

        private ManaCounter _manaCounter;

        static BadunScript()
        {
            // Task.Run(() =>
            // {
            //     while (true)
            //     {
            //         Logger.Log("Ticked.");
            //         Thread.Sleep(1000);
            //     }
            // });
        }

        Random random = new Random(110512);

        static ColorStruct innerColor = new ColorStruct(255, 255, 255);
        static ColorStruct outerColor = new ColorStruct(255, 0, 0);
        static ColorStruct outerSpread = new ColorStruct(255, 0, 0);


        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> pWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BaidunAOEffect");
        static Pointer<WarheadTypeClass> sWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BaidunLaserReportWh");

        static Pointer<WarheadTypeClass> showWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BaidunAnimEffectWh");

        

        private int burstCount = 12;

        private bool isBurst = false;

        private int currentBurst = 0;

        private CoordStruct target;

        private int delay = 2;

        private int currentInterval = 0;

        private int burstRate = 10;

        //private int needBurstCount = 10;

        private bool reloaded = false;

        private int currentDuration = 0;


        public override void OnUpdate()
        {
            _manaCounter.OnUpdate(Owner);

            if (reloaded)
            {
                if (currentDuration >= 300)
                {
                    reloaded = false;
                    burstRate = 10;
                    currentDuration = 0;
                }
                else
                {
                    currentDuration++;
                }
            }

            if (isBurst)
            {
                if (currentInterval >= delay)
                {
                    if (currentBurst < burstCount)
                    {
                        currentBurst++;

                        var pTechno = Owner.OwnerObject;
                        var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                        var launchLocation = new CoordStruct(currentLocation.X, currentLocation.Y, currentLocation.Z + 100);

                        var ntarget = new CoordStruct(target.X + random.Next(-360, 360), target.Y + random.Next(-360, 360), target.Z);

                        Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(launchLocation, ntarget, innerColor, outerColor, outerSpread, 10);
                        pLaser.Ref.Thickness = 10;
                        pLaser.Ref.IsHouseColor = false;

                        Pointer<BulletClass> pBullets = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, sWH, 100, true);
                        pBullets.Ref.DetonateAndUnInit(ntarget);

                        Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 16, pWH, 100, true);
                        pBullet.Ref.DetonateAndUnInit(ntarget);
                    }
                    else
                    {
                        isBurst = false;
                        currentBurst = 0;
                    }
                    currentInterval = 0;
                }
                currentInterval++;
            }
        }

        //public override void OnRender()
        //{
        //    _manaCounter.OnRender(Owner);
        //}
        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            bool controlledByAi = false;

            if (!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                    controlledByAi = true;
            }

            if (weaponIndex==0 && !controlledByAi)
            {
                if (isBurst == false)
                {
                    if (random.Next(100) <= burstRate)
                    {
                        isBurst = true;
                        currentBurst = 0;
                        target = pTarget.Ref.GetCoords();
                    }
                }
            }
            else
            {
                if (_manaCounter.Cost(100))
                {
                    var pTechno = Owner.OwnerObject;

                    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 16, showWH, 100, true);
                    pBullet.Ref.DetonateAndUnInit(pTechno.Ref.Base.Base.GetCoords());
                    reloaded = true;
                    currentDuration = 0;
                    burstRate = 50;
                }
            }
        }
    }
}