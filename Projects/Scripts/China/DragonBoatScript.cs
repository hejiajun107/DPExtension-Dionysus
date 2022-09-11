using DynamicPatcher;
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
    class DragonBoatScript : TechnoScriptable
    {
        public DragonBoatScript(TechnoExt owner) : base(owner) { }



        static ColorStruct innerColor1 = new ColorStruct(255, 0, 0);
        static ColorStruct outerColor1 = new ColorStruct(128, 0, 0);

        static ColorStruct innerColor2 = new ColorStruct(0, 255, 0);
        static ColorStruct outerColor2 = new ColorStruct(0, 128, 0);

        static ColorStruct outerSpread = new ColorStruct(0, 0, 0);

        private int height = 150;

        ExtensionReference<TechnoExt> pTargetRef;


        static Pointer<BulletTypeClass> bullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ZAFKWH");

        static Pointer<WarheadTypeClass> supWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DBoatSupWH");

        static Pointer<WarheadTypeClass> animWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ZAFKMockAnimWH");

        static List<string> supportIds = new List<string>()
        {
           "HSZJ"
        };


        private int atkCoolDown = 0;

        private int coolDown = 0;

        public override void OnUpdate()
        {
            if (coolDown > 0)
            {
                coolDown--;
            }

            if (atkCoolDown > 0)
            {
                atkCoolDown--;
            }
        }



        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (atkCoolDown <= 0 && weaponIndex == 1)
            {

                DrawLaser(Owner.OwnerObject.Ref.Base.Base.GetCoords(), pTarget.Ref.GetCoords(), innerColor1, outerColor1);
                Pointer<BulletClass> damageBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 8, warhead, 100, false);
                damageBullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
                atkCoolDown = 10;




                if (pTarget.CastToTechno(out Pointer<TechnoClass> pTechno))
                {
                    var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                    Pointer<BulletClass> supBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pTechno, 1, supWarhead, 100, false);
                    supBullet.Ref.DetonateAndUnInit(location);

                    //var currentCell = CellClass.Coord2Cell(location);

                    //CellSpreadEnumerator enumerator = new CellSpreadEnumerator(6);

                    //foreach (CellStruct offset in enumerator)
                    //{
                    //    CoordStruct where = CellClass.Cell2Coord(currentCell + offset, location.Z);


                    //    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                    //    {
                    //        if (pCell.IsNull)
                    //        {
                    //            continue;
                    //        }

                    //        Point2D p2d = new Point2D(60, 60);
                    //        Pointer<TechnoClass> target = pCell.Ref.FindTechnoNearestTo(p2d, false, Owner.OwnerObject);


                    //        if (TechnoExt.ExtMap.Find(target) == null)
                    //        {
                    //            continue;
                    //        }

                    //        ExtensionReference<TechnoExt> tref = default;

                    //        tref.Set(TechnoExt.ExtMap.Find(target));

                    //        if (tref.TryGet(out TechnoExt ptower))
                    //        {
                    //            if (ptower.OwnerObject.Ref.Owner.IsNull)
                    //            {
                    //                continue;
                    //            }
                    //            if (Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex != ptower.OwnerObject.Ref.Owner.Ref.ArrayIndex && !Owner.OwnerObject.Ref.Owner.Ref.IsAlliedWith(ptower.OwnerObject.Ref.Owner))
                    //            {
                    //                continue;
                    //            }

                    //            var id = ptower.Type.OwnerObject.Ref.Base.Base.ID.ToString();
                    //            if (!supportIds.Contains(id))
                    //            {
                    //                continue;
                    //            }

                    //            Pointer<BulletClass> supBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pTechno, 1, supWarhead, 100, false);
                    //            supBullet.Ref.DetonateAndUnInit(ptower.OwnerObject.Ref.Base.Base.GetCoords());
                    //        }
                    //    }
                    //}

                }
            }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (coolDown <= 0)
            {
                //if (Owner.OwnerObject.Ref.Target.IsNull)
                //{
                    if (pWH.Ref.Base.ID == supWarhead.Ref.Base.ID)
                    {
                        DrawLaser(Owner.OwnerObject.Ref.Base.Base.GetCoords(), pAttacker.Ref.Base.GetCoords(), innerColor2, outerColor2);
                        Pointer<BulletClass> damageBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 6, warhead, 100, false);
                        damageBullet.Ref.DetonateAndUnInit(pAttacker.Ref.Base.GetCoords());

                        Pointer<BulletClass> animBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, animWarhead, 100, false);
                        animBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, height));

                        coolDown = 10;
                    }
                //}
            }
        }


        private void DrawLaser(CoordStruct from, CoordStruct to, ColorStruct innerColor, ColorStruct outerColor)
        {
            CoordStruct center = from + new CoordStruct(0, 0, height);
            CoordStruct focus = center + new CoordStruct(0, 0, 150);

            //var radius = 100;

            //for (var angle = -180 - 45; angle < 180 - 45; angle += 90)
            //{
            //    var pos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z);
            //    Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(pos, focus, innerColor, outerColor, outerSpread, 15);
            //}

            Pointer<LaserDrawClass> pLaser2 = YRMemory.Create<LaserDrawClass>(focus, to, innerColor, outerColor, outerSpread, 15);
            pLaser2.Ref.IsHouseColor = true;
            pLaser2.Ref.Thickness = 2;

        }

    }

}
