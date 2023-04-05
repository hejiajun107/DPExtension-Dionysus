using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(WornHoleBuildingScript))]
    public class WornHoleBuildingScript : TechnoScriptable
    {
        public WornHoleBuildingScript(TechnoExt owner) : base(owner)
        {
        }

        //private bool inited = false;

        //TechnoExt center;
        private int delay = 100;


        public override void OnUpdate()
        {
            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            List<Pointer<FootClass>> passengers = new List<Pointer<FootClass>>();

            while (!Owner.OwnerObject.Ref.Passengers.GetFirstPassenger().IsNull)
            {
                var passenger = Owner.OwnerObject.Ref.Passengers.GetFirstPassenger();
                if (passenger.Convert<AbstractClass>().CastToTechno(out Pointer<TechnoClass> pPassenger))
                {
                    Logger.Log(passenger.Ref.BaseMission.CurrentMission);
                    Logger.Log(pPassenger.Ref.Base.InLimbo);

                    passenger.Ref.BaseMission.ForceMission(Mission.Stop);
                    passengers.Insert(0, passenger);
                }
                Owner.OwnerObject.Ref.Passengers.RemoveFirstPassenger();
            }

            foreach (var passenger in passengers)
            {
                Owner.OwnerObject.Ref.Passengers.AddPassenger(passenger);
            }
        }



    }
}
