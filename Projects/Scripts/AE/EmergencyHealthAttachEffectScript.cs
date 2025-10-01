using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(EmergencyHealthAttachEffectScript))]
    public class EmergencyHealthAttachEffectScript : AttachEffectScriptable
    {
        public EmergencyHealthAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private int strength = 500;

        private bool actived = false;

        public override void Awake()
        {
            strength = Owner.OwnerObject.Ref.Type.Ref.Base.Strength;
            base.Awake();
        }

        public override void OnUpdate()
        {
            if (actived)
            {
                Duration = 0;
                return;
            }

            Duration = 100;

            if (Owner.OwnerObject.Ref.Base.Health < strength * 0.1)
            {
                actived = true;
                Owner.OwnerObject.Ref.Base.Health = Owner.OwnerObject.Ref.Base.Health + 300 > strength ? strength : Owner.OwnerObject.Ref.Base.Health + 300;
                YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("VOLHEAL"), Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 100));
            }

            base.OnUpdate();
        }
    }
}
