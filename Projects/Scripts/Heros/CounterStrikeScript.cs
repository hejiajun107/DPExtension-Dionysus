using Extension.Coroutines;
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using Scripts;
using System;
using System.Collections;

namespace DpLib.Scripts.Heros
{

    [Serializable]
    [ScriptAlias(nameof(CounterStrikeScript))]

    public class CounterStrikeScript : TechnoScriptable
    {
        public CounterStrikeScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner,16);
            _voc = new VocExtensionComponent(owner);
            _vwatcher = new VertenceyWatcher(owner, _voc);
        }


        private int weaponInitialDamage = 90;
        private int weaponInitialROF = 40;
        private int eWeaponInitialDamage = 120;
        private int eWeaponInitialROF = 40;

        //光束颜色
        static ColorStruct innerColor = new ColorStruct(34, 177, 76);
        static ColorStruct outerColor = new ColorStruct(34, 177, 76);
        static ColorStruct outerSpread = new ColorStruct(34, 177, 76);

        private ManaCounter _manaCounter;
        private VocExtensionComponent _voc;
        private VertenceyWatcher _vwatcher;

        private bool isActived = false;

        static Pointer<WeaponTypeClass> weapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("HM4M");
        static Pointer<WeaponTypeClass> eWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("HM4ME");
        static Pointer<WarheadTypeClass> blastWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DPIonBlastGreen");
        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<SuperWeaponTypeClass> airStrike => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("A5AirstrikeSpecial");

        //private Pointer<AnimTypeClass> pblood => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("FKBLOOD");


        private int maxHealth = 650;

        /// <summary>
        /// 扫描的间隔，为0退出当前攻击
        /// </summary>
        private int scaningDelay = 0;

        private bool isWorking = false;

        private double angle = 0d;

        private double radius;

        private double currentAngle;

        private int coolDown = 0;

        private CoordStruct start;
        private CoordStruct target;

        private int maxAttackCount = 300;
        private int currentAttackCount = 0;

        Random random = new Random(130522);

        //private int healthCheckRof = 5;
        //private int animCheckRof = 150;

        public override void Awake()
        {
            _voc.Awake();
            base.Awake();
        }

        public override void OnUpdate()
        {
            _vwatcher.Update();
            //if (healthCheckRof-- <= 0)
            //{
            //    healthCheckRof = 5;
            //    var strength = Owner.OwnerObject.Ref.Type.Ref.Base.Strength;
            //    var health = Owner.OwnerObject.Ref.Base.Health;
            //    if (health <= strength * 0.3)
            //    {
            //        if(_manaCounter.Cost(5))
            //        {
            //            if (health > strength * 0.2)
            //            {
            //                Owner.OwnerRef.Base.Health += 10;
            //            }
            //            else
            //            {
            //                Owner.OwnerRef.Base.Health += 20;
            //            }
            //        }

            //    }
            //}

            //if (animCheckRof-- <= 0)
            //{
            //    animCheckRof = 150;
            //    var strength = Owner.OwnerObject.Ref.Type.Ref.Base.Strength;
            //    var health = Owner.OwnerObject.Ref.Base.Health;
            //    if (health <= strength * 0.35)
            //    {
            //        var anim = YRMemory.Create<AnimClass>(pblood, Owner.OwnerObject.Ref.Base.Base.GetCoords());
            //        anim.Ref.SetOwnerObject(Owner.OwnerObject.Convert<ObjectClass>());
            //    }
            //}
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                if (_manaCounter.Cost(100))
                {
                    //YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("ArcherSpAnim"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    if(Owner.GameObject.GetComponent<ExtraUnitMasterScript>() == null)
                    {
                        _voc.PlaySpecialVoice(2, true);
                        var eu = new ExtraUnitMasterScript(Owner, new ExtraUnitSetting() { ExtraUnitDefinations = new string[] { "CXUNIT1", "CXUNIT2", "CXUNIT3", "CXUNIT4" } });
                        eu.AttachToComponent(Owner.GameObject);
                        Owner.GameObject.StartCoroutine(RemoveExtraUnit());
                        YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("CXDUST"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    }
                }
                else
                {
                    _voc.PlaySpecialVoice(3, true);
                }
            }


            if (!isActived && coolDown > 0)
            {
                coolDown--;
            }

            if (scaningDelay > 0)
            {
                if (isActived)
                {
                    if (currentAngle > 0)
                    {
                        var posL = new CoordStruct(start.X + (int)(radius * Math.Round(Math.Cos((angle + currentAngle) * Math.PI / 180), 5)), start.Y + (int)(radius * Math.Round(Math.Sin((angle + currentAngle) * Math.PI / 180), 5)), target.Z);
                        Pointer<LaserDrawClass> pLaser1 = YRMemory.Create<LaserDrawClass>(start, posL, innerColor, outerColor, outerSpread, 5);
                        pLaser1.Ref.Thickness = 10;
                        pLaser1.Ref.IsHouseColor = false;

                        var posR = new CoordStruct(start.X + (int)(radius * Math.Round(Math.Cos((angle - currentAngle) * Math.PI / 180), 5)), start.Y + (int)(radius * Math.Round(Math.Sin((angle - currentAngle) * Math.PI / 180), 5)), target.Z);
                        Pointer<LaserDrawClass> pLaser2 = YRMemory.Create<LaserDrawClass>(start, posR, innerColor, outerColor, outerSpread, 5);
                        pLaser2.Ref.Thickness = 10;
                        pLaser2.Ref.IsHouseColor = false;

                        currentAngle -= 0.3;
                    }
                    else
                    {
                        //if(currentAttackCount<maxAttackCount)
                        //{
                        //    var posL = new CoordStruct(start.X + (int)(radius * Math.Round(Math.Cos((angle + currentAngle) * Math.PI / 180), 5)), start.Y + (int)(radius * Math.Round(Math.Sin((angle + currentAngle) * Math.PI / 180), 5)), target.Z);
                        //    Pointer<LaserDrawClass> pLaser1 = YRMemory.Create<LaserDrawClass>(start, posL, innerColor, outerColor, outerSpread, 5);
                        //    pLaser1.Ref.Thickness = 10;
                        //    pLaser1.Ref.IsHouseColor = false;

                        //    var posR = new CoordStruct(start.X + (int)(radius * Math.Round(Math.Cos((angle - currentAngle) * Math.PI / 180), 5)), start.Y + (int)(radius * Math.Round(Math.Sin((angle - currentAngle) * Math.PI / 180), 5)), target.Z);
                        //    Pointer<LaserDrawClass> pLaser2 = YRMemory.Create<LaserDrawClass>(start, posR, innerColor, outerColor, outerSpread, 5);
                        //    pLaser2.Ref.Thickness = 10;
                        //    pLaser2.Ref.IsHouseColor = false;


                        //    var ntarget = new CoordStruct(target.X + random.Next(-50, 50), target.Y + random.Next(-50, 50), target.Z);
                        //    Pointer<LaserDrawClass> pLaser3 = YRMemory.Create<LaserDrawClass>(ntarget + new CoordStruct(0, 0, 3000), ntarget, innerColor, outerColor, outerSpread, 10);
                        //    pLaser3.Ref.IsHouseColor = true;
                        //    pLaser3.Ref.Thickness = 20;

                        //    int damage = 6;
                        //    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, blastWarhead, 100, true);
                        //    pBullet.Ref.DetonateAndUnInit(new CoordStruct(ntarget.X, ntarget.Y, ntarget.Z + 1));


                        //    currentAttackCount++;
                        //}
                        //else
                        //{
                        //    Init();
                        //}
                        _voc.PlaySpecialVoice(1, true);
                        CallAirStrike(target, Owner.OwnerObject.Ref.Veterancy.IsElite() ? 2 : 1);
                        Init();
                        ResetCoolDown();
                    }
                }

            }
            else
            {
                Init();
            }

            if (scaningDelay > 0)
            {
                scaningDelay--;
            }
        }

        IEnumerator RemoveExtraUnit()
        {
            var frame = Owner.OwnerObject.Ref.Veterancy.IsElite() ? 500 : 400;
            yield return new WaitForFrames(frame);
            var eu = GameObject.GetComponent<ExtraUnitMasterScript>();
            if (eu != null)
            {
                eu.Clear();
                eu.DetachFromParent();
            }
        }

        private void CallAirStrike(CoordStruct target, int count = 1)
        {
            Pointer<SuperClass> pSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(airStrike);
            CellStruct targetCell = CellClass.Coord2Cell(target);
            pSuper.Ref.IsCharged = true;
            for (var i = 0; i < count; i++)
            {
                pSuper.Ref.Launch(targetCell, true);
            }
            pSuper.Ref.IsCharged = false;
        }

        private void ResetCoolDown()
        {
            coolDown = 250;
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            var pTechno = Owner.OwnerObject;
            if (weaponIndex == 0)
            {
                var health = pTechno.Ref.Base.Health;
                var strength = maxHealth;

                var damage = (int)(weapon.Ref.Damage * ((1d - (double)health / strength) * 0.6d));
                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, weapon.Ref.Warhead, 100, true);
                pBullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
                //以下会有问题，直接更改武器属性会导致有多个单位的情况下攻击力会相互影响，所以采用以上方案，补偿一个追加伤害。
                //weapon.Ref.Damage = (int)(weaponInitialDamage * (1 + (1d - (double)health / strength) * 0.4d));
                //eWeapon.Ref.Damage = (int)(eWeaponInitialDamage * (1 + (1d - (double)health / strength) * 0.4d));
                //weapon.Ref.ROF = (int)(weaponInitialROF - 0.6d * weaponInitialROF * ((strength - health) / (double)strength));
                //eWeapon.Ref.ROF = (int)(eWeaponInitialROF - 0.6d * eWeaponInitialROF * ((strength - health) / (double)strength));
            }
            else if (weaponIndex == 1 && coolDown <= 0)
            {
                scaningDelay = 20;
                if (isWorking == false)
                {
                    Init();
                    isWorking = true;
                    var start = pTechno.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 120);
                    var target = pTarget.Ref.GetCoords();

                    //半径
                    var radius = new CoordStruct(start.X, start.Y, 0).DistanceFrom(new CoordStruct(target.X, target.Y, 0));

                    var angle = 0d;

                    //判断是否与水平线重合
                    if (start.Y == target.Y)
                    {
                        if (start.X > target.X)
                        {
                            angle = 180;
                        }
                        else
                        {
                            angle = 0;
                        }
                    }
                    else if (start.X == target.X)
                    {
                        if (start.Y < target.Y)
                        {
                            angle = 90;
                        }
                        else
                        {
                            angle = 270;
                        }
                    }
                    else
                    {
                        var k = (double)((double)target.Y - start.Y) / ((double)target.X - start.X);
                        angle = Math.Atan(Math.Abs(k)) * 180 / Math.PI;


                        //第一象限
                        if (start.X < target.X && start.Y < target.Y)
                        {

                        }
                        else if (start.X < target.X && start.Y > target.Y)
                        {
                            angle = 360 - angle;
                        }
                        else if (start.X > target.X && start.Y > target.Y)
                        {
                            //第三象限
                            angle = 180 + angle;
                        }
                        else
                        {
                            //第四象限
                            angle = 180 - angle;
                        }

                    }

                    this.start = start;
                    this.target = target;
                    this.angle = angle;
                    this.radius = radius;
                    currentAngle = 30;

                    isActived = true;

                }


            }
        }


        private void Init()
        {
            angle = 0;
            currentAngle = 0;
            radius = 0;
            isWorking = false;
            isActived = false;
            currentAttackCount = 0;
        }


    }
}
