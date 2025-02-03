using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Soviet
{
    [ScriptAlias(nameof(DTruckScript))]
    [Serializable]
    public class DTruckScript : TechnoScriptable
    {
        public DTruckScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}
