
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(YuriPrimeScript))]

    public class YuriPrimeScript : TechnoScriptable
    {
        public YuriPrimeScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner);
        }

        private ManaCounter _manaCounter;



        Random random = new Random(133142);

        TechnoExt pTargetRef;



        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SuperPsiPulse2");

        static Pointer<WarheadTypeClass> immnueWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("WaveImmnueWH");



        public override void OnUpdate()
        {
            //_manaCounter.OnUpdate(Owner);
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 1)
            {
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var currentCell = CellClass.Coord2Cell(location);

                Pointer<BulletClass> pBullet1 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, immnueWarhead, 100, false);
                pBullet1.Ref.DetonateAndUnInit(location);


                CellSpreadEnumerator enumerator = new CellSpreadEnumerator(6);

                foreach (CellStruct offset in enumerator)
                {
                    CoordStruct where = CellClass.Cell2Coord(currentCell + offset, pTarget.Ref.GetCoords().Z);

                    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                    {
                        Point2D p2d = new Point2D(60, 60);
                        Pointer<TechnoClass> target = pCell.Ref.FindTechnoNearestTo(p2d, false, Owner.OwnerObject);

                        pTargetRef = TechnoExt.ExtMap.Find(target);
                        if (!pTargetRef.IsNullOrExpired())
                        {
                            if (pTargetRef.OwnerObject.Ref.Owner.Ref.ArrayIndex == Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                            {
                                if (pTargetRef.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Building && pTargetRef.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.BuildingType)
                                {
                                    if (_manaCounter.Cost(30))
                                    {
                                        Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 150, warhead, 100, false);
                                        pBullet.Ref.DetonateAndUnInit(pTargetRef.OwnerObject.Ref.Base.Base.GetCoords());
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

