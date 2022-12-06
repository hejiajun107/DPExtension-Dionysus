using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using System;

namespace Scripts
{
    [ScriptAlias(nameof(AutoDeployScript))]
    [Serializable]
    public class AutoDeployScript : TechnoScriptable
    {
        public AutoDeployScript(TechnoExt owner) : base(owner)
        {
        }


        private string deloyOnFire;

        private string deloyOnMove;

        public override void Awake()
        {
            var ini = this.CreateRulesIniComponentWith<AutoDeployConfigData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            deloyOnFire = ini.Data.DeloyOnFire;
            deloyOnMove = ini.Data.DeloyOnMove;
        }

        public override void OnUpdate()
        {
            //if (Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID != deloyOnMove)
            //{
            //    if (Owner.OwnerObject.CastToFoot(out var pfoot))
            //    {
            //        if(pfoot.Ref.GetCurrentSpeed() > 0)
            //        {
            //            var mission = Owner.OwnerObject.Convert<MissionClass>();
            //            pfoot.Ref.Destination = default ;
            //            mission.Ref.ForceMission(Mission.Stop);
            //            mission.Ref.ForceMission(Mission.Unload);
            //        }
                    
            //    }
            //}
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID != deloyOnFire)
            {
                var mission = Owner.OwnerObject.Convert<MissionClass>();
                mission.Ref.ForceMission(Mission.Unload);
            }
        }
    }


    [Serializable]
    public class AutoDeployConfigData : INIAutoConfig
    {

        [INIField(Key = "AutoDeploy.DeloyOnFire")]
        public string DeloyOnFire = "";

        [INIField(Key = "AutoDeploy.DeloyOnMove")]
        public string DeloyOnMove = "";

    }
}
