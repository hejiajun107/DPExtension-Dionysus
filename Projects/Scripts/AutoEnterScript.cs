using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    [ScriptAlias(nameof(AutoEnterScript))]
    public class AutoEnterScript : SuperWeaponScriptable
    {
        public AutoEnterScript(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            var coord = CellClass.Cell2Coord(cell);
            var technos = ObjectFinder.FindTechnosNear(coord, Game.CellSize * 6).Select(x=> x.Convert<TechnoClass>()).ToList().Where(x=>x.Ref.Owner == Owner.OwnerObject.Ref.Owner && !x.Ref.Base.InLimbo).ToList();

            var passengers = technos.Where(x => x.Ref.Base.Base.WhatAmI() == AbstractType.Infantry).Where(x => x.Ref.Type.Ref.Size <= 2).ToList();
            Logger.Log(passengers.Count());

            var carriers = technos.Where(x=>!x.Ref.Base.Base.IsInAir() && x.Ref.Type.Ref.Passengers >= 0 && x.Ref.Passengers.GetTotalSize() < x.Ref.Type.Ref.Passengers).ToList()
                .Select(x=> new PassengerCarrier()
                {
                    Current = x.Ref.Passengers.GetTotalSize(),
                    Max = x.Ref.Type.Ref.Passengers,
                    Limit = (int)x.Ref.Type.Ref.SizeLimit,
                    TechnoExt = TechnoExt.ExtMap.Find(x)
                }).ToList()
            ;

            Logger.Log("车：" + carriers.Count());

            foreach(var passenger in passengers)
            {
                var mission = passenger.Convert<MissionClass>();
                if (mission.Ref.CurrentMission != Mission.Guard)
                    continue;

                var carrier = carriers.Where(x => x.Current + passenger.Ref.Type.Ref.Size <= x.Max).FirstOrDefault();

                if (carrier is null)
                    continue;

                carrier.Current += (int)passenger.Ref.Type.Ref.Size;

                if(carrier.Current >= carrier.Max)
                    carriers.Remove(carrier);

                mission.Ref.ForceMission(Mission.Stop);
                passenger.Ref.SetFocus(carrier.TechnoExt.OwnerObject.Convert<AbstractClass>());
                mission.Ref.ForceMission(Mission.Enter);
                mission.Ref.Mission_Enter();
            }

            base.OnLaunch(cell, isPlayer);
        }
    }

    [Serializable]
    public class PassengerCarrier
    {
        public int Current { get; set; } = 0;

        public int Max { get; set; } = 0;

        public int Limit { get; set; } = 1;

        public TechnoExt TechnoExt { get; set; }
    }
}
