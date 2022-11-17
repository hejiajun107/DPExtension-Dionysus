using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;

namespace DpLib.Scripts.Yuri
{
    [Serializable]
    [ScriptAlias(nameof(RegeneratorScript))]
    public class RegeneratorScript : TechnoScriptable
    {
        public RegeneratorScript(TechnoExt owner) : base(owner)
        {
        }

        public int checkDelay = 50;

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                checkDelay = 500;
            }

            if (checkDelay-- <= 0)
            {
                checkDelay = 50;
                //吸收附近的乘客

                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                var currentCell = CellClass.Coord2Cell(location);

                CellSpreadEnumerator enumerator = new CellSpreadEnumerator(2);

                int Count = 0;

                foreach (CellStruct offset in enumerator)
                {
                    if (Owner.OwnerObject.Ref.Passengers.GetTotalSize() >= 15 || Count >= 5)
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
                                continue;

                            if (tref.OwnerObject.Ref.Owner != Owner.OwnerObject.Ref.Owner)
                                continue;

                            if (tref.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Infantry)
                                continue;

                            if (tref.OwnerObject.Ref.Type.Ref.Size != 1)
                                continue;

                            if (tref.OwnerObject.Ref.Base.IsOnMap == false || tref.OwnerObject.Ref.Base.InLimbo == true)
                                continue;

                            var pmission = tref.OwnerObject.Convert<MissionClass>();

                            tref.OwnerObject.Ref.Base.Remove();

                            if (pmission.Ref.CurrentMission == Mission.Enter)
                                continue;

                            pmission.Ref.ForceMission(Mission.Stop);

                            Owner.OwnerObject.Ref.Passengers.AddPassenger(tref.OwnerObject.Convert<FootClass>());

                            Count++;
                        }
                    }
                }
            }
        }
    }
}
