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
    public class SelfUninitScript : TechnoScriptable
    {
        public SelfUninitScript(TechnoExt owner) : base(owner)
        {
            var scriptArgs = "";
            var gext = owner.GameObject.GetComponent<TechnoGlobalExtension>();
            if (gext != null)
            {
                scriptArgs = gext.Data.ScriptArgs;
            }

           
            if (!string.IsNullOrEmpty(scriptArgs))
            {
                var args = scriptArgs;
                if (int.TryParse(args,out var num))
                {
                    duration = num;
                }
            }
        }

        private int duration = 500;

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
