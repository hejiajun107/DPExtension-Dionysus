﻿using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    public class LaserFenceScript : TechnoScriptable
    {
        public LaserFenceScript(TechnoExt owner) : base(owner)
        {
        }

        private bool opened = true;

        public override void OnUpdate()
        {
            base.OnUpdate();
            var mission = Owner.OwnerObject.Convert<MissionClass>();

            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                if (opened)
                {
                    Owner.OwnerObject.Convert<BuildingClass>().Ref.HasPower = false;
                }
                else
                {
                    Owner.OwnerObject.Convert<BuildingClass>().Ref.HasPower = true;
                }
                opened = !opened;

            }else if(mission.Ref.CurrentMission == Mission.Selling)
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
