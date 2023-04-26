using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.Heros
{

    [Serializable]
    [ScriptAlias(nameof(BlackLotusScript))]

    public class BlackLotusScript : TechnoScriptable
    {
        public BlackLotusScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner, 15);
        }

        private ManaCounter _manaCounter;


        //光束颜色
        //static ColorStruct innerColor = new ColorStruct(0, 64, 128);
        //static ColorStruct outerColor = new ColorStruct(0, 64, 128);
        //static ColorStruct outerSpread = new ColorStruct(0, 64, 128);

        static ColorStruct innerColor = new ColorStruct(255, 64, 32);
        static ColorStruct outerColor = new ColorStruct(255, 64, 32);
        static ColorStruct outerSpread = new ColorStruct(255, 64, 32);

        private bool isActived = false;

        private Pointer<WeaponTypeClass> weaponType => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("LotusIonLaser");

        //光束聚集时使用的弹头
        static Pointer<WarheadTypeClass> beanWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DPIonBeanBlue");
        //冲击波爆炸使用的弹头
        static Pointer<WarheadTypeClass> blastWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DPIonBlastBlue");

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<WarheadTypeClass> showWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("LotusEffectWh");

        static Pointer<WarheadTypeClass> waveWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DPIonSmallWave");

        //光束初始角度
        private int startAngle = -180;
        //光束半径
        private int radius { get; set; } = 1000;

        //冲击波当前半径
        private int blastRadius = 0;

        private int height = 0;

        private int radiusSpeed = 5;


        //中心点位置
        private CoordStruct center;

        //冲击波生成间隔
        private int blastDamageRof = 1;
        //当前冲击波的帧数
        private int currentBlastFrame = 0;


        //正在选择目标,下次攻击将发射离子炮
        private bool IsTargetSelecting = false;

        private bool isWaveRelased = false;

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if(mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);
                if (_manaCounter.Cost(100))
                {
                    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, showWarhead, 100, false);
                    pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    IsTargetSelecting = true;
                }
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
                        //Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(pos + new CoordStruct(0, 0, 9000), pos + new CoordStruct(0, 0, -center.Z), innerColor, outerColor, outerSpread, 5);
                        //pLaser.Ref.Thickness = 10;
                        //pLaser.Ref.IsHouseColor = true;



                        //每条光束/帧的伤害
                        int damage = weaponType.Ref.Damage;
                        Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, beanWarhead, 100, true);
                        pBullet.Ref.Base.SetLocation(pos + new CoordStruct(0, 0, -height));
                        Owner.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, weaponType, pos + new CoordStruct(0, 0, 9000));
                        pBullet.Ref.DetonateAndUnInit(pos + new CoordStruct(0, 0, -height));
                    }

                    //每次半径缩小越大，光束聚集越快
                    radius -= (radiusSpeed <= 40 ? radiusSpeed++ / 2 : 20);
                    //每次旋转的角度，角度越大旋转越快
                    //startAngle -= 2;
                }
                else
                {
                    if (currentBlastFrame <= blastDamageRof)
                    {
                        currentBlastFrame++;
                    }
                    else
                    {

                        if (!isWaveRelased)
                        {
                            int damage = Owner.OwnerObject.Ref.Veterancy.IsElite() ? 100 : 50;
                            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, waveWarhead, 100, false);
                            pBullet.Ref.DetonateAndUnInit(center + new CoordStruct(0, 0, -height));
                            isWaveRelased = true;
                        }

                        //冲击波的扩散半径
                        if (blastRadius <= (Owner.OwnerObject.Ref.Veterancy.IsElite() ? 1500 : 1200))
                        {
                            //每xx角度生成一个动画，越小越密集
                            for (var angle = -180; angle < 180; angle += 20)
                            {
                                var pos = new CoordStruct(center.X + (int)(blastRadius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(blastRadius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z);
                                //每个冲击波/帧的伤害
                                int damage = Owner.OwnerObject.Ref.Veterancy.IsElite() ? 8 : 5;
                                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, blastWarhead, 100, false);
                                pBullet.Ref.DetonateAndUnInit(pos + new CoordStruct(0, 0, -height));
                            }
                            //每次冲击波扩展的距离，距离越大扩散越快
                            blastRadius += 30;
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
            bool controlledByAi = false;

            if (!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                    controlledByAi = true;
            }

            if (IsTargetSelecting == false)
            {
                if (controlledByAi)
                {
                    if (Owner.OwnerObject.Ref.Base.InLimbo)
                        return;
                    if (_manaCounter.Cost(100))
                    {
                        Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, showWarhead, 100, false);
                        pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                        IsTargetSelecting = true;
                    }
                }
            }
            else
            {
                if (weaponIndex == 0)
                {
                    if (isActived == false)
                    {
                        IsTargetSelecting = false;
                        isActived = true;

                        //光束开始聚集的半径
                        radius = 1000;
                        blastRadius = 0;
                        startAngle = -160;
                        radiusSpeed = 5;

                        height = Owner.OwnerObject.Ref.Base.GetHeight();

                        isWaveRelased = false;
                        //中心点
                        center = pTarget.Ref.GetCoords();
                    }
                }
            }

        }

      
    }
}
