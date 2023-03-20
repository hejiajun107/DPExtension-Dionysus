using DynamicPatcher;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using System;
using static DpLib.Scripts.China.IonCannonReshadeBulletScript;

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

        private int maxDistance;

        private int minDistance;

        static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> pWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DpStop100Wh");


        public override void Awake()
        {
            var ini = this.CreateRulesIniComponentWith<AutoDeployConfigData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            deloyOnFire = ini.Data.DeloyOnFire;
            deloyOnMove = ini.Data.DeloyOnMove;
            maxDistance = ini.Data.PrimaryUnloadMaxRange;
            minDistance = ini.Data.PrimaryMinRange;
        }

        private int checkTargetRof = 50;

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();

            if (Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID != deloyOnMove)
            {
                var dir = new DirStruct(3, 2);
                Owner.OwnerObject.Ref.Facing.set(dir);

                if (Owner.OwnerObject.CastToFoot(out var pfoot))
                {
                    pfoot.Ref.SpeedMultiplier = 0;

                    //不允许选取最近目标
                  
                    if(Owner.OwnerObject.Ref.Target.IsNotNull)
                    {
                        var weaponIndex = Owner.OwnerObject.Ref.SelectWeapon(Owner.OwnerObject.Ref.Target);
                        if (weaponIndex == 0)
                        {
                            var distance = Owner.OwnerObject.Ref.Base.Base.GetCoords().DistanceFrom(Owner.OwnerObject.Ref.Target.Ref.GetCoords());
                            if (!double.IsNaN(distance) && distance < minDistance * 256)
                            {
                                Owner.OwnerObject.Ref.SetTarget(default);
                                mission.Ref.ForceMission(Mission.Stop);
                            }
                        }
                    }

                    if (mission.Ref.CurrentMission == Mission.Move)
                    {
                        mission.Ref.ForceMission(Mission.Stop);
                        mission.Ref.ForceMission(Mission.Unload);
                    }
                    else if(checkTargetRof--<=0)
                    {
                        checkTargetRof = 50;
                        if (Owner.OwnerObject.Ref.Target.IsNotNull)
                        {
                            if(!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                            {
                                var weaponIndex = Owner.OwnerObject.Ref.SelectWeapon(Owner.OwnerObject.Ref.Target);
                                if (weaponIndex == 0)
                                {
                                    var distance = Owner.OwnerObject.Ref.Base.Base.GetCoords().DistanceFrom(Owner.OwnerObject.Ref.Target.Ref.GetCoords());
                                    if (double.IsNaN(distance) || distance > maxDistance * 256)
                                    {
                                        mission.Ref.ForceMission(Mission.Stop);
                                        mission.Ref.ForceMission(Mission.Unload);
                                    }
                                }
                            }
                        }
                    }

                }
            }

            if(mission.Ref.CurrentMission == Mission.Unload)
            {
                var bullet= pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Pointer<TechnoClass>.Zero, 1, pWh, 100, false);
                bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                if(!Owner.IsNullOrExpired())
                {
                    if (Owner.GameObject.GetComponent(StopFacingScript.UniqueId) == null)
                    {
                        Owner.GameObject.CreateScriptComponent(nameof(StopFacingScript), StopFacingScript.UniqueId, "StopFacingScriptDecorator", Owner);
                    }
                }
            }

          
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

    [ScriptAlias(nameof(StopFacingScript))]
    [Serializable]
    public class StopFacingScript : TechnoScriptable
    {
        public StopFacingScript(TechnoExt owner) : base(owner)
        {
        }

        public static int UniqueId = 2022120823; 

        private int delay = 80;

        public override void OnUpdate()
        {
            if (delay-- <= 0)
            {
                DetachFromParent();
                return;
            }

            if(Owner.IsNullOrExpired())
            {
                return;
            }

            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                return;
            }

            var dir = new DirStruct(3,2);
            Owner.OwnerObject.Ref.Facing.set(dir);
            Owner.OwnerObject.Ref.TurretFacing.set(dir);

        }


    }

    [Serializable]
    public class AutoDeployConfigData : INIAutoConfig
    {

        [INIField(Key = "AutoDeploy.DeloyOnFire")]
        public string DeloyOnFire = "";

        [INIField(Key = "AutoDeploy.DeloyOnMove")]
        public string DeloyOnMove = "";

        [INIField(Key = "AutoDeploy.PrimaryMaxRange")]
        public int PrimaryUnloadMaxRange;

        [INIField(Key = "AutoDeploy.PrimaryMinRange")]
        public int PrimaryMinRange;

    }
}
