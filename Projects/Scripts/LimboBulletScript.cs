using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    [ScriptAlias(nameof(LimboBulletScript))]
    public class LimboBulletScript : BulletScriptable
    {
        public LimboBulletScript(BulletExt owner) : base(owner)
        {
        }

        private bool inited = false;

        public override void OnUpdate()
        {
            if (!inited)
            {
                inited = true;
                if(Owner.OwnerObject.Ref.Owner.IsNotNull)
                {
                    Owner.OwnerObject.Ref.Owner.Ref.Base.Remove();
                }
            }
        }

        public override void OnDestroy()
        {
            if (Owner.OwnerObject.Ref.Owner.IsNotNull)
            {
                Owner.OwnerObject.Ref.Owner.Ref.Base.UnInit();
            }
        }
    }
}
