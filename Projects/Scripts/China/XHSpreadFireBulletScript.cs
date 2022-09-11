using DynamicPatcher;
using Extension.Decorators;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.China
{


    public class XHSpreadFireBulletScript : BulletScriptable
    {

        Dictionary<string, int> SpecialLevels = new Dictionary<string, int>()
        {

        };

        private Random random = new Random(150523);

        public XHSpreadFireBulletScript(BulletExt owner) : base(owner) { }

        public bool isActived = false;

        private bool IsElite = false;

        ExtensionReference<TechnoExt> pOwnerRef;

        ExtensionReference<TechnoExt> pTargetRef;

        static List<ColorStruct> LaserLevels = new List<ColorStruct>()
        {
            new ColorStruct(0, 223, 223),
            new ColorStruct(255, 127, 39),
            new ColorStruct(255, 0, 0),
            new ColorStruct(128, 0, 255),
            new ColorStruct(34, 177, 76),
        };


        ColorStruct bigLaserColor = new ColorStruct(34,177,76);



        static Pointer<WarheadTypeClass> fireWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHFireWH");
        static Pointer<WarheadTypeClass> empWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHEmpWh");
        static Pointer<WarheadTypeClass> powerWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XhPowerWh");

        //雷达，战车工厂，巨炮
        private static List<string> Level2Buildings = new List<string>() {
           "GAAIRC","NARADR","AMRADR","NAPSIS","ZGYGLD","JPRADR","CAAIRCOMD","GAWEAP","NAWEAP","YAWEAP","ZGZCGC","JPWEAP","GTGCAN","UAPOWR","RAWEAP","RAAIRC","RASTOM"
        };
        //2500 超级武器
        private static List<string> Level3Buildings = new List<string>() {
            "GACSPH","NAIRON","YAGNTC","CATYZZ","JPPONR","RABIGDOOR"
        };
        //基地
        private static List<string> Level4Buildings = new List<string>() {
             "GACNST","NACNST","YACNST","ZGJZC","JPCNST","RACNST"
        };
        //核电站,5000超级武器
        private static List<string> Level5Buildings = new List<string>() {
           "NANRCT","YHKZT","JPWIND","YAPPET","GAWEAT","NAMISL","RARIFT"
        };

        //不被计算的建筑
        private static List<string> ExcludeBuildings = new List<string>()
        {
            "PowerDownBuilding"
        };


        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        //用于降低电力
        static Pointer<SuperWeaponTypeClass> pwd200 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SWPowerDown200");
        static Pointer<SuperWeaponTypeClass> pwd400 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SWPowerDown400");
        static Pointer<SuperWeaponTypeClass> pwd600 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SWPowerDown600");
        static Pointer<SuperWeaponTypeClass> pwd800 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SWPowerDown800");
        static Pointer<SuperWeaponTypeClass> mockLight => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("MockLightOrangeSpecial");


        static Pointer<BulletTypeClass> pSeeker => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisileSeeker");
        


        static Pointer<WarheadTypeClass> exp1 =>  WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHExpWH1");
        static Pointer<WarheadTypeClass> exp2 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHExpWH2");
        static Pointer<WarheadTypeClass> exp3 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHExpWH3");
        static Pointer<WarheadTypeClass> exp4 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHExpWH4");
        static Pointer<WarheadTypeClass> exp5 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHExpWH5");
        static Pointer<WarheadTypeClass> nukeExp => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHNukeExpWH");

        static Pointer<WarheadTypeClass> friendExp1 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHAllyExpWH1");
        static Pointer<WarheadTypeClass> friendExp2 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHAllyExpWH2");
        static Pointer<WarheadTypeClass> friendExp3 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHAllyExpWH3");
        static Pointer<WarheadTypeClass> friendExp4 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHAllyExpWH4");
        static Pointer<WarheadTypeClass> friendExp5 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHAllyExpWH5");


        public bool reverse = false;

        List<ExtensionReference<TechnoExt>> targets = new List<ExtensionReference<TechnoExt>>();
        List<int> codes = new List<int>();
        List<int> levels = new List<int>();

        private CoordStruct startLocation;

        private int max = 12;

        private int delay = 8;

        private int totalLevel = 0;

        private bool attacked = false;


        private CoordStruct targetLocation;

        public override void OnUpdate()
        {

            if (isActived != true)
            {
                isActived = true;

                IsElite = Owner.Type.OwnerObject.Ref.Base.Base.ID == "XhGrandBulletE";
             

                InitLevelDic();

                //检索目标
                pOwnerRef.Set(Owner.OwnerObject.Ref.Owner);
                int height = Owner.OwnerObject.Ref.Base.GetHeight();

              
                targetLocation = Owner.OwnerObject.Ref.Target.Ref.GetCoords();

                startLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                //从附近获取建筑
                if (pOwnerRef.TryGet(out TechnoExt pOwner))
                {

                    var location = pOwner.OwnerObject.Ref.Base.Base.GetCoords();

                    var currentCell = CellClass.Coord2Cell(location);

                    CellSpreadEnumerator enumerator = new CellSpreadEnumerator(12);

                    foreach (CellStruct offset in enumerator)
                    {
                        if (targets.Count() > max)
                        {
                            break;
                        }

                        CoordStruct where = CellClass.Cell2Coord(currentCell + offset, location.Z);


                        if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                        {
                            if (pCell.IsNull)
                            {
                                continue;
                            }

                            Point2D p2d = new Point2D(60, 60);
                            Pointer<TechnoClass> target = pCell.Ref.FindTechnoNearestTo(p2d, false, pOwner.OwnerObject);


                            if (TechnoExt.ExtMap.Find(target) == null)
                            {
                                continue;
                            }

                            ExtensionReference<TechnoExt> tref = default;

                            tref.Set(TechnoExt.ExtMap.Find(target));

                            if (tref.TryGet(out TechnoExt ptechno))
                            {
                                if (ptechno.OwnerObject.Ref.Owner.IsNull)
                                {
                                    continue;
                                }
                                if (pOwner.OwnerObject.Ref.Owner.Ref.ArrayIndex != ptechno.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                                {
                                    continue;
                                }
                                if (ptechno.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Building)
                                {
                                    continue;
                                }
                                

                                var hashCode = ptechno.OwnerObject.GetHashCode();
                                var id = ptechno.Type.OwnerObject.Ref.Base.Base.ID.ToString();

                                if (ExcludeBuildings.Contains(id))
                                {
                                    continue;
                                }

                                if (!codes.Where(c => c == hashCode).Any())
                                {
                                    targets.Add(tref);
                                    codes.Add(hashCode);

                                    if (SpecialLevels.ContainsKey(id))
                                    {
                                        levels.Add(SpecialLevels[id]);
                                    }
                                    else
                                    {
                                        levels.Add(1);
                                    }

                                }
                            }
                        }

                    }


                    var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords(); //targetLocation; //Owner.OwnerObject.Ref.Base.Base.GetCoords();

                    int index = 0;

                    if (targets.Count > 0)
                    {
                        //对每个建筑进行操作
                        foreach (var item in targets)
                        {
                            if (item.TryGet(out TechnoExt pTargetExt))
                            {
                                var level = levels[index];

                                //绘制激光
                                var targetPos = pTargetExt.OwnerObject.Ref.Base.Base.GetCoords();
                                var pos1 = new CoordStruct(targetPos.X, targetPos.Y, location.Z);

                                var laserColor = LaserLevels[level-1];

                                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(targetPos, pos1, laserColor, laserColor, new ColorStruct(0,0,0), 200);
                                pLaser.Ref.IsHouseColor = true;
                                pLaser.Ref.Thickness = 7;

                                Pointer<LaserDrawClass> pLaser2 = YRMemory.Create<LaserDrawClass>(pos1, location, laserColor, laserColor, new ColorStruct(0, 0, 0), 200);
                                pLaser2.Ref.IsHouseColor = true;
                                pLaser2.Ref.Thickness = 7;


                                //关闭当前建筑
                                Pointer<BulletClass> pEmpBullet = pBulletType.Ref.CreateBullet(pOwner.OwnerObject.Convert<AbstractClass>(), pOwner.OwnerObject, 1, empWarhead, 100, false);
                                pEmpBullet.Ref.DetonateAndUnInit(targetPos);


                                for(var i = 0; i < level; i++)
                                {
                                    Pointer<BulletClass> pPowerBullet = pBulletType.Ref.CreateBullet(pOwner.OwnerObject.Convert<AbstractClass>(), pOwner.OwnerObject, 1, powerWarhead, 100, false);
                                    pPowerBullet.Ref.DetonateAndUnInit(location + new CoordStruct(random.Next(-600,600), random.Next(-600, 600),100));
                                }
                                //var damage = 100;
                                //Pointer<BulletClass> pBullet = plaserBullet.Ref.CreateBullet(pOwner.OwnerObject.Convert<AbstractClass>(), pOwner.OwnerObject, damage, fireWarhead, 100, false);
                                //pBullet.Ref.SetTarget(pTargetExt.OwnerObject.Convert<AbstractClass>());
                                //pBullet.Ref.MoveTo(currentLocation + new CoordStruct(random.Next(-10, 10), random.Next(-10, 10), 0), new BulletVelocity(0, 0, 0));
                            }

                            index++;
                        }


                        //消耗电力
                        if (targets.Count() >=1  && targets.Count()<=3)
                        {
                            PowerDown(pwd200,pOwner);
                        }
                        else if(targets.Count()>3 && targets.Count() <= 6)
                        {
                            PowerDown(pwd400, pOwner);
                        }
                        else if(targets.Count()>6 && targets.Count()<=8)
                        {
                            PowerDown(pwd600, pOwner);
                        }
                        else
                        {
                            PowerDown(pwd800, pOwner);
                        }

                    }


                    //总的充能级别
                    totalLevel = levels.Sum();

                    if(IsElite)
                    {
                        totalLevel += 3;
                    }    

                    if(totalLevel>20)
                    {
                        totalLevel = 20;
                    }
                }
            }

            if (delay-- > 0 || attacked) { 
                return; 
            }

            attacked = true;
            pOwnerRef.Set(Owner.OwnerObject.Ref.Owner);
            if (pOwnerRef.TryGet(out TechnoExt pAtOwner))
            {
                DoAttack(pAtOwner);
            }
        }

        private void DoAttack(TechnoExt pOwner)
        {
            var baseDamage = 280;

            if(IsElite)
            {
                baseDamage += 50;
            }

            //绘制大激光
            Pointer<LaserDrawClass> pBigLaser = YRMemory.Create<LaserDrawClass>(startLocation, targetLocation, bigLaserColor, bigLaserColor, new ColorStruct(0, 0, 0), 120);
            pBigLaser.Ref.IsHouseColor = true;
            var thickness = 15 + totalLevel * 3;
            pBigLaser.Ref.Thickness = thickness;

            if (totalLevel == 0)
            {
                //基础伤害
                var damage = baseDamage;
                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pOwner.OwnerObject.Convert<AbstractClass>(), pOwner.OwnerObject, damage, exp1, 100, true);
                pBullet.Ref.DetonateAndUnInit(targetLocation);
            }
            else if (totalLevel > 0 && totalLevel <= 3)
            {
                var damage = (20 * totalLevel + baseDamage);

                DoNormalDamage(damage, 3, exp1, pOwner);
                DoFriendlyDamage(damage, friendExp1, pOwner);


            }
            else if (totalLevel > 3 && totalLevel <= 6)
            {
                var damage = (20 * totalLevel + baseDamage);
                DoNormalDamage(damage, 5, exp2, pOwner);
                DoFriendlyDamage(damage, friendExp2, pOwner);

            }
            else if (totalLevel > 6 && totalLevel <= 9)
            {
                var damage = (20 * totalLevel + baseDamage);
                DoNormalDamage(damage, 6, exp3, pOwner);
                DoFriendlyDamage(damage, friendExp3, pOwner);

                DoBlast(4);
            }
            else if (totalLevel > 9 && totalLevel <= 12)
            {
                var damage = (20 * totalLevel + baseDamage);
                DoNormalDamage(damage, 8, exp4, pOwner);
                DoFriendlyDamage(damage, friendExp4, pOwner);
                DoBlast(6);
            }
            else if (totalLevel > 12 && totalLevel <= 15)
            {
                var damage = (20 * totalLevel + baseDamage);
                DoNormalDamage(damage, 10, exp5, pOwner);
                DoFriendlyDamage(damage, friendExp5, pOwner);
                DoBlast(8);
            }
            else if (totalLevel > 15 && totalLevel <= 19)
            {
                var damage = 25 * totalLevel + 100;
                DoNormalDamage(damage, 12, exp5, pOwner);
                DoNukeExplode(300, pOwner);
                DoFriendlyDamage(damage + 150, friendExp5, pOwner);
                DoBlast(8);
            }
            else
            {
                ShowLight(pOwner);
                var damage = 25 * totalLevel + 100;
                DoNormalDamage(damage, 12, exp5, pOwner);
                DoNukeExplode(400, pOwner);
                DoFriendlyDamage(damage + 200, friendExp5, pOwner);
                DoBlast(8);
            }
        }


        private void DoNormalDamage(int totalDamage,int explodeCount,Pointer<WarheadTypeClass> warhead,TechnoExt pOwner)
        {
            var damage = totalDamage / explodeCount;

            for (var i = 0; i < explodeCount; i++)
            {
                Pointer<BulletClass> pBullet = pSeeker.Ref.CreateBullet(pOwner.OwnerObject.Convert<AbstractClass>(), pOwner.OwnerObject, damage, warhead, 100, true);
                if (i == 0)
                {
                    pBullet.Ref.DetonateAndUnInit(targetLocation);
                }
                else
                {
                    var rdlocaton = targetLocation + new CoordStruct(random.Next(-500, 500), random.Next(-500, 500), 0);
                    //模拟多重爆炸
                    if (MapClass.Instance.TryGetCellAt(rdlocaton, out Pointer<CellClass> cell))
                    {
                        pBullet.Ref.SetTarget(cell.Convert<AbstractClass>());
                        pBullet.Ref.MoveTo(rdlocaton + new CoordStruct(0, 0, i * 30), new BulletVelocity(0, 0, 0));
                    }
                }
            }
        }

        private void DoFriendlyDamage(int totalDamge, Pointer<WarheadTypeClass> warhead, TechnoExt pOwner)
        {
            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pOwner.OwnerObject.Convert<AbstractClass>(), pOwner.OwnerObject, totalDamge, warhead, 100, true);
            pBullet.Ref.DetonateAndUnInit(targetLocation);
        }

        private void DoNukeExplode(int damage, TechnoExt pOwner)
        {
            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pOwner.OwnerObject.Convert<AbstractClass>(), pOwner.OwnerObject, damage, nukeExp, 100, true);
            pBullet.Ref.DetonateAndUnInit(targetLocation);
        }

        private void DoBlast(int range)
        {
            pTargetRef.Set(Owner.OwnerObject.Ref.Owner);
            int height = Owner.OwnerObject.Ref.Base.GetHeight();
            if (pTargetRef.TryGet(out TechnoExt pTargetExt))
            {
                if (pTargetExt.GameObject.GetComponent(XhWeaponExplodeDecorator.ID) == null)
                {
                    var pos = Owner.OwnerObject.Ref.Target.Ref.GetCoords();
                    pTargetExt.GameObject.CreateScriptComponent(nameof(XhWeaponExplodeDecorator),XhWeaponExplodeDecorator.ID, "XhWeaponExplodeDecorator Decorator", pTargetExt, pos, 0, range);
                }
            }
        }


        private void PowerDown(Pointer<SuperWeaponTypeClass> swType,TechnoExt pOwner)
        {
            Pointer<SuperClass> pSuper = pOwner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(swType);
            CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            pSuper.Ref.IsCharged = true;
            pSuper.Ref.Launch(targetCell, true);
            pSuper.Ref.IsCharged = false;
        }

        private void ShowLight(TechnoExt pOwner)
        {
            Pointer<SuperClass> pSuper = pOwner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(mockLight);
            CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            pSuper.Ref.IsCharged = true;
            pSuper.Ref.Launch(targetCell, true);
            pSuper.Ref.IsCharged = false;
        }


        private void InitLevelDic()
        {
            foreach(var item in Level2Buildings)
            {
                if(!SpecialLevels.ContainsKey(item))
                {
                    SpecialLevels.Add(item, 2);
                }
            }

            foreach (var item in Level3Buildings)
            {
                if (!SpecialLevels.ContainsKey(item))
                {
                    SpecialLevels.Add(item, 3);
                }
            }

            foreach (var item in Level4Buildings)
            {
                if (!SpecialLevels.ContainsKey(item))
                {
                    SpecialLevels.Add(item, 4);
                }
            }

            foreach (var item in Level5Buildings)
            {
                if (!SpecialLevels.ContainsKey(item))
                {
                    SpecialLevels.Add(item, 5);
                }
            }
        }

        #region 已经废弃
        /// <summary>
        /// 会射出一堆激光，废弃了
        /// </summary>
        //public override void OnUpdate()
        //{
        //    if (isActived != true)
        //    {
        //        isActived = true;

        //        //检索目标
        //        pOwnerRef.Set(Owner.OwnerObject.Ref.Owner);
        //        int height = Owner.OwnerObject.Ref.Base.GetHeight();

        //        targetLocation = Owner.OwnerObject.Ref.Target.Ref.GetCoords();

        //        if (pOwnerRef.TryGet(out TechnoExt pOwner))
        //        {

        //            var location = targetLocation;

        //            var currentCell = CellClass.Coord2Cell(location);

        //            CellSpreadEnumerator enumerator = new CellSpreadEnumerator(7);

        //            foreach (CellStruct offset in enumerator)
        //            {
        //                if (targets.Count() > max)
        //                {
        //                    break;
        //                }

        //                CoordStruct where = CellClass.Cell2Coord(currentCell + offset, location.Z);


        //                if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
        //                {
        //                    if (pCell.IsNull)
        //                    {
        //                        continue;
        //                    }

        //                    Point2D p2d = new Point2D(60, 60);
        //                    Pointer<TechnoClass> target = pCell.Ref.FindTechnoNearestTo(p2d, false, pOwner.OwnerObject);


        //                    if (TechnoExt.ExtMap.Find(target) == null)
        //                    {
        //                        continue;
        //                    }

        //                    ExtensionReference<TechnoExt> tref = default;

        //                    tref.Set(TechnoExt.ExtMap.Find(target));

        //                    if(tref.TryGet(out TechnoExt ptechno))
        //                    {
        //                        if (ptechno.OwnerObject.Ref.Owner.IsNull)
        //                        {
        //                            continue; 
        //                        }
        //                        if (pOwner.OwnerObject.Ref.Owner.Ref.ArrayIndex == ptechno.OwnerObject.Ref.Owner.Ref.ArrayIndex)
        //                        {
        //                            continue;
        //                        }

        //                        var hashCode = ptechno.OwnerObject.GetHashCode();

        //                        if (codes.Where(c => c == hashCode).Count() < 3)
        //                        {
        //                            targets.Add(tref);
        //                            codes.Add(hashCode);
        //                        }
        //                    }
        //                }

        //            }

        //            var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords(); //targetLocation; //Owner.OwnerObject.Ref.Base.Base.GetCoords();

        //            if (targets.Count > 0)
        //            {
        //                foreach (var item in targets)
        //                {
        //                    //var item = targets[0];

        //                    if (item.TryGet(out TechnoExt pTargetExt))
        //                    {
        //                        var targetPos = pTargetExt.OwnerObject.Ref.Base.Base.GetCoords();

        //                        var damage = 100;
        //                        Pointer<BulletClass> pBullet = plaserBullet.Ref.CreateBullet(pOwner.OwnerObject.Convert<AbstractClass>(), pOwner.OwnerObject, damage, fireWarhead, 100, false);
        //                        pBullet.Ref.SetTarget(pTargetExt.OwnerObject.Convert<AbstractClass>());
        //                        pBullet.Ref.MoveTo(currentLocation + new CoordStruct(random.Next(-10, 10), random.Next(-10, 10), 0), new BulletVelocity(0, 0, 0));
        //                    }
        //                }



        //            }

        //            if (targets.Count() < max)
        //            {
        //                for (var i = 0; i < max - targets.Count; i++)
        //                {

        //                    var damage = 100;
        //                    Pointer<BulletClass> pBullet = plaserBullet.Ref.CreateBullet(pOwner.OwnerObject.Convert<AbstractClass>(), pOwner.OwnerObject, damage, fireWarhead, 50, false);

        //                    var rdlocaton = targetLocation + new CoordStruct(random.Next(-1000, 1000), random.Next(-1000, 1000), 0);

        //                    if (MapClass.Instance.TryGetCellAt(rdlocaton, out Pointer<CellClass> cell))
        //                    {
        //                        pBullet.Ref.SetTarget(cell.Convert<AbstractClass>());
        //                        pBullet.Ref.MoveTo(currentLocation + new CoordStruct(random.Next(-10, 10), random.Next(-10, 10), 0), new BulletVelocity(0, 0, 0));
        //                    }
        //                }
        //            }
        //        }
        //    }

        //}
        #endregion

    }



    [Serializable]
    public class XhWeaponExplodeDecorator : TechnoScriptable
    {
        public static int ID = 414008;
        public XhWeaponExplodeDecorator(TechnoExt self, CoordStruct center, int height, int range) : base(self)
        {
            Self.Set(self);
            this.center = center;
            isActived = true;
            this.range = range;
            this.height = height;
        }

        ExtensionReference<TechnoExt> Self;


        private bool isActived = false;
        private int height = 0;
        private int range = 0;

        //光束聚集时使用的弹头
        static Pointer<WarheadTypeClass> boomWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHFireWH");
        //冲击波爆炸使用的弹头
        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");


        private int startAngle = -180;

        //光束半径
        private int radius { get; set; } = 0;

        //中心点位置
        private CoordStruct center;

        //冲击波生成间隔
        private int rof = 2;
        //当前冲击波的帧数

        public override void OnUpdate()
        {

            if (Self.Get() == null)
            {
                DetachFromParent();
                //Decorative.Remove(this);
                return;
            }

            //if (rof-- > 0)
            //{
            //    return;
            //}

            //rof = 3;

            var Owner = Self.Get();


            if (isActived)
            {
                if (radius <= this.range*256)
                {
                    for (var angle = startAngle; angle < startAngle + 360; angle += 30)
                    {
                        var pos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z);
                        int damage = 5;
                        Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, boomWarhead, 100, false);
                        pBullet.Ref.DetonateAndUnInit(pos + new CoordStruct(0, 0, -height));
                    }

                    radius += 25;
                    startAngle -= 2;
                }
                else
                {
                    this.isActived = false;
                    DetachFromParent();
                    return;
                }

            }


        }

    }



}
