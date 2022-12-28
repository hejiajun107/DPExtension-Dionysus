using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(DragonTankScript))]
    class DragonTankScript : TechnoScriptable
    {
        public DragonTankScript(TechnoExt owner) : base(owner) { }



        static ColorStruct innerColor1 = new ColorStruct(255, 0, 0);
        static ColorStruct outerColor1 = new ColorStruct(128, 0, 0);

        static ColorStruct innerColor2 = new ColorStruct(0, 255, 0);
        static ColorStruct outerColor2 = new ColorStruct(0, 128, 0);

        static ColorStruct outerSpread = new ColorStruct(0, 0, 0);

        private int height = 150;

        TechnoExt pTargetRef;


        static Pointer<BulletTypeClass> bullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ZAFKWH");

        static Pointer<WarheadTypeClass> supWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ZAFKSupAOEWH");

        static Pointer<WarheadTypeClass> fireMultWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ZAFKFireMultWH");

        static Pointer<WarheadTypeClass> animWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ZAFKMockAnimWH");

        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");

        private bool IsMkIIUpdated = false;


        static List<string> supportIds = new List<string>()
        {
           "ZAFKTR"
        };


        private int atkCoolDown = 0;


        public override void OnUpdate()
        {
            if (atkCoolDown > 0)
            {
                atkCoolDown--;
            }
        }



        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (atkCoolDown <= 0 && (weaponIndex == 1 || weaponIndex == 3 || weaponIndex == 5) || IsMkIIUpdated)
            {
                atkCoolDown = 10;

                Pointer<BulletClass> supBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, supWarhead, 100, false);
                supBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

                if(IsMkIIUpdated)
                {
                    Pointer<BulletClass> damageBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, fireMultWh, 100, false);
                    damageBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

                }
                //if (pTarget.CastToTechno(out Pointer<TechnoClass> pTechno))
                //{
                //    var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                //    var currentCell = CellClass.Coord2Cell(location);


                //    Pointer<BulletClass> supBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pTechno, 1, supWarhead, 100, false);
                //    supBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

                //    //CellSpreadEnumerator enumerator = new CellSpreadEnumerator(5);

                //    //foreach (CellStruct offset in enumerator)
                //    //{
                //    //    CoordStruct where = CellClass.Cell2Coord(currentCell + offset, location.Z);


                //    //    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                //    //    {
                //    //        if (pCell.IsNull)
                //    //        {
                //    //            continue;
                //    //        }

                //    //        Point2D p2d = new Point2D(60, 60);
                //    //        Pointer<TechnoClass> target = pCell.Ref.FindTechnoNearestTo(p2d, false, Owner.OwnerObject);


                //    //        if (TechnoExt.ExtMap.Find(target) == null)
                //    //        {
                //    //            continue;
                //    //        }

                //    //        TechnoExt tref = default;

                //    //        tref.Set(TechnoExt.ExtMap.Find(target));

                //    //        if (tref.TryGet(out TechnoExt ptower))
                //    //        {
                //    //            if (ptower.OwnerObject.Ref.Owner.IsNull)
                //    //            {
                //    //                continue;
                //    //            }
                //    //            if (Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex != ptower.OwnerObject.Ref.Owner.Ref.ArrayIndex && !Owner.OwnerObject.Ref.Owner.Ref.IsAlliedWith(ptower.OwnerObject.Ref.Owner))
                //    //            {
                //    //                continue;
                //    //            }

                //    //            var id = ptower.Type.OwnerObject.Ref.Base.Base.ID.ToString();
                //    //            if (!supportIds.Contains(id))
                //    //            {
                //    //                continue;
                //    //            }

                //    //            Pointer<BulletClass> supBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pTechno, 1, supWarhead, 100, false);
                //    //            supBullet.Ref.DetonateAndUnInit(ptower.OwnerObject.Ref.Base.Base.GetCoords());
                //    //        }
                //    //    }
                //    //}

                //}
            }
        }


        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
        Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (IsMkIIUpdated == false)
            {
                //判断是否来自升级弹头
                if (pWH.Ref.Base.ID.ToString() == "MarkIISpWh")
                {
                    IsMkIIUpdated = true;
                    Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                    CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                    Pointer<BulletClass> mk2bullet = bullet.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, mk2Warhead, 100, false);
                    mk2bullet.Ref.DetonateAndUnInit(currentLocation);
                }
            }
        }


    }

}
