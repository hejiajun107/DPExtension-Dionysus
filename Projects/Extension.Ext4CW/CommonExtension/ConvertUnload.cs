using Extension.CWUtilities;
using Extension.INI;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.CW
{
    



    public partial class TechnoGlobalExtension
    {
        private bool needConvertWhenLanding = false;
        private bool landed = false;
        private string FloatingType;
        private string LandingType;

        [AwakeAction]
        public void TechnoClass_Init_Convert_Unload()
        {
            if (string.IsNullOrEmpty(Data.ConvertUnloadTo)) return;
            needConvertWhenLanding = true;
            FloatingType = Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID;
            LandingType = Data.ConvertUnloadTo;
        }

        [UpdateAction]
        public void TechnoClass_Update_Convert_Unload()
        {
            if (!needConvertWhenLanding) return;
            var mission = Owner.OwnerObject.Convert<MissionClass>();

            if (landed == false)
            {
                if (mission.Ref.CurrentMission == Mission.Unload)
                {
                    Owner.OwnerObject.Convert<UnitClass>().Ref.Type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(LandingType).Convert<UnitTypeClass>();
                    landed = true;
                }
            }
            else
            {
                if (mission.Ref.CurrentMission == Mission.Move)
                {
                    Owner.OwnerObject.Convert<UnitClass>().Ref.Type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(FloatingType).Convert<UnitTypeClass>();
                    landed = false;
                }
            }
        }

    }

    public partial class TechnoGlobalTypeExt
    {
        [INIField(Key = "Convert.Unload")]
        public string ConvertUnloadTo;
    }

}
