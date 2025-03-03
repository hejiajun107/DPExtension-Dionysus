using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using Scripts.Japan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.China
{
    [ScriptAlias(nameof(CNSniperScript))]
    [Serializable]
    public class CNSniperScript : TechnoScriptable
    {
        public CNSniperScript(TechnoExt owner) : base(owner)
        {
            _voc = new VocExtensionComponent(owner);
        }

        private VocExtensionComponent _voc;

        static Pointer<AnimTypeClass> pAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SniperReloadSound");

        public override void Awake()
        {
            _voc.Awake();
            Owner.OwnerObject.Ref.Ammo = 0;
        }

        private int rof = 200;

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);
                reload();
            }

            if (rof > 0 && Owner.OwnerObject.Ref.Ammo == 0)
            {
                rof--;
            }

            if (Owner.OwnerObject.Ref.Ammo == 1)
            {
                if (Owner.OwnerObject.Ref.Base.InLimbo)
                {
                    Owner.OwnerObject.Ref.Ammo = 0;
                }
            }

            if (rof <= 0)
            {
                if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman() && rof <= 0 && !Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap)
                {
                    reload();
                }
            }
            
        }

        private bool weaponChanged = false;
        private bool toRecover = false;

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 0)
            {
                _voc.PlaySpecialVoice(3, true);
            }
        }

        private void reload()
        {
            if (rof <= 0)
            {
                rof = 500;
                Owner.OwnerObject.Ref.Ammo = 1;
                YRMemory.Create<AnimClass>(pAnim, Owner.OwnerObject.Ref.Base.Base.GetCoords());
                _voc.PlaySpecialVoice(1, true);
            }
            else
            {
                _voc.PlaySpecialVoice(2, true);
            }
        }
    }
}
