using Extension.Ext;
using Extension.Script;
using System;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(PowerDownVirtualBuilding))]

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
