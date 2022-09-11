using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.Soviet
{
    [Serializable]
    public class SovietParaDropPlaneScript : TechnoScriptable
    {
        public SovietParaDropPlaneScript(TechnoExt owner) : base(owner)
        {
        }

        private bool inited;

        public override void OnUpdate()
        {
            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;


            if (!inited)
            {
                if (Owner.OwnerObject.Ref.Passengers.NumPassengers > 0)
                {
                    inited = true;
                    //更改载员所属
                    List<Pointer<FootClass>> passengers = new List<Pointer<FootClass>>();

                    while (!Owner.OwnerObject.Ref.Passengers.GetFirstPassenger().IsNull)
                    {
                        var passenger = Owner.OwnerObject.Ref.Passengers.GetFirstPassenger();
                        if (passenger.Convert<AbstractClass>().CastToTechno(out Pointer<TechnoClass> pPassenger))
                        {
                            passengers.Insert(0, passenger);
                            Owner.OwnerObject.Ref.Passengers.RemoveFirstPassenger();
                            if (passenger.Ref.Base.Veterancy.IsRookie())
                            {
                                passenger.Ref.Base.Veterancy.SetVeteran();
                            }else if(passenger.Ref.Base.Veterancy.IsVeteran())
                            {
                                passenger.Ref.Base.Veterancy.SetElite();
                            }
                        }
                    }

                    foreach (var passenger in passengers)
                    {
                        Owner.OwnerObject.Ref.Passengers.AddPassenger(passenger);
                    }
                }
            }
            base.OnUpdate();
        }
    }
}
