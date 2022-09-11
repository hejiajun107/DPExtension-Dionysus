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

namespace DpLib.Scripts
{
    [Serializable]
    public class CampFireScript : TechnoScriptable
    {
        private bool inited = false;

      
        private int startDelay = 10;

        public CampFireScript(TechnoExt owner) : base(owner)
        {
        }

        private int rof = 200;

        static Pointer<WarheadTypeClass> immnueWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("CampFireBuffWh");

        static Pointer<WarheadTypeClass> breakWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("CampFireBreakWh");
        
        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        ExtensionReference<TechnoExt> tref;


        public override void OnUpdate()
        {
            if (!inited)
            {
                if (startDelay-- <= 0)
                {
                    //搜索附近单位
                    var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                    var currentCell = CellClass.Coord2Cell(location);
                    CellSpreadEnumerator enumeratorSource = new CellSpreadEnumerator(10);


                    foreach (CellStruct offset in enumeratorSource)
                    {
                        CoordStruct where = CellClass.Cell2Coord(currentCell + offset, location.Z);

                        if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                        {
                            if (pCell.IsNull)
                                continue;

                            Point2D p2d = new Point2D(60, 60);
                            Pointer<TechnoClass> ptargetTechno = pCell.Ref.FindTechnoNearestTo(p2d,false,Owner.OwnerObject);

                            if (TechnoExt.ExtMap.Find(ptargetTechno) == null)
                            {
                                continue;
                            }

                            tref.Set(TechnoExt.ExtMap.Find(ptargetTechno));

                            if(tref.TryGet(out var technoExt))
                            {
                                var ptechno = technoExt.OwnerObject;

                                if (ptechno.IsNull)
                                    continue;

                                if (ptechno.Ref.Base.IsOnMap == false || ptechno.Ref.Base.InLimbo || !ptechno.Ref.Base.IsAlive)
                                    continue;

                                if (ptechno.Ref.Owner.IsNull)
                                    continue;

                                var ownerName = ptechno.Ref.Owner.Ref.Type.Ref.Base.ID.ToString();
                                if (ownerName == "Special" || ownerName == "Neutral")
                                    continue;
                                Owner.OwnerObject.Ref.Owner = ptechno.Ref.Owner;
                                Owner.OwnerObject.Ref.Base.UpdatePlacement(PlacementType.Redraw);
                                inited = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (rof-- <= 0)
            {
                rof = 200;
                if (Owner.OwnerObject.Ref.Base.IsAlive && Owner.OwnerObject.Ref.Base.IsOnMap && Owner.OwnerObject.Ref.Base.InLimbo == false)
                {
                    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, immnueWarhead, 100, false);
                    pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

                    Pointer<BulletClass> pBullet2 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, breakWarhead, 100, false);
                    pBullet2.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }
            base.OnUpdate();
        }
    }
}
