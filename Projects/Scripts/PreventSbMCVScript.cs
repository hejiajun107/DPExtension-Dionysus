using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(PreventSbMCVScript))]
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
