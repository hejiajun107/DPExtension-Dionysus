using Extension.CW;
using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(SelfUninitScript))]
    public class SelfUninitScript : TechnoScriptable
    {
        public SelfUninitScript(TechnoExt owner) : base(owner)
        {
           
        }

        private int duration = 500;

        public override void Awake()
        {
            var scriptArgs = "";
            var gext = Owner.GameObject.GetComponent<TechnoGlobalExtension>();
            if (gext != null)
            {
                scriptArgs = gext.Data.ScriptArgs;
            }


            if (!string.IsNullOrEmpty(scriptArgs))
            {
                var args = scriptArgs;
                if (int.TryParse(args, out var num))
                {
                    duration = num;
                }
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (duration-- <= 0)
            {
                Owner.OwnerObject.Ref.Base.UnInit();
                return;
            }
        }
    }
}
