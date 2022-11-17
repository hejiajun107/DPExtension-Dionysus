using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts
{

    [Serializable]
    [ScriptAlias(nameof(IonCannonScript))]
    public class IonCannonScript : TechnoScriptable
    {
        public IonCannonScript(TechnoExt owner) : base(owner) { }

        //static ColorStruct innerColor = new ColorStruct(0, 162, 232);
        //static ColorStruct outerColor = new ColorStruct(0, 162, 232);
        //static ColorStruct outerSpread = new ColorStruct(0, 162, 232);
        //光束颜色
        static ColorStruct innerColor = new ColorStruct(255, 0, 0);
        static ColorStruct outerColor = new ColorStruct(255, 0, 0);
        static ColorStruct outerSpread = new ColorStruct(255, 0, 0);

        private bool isActived = false;

        //光束聚集时使用的弹头
        static Pointer<WarheadTypeClass> beanWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DPIonBeanRED");
        //冲击波爆炸使用的弹头
        static Pointer<WarheadTypeClass> blastWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DPIonBlastRED");

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        //光束初始角度
        private int startAngle = -180;
        //光束半径
        private int radius { get; set; } = 1000;

        //冲击波当前半径
        private int blastRadius = 0;

        private int height = 0;

        //中心点位置
        private CoordStruct center;

        //冲击波生成间隔
        private int blastDamageRof = 1;
        //当前冲击波的帧数
        private int currentBlastFrame = 0;

        public override void OnUpdate()
        {
            if (isActived)
            {
                //光束聚集
                if (radius >= 0)
                {
                    //每xx角度生成一条光束，越小越密集
                    for (var angle = startAngle; angle < startAngle + 360; angle += 45)
                    {
                        var pos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z);
                        Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(pos + new CoordStruct(0, 0, 9000), pos + new CoordStruct(0, 0, -height), innerColor, outerColor, outerSpread, 5);
                        pLaser.Ref.Thickness = 10;
                        pLaser.Ref.IsHouseColor = false;

                        //每条光束/帧的伤害
                        int damage = 5;
                        Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, beanWarhead, 100, false);
                        pBullet.Ref.DetonateAndUnInit(pos + new CoordStruct(0, 0, -height));
                    }

                    //每次半径缩小越大，光束聚集越快
                    radius -= 5;
                    //每次旋转的角度，角度越大旋转越快
                    startAngle -= 2;
                }
                else
                {
                    if (currentBlastFrame <= blastDamageRof)
                    {
                        currentBlastFrame++;
                    }
                    else
                    {
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
                        }

                        currentBlastFrame = 0;
                    }

                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (isActived == false)
            {
                isActived = true;

                //光束开始聚集的半径
                radius = 1000;
                blastRadius = 0;
                startAngle = -180;

                height = Owner.OwnerObject.Ref.Base.GetHeight();

                //中心点
                center = pTarget.Ref.GetCoords();
            }



        }

    }








}

