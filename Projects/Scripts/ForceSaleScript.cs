﻿using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(ForceSaleScript))]
    public class ForceSaleScript : TechnoScriptable
    {
        public ForceSaleScript(TechnoExt owner) : base(owner)
        {

        }

        private int delay = 2000;

        private bool selling = false;

        public override void OnUpdate()
        {
            if (selling)
                return;
            if (delay-- <= 0 || Owner.OwnerObject.Ref.Base.Health <= 100)
            {
                var mission = Owner.OwnerObject.Convert<MissionClass>();
                mission.Ref.ForceMission(Mission.Selling);
                selling = true;
            }
            base.OnUpdate();
        }
    }
}
