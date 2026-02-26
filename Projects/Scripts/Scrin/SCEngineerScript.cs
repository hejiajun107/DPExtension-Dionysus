using DpLib.Scripts;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(SCEngineerScript))]
    public class SCEngineerScript : TechnoScriptable
    {
        public SCEngineerScript(TechnoExt owner) : base(owner)
        {
        }

        private int rof = 200;

        public override void OnUpdate()
        {
            if (rof-- > 0)
            {
                return;
            }
            rof = 200;

            if (Owner.OwnerObject.Ref.Base.IsAlive && Owner.OwnerObject.Ref.Base.IsOnMap && Owner.OwnerObject.Ref.Base.InLimbo == false)
            {
                Pointer<BulletClass> pBullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 30, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SCEngineerWh"), 100, false);
                pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }
        }
    }
}
