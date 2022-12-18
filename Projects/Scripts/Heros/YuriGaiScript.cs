
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(YuriGaiScript))]

    public class YuriGaiScript : TechnoScriptable
    {
        public YuriGaiScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner, 8);
        }

        private ManaCounter _manaCounter;



        Random random = new Random(124446);

        private List<string> BruteNameList = new List<string>()
        {
            "BRUTE","GREENS"
        };


        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BlackHoleStopMoveWH");

        static Pointer<SuperWeaponTypeClass> swTower => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("UnitDeliveryYuriGaiTower");

        TechnoExt pTargetRef;

        static Pointer<WarheadTypeClass> makeBruteWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("GRUNTWH");
        static Pointer<WarheadTypeClass> showBruteWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("GRUNTWHNone");
        //static Pointer<WarheadTypeClass> makeBruteWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("AP");
        //static Pointer<WarheadTypeClass> showBruteWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("AP");

        static Pointer<WarheadTypeClass> chaosWarheads => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChaosWaveWh");

        static Pointer<WarheadTypeClass> ChaosFeedbackWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChaosFbWh");
        

        public override void OnUpdate()
        {
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 0)
            {
                //if (_manaCounter.Cost(100))
                //{
                //    CreateTower();
                //}
            }
            else if (weaponIndex == 1)
            {
                var bullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 300, chaosWarheads, 100, false);
                bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                CreateBrute();
            }

        }



        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
      Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if(pAttacker.IsNull || pAttackingHouse.IsNull)
            {
                return;
            }

            if(pAttackingHouse.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
            {
                return;
            }

            if (_manaCounter.Cost(8))
            {
                var bullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 50, ChaosFeedbackWh, 100, false);
                bullet.Ref.DetonateAndUnInit(pAttacker.Ref.Base.GetCoords());
            }
        }

        private void CreateBrute()
        {
            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var currentCell = CellClass.Coord2Cell(location);
            CellSpreadEnumerator enumerator = new CellSpreadEnumerator(4);

            List<TechnoExt> targets = new List<TechnoExt>();
            List<int> codes = new List<int>();

            //检索狂兽人
            foreach (CellStruct offset in enumerator)
            {
                if (targets.Count() >= 6)
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

                    TechnoExt tref = default;

                    tref = (TechnoExt.ExtMap.Find(target));

                    if (!tref.IsNullOrExpired())
                    {
                        if (tref.OwnerObject.Ref.Owner.IsNull)
                        {
                            continue;
                        }
                        if (Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex != tref.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                        {
                            continue;
                        }

                        var hashCode = tref.OwnerObject.GetHashCode();
                        var id = tref.Type.OwnerObject.Ref.Base.Base.ID.ToString();

                        if (!BruteNameList.Contains(id))
                        {
                            continue;
                        }

                        if (!codes.Where(c => c == hashCode).Any())
                        {
                            targets.Add(tref);
                            codes.Add(hashCode);
                        }
                    }
                }

            }

            //处理狂兽人
            if (targets.Count() >= 3)
            {
                if (_manaCounter.Cost(100))
                {
                    int makeCount = (int)Math.Floor(targets.Count() / 3d);


                    var index = 0;
                    for (index = 0; index < makeCount; index++)
                    {
                        var currentBrute = targets[index];
                        if (!currentBrute.IsNullOrExpired())
                        {
                            var targetCoord = currentBrute.OwnerObject.Ref.Base.Base.GetCoords();
                            Pointer<BulletClass> pbullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1000, makeBruteWarhead, 100, false);
                            pbullet.Ref.DetonateAndUnInit(targetCoord);
                        }
                    }

                    for (index = makeCount; index < makeCount + makeCount * 2; index++)
                    {
                        var currentBrute = targets[index];
                        if (!currentBrute.IsNullOrExpired())
                        {
                            var targetCoord = currentBrute.OwnerObject.Ref.Base.Base.GetCoords();
                            Pointer<BulletClass> pbullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1000, showBruteWarhead, 100, false);
                            pbullet.Ref.DetonateAndUnInit(targetCoord);
                        }
                    }

                }
            }

        }

        private void CreateTower()
        {
            Pointer<TechnoClass> pTechno = Owner.OwnerObject;
            Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
            Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(swTower);
            CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            pSuper.Ref.IsCharged = true;
            pSuper.Ref.Launch(targetCell, true);
            pSuper.Ref.IsCharged = false;
        }


    }


}

