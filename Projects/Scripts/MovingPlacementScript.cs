using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(MovingPlacementScript))]
    public class MovingPlacementScript : TechnoScriptable
    {
        public MovingPlacementScript(TechnoExt owner) : base(owner)
        {

        }

        static Pointer<TechnoTypeClass> placementType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("VirtualPlacement");

        TechnoExt buildingReference;

        //CellStruct lastCell;

        public override void OnUpdate()
        {
            if (buildingReference == null || buildingReference.IsNullOrExpired())
            {
                var building = placementType.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();

                var tExt = TechnoExt.ExtMap.Find(building);
                if (tExt != null)
                {
                    buildingReference = (tExt);
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


            if (!buildingReference.IsNullOrExpired())
            {
                buildingReference.OwnerObject.Ref.Base.Remove();

                if (Owner.OwnerObject.Ref.Base.IsOnMap && !Owner.OwnerObject.Ref.Base.InLimbo)
                {
                    if (MapClass.Instance.TryGetCellAt(coord, out var pcell))
                    {
                        var building = pcell.Ref.GetBuilding();
                        if (building.IsNull)
                        {
                            buildingReference.OwnerObject.Ref.Base.Put(Owner.OwnerObject.Ref.Base.Base.GetCoords(), Direction.N);
                        }
                    }
                }
            }

            base.OnUpdate();
        }

        public override void OnRemove()
        {
            if (buildingReference != null && !buildingReference.IsNullOrExpired())
            {
                buildingReference.OwnerObject.Ref.Base.Remove();
                buildingReference.OwnerObject.Ref.Base.UnInit();
            }
            base.OnRemove();
        }
    }
}
