using DynamicPatcher;
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
    public class EKingScript : TechnoScriptable
    {
        public EKingScript(TechnoExt owner) : base(owner) { }
        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");

        static Pointer<BulletTypeClass> shootBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("EkingSeeker");
        static Pointer<WarheadTypeClass> shootWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EkingHealShWh");

        static Pointer<WarheadTypeClass> damageWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EkingDamageAP");

        static Pointer<WarheadTypeClass> healthWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("KOGHEALWarhead");
        

        private bool IsMkIIUpdated = false;

        private int FireKeepTime = 0;

        private int rof = 0;

        public override void OnUpdate()
        {
            if (!IsMkIIUpdated)
            {
                return;
            }

            if (FireKeepTime <= 0)
            {
                return;
            }

            FireKeepTime--;

            if (rof-- > 0)
            {
                return;
            }

            rof = 100;

            int count = 0;
            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var currentCell = CellClass.Coord2Cell(location);

            CellSpreadEnumerator enumerator = new CellSpreadEnumerator(4);

            foreach (CellStruct offset in enumerator)
            {
                if (count >= 2)
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
                    Pointer<TechnoClass> target = pCell.Ref.FindTechnoNearestTo(p2d, false, Owner.OwnerObject);


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

                        if (ptechno.OwnerObject.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex))
                        {
                            continue;
                        }

                        if(ptechno.OwnerObject.Ref.Base.IsVisible == false || ptechno.OwnerObject.Ref.Base.InLimbo==true || ptechno.OwnerObject.Ref.Base.IsAlive == false)
                        {
                            continue;
                        }

                        Pointer<BulletClass> dbullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 50, damageWarhead, 30, true);
                        dbullet.Ref.DetonateAndUnInit(ptechno.OwnerObject.Ref.Base.Base.GetCoords());

                        Pointer<BulletClass> bullet = shootBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 50, shootWarhead, 30, true);
                        bullet.Ref.MoveTo(ptechno.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0,0,150), new BulletVelocity(0, 0, 0));
                        bullet.Ref.SetTarget(Owner.OwnerObject.Convert<AbstractClass>());
                        count++;
                    }
                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(weaponIndex==0 && (pTarget.Ref.WhatAmI() == AbstractType.Unit || pTarget.Ref.WhatAmI() == AbstractType.Infantry || pTarget.Ref.WhatAmI() == AbstractType.Building))
            {
                if (pTarget.CastToTechno(out var ptech))
                {
                    var bullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), ptech, 45, healthWarhead, 100, false);
                    bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }
            if (IsMkIIUpdated)
            {
                FireKeepTime = 250;
            }
            base.OnFire(pTarget, weaponIndex);
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
                    Pointer<BulletClass> mk2bullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, mk2Warhead, 50, false);
                    mk2bullet.Ref.DetonateAndUnInit(currentLocation);

                }
            }
            else
            {
                FireKeepTime = 200;
            }
        }

    }
}