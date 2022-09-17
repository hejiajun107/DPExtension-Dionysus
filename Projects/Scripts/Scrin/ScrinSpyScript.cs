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

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    public class ScrinSpyScript : TechnoScriptable
    {
        public ScrinSpyScript(TechnoExt owner) : base(owner)
        {
        }

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> transWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ExileTransWh");
        static Pointer<WarheadTypeClass> arriveWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ExileGoWh");


        private int delay = 0;
        public override void OnUpdate()
        {
            if (delay <= 0)
                return;

            delay--;
            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);

            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;

            if (weaponIndex != 1)
                return;

            if (delay > 0)
                return;

            var myLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            //寻找基地
            var cnst = Finder.FineOneTechno(Owner.OwnerObject.Ref.Owner, x => x.Ref.Type.Ref.Base.Base.ID == "RACNST" && x.Ref.Base.IsOnMap,FindRange.Owner);

            if (!cnst.IsNullOrExpired())
            {
                //在基地附近寻找空地
                var cnstLocation = cnst.OwnerObject.Ref.Base.Base.GetCoords();
                var currentCell = CellClass.Coord2Cell(cnstLocation);
                CellSpreadEnumerator enumeratorTarget = new CellSpreadEnumerator(6);

                foreach (CellStruct offset in enumeratorTarget)
                {
                    CoordStruct where = CellClass.Cell2Coord(currentCell + offset, cnstLocation.Z);

                    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                    {
                        if (pCell.IsNull)
                        {
                            continue;
                        }

                        Point2D p2d = new Point2D(60, 60);
                        Pointer<TechnoClass> ptargetTechno = pCell.Ref.FindTechnoNearestTo(p2d, false, Owner.OwnerObject);

                        if (TechnoExt.ExtMap.Find(ptargetTechno) == null)
                        {
                            var targetLocation = pCell.Ref.Base.GetCoords();
                            if(Owner.OwnerObject.Ref.Base.IsOnMap)
                            {
                                Owner.OwnerObject.Ref.Base.Remove();
                            }

                            if(Owner.OwnerObject.Ref.Base.Put(targetLocation, Direction.N))
                            {
                                var bullet1 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, transWarhead, 100, false);
                                var bullet2 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, arriveWarhead, 100, false);

                                bullet1.Ref.DetonateAndUnInit(myLocation);
                                bullet2.Ref.DetonateAndUnInit(targetLocation);
                                delay = 3000;
                                break;
                            }
                            else
                            {
                                continue;
                            } 
                        }
                    }
                }



            }


            //折越失败直接清理掉该单位
            if(!Owner.OwnerObject.Ref.Base.IsOnMap)
            {
                Owner.OwnerObject.Ref.Base.UnInit();
            }




        }
    }
}
