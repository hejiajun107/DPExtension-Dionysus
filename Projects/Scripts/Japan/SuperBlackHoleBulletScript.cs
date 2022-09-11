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
    [Serializable]
    public class SuperBlackHoleBulletScript : BulletScriptable
    {
        public SuperBlackHoleBulletScript(BulletExt owner) : base(owner) { }

        public bool isActived = false;

        ExtensionReference<TechnoExt> pTargetRef;

        ExtensionReference<TechnoExt> pOwnerRef;
        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisibleAll");

        static Pointer<WarheadTypeClass> animWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SBHAnimWh");

        static Pointer<SuperWeaponTypeClass> swLight => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("MockLightGreenSpecial");

        internal static List<string> immnueToBlackHoles => new List<string>() {  "ZSTNK", "TU160", "GNTNK", "CNXHWSHIP", "CNXHWSHIP", "EPICTNK" };

        public override void OnUpdate()
        {
            if (isActived == false)
            {
                var location = Owner.OwnerObject.Ref.Target.Ref.GetCoords();
                var currentCell = CellClass.Coord2Cell(location);
                var launcher = Owner.OwnerObject.Ref.Owner;


                pOwnerRef.Set(launcher);

                Pointer<BulletClass> spBullet = bulletType.Ref.CreateBullet(launcher.Convert<AbstractClass>(), launcher, 1, animWarhead, 100, false);
                spBullet.Ref.DetonateAndUnInit(location + new CoordStruct(0,0,1500));

                Pointer<HouseClass> pOwner = launcher.Ref.Owner;
                Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(swLight);
                CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                pSuper.Ref.IsCharged = true;
                pSuper.Ref.Launch(targetCell, true);
                pSuper.Ref.IsCharged = false;


                if (pOwnerRef.TryGet(out TechnoExt pTargetExt))
                {
                    if (pTargetExt.GameObject.GetComponent(BlackHoleLauncherDecorator.ID) == null)
                    {
                        pTargetExt.GameObject.CreateScriptComponent(nameof(BlackHoleLauncherDecorator),BlackHoleLauncherDecorator.ID, "BlackHoleLauncherDecorator Decorator", pTargetExt, location);
                    }
                }
            }



        }
    }





    [Serializable]
    public class BlackHoleTargetDecorator : TechnoScriptable
    {
        public static int ID = 514001;
        public BlackHoleTargetDecorator(TechnoExt owner, TechnoExt self, CoordStruct center):base(owner)
        {
            Owner.Set(owner);
            Self.Set(self);
            this.center = center;
        }

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisibleAll");
        static Pointer<WarheadTypeClass> killWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BhForceKillWh");

        static Pointer<WarheadTypeClass> stopWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SBHStopMoveWH");





        ExtensionReference<TechnoExt> Owner;
        ExtensionReference<TechnoExt> Self;
        CoordStruct center;

        int lifetime = 200;


        public override void OnUpdate()
        {
            if (Owner.Get() == null || Self.Get() == null || lifetime <= 0)
            {
                DetachFromParent();
                return;
            }

            lifetime--;


            var current = Self.Get().OwnerObject.Ref.Base.Base.GetCoords();



            Pointer<BulletClass> spBullet = bulletType.Ref.CreateBullet(Owner.Get().OwnerObject.Convert<AbstractClass>(), Owner.Get().OwnerObject, 1, stopWarhead, 100, false);
            spBullet.Ref.DetonateAndUnInit(current);

            var centerBlackHole = new CoordStruct(center.X, center.Y, center.Z + 5000);

            if (current.Z > center.Z + 1000)
            {
                var damage = 1;
                Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(Owner.Get().OwnerObject.Convert<AbstractClass>(), Owner.Get().OwnerObject, damage, killWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(current);
                Self.Get().OwnerObject.Ref.Base.Remove();
                Self.Get().OwnerObject.Ref.Base.UnInit();
                return;
            }

            var deltaR = 30d;//25d;

            var angle = Math.Atan(((double)current.Y - center.Y) / ((double)current.X - center.X));
            var deltaX = Math.Cos(angle) * deltaR;
            var deltaY = Math.Sin(angle) * deltaR;
            var deltaZ = 0d;

            if (current.Z > center.Z + 1000)
            {
                deltaZ = -25;
            }
            else
            {
                deltaZ = 25;
            }


            if (center.X == current.X)
            {
                deltaX = 0;
            }
            else if (center.X < current.X)
            {
                deltaX = -Math.Abs(deltaX);
            }
            else
            {
                deltaX = Math.Abs(deltaX);
            }

            if (center.Y == current.Y)
            {
                deltaY = 0;
            }
            else if (center.Y < current.Y)
            {
                deltaY = -Math.Abs(deltaY);
            }
            else
            {
                deltaY = Math.Abs(deltaY);
            }

            Self.Get().OwnerObject.Ref.Base.Remove();
            if (!Self.Get().OwnerObject.Ref.Base.Put(new CoordStruct((int)(current.X + deltaX), (int)(current.Y + deltaY), (int)(current.Z + deltaZ)), Direction.N))
            {
                //放回原位也失败的话附近找一个位置放置防止吞单位
                CellSpreadEnumerator enumerator = new CellSpreadEnumerator(2);

                foreach (CellStruct offset in enumerator)
                {
                    var currentCell = CellClass.Coord2Cell(current);
                    CoordStruct where = CellClass.Cell2Coord(currentCell + offset, (int)(current.Z + deltaZ));

                    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                    {
                        var newPos = CellClass.Cell2Coord(pCell.Ref.MapCoords);
                        newPos.Z = (int)(current.Z + deltaZ);
                        if (Self.Get().OwnerObject.Ref.Base.Put(newPos, Direction.N))
                        {
                            break;
                        }
                    }
                }
            }
        }
    }









    [Serializable]
    public class BlackHoleLauncherDecorator : TechnoScriptable
    {
        public static int ID = 514002;
        public BlackHoleLauncherDecorator(TechnoExt self, CoordStruct center) : base(self)
        {
            Self.Set(self);
            this.center = center;
        }


        ExtensionReference<TechnoExt> Self;
        CoordStruct center;

        int lifetime = 300;

        int rof = 0;

        ExtensionReference<TechnoExt> pTargetRef;

        ExtensionReference<TechnoExt> pOwnerRef;

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisibleAll");
        static Pointer<WarheadTypeClass> damageWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SBHDamageWh");

        static Pointer<WarheadTypeClass> killWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BhForceKillWh");


        public override void OnUpdate()
        {
            if (Self.Get() == null || lifetime <= 0)
            {
                DetachFromParent();
                return;
            }

            if (rof-- > 0 && --lifetime > 0)
            {
                return;
            }
            rof = 50;

            var location = center;
            var currentCell = CellClass.Coord2Cell(location);
            var owner = Self.Get();
            if (owner != null)
            {
                var launcher = owner.OwnerObject;

                pOwnerRef.Set(launcher);

                CellSpreadEnumerator enumerator = new CellSpreadEnumerator(6);

                foreach (CellStruct offset in enumerator)
                {
                    CoordStruct where = CellClass.Cell2Coord(currentCell + offset, location.Z);

                    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                    {
                        Point2D p2d = new Point2D(60, 60);
                        Pointer<TechnoClass> target = pCell.Ref.FindTechnoNearestTo(p2d, false, launcher);

                        pTargetRef.Set(TechnoExt.ExtMap.Find(target));
                        if (pTargetRef.TryGet(out TechnoExt pTargetExt))
                        {
                            if (pTargetExt.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Building && pTargetExt.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.BuildingType)
                            {
                                if(SuperBlackHoleBulletScript.immnueToBlackHoles.Contains(pTargetExt.OwnerObject.Ref.Type.Ref.Base.Base.ID))
                                {
                                    continue;
                                }
                                if (pTargetExt.GameObject.GetComponent(BlackHoleTargetDecorator.ID) == null)
                                {
                                    pTargetExt.GameObject.CreateScriptComponent(nameof(BlackHoleTargetDecorator),BlackHoleTargetDecorator.ID, "BlackHoleTargetDecorator Decorator", pOwnerRef.Get(), pTargetExt, location);
                                }
                            }
                        }
                    }
                }


                var damage = 150;
                Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(owner.OwnerObject.Convert<AbstractClass>(), owner.OwnerObject, damage, damageWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(center);

                //消除抛射体的代码，不需要的话以下都注释掉
                ref DynamicVectorClass<Pointer<BulletClass>> bullets = ref BulletClass.Array;
                for (int i = bullets.Count - 1; i >= 0; i--)
                {
                    Pointer<BulletClass> xBullet = bullets.Get(i);
            
                    var bulletLocation = xBullet.Ref.Base.Base.GetCoords();

                    if (xBullet.Ref.Type.Ref.Inviso == true)
                    {
                        continue;
                    }

                    if(bulletLocation.DistanceFrom(center)<= 2560)
                    {
                        Pointer<BulletClass> sBullet = bulletType.Ref.CreateBullet(launcher.Convert<AbstractClass>(), launcher, 1, killWarhead, 100, false);
                        sBullet.Ref.DetonateAndUnInit(bulletLocation);
                        xBullet.Ref.Base.Health = 0;
                        xBullet.Ref.Base.Remove();
                        xBullet.Ref.Base.UnInit();
                    }
                }
            }
        }
    }



}


