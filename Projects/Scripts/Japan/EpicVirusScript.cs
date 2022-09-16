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
    public class EpicVirusScript : TechnoScriptable
    {
        public EpicVirusScript(TechnoExt owner) : base(owner) { }

        private int liftTime = 2500;

        private int delay = 30;

        static Pointer<BulletTypeClass> expBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("RegenMissile");

        static Pointer<WarheadTypeClass> expWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EpicPoExpWh");


        public override void OnUpdate()
        {
            base.OnUpdate();
            if (liftTime-- <= 0)
            {
                Owner.OwnerObject.Ref.Base.UnInit();
                return;
            }

            if (delay-- <= 0)
            {

                List<TechnoExt> targets = new List<TechnoExt>();

                delay = 30;
                //检测单位

                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                var currentCell = CellClass.Coord2Cell(location);

                CellSpreadEnumerator enumerator = new CellSpreadEnumerator(8);

                foreach (CellStruct offset in enumerator)
                {
                
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

                        tref=(TechnoExt.ExtMap.Find(target));



                        if (!tref.Expired)
                        {
                            if (tref.OwnerObject.Ref.Owner.IsNull)
                            {
                                continue;
                            }
                            if (Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex != tref.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                            {
                                continue;
                            }
                         
                         
                            var id = tref.Type.OwnerObject.Ref.Base.Base.ID.ToString();

                            if(id != "EPVIRUS")
                            {
                                continue;
                            }

                            targets.Add(tref);
                        }


                    }


                    if (targets.Count() >= 4)
                    {
                        int i = 1;
                        //对每个建筑进行操作
                        foreach (var item in targets)
                        {
                            if (!item.Expired)
                            {
                                Explode(item.OwnerObject.Ref.Base.Base.GetCoords(), i);
                                item.OwnerObject.Ref.Base.UnInit();
                                i++;
                            }
                        }

                        Explode(Owner.OwnerObject.Ref.Base.Base.GetCoords(), 0);
                        Owner.OwnerObject.Ref.Base.UnInit();
                    }


                }
            }
           
        }

        private void Explode(CoordStruct location,int delay)
        {
            var cell = CellClass.Coord2Cell(location);
            var bullet = expBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 200, expWarhead, 30, false);

            if (delay == 0)
            {
                bullet.Ref.DetonateAndUnInit(location);
            }
            else
            {
                if (MapClass.Instance.TryGetCellAt(cell, out Pointer<CellClass> pcell))
                {
                    bullet.Ref.SetTarget(pcell.Convert<AbstractClass>());
                    bullet.Ref.MoveTo(location + new CoordStruct(0, 0, delay * 1000), new BulletVelocity(0, 0, 0));
                }
            }
        }

    }
}
