using DynamicPatcher;
using Extension.CWUtilities;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Extension.CW
{


    public partial class TechnoGlobalExtension
    {
        private int passengerCheckDelay = 20;

        private int lastHouseIndex = -1;

        private bool passengerInited = false;

        [UpdateAction]
        public void TechnoClass_Update_Passenger_Extension()
        {
     
            if (!Data.PassengerAlwaysSync)
                return;

            if (passengerCheckDelay-- > 0)
                return;

            passengerCheckDelay = 20;

            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;

            if (Owner.OwnerObject.Ref.Passengers.NumPassengers == 0 && Data.InitPassengers != null && Data.PassengerNums != null && !passengerInited)
            {
                //数量不一致说明ini写错了
                if (Data.InitPassengers.Count() == Data.PassengerNums.Count())
                {
                    //添加乘客
                    if (Data.InitPassengers.Count() > 0)
                    {
                        for (var i = 0; i < Data.InitPassengers.Count(); i++)
                        {
                            var typeStr = Data.InitPassengers[i];
                            var num = Data.PassengerNums[i];

                            var technoType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(typeStr);

                            if (technoType != null)
                            {
                                for (var j = num; j > 0; j--)
                                {
                                    var techno = technoType.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();
                                    if (techno == null)
                                        continue;

                                    techno.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                                    techno.Ref.Base.Remove();

                                    //这里要加个判断OpenTopped 这里没写因为一般用到的时候都是需要乘员开火的情况
                                    Owner.OwnerObject.Ref.EnteredOpenTopped(techno);

                                    if (techno.CastToFoot(out var pfoot))
                                    {
                                        Owner.OwnerObject.Ref.Passengers.AddPassenger(pfoot);
                                    }
                                }
                            }
                        }
                    }

                    passengerInited = true;
                }
            }
            else
            {
                passengerInited = true;
            }

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

                foreach (var passenger in passengers)
                {
                    Owner.OwnerObject.Ref.Passengers.AddPassenger(passenger);
                }
            }

            lastHouseIndex = currentHouse.Ref.ArrayIndex;
        }


        [UpdateAction]
        public void TechnoClass_Passenger_InTransport_Extension()
        {
            Pointer<TechnoClass> pTechno = Owner.OwnerObject;
            Pointer<TechnoClass> pTransporter = pTechno.Ref.Transporter;
            if (!pTransporter.IsNull)
            {
                TechnoExt transporterExt = TechnoExt.ExtMap.Find(pTransporter);
                var gextenion = transporterExt.GameObject.GetTechnoGlobalComponent();
               

                if (gextenion.Data.OpenTopped == false)
                    return;

                if (gextenion.Data.OpenToppedAffectTarget == "all" || string.IsNullOrEmpty(gextenion.Data.OpenToppedAffectTarget))
                    return;

                if (transporterExt != null)
                {
                    if (transporterExt.OwnerObject.Ref.Target.IsNull)
                        return;

                    var target = transporterExt.OwnerObject.Ref.Target;
                    if (target.CastToTechno(out var ptarget))
                    {
                        if (ptarget.Ref.Owner.IsNull)
                            return;

                        if (pTransporter.Ref.Owner.IsNull)
                            return;

                        var isAllied = ptarget.Ref.Owner.Ref.IsAlliedWith(pTransporter.Ref.Owner);

                        if ((isAllied && gextenion.Data.OpenToppedAffectTarget == "enermy") || (!isAllied && gextenion.Data.OpenToppedAffectTarget == "ally"))
                        {
                            pTechno.Ref.SetTarget(default);
                            pTechno.Convert<MissionClass>().Ref.ForceMission(Mission.Stop);
                        }
                    }
                }
            }
        }

    }

    public partial class TechnoGlobalTypeExt
    {

        [INIField(Key = "OpenTopped.AffectTarget")]
        public string OpenToppedAffectTarget = "all";

        [INIField(Key = "OpenTopped")]
        public bool OpenTopped = false;

        [INIField(Key = "Passenger.AlwaysSync")]
        public bool PassengerAlwaysSync = false;

        [INIField(Key = "InitialPayload.Types")]
        public string[] InitPassengers = Array.Empty<string>();

        [INIField(Key = "InitialPayload.Nums")]
        public int[] PassengerNums = Array.Empty<int>();
    }
}
