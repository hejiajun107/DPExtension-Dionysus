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

namespace DpLib.Scripts.Japan
{
    [Serializable]

    public class NanoLinkScript : TechnoScriptable
    {
        public NanoLinkScript(TechnoExt owner) : base(owner) { }

        private int delay = 50;

        private int lifeTime = 1000;

        private bool isActived = false;
        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChaosUnitKillWh");

        
        private TechnoExt pTargetRef;

        public override void OnUpdate()
        {
            if (delay-- > 0)
                return;

            if (lifeTime-- <= 0)
            {
                //销毁
                var damage = 10;
                Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, warhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                return;
            }

            if(!isActived)
            {
                //建立连接

                CellSpreadEnumerator enumerator = new CellSpreadEnumerator(6);

                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var currentCell = CellClass.Coord2Cell(location);

                foreach (CellStruct offset in enumerator)
                {
                    CoordStruct where = CellClass.Cell2Coord(currentCell + offset, location.Z);

                    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                    {
                        Point2D p2d = new Point2D(60, 60);
                        Pointer<TechnoClass> target = pCell.Ref.FindTechnoNearestTo(p2d, false, Owner.OwnerObject);

                        pTargetRef = TechnoExt.ExtMap.Find(target);
                        if (pTargetRef!=null && !pTargetRef.IsNullOrExpired())
                        {
                            if (pTargetRef.OwnerObject.Ref.Base.Base.WhatAmI() == AbstractType.Building)
                                continue;

                            if (pTargetRef.OwnerObject.Ref.Owner.Ref.ArrayIndex != Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex && !Owner.OwnerObject.Ref.Owner.Ref.IsAlliedWith(pTargetRef.OwnerObject.Ref.Owner.Ref.ArrayIndex))
                                continue;
                            var id = pTargetRef.Type.OwnerObject.Ref.Base.Base.ID.ToString();
                            if (id == "NANOLK")
                                continue;
                            if (pTargetRef.GameObject.GetComponent(LinkedTechnoDecorator.ID) == null)
                            {
                                pTargetRef.GameObject.CreateScriptComponent(nameof(LinkedTechnoDecorator),LinkedTechnoDecorator.ID, "LinkedTechnoDecorator Decorator", pTargetRef, Owner);
                            }
                        }
                    }
                }
                isActived = true;
            }
        }

    }


    [Serializable]
    public class LinkedTechnoDecorator : TechnoScriptable
    {
        public static int ID = 514003;
        public LinkedTechnoDecorator(TechnoExt self, TechnoExt target) : base(self)
        {
            Self = self;
            Target = target;
        }


        TechnoExt Self;
        TechnoExt Target;

        private static ColorStruct innerColor = new ColorStruct(255, 0, 128);
        private static ColorStruct outerColor = new ColorStruct(255, 0, 128);
        private static ColorStruct outerSpread = new ColorStruct(10, 10, 10);

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChaosDamageWh");

        static Pointer<WarheadTypeClass> immnueWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("NanoImmnueWh");

        private int rof = 0;

        public override void OnUpdate()
        {
            if (Self.IsNullOrExpired())
            {
                DetachFromParent();
                return;
            }

            if (Target.IsNullOrExpired())
            {
                DetachFromParent();
                return;
            }

            var selfLocation = Self.OwnerObject.Ref.Base.Base.GetCoords(); 
            var targetLocaton = Target.OwnerObject.Ref.Base.Base.GetCoords();
            //超过一定距离中断连接
            if (selfLocation.DistanceFrom(targetLocaton) > 2560)
            {
                DetachFromParent();
                return;
            }

            if (rof-- < 0)
            {
                rof = 20;
                if (!Self.IsNullOrExpired())
                {
                    var damage = 1;
                    Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(Self.OwnerObject.Convert<AbstractClass>(), Self.OwnerObject, damage, immnueWarhead, 100, false);
                    pBullet.Ref.DetonateAndUnInit(Self.OwnerObject.Ref.Base.Base.GetCoords());

                }

            }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if(!Self.IsNullOrExpired())
            {
                if(!Target.IsNullOrExpired())
                {
                    if (pAttackingHouse.IsNull)
                    {
                        return;
                    }
                    //if (pAttackingHouse.Ref.ArrayIndex == pself.OwnerObject.Ref.Owner.Ref.ArrayIndex || pself.OwnerObject.Ref.Owner.Ref.IsAlliedWith(pAttackingHouse))
                    //{
                    //    return;
                    //}

                    if (pDamage.Ref <= 1)
                    {
                        return;
                    }


                    var realDamage = MapClass.GetTotalDamage(pDamage.Ref, pWH, Self.OwnerObject.Ref.Type.Ref.Base.Armor, DistanceFromEpicenter) / 2;

                    Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(Self.OwnerObject.Convert<AbstractClass>(), Self.OwnerObject, realDamage, warhead, 100, true);
                    pBullet.Ref.DetonateAndUnInit(Target.OwnerObject.Ref.Base.Base.GetCoords());

                    var skyCoord = Self.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 200);

                    Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(Self.OwnerObject.Ref.Base.Base.GetCoords(), skyCoord, innerColor, outerColor, outerSpread, 2);
                    Pointer<LaserDrawClass> pLaser2 = YRMemory.Create<LaserDrawClass>(Target.OwnerObject.Ref.Base.Base.GetCoords(), skyCoord, innerColor, outerColor, outerSpread, 2);


                }
            }

         



           
        }


    }



}
