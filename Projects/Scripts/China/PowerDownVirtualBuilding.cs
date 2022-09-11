using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.China
{
    [Serializable]
    public class PowerDownVirtualBuilding : TechnoScriptable
    {
        public PowerDownVirtualBuilding(TechnoExt owner) : base(owner) { }

        int lifetime = 500;
        public override void OnUpdate()
        {
            if (lifetime-- < 0)
            {
                Owner.OwnerObject.Ref.Base.UnInit();
            }
        }

    }
}
