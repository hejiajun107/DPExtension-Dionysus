using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.American
{
    [Serializable]
    [ScriptAlias(nameof(FixChronoMinerSBScript))]
    public class FixChronoMinerSBScript : TechnoScriptable
    {
        public FixChronoMinerSBScript(TechnoExt owner) : base(owner)
        {
        }

        private bool sleeped = false;
        private Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("FixCMINEmpWH");
        private Pointer<BulletTypeClass> bullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");


        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();


            if (mission.Ref.CurrentMission == Mission.Sleep)
            {
                sleeped = true;
                return;
            }

            if (sleeped == true && mission.Ref.CurrentMission == Mission.Enter && Owner.OwnerObject.Ref.Base.IsOnMap)
            {
                mission.Ref.ForceMission(Mission.Guard);
                var pBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, warhead, 100, false);
                //pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                sleeped = false;
                return;
            }

            base.OnUpdate();
        }
    }
}
