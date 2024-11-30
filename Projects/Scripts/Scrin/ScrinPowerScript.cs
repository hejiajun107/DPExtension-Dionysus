using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Scrin
{
    [ScriptAlias(nameof(ScrinPowerScript))]
    [Serializable]
    public class ScrinPowerScript : TechnoScriptable
    {
        public ScrinPowerScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            var mission = Owner.OwnerObject.Convert<MissionClass>();

            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);
                TechnoPlacer.PlaceTechnoNear(TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("RAPOWRUP"), Owner.OwnerObject.Ref.Owner, CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords()));
            }
            else if (mission.Ref.CurrentMission == Mission.Selling)
            {
                var passenger = Owner.OwnerObject.Ref.Passengers.GetFirstPassenger();
                if (!passenger.IsNull)
                {
                    passenger.Ref.Base.Base.UnInit();
                }
            }
        }
    }


}
