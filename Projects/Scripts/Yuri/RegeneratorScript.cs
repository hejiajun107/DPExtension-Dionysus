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

namespace DpLib.Scripts.Yuri
{
    [Serializable]
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

                        ExtensionReference<TechnoExt> tref = default;

                        tref.Set(TechnoExt.ExtMap.Find(target));

                        if (tref.TryGet(out TechnoExt ptechno))
                        {
                            if (ptechno.OwnerObject.Ref.Owner.IsNull)
                                continue;

                            if (ptechno.OwnerObject.Ref.Owner != Owner.OwnerObject.Ref.Owner)
                                continue;

                            if (ptechno.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Infantry)
                                continue;

                            if (ptechno.OwnerObject.Ref.Type.Ref.Size != 1)
                                continue;

                            if (ptechno.OwnerObject.Ref.Base.IsOnMap == false || ptechno.OwnerObject.Ref.Base.InLimbo == true)
                                continue;

                            var pmission = ptechno.OwnerObject.Convert<MissionClass>();

                            ptechno.OwnerObject.Ref.Base.Remove();

                            if (pmission.Ref.CurrentMission == Mission.Enter)
                                continue;

                            pmission.Ref.ForceMission(Mission.Stop);

                            Owner.OwnerObject.Ref.Passengers.AddPassenger(ptechno.OwnerObject.Convert<FootClass>());

                            Count++;
                        }
                    }
                }
            }
        }
    }
}
