using Extension.Decorators;
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
    public class IonCannonBulletScript : BulletScriptable
    {
        public IonCannonBulletScript(BulletExt owner) : base(owner) { }

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
                    if (pTargetRef.GameObject.GetComponent(IonCannonLauncherDecorator.ID) == null)
                    {
                        var pos = Owner.OwnerObject.Ref.Target.Ref.GetCoords();
                        pTargetRef.GameObject.CreateScriptComponent(nameof(IonCannonLauncherDecorator),IonCannonLauncherDecorator.ID, "IonCannonLauncherDecorator Decorator", pTargetRef, pos,height);
                    }
                }
            }
        }



        [Serializable]
        public class IonCannonLauncherDecorator : TechnoScriptable
        {
            public static int ID = 414001;
            public IonCannonLauncherDecorator(TechnoExt self, CoordStruct center, int height) : base(self)
            {
                Self=(self);
                this.center = center;
                this.height = height;
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
                        //每xx角度生成一条光束，越小越密集
                        for (var angle = startAngle; angle < startAngle + 360; angle += 45)
                        {
                            var pos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z);
                            Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(pos + new CoordStruct(0, 0, 9000), pos + new CoordStruct(0, 0, -center.Z), innerColor, outerColor, outerSpread, 5);
                            pLaser.Ref.Thickness = 10;
                            pLaser.Ref.IsHouseColor = false;

                            //每条光束/帧的伤害
                            int damage = 50;
                            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, beanWarhead, 100, true);
                            pBullet.Ref.DetonateAndUnInit(pos + new CoordStruct(0, 0, -height));
                        }

                        //每次半径缩小越大，光束聚集越快
                        radius -= 5;
                        //每次旋转的角度，角度越大旋转越快
                        startAngle -= 2;
                    }
                    else
                    {
                        if (!isWaveRelased)
                        {
                            Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                            Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
                            Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(swLight);
                            CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                            pSuper.Ref.IsCharged = true;
                            pSuper.Ref.Launch(targetCell, true);
                            pSuper.Ref.IsCharged = false;

                            int damage = 700;
                            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, waveWarhead, 100, false);
                            pBullet.Ref.DetonateAndUnInit(center+new CoordStruct(0,0,-height));
                            isWaveRelased = true;
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
                                int damage = 5;
                                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, blastWarhead, 100, false);
                                pBullet.Ref.DetonateAndUnInit(pos + new CoordStruct(0, 0, -height));
                            }
                            //每次冲击波扩展的距离，距离越大扩散越快
                            blastRadius += 25;
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

        }


    }

}
