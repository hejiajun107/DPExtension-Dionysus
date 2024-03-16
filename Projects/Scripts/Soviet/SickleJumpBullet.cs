using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.Soviet
{
    [Serializable]
    [ScriptAlias(nameof(SickleJumpBullet))]
    public class SickleJumpBullet : BulletScriptable
    {
        public SickleJumpBullet(BulletExt owner) : base(owner)
        {

        }

        private bool inited = false;

        public override void OnUpdate()
        {

            if (!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                var pTechno = Owner.OwnerObject.Ref.Owner;
                var mission = pTechno.Convert<MissionClass>();

                //pTechno.Ref.SetTarget(default);
                //pTechno.Ref.SetDestination(default, false);

                //mission.Ref.QueueMission(Mission.Stop, false);
                //mission.Ref.NextMission();



                //if (!inited)
                //{
                //    inited = true;
                //    return;
                //}

                //位置
                if (pTechno.CastToFoot(out Pointer<FootClass> pfoot))
                {
                    var source = pTechno.Ref.Base.Base.GetCoords();
                    pfoot.Ref.Locomotor.Mark_All_Occupation_Bits(0);
                    pfoot.Ref.Locomotor.Force_Track(-1, source);
                    pTechno.Ref.Base.UnmarkAllOccupationBits(source);
                    var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                    pTechno.Ref.Base.SetLocation(location);
                    pTechno.Ref.Base.UnmarkAllOccupationBits(location);

                    //pTechno.Ref.SetTarget(default);
                    //pTechno.Ref.SetDestination(default, false);
                    //mission.Ref.ForceMission(Mission.Stop);
                    //mission.Ref.ForceMission(Mission.Guard);
                    //mission.Ref.Mission_Guard();

                }

            }
        }

        public override void OnDetonate(Pointer<CoordStruct> pCoords)
        {
            if (!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                if(Owner.OwnerObject.Ref.Owner.Ref.Type.Ref.Base.Base.ID == "SICKLE")
                {
                    var pWarhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ICTKCoreIronOtherWh");
                    var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                    var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Ref.Owner.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner, 0, pWarhead, 100, false);
                    pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Owner.Ref.Base.Base.GetCoords());
                }
            }
        }
    }


}
