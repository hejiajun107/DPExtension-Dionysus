using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(TiberiumScannerScript))]
    public class TiberiumScannerScript : TechnoScriptable
    {
        public TiberiumScannerScript(TechnoExt owner) : base(owner)
        {
        }

        static Pointer<SuperWeaponTypeClass> revealSW => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("PsychicReveal4Tiberium");

        private int delay = 500;

        private bool actived = false;

        List<CoordStruct> locations = new List<CoordStruct>();


        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!actived)
            {
                actived = true;
                var current = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                locations.Add(current);

                for (var angle = 0; angle < 360; angle += 45)
                {
                    int r = 1280;
                    var pos = new CoordStruct(current.X + (int)(r * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), current.Y + (int)(r * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), current.Z);
                    locations.Add(pos);
                }


                for (var angle = 0; angle < 360; angle += 30)
                {
                    int r = 3840;
                    var pos = new CoordStruct(current.X + (int)(r * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), current.Y + (int)(r * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), current.Z);
                    locations.Add(pos);
                }


                for (var angle = 0; angle < 360; angle += 20)
                {
                    int r = 6400;
                    var pos = new CoordStruct(current.X + (int)(r * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), current.Y + (int)(r * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), current.Z);
                    locations.Add(pos);
                }

                for (var angle = 0; angle < 360; angle += 10)
                {
                    int r = 7680;
                    var pos = new CoordStruct(current.X + (int)(r * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), current.Y + (int)(r * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), current.Z);
                    locations.Add(pos);
                }
            }


            if (delay % 5 == 0)
            {
                if (locations.Count > 0)
                {
                    var location = locations.FirstOrDefault();
                    locations.RemoveAt(0);

                    var cell = CellClass.Coord2Cell(location);

                    CellSpreadEnumerator enumerator = new CellSpreadEnumerator(5);

                    foreach (CellStruct offset in enumerator)
                    {
                        CoordStruct where = CellClass.Cell2Coord(cell + offset, location.Z);

                        if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                        {
                            if (pCell.IsNull)
                            {
                                continue;
                            }

                            bool hasTiberium = pCell.Ref.GetContainedTiberiumValue() > 0;

                            if (hasTiberium)
                            {
                                RevealLocation(where);
                                break;
                            }
                        }
                    }
                }
            }


            if (delay-- <= 0)
            {
                Owner.OwnerObject.Ref.Base.UnInit();
                return;
            }

        }



        private void RevealLocation(CoordStruct location)
        {
            Pointer<HouseClass> pOwner = Owner.OwnerObject.Ref.Owner;
            if (pOwner.IsNull)
            {
                return;
            }
            Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(revealSW);
            CellStruct targetCell = CellClass.Coord2Cell(location);
            pSuper.Ref.IsCharged = true;
            pSuper.Ref.Launch(targetCell, true);
            pSuper.Ref.IsCharged = false;
        }

    }
}
