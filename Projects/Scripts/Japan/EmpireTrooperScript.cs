using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.Japan
{
    [Serializable]
    [ScriptAlias(nameof(EmpireTrooperScript))]
    public class EmpireTrooperScript : TechnoScriptable
    {
        public EmpireTrooperScript(TechnoExt owner) : base(owner) { }

        private static Pointer<TechnoTypeClass> GunType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("JPSR");

        private static Pointer<TechnoTypeClass> SwordType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("JPSR2");

        private static Pointer<WarheadTypeClass> pWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("JPSRDeployWh");
        private static Pointer<BulletTypeClass> pBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");



        private int type = 0;

        private int duration = 600;

        private int guardDelay = 25;

        public override void OnUpdate()
        {
            if (type == 1)
            {
                if (guardDelay-- == 0)
                {
                    var mission = Owner.OwnerObject.Convert<MissionClass>();
                    if (Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                    {
                        mission.Ref.ForceMission(Mission.Area_Guard);
                    }
                    else
                    {
                        mission.Ref.ForceMission(Mission.Stop);
                        mission.Ref.ForceMission(Mission.Hunt);
                    }
                    //mission.Ref.NextMission();
                    //mission.Ref.QueueMission(Mission.Guard, false);
                    //mission.Ref.NextMission();
                }

                if (duration-- <= 0)
                {
                    duration = 600;
                    type = 0;
                    Owner.OwnerObject.Convert<InfantryClass>().Ref.Type = GunType.Convert<InfantryTypeClass>();
                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 1 && type == 0)
            {
                Owner.OwnerObject.Convert<InfantryClass>().Ref.Type = SwordType.Convert<InfantryTypeClass>();
                type = 1;
                duration = 600;

                var bullet = pBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, pWarhead, 100, false);
                bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

                Owner.OwnerObject.Ref.SetTarget(default);


                guardDelay = 25;
            }
            else
            {
                base.OnFire(pTarget, weaponIndex);
            }
        }
    }
}
