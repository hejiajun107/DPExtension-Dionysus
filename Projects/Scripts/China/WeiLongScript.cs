using Extension.CW;
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.China
{
    [ScriptAlias(nameof(WeiLongScript))]
    [Serializable]
    public class WeiLongScript : TechnoScriptable
    {
        public WeiLongScript(TechnoExt owner) : base(owner)
        {
            _voc = new VocExtensionComponent(Owner);
        }

        VocExtensionComponent _voc;



        public override void Awake()
        {
            _voc.Awake();
            base.Awake();
        }

        bool lstAreaGuardStatus = false;

        public override void OnUpdate()
        {
            var ext = (TechnoGlobalExtension)Owner.GameObject.FastGetScript1;

            if(ext.isAreaProtecting && !lstAreaGuardStatus)
            {
                _voc.PlaySpecialVoice(1, true);
            }

            lstAreaGuardStatus = ext.isAreaProtecting;
            
        }
    }
}
