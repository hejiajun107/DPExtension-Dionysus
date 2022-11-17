using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(TiberiumInfestScript))]
    public class TiberiumInfestScript : TechnoScriptable
    {
        public TiberiumInfestScript(TechnoExt owner) : base(owner)
        {
        }

        private int delay = 500;

        private bool actived = false;

        private List<CoordStruct> locations = new List<CoordStruct>();

        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private static Pointer<WarheadTypeClass> expWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("TiberInfestWh");

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!actived)
            {
                actived = true;
                var current = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var height = Owner.OwnerObject.Ref.Base.GetHeight();

                var cell = CellClass.Coord2Cell(current);

                CellSpreadEnumerator enumerator = new CellSpreadEnumerator(8);

                foreach (CellStruct offset in enumerator)
                {
                    CoordStruct where = CellClass.Cell2Coord(cell + offset, current.Z - height);

                    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                    {
                        if (pCell.IsNull)
                        {
                            continue;
                        }

                        bool hasTiberium = pCell.Ref.GetContainedTiberiumValue() > 0;

                        if (hasTiberium)
                        {
                            locations.Add(where);
                        }
                    }
                }
            }
            else
            {
                if (locations.Count > 0)
                {
                    var location = locations[0];
                    locations.RemoveAt(0);

                    var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 75, expWarhead, 100, true);
                    pBullet.Ref.DetonateAndUnInit(location);
                }
            }



            if (delay-- <= 0)
            {
                Owner.OwnerObject.Ref.Base.UnInit();
                locations.Clear();
                return;
            }

        }
    }
}
