using DynamicPatcher;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [ScriptAlias(nameof(SafeGuardTechnoScript))]
    [Serializable]
    public class SafeGuardTechnoScript : TechnoScriptable
    {
        public SafeGuardTechnoScript(TechnoExt owner) : base(owner)
        {
        }

        private string protectTechnoType = string.Empty;
        private bool includeingAllies = false;

        TechnoExt ProtectTarget = null;

        private int waitFrame = 20;

        private bool targetPicked = false;

        public override void Awake()
        {
            var ini = GameObject.CreateRulesIniComponentWith<SafeGuardTechnoData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            protectTechnoType = ini.Data.TechnoType;
            includeingAllies = ini.Data.IncludeingAllies;
        }

        public override void OnUpdate()
        {
            if (!targetPicked)
            {
                var technos = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), 6 * Game.CellSize)
                    .Where(x =>
                    {
                        var ptech = x.Convert<TechnoClass>();
                        if (ptech.Ref.Type.Ref.Base.Base.ID != protectTechnoType)
                            return false;

                        if (includeingAllies)
                        {
                            if (!ptech.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                                return false;
                        }
                        else
                        {
                            if (ptech.Ref.Owner != Owner.OwnerObject.Ref.Owner)
                                return false;
                        }

                        return true;
                    }).ToList();

                if (technos.Any())
                {
                    var ptechno = technos.First().Convert<TechnoClass>();
                    ProtectTarget = TechnoExt.ExtMap.Find(ptechno);
                    targetPicked = true;
                }
            }

            if (ProtectTarget.IsNullOrExpired())
                return;

            if (Owner.OwnerObject.Ref.Target.IsNull)
            {
                var mission = Owner.OwnerObject.Convert<MissionClass>();
                if (Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(ProtectTarget.OwnerObject.Ref.Base.Base.GetCoords()) < 2 * Game.CellSize)
                {
                    if (waitFrame <= 0)
                    {
                        waitFrame = 20;
                        mission.Ref.ForceMission(Mission.Area_Guard);
                    }
                }
                else
                {
                    var pfoot = Owner.OwnerObject.Convert<FootClass>();
                    pfoot.Ref.MoveTo(ProtectTarget.OwnerObject.Ref.Base.Base.GetCoords());
                    //mission.Ref.QueueMission(Mission.Move, true);
                }
            }
            else
            {
                if (Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(ProtectTarget.OwnerObject.Ref.Base.Base.GetCoords()) > 8 * Game.CellSize)
                {
                    var mission = Owner.OwnerObject.Convert<MissionClass>();
                    mission.Ref.ForceMission(Mission.Stop);
                    var pfoot = Owner.OwnerObject.Convert<FootClass>();
                    pfoot.Ref.MoveTo(ProtectTarget.OwnerObject.Ref.Base.Base.GetCoords());
                    //mission.Ref.QueueMission(Mission.Move, true);
                }
            }
        }
    }



    public class SafeGuardTechnoData : INIAutoConfig
    {
        [INIField(Key = "SafeGuard.Techno")]
        public string TechnoType;

        [INIField(Key = "SafeGuard.IncludeingAllies")]
        public bool IncludeingAllies = false;
    }
}
