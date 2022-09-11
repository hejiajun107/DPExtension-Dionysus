using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{
    [Serializable]
    public class MovingPlacementScript : TechnoScriptable
    {
        public MovingPlacementScript(TechnoExt owner) : base(owner)
        {

        }

        static Pointer<TechnoTypeClass> placementType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("VirtualPlacement");

        ExtensionReference<TechnoExt> buildingReference;

        //CellStruct lastCell;

        public override void OnUpdate()
        {
            if (!buildingReference.TryGet(out var technoExt))
            {
                var building = placementType.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();

                var tExt = TechnoExt.ExtMap.Find(building);
                if (tExt != null)
                {
                    buildingReference.Set(tExt);
                }
            }

            var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            var cell = CellClass.Coord2Cell(coord);

            //if (lastCell == default)
            //{
            //    lastCell = cell;
            //}
            //else
            //{
            //    if (lastCell == cell)
            //    {
            //        return;
            //    }
            //    lastCell = cell;
            //}


            if (buildingReference.TryGet(out var technoExt2))
            {
                technoExt2.OwnerObject.Ref.Base.Remove();

                if (Owner.OwnerObject.Ref.Base.IsOnMap && !Owner.OwnerObject.Ref.Base.InLimbo)
                {
                    if (MapClass.Instance.TryGetCellAt(coord, out var pcell))
                    {
                        var building = pcell.Ref.GetBuilding();
                        if (building.IsNull)
                        {
                            technoExt2.OwnerObject.Ref.Base.Put(Owner.OwnerObject.Ref.Base.Base.GetCoords(), Direction.N);
                        }
                    }
                }
            }

            base.OnUpdate();
        }

        public override void OnRemove()
        {
            if (buildingReference.TryGet(out var technoExt))
            {
                technoExt.OwnerObject.Ref.Base.Remove();
                technoExt.OwnerObject.Ref.Base.UnInit();
            }
            base.OnRemove();
        }
    }
}
