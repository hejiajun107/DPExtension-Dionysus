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
    [ScriptAlias(nameof(PassengerChangeHouseScript))]
    public class PassengerChangeHouseScript : TechnoScriptable
    {
       

        private int checkDelay = 20;

        private int lastHouseIndex = -1;

        public PassengerChangeHouseScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnUpdate()
        {
            if (checkDelay-- > 0) 
                return;

            checkDelay = 20;

            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;

            var currentHouse = Owner.OwnerObject.Ref.Owner;

            if (lastHouseIndex == -1)
            {
                lastHouseIndex = currentHouse.Ref.ArrayIndex;
                return;
            }

            if (currentHouse.Ref.ArrayIndex != lastHouseIndex)
            {
                //更改载员所属
                List<Pointer<FootClass>> passengers = new List<Pointer<FootClass>>();

                while (!Owner.OwnerObject.Ref.Passengers.GetFirstPassenger().IsNull)
                {
                    var passenger = Owner.OwnerObject.Ref.Passengers.GetFirstPassenger();
                    if (passenger.Convert<AbstractClass>().CastToTechno(out Pointer<TechnoClass> pPassenger))
                    {
                        passengers.Insert(0, passenger);
                        Owner.OwnerObject.Ref.Passengers.RemoveFirstPassenger();
                        pPassenger.Ref.Owner = currentHouse;
                    }
                }

                foreach(var passenger in passengers)
                {
                    Owner.OwnerObject.Ref.Passengers.AddPassenger(passenger);
                }
            }

            lastHouseIndex = currentHouse.Ref.ArrayIndex;
            base.OnUpdate();
        }
    }
}
