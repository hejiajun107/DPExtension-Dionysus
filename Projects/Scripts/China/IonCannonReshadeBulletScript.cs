using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(IonCannonReshadeBulletScript))]
    public class IonCannonReshadeBulletScript : BulletScriptable
    {
        public IonCannonReshadeBulletScript(BulletExt owner) : base(owner) { }

        public bool isActived = false;

        TechnoExt pTargetRef;



        public override void OnUpdate()
        {
            if (isActived == false)
            {
                pTargetRef = TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
                int height = Owner.OwnerObject.Ref.Base.GetHeight();
                if (!pTargetRef.IsNullOrExpired())
                {
                    if (pTargetRef.GameObject.GetComponent(IonCannonReshadeLauncherDecorator.ID) == null)
                    {
                        var pos = Owner.OwnerObject.Ref.Target.Ref.GetCoords();
                        pTargetRef.GameObject.CreateScriptComponent(nameof(IonCannonReshadeLauncherDecorator), IonCannonReshadeLauncherDecorator.ID, "IonCannonReshadeLauncherDecorator Decorator", pTargetRef, pos, height);
                    }
                }
            }
        }



        [Serializable]
        [ScriptAlias(nameof(IonCannonReshadeLauncherDecorator))]
        public class IonCannonReshadeLauncherDecorator : TechnoScriptable
        {
            public static int ID = 414001;
            public IonCannonReshadeLauncherDecorator(TechnoExt self, CoordStruct center, int height) : base(self)
            {
                Self = (self);
                this.center = center;
                this.height = height;
                //sound = VocClass.VoicesEnabled;
            }

            TechnoExt Self;

            //static ColorStruct innerColor = new ColorStruct(255, 0, 0);
            //static ColorStruct outerColor = new ColorStruct(255, 0, 0);
            //static ColorStruct outerSpread = new ColorStruct(255, 0, 0);

            static ColorStruct innerColor = new ColorStruct(0, 0, 160);
            static ColorStruct outerColor = new ColorStruct(0, 0, 160);
            static ColorStruct outerSpread = new ColorStruct(0, 0, 160);

            private bool isActived = false;
            private int height = 0;

            //光束聚集时使用的弹头
            static Pointer<WarheadTypeClass> beanWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DPIonBeanRED");
            //冲击波爆炸使用的弹头
            static Pointer<WarheadTypeClass> blastWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DPIonBlastRED");

            static Pointer<WarheadTypeClass> waveWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DPIonWave");


            static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

            static Pointer<SuperWeaponTypeClass> swLight => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("MockLightBlueSpecial");

            static Pointer<SuperWeaponTypeClass> envLight => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("IonEnvSpecial");

            static Pointer<AnimTypeClass> pRain => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("IonRain");

            static Pointer<WeaponTypeClass> laserWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("IonLaser");

            static Pointer<BulletTypeClass> pBulletExplode => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("IonToGBullet");

            static Pointer<AnimTypeClass> pPreAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("PreIon");

            static Pointer<AnimTypeClass> ionActiveSound=> AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("IonActiveImpact");

            static Pointer<AnimTypeClass> ionScanSound => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("IonScanImpact");

            private Random random = new Random(114514);


            //光束初始角度
            private int startAngle = -180;
            //光束半径
            private int radius { get; set; } = 1000;

            //冲击波当前半径
            private int blastRadius = 0;

            private bool isWaveRelased = false;

            //中心点位置
            private CoordStruct center;

            //冲击波生成间隔
            private int blastDamageRof = 1;
            //当前冲击波的帧数
            private int currentBlastFrame = 0;

            private int delay = 15;

            private int readyState = 0;

            private List<bool> beamDisplay = new List<bool>() { false, false, false, false, false, false, false, false };

            private int lineSpeed = 4;
            private int rotSpeed = 1;
            private int speedUpDelay = 0;

            private int pausedTime = 0;

            //private bool sound = false;

            private bool preAnimReleased = false;

            private SwizzleablePointer<AnimClass> pScan = new SwizzleablePointer<AnimClass>(IntPtr.Zero);

            private bool pScanKilled = false;

            public override void OnUpdate()
            {
                if (Self.IsNullOrExpired())
                {
                    DetachFromParent();
                    return;
                }

                var Owner = Self;

                if (isActived == false)
                {
                    isActived = true;

                    Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                    Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
                    Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(envLight);
                    CellStruct targetCell = CellClass.Coord2Cell(center);
                    pSuper.Ref.IsCharged = true;
                    pSuper.Ref.Launch(targetCell, true);
                    pSuper.Ref.IsCharged = false;

                    return;
                }

                if (delay-- >= 0)
                {
                    return;
                }

                if (isActived)
                {
                    //光束聚集
                    if (radius >= 0)
                    {
                        if (readyState <= 80)
                        {
                            readyState++;
                            beamDisplay[0] = true;
                            if (readyState >= 15)
                            {
                                beamDisplay[1] = true;
                                if (readyState == 15)
                                {
                                    YRMemory.Create<AnimClass>(ionActiveSound, center + new CoordStruct(0, 0, -height));
                                }
                            }

                            if (readyState >= 30)
                            {
                                beamDisplay[3] = true;
                                if (readyState == 30)
                                {
                                    YRMemory.Create<AnimClass>(ionActiveSound, center + new CoordStruct(0, 0, -height));
                                }
                            }

                            if (readyState >= 40)
                            {
                                beamDisplay[2] = true;
                                if (readyState == 40)
                                {
                                    YRMemory.Create<AnimClass>(ionActiveSound, center + new CoordStruct(0, 0, -height));
                                }
                            }

                            if (readyState >= 45)
                            {
                                beamDisplay[5] = true;
                                if (readyState == 45)
                                {
                                    YRMemory.Create<AnimClass>(ionActiveSound, center + new CoordStruct(0, 0, -height));
                                }
                            }

                            if (readyState >= 55)
                            {
                                beamDisplay[7] = true;
                            }

                            if (readyState >= 60)
                            {
                                beamDisplay[6] = true;
                                beamDisplay[4] = true;
                                if (readyState == 60)
                                {
                                    YRMemory.Create<AnimClass>(ionActiveSound, center + new CoordStruct(0, 0, -height));
                                }
                                if (readyState == 61)
                                {
                                    YRMemory.Create<AnimClass>(ionActiveSound, center + new CoordStruct(0, 0, -height));
                                }
                            }

                            if (readyState == 80)
                            {
                                var anim = YRMemory.Create<AnimClass>(ionScanSound, center + new CoordStruct(0, 0, -height));
                                pScan.Pointer = anim;
                            }
                        }

                        int i = 0;


                        //每xx角度生成一条光束，越小越密集
                        for (var angle = startAngle; angle < startAngle + 360; angle += 45)
                        {
                            if (beamDisplay[i])
                            {
                                var pos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z);
                                //Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(pos + new CoordStruct(0, 0, 9000), pos + new CoordStruct(0, 0, -center.Z), innerColor, outerColor, outerSpread, readyState >= 80 ? 5 : 1);
                                //pLaser.Ref.Thickness = 10;
                                //pLaser.Ref.IsHouseColor = false;

                                //每条光束/帧的伤害
                                int damage = readyState > 80 ? 8 : 5; //was50
                                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, beanWarhead, 100, true);
                                pBullet.Ref.Base.SetLocation(pos + new CoordStruct(0, 0, -height));
                                Self.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, laserWeapon, pos + new CoordStruct(0, 0, 9000));
                                pBullet.Ref.DetonateAndUnInit(pos + new CoordStruct(0, 0, -height));
                            }

                            i++;
                            //if (i > (readyState / 10))
                            //{
                            //    return;
                            //}
                        }

                        if (readyState >= 80)
                        {
                            if (speedUpDelay++ >= 15)
                            {
                                speedUpDelay = 0;
                                lineSpeed++;
                                rotSpeed++;
                            }


                            //每次半径缩小越大，光束聚集越快
                            radius -= lineSpeed;
                            //每次旋转的角度，角度越大旋转越快
                            startAngle -= rotSpeed;
                        }
                    }
                    else
                    {
                        if (!isWaveRelased)
                        {
                            if (pScanKilled == false)
                            {
                                pScanKilled = true;
                                if (!pScan.IsNull)
                                {
                                    pScan.Ref.Base.UnInit();
                                }
                            }

                            if (preAnimReleased == false && pausedTime>=10)
                            {
                                preAnimReleased = true;
                                YRMemory.Create<AnimClass>(pPreAnim, center + new CoordStruct(0, 0, -height));
                            }

                            if (pausedTime++ >= 80)
                            {
                                //VocClass.VoicesEnabled = sound;
                                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                                Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
                                Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(swLight);
                                CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                                pSuper.Ref.IsCharged = true;
                                pSuper.Ref.Launch(targetCell, true);
                                pSuper.Ref.IsCharged = false;

                                //if(MapClass.Instance.TryGetCellAt(center + new CoordStruct(0, 0, -height),out var pCell))
                                //{

                                var blue = new ColorStruct(0, 255, 255);

                                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(center + new CoordStruct(0, 0, 9000), center + new CoordStruct(0, 0, -center.Z), blue, blue, blue, 10);
                                pLaser.Ref.IsHouseColor = true;
                                pLaser.Ref.Thickness = 80;

                                int damage = 700;
                                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, waveWarhead, 100, false);
                                //pBullet.Ref.MoveTo(center + new CoordStruct(0, 0, 2000), new BulletVelocity(0, 0, 0));
                                //pBullet.Ref.SetTarget(pCell.Convert<AbstractClass>());
                                //}

                                pBullet.Ref.DetonateAndUnInit(center + new CoordStruct(0, 0, -height));
                                isWaveRelased = true;
                            }
                            else
                            {
                                //VocClass.VoicesEnabled = false;
                                return;
                            }
                        }

                        //if (currentBlastFrame <= blastDamageRof)
                        //{
                        //    currentBlastFrame++;
                        //}
                        //else
                        //{
                        //冲击波的扩散半径
                        if (blastRadius <= 2400)
                        {
                            //每xx角度生成一个动画，越小越密集
                            for (var angle = -180; angle < 180; angle += 20)
                            {
                                var pos = new CoordStruct(center.X + (int)(blastRadius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(blastRadius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z);
                                //每个冲击波/帧的伤害
                                int damage = 3;
                                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, blastWarhead, 100, false);
                                pBullet.Ref.DetonateAndUnInit(pos + new CoordStruct(0, 0, -height));
                            }
                            //每次冲击波扩展的距离，距离越大扩散越快
                            blastRadius += 25;//25;
                        }
                        else
                        {
                            isActived = false;
                            DetachFromParent();
                            return;
                        }

                        currentBlastFrame = 0;
                        //}
                    }
                }
            }

            public override void OnDestroy()
            {
                if (pScanKilled == false)
                {
                    pScanKilled = true;
                    if (!pScan.IsNull)
                    {
                        pScan.Ref.Base.UnInit();
                    }
                }
            }
        }

    }

}
