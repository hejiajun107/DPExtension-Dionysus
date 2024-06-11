using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Experimental
{
    [Serializable]
    [ScriptAlias(nameof(RealAircraftCarrierScript))]
    public class RealAircraftCarrierScript : TechnoScriptable
    {
        private List<AircraftCarrierFighterScript> Docks = new List<AircraftCarrierFighterScript>();

        private CoordStruct[] DockPos;

        public override void Awake()
        {
            var ini = this.CreateRulesIniComponentWith<RealAircraftCarrierData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            


            base.Awake();
        }

        public RealAircraftCarrierScript(TechnoExt owner) : base(owner)
        {
        }


    }



    [Serializable]
    [ScriptAlias(nameof(AircraftCarrierFighterScript))]
    public class AircraftCarrierFighterScript : TechnoScriptable
    {
        public AircraftCarrierFighterStatus Status = AircraftCarrierFighterStatus.Sleeping;

        public AircraftCarrierFighterScript(TechnoExt owner, RealAircraftCarrierScript Parent,int dock) : base(owner)
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }
    }

    [Serializable]
    public enum AircraftCarrierFighterStatus
    {
        Returning,
        Sleeping,
        Working
    }


    public class RealAircraftCarrierData : INIAutoConfig
    {

    }

    public class IntArrToCoord : IParser<CoordStruct>
    {
        public bool Parse(string val, ref CoordStruct buffer)
        {
            if(!string.IsNullOrWhiteSpace(val))
            {
                var arr = val.Split(',');
                if (arr.Length == 3)
                {
                    if (int.TryParse(arr[0],out var x) && int.TryParse(arr[1],out var y) && int.TryParse(arr[2],out var z))
                    {
                        buffer = new CoordStruct(x, y, z);
                        return true;
                    }
                }
            }

            buffer = default;
            return false;
        }
    }



}
