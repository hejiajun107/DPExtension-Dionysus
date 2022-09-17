using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(AircraftScript))]
    public class AircraftScript : TechnoScriptable
    {
        public AircraftScript(TechnoExt owner) : base(owner) { }

        private bool isAreaProtecting = false;

        private CoordStruct areaProtectTo;

        private Random random = new Random(30021);

        public override void OnUpdate()
        {
            base.OnUpdate();

            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (Owner.OwnerObject.CastToFoot(out Pointer<FootClass> pfoot))
            {
                if (!isAreaProtecting)
                {
                    if (mission.Ref.CurrentMission == Mission.Area_Guard)
                    {
                        isAreaProtecting = true;

                        CoordStruct dest = pfoot.Ref.Locomotor.Destination();
                        areaProtectTo = dest;
                    }
                }


                if (isAreaProtecting)
                {
                    if (mission.Ref.CurrentMission == Mission.Move || mission.Ref.CurrentMission == Mission.Attack)
                    {
                        isAreaProtecting = false;
                        return;
                    }
                    else if (mission.Ref.CurrentMission == Mission.Enter)
                    {
                        //预留有可能回不了机场要做些处理，目前没发现问题
                        ;
                    }


                    if (areaProtectTo != null)
                    {
                       
                        var dest = areaProtectTo;

                        if (areaProtectTo.DistanceFrom(Owner.OwnerObject.Ref.Base.Base.GetCoords()) <= 2000)
                        {
                            dest += new CoordStruct(random.Next(-300, 300), random.Next(-300, 300), 0);
                        }

                        pfoot.Ref.Locomotor.Move_To(dest);
                        var cell = CellClass.Coord2Cell(dest);
                        if (MapClass.Instance.TryGetCellAt(cell, out Pointer<CellClass> pcell))
                        {
                            Owner.OwnerObject.Ref.SetDestination(pcell.Convert<AbstractClass>(), false);
                        }

                    }
                }
            }

        }
    }
}
