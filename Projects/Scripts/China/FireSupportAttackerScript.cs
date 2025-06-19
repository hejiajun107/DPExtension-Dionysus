using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.China
{
    [ScriptAlias(nameof(FireSupportAttackerScript))]
    [Serializable]
    public class FireSupportAttackerScript : TechnoScriptable
    {
        public FireSupportAttackerScript(TechnoExt owner) : base(owner)
        {
            _voc = new VocExtensionComponent(owner);
        }

        private VocExtensionComponent _voc;

        private int rof = 0;

        public override void Awake()
        {
            _voc.Awake();
            base.Awake();
        }

        public override void OnUpdate()
        {
            if (rof > 0)
            {
                rof--;
            }
            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(weaponIndex == 1)
            {
                if (rof == 0)
                {
                    _voc.PlaySpecialVoice(1, true);
                    rof = 20;
                }
            }
        }
    }
}
