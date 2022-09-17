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
    [ScriptAlias(nameof(UnOnMapaleScript))]
    public class UnOnMapaleScript : TechnoScriptable
    {
        public UnOnMapaleScript(TechnoExt owner) : base(owner)
        {

        }

        public override void OnUpdate()
        {
            if (Owner.OwnerObject.Ref.Base.IsOnMap == true  || Owner.OwnerObject.Ref.Base.InLimbo == true)
            {
                Owner.OwnerObject.Ref.Base.UnInit();
            }
            base.OnUpdate();
        }

    }
}
