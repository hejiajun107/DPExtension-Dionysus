using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(JumpetCarrierScript))]
    public class JumpetCarrierScript : TechnoScriptable
    {

        public JumpetCarrierScript(TechnoExt owner) : base(owner)
        {
            floatType = Owner.OwnerObject.Ref.Type;
            landType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID + "2");
        }

        private Pointer<TechnoTypeClass> floatType;
        private Pointer<TechnoTypeClass> landType;

        private bool landed = false;

        public override void OnUpdate()
        {
            base.OnUpdate();
            var mission = Owner.OwnerObject.Convert<MissionClass>();

            if (landed == false)
            {
                if (mission.Ref.CurrentMission == Mission.Unload)
                {
                    Owner.OwnerObject.Convert<UnitClass>().Ref.Type = landType.Convert<UnitTypeClass>();
                    landed = true;
                }
            }
            else
            {
                if (mission.Ref.CurrentMission == Mission.Move)
                {
                    Owner.OwnerObject.Convert<UnitClass>().Ref.Type = floatType.Convert<UnitTypeClass>();
                    landed = false;
                }
            }
        }

    }
}
