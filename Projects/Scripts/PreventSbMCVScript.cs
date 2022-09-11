﻿using Extension.Ext;
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
    public class PreventSbMCVScript : TechnoScriptable
    {
        public PreventSbMCVScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();

            if (mission.Ref.CurrentMission == Mission.Hunt)
            {
                mission.Ref.ForceMission(Mission.Guard);
            }
        }
    }
}
