using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Linq;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(WaveEnergyMechScript))]
    [Serializable]
    public class WaveEnergyMechScript : TechnoScriptable
    {
        public WaveEnergyMechScript(TechnoExt owner) : base(owner)
        {
        }

        private bool reload = false;

        private CoordStruct? targetCoord;

        public override void OnUpdate()
        {
            if (reload) {
                reload = false;
                Owner.OwnerObject.Ref.Ammo = 6;
            }

            var mission = Owner.OwnerObject.Convert<MissionClass>();

            
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);
                Owner.OwnerObject.Ref.Ammo = 0;

                var lpbullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("WaveEnergyNPWh"), 100, false);
                lpbullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

                if (targetCoord.HasValue)
                {
                    var target = ObjectFinder.FindTechnosNear(targetCoord.Value, Game.CellSize).OrderBy(x=>x.Ref.Base.GetCoords().BigDistanceForm(targetCoord.Value)).FirstOrDefault();
                    if (target.IsNotNull)
                    {
                        Owner.OwnerObject.Ref.SetTarget(target.Convert<AbstractClass>());
                        mission.Ref.ForceMission(Mission.Attack);
                    }
                   
                }
            }

            if (Owner.OwnerObject.Ref.Target.IsNull)
            {
                targetCoord = null;
            }
            else
            {
                targetCoord = Owner.OwnerObject.Ref.Target.Ref.GetCoords();
            }

            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 1)
            {
                reload = true;
            }
        }

    }
}
