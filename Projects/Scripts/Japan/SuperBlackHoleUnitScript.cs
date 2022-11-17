using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;

namespace DpLib.Scripts.Japan
{
    [Serializable]
    [ScriptAlias(nameof(SuperBlackHoleUnitScript))]
    public class SuperBlackHoleUnitScript : TechnoScriptable
    {
        public SuperBlackHoleUnitScript(TechnoExt owner) : base(owner)
        {
        }

        TechnoExt pTargetRef;

        //起爆点位置调整（相当于投放点坐标X，Y，Z的变化）
        static CoordStruct XYZAdujust = new CoordStruct(0, 0, 1500);

        public bool isActived = false;

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisibleAll");

        //用来引爆及产生动画的弹头
        static Pointer<WarheadTypeClass> animWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SBHAnimWh");

        //用于模拟环境光的核弹超武
        static Pointer<SuperWeaponTypeClass> swLight => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("MockLightGreenSpecial");

        //免疫黑洞的单位清单
        internal static List<string> immnueToBlackHoles => new List<string>() { "ZSTNK", "TU160", "GNTNK", "CNXHWSHIP", "CNXHWSHIP", "EPICTNK" };

        //持续范围伤害的弹头，其实用动画伤害即可
        static Pointer<WarheadTypeClass> damageWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SBHDamageWh");

        //黑洞用于强制秒杀的弹头
        static Pointer<WarheadTypeClass> killWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BhForceKillWh");

        //用于模拟轨迹的抛射体
        static Pointer<BulletTypeClass> trackBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("SBHTrackBullet");

        static Pointer<WarheadTypeClass> trackWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Special");


        //此处修改持续时间
        private int delay = 300;

        //抛射体检测间隔
        private int bulletCheckRof = 0;

        private int unitCheckRof = 0;

        private int rof = 0;

        private CoordStruct location = default;

        public override void OnUpdate()
        {
            if (delay-- <= 0)
            {
                Owner.OwnerObject.Ref.Base.Remove();
                Owner.OwnerObject.Ref.Base.UnInit();
                return;
            }

            var launcher = Owner.OwnerObject;

            if (isActived == false)
            {
                //初始化黑洞
                isActived = true;

                //拿到初始目标点
                location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                //移动到中心点
                Owner.OwnerObject.Ref.Base.SetLocation(location + XYZAdujust);

                Pointer<BulletClass> spBullet = bulletType.Ref.CreateBullet(launcher.Convert<AbstractClass>(), launcher, 1, animWarhead, 100, false);
                spBullet.Ref.DetonateAndUnInit(location + XYZAdujust);

                Pointer<HouseClass> pOwner = launcher.Ref.Owner;
                Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(swLight);
                CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                pSuper.Ref.IsCharged = true;
                pSuper.Ref.Launch(targetCell, true);
                pSuper.Ref.IsCharged = false;

            }
            else
            {
                if (bulletCheckRof-- <= 0)
                {
                    bulletCheckRof = 20;
                    //消除抛射体
                    ref DynamicVectorClass<Pointer<BulletClass>> bullets = ref BulletClass.Array;

                    for (int i = bullets.Count - 1; i >= 0; i--)
                    {
                        Pointer<BulletClass> xBullet = bullets.Get(i);

                        var bulletLocation = xBullet.Ref.Base.Base.GetCoords();

                        if (xBullet.Ref.Type.Ref.Inviso == true)
                        {
                            continue;
                        }

                        if (xBullet.Ref.Type.Ref.Base.Base.ID == trackBullet.Ref.Base.Base.ID)
                        {
                            continue;
                        }

                        //抛射体距离检测
                        if (bulletLocation.DistanceFrom(location) <= 2560)
                        {
                            Pointer<BulletClass> sBullet = bulletType.Ref.CreateBullet(launcher.Convert<AbstractClass>(), launcher, 1, killWarhead, 100, false);
                            sBullet.Ref.DetonateAndUnInit(bulletLocation);
                            xBullet.Ref.Base.Health = 0;
                            xBullet.Ref.Base.Remove();
                            xBullet.Ref.Base.UnInit();
                        }
                    }
                }

                if (rof-- <= 0)
                {
                    rof = 50;
                    //间歇性造成范围伤害
                    var damage = 150;
                    Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, damageWarhead, 100, false);
                    pBullet.Ref.DetonateAndUnInit(location);
                }

                if (unitCheckRof-- <= 0)
                {
                    unitCheckRof = 25;

                    //寻找范围内的单位并引导向黑洞
                    var currentCell = CellClass.Coord2Cell(location);

                    CellSpreadEnumerator enumerator = new CellSpreadEnumerator(7);

                    foreach (CellStruct offset in enumerator)
                    {
                        CoordStruct where = CellClass.Cell2Coord(currentCell + offset, location.Z);

                        if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                        {
                            Point2D p2d = new Point2D(60, 60);
                            Pointer<TechnoClass> target = pCell.Ref.FindTechnoNearestTo(p2d, false, launcher);


                            pTargetRef = (TechnoExt.ExtMap.Find(target));
                            if (!pTargetRef.IsNullOrExpired())
                            {
                                if (pTargetRef.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Building && pTargetRef.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.BuildingType)
                                {
                                    if (immnueToBlackHoles.Contains(pTargetRef.OwnerObject.Ref.Type.Ref.Base.Base.ID))
                                    {
                                        continue;
                                    }

                                    //仅仅用于标记该单位已经在黑洞作用中了
                                    if (pTargetRef.GameObject.GetComponent(BlackHoleEffectedDecorator.ID) == null)
                                    {
                                        Pointer<BulletClass> bullet = trackBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pTargetRef.OwnerObject, 1, trackWarhead, 100, true);
                                        bullet.Ref.MoveTo(pTargetRef.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 50), new BulletVelocity(0, 0, 800));
                                        bullet.Ref.SetTarget(Owner.OwnerObject.Convert<AbstractClass>());

                                        pTargetRef.GameObject.CreateScriptComponent(nameof(BlackHoleEffectedDecorator), BlackHoleEffectedDecorator.ID, "BlackHoleEffectedDecorator Decorator", pTargetRef);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


    }


    [Serializable]
    [ScriptAlias(nameof(BlackHoleEffectedDecorator))]
    public class BlackHoleEffectedDecorator : TechnoScriptable
    {
        public static int ID = 514007;
        public BlackHoleEffectedDecorator(TechnoExt self) : base(self)
        {

        }


        TechnoExt Self;

        int lifetime = 200;


        public override void OnUpdate()
        {
            if (Self.IsNullOrExpired() || lifetime <= 0)
            {
                DetachFromParent();
                return;
            }

            lifetime--;
        }
    }


}
