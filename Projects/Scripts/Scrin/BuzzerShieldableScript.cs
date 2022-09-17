using DpLib.Scripts.Japan;
using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(BuzzerShieldableScript))]
    public class BuzzerShieldableScript : TechnoScriptable
    {
        public BuzzerShieldableScript(TechnoExt owner) : base(owner)
        {
        }


        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private static Pointer<WarheadTypeClass> attachWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("GiveBuzzerShiedsWh");

        private static Pointer<WarheadTypeClass> removeWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RemoveBuzzerShiedsWh");


        private bool shielded = false;

        private int delay = 200;

        public override void OnUpdate()
        {
            if (delay-- > 0)
            {
                return;
            }

            delay = 200;

            if (Owner.OwnerObject.Ref.Passengers.NumPassengers > 0)
            {
                if (!shielded)
                {
                    shielded = true;
                    var inviso = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, attachWarhead, 100, false);
                    inviso.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }
            else
            {
                if (shielded)
                {
                    shielded = false;
                    var inviso = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, removeWarhead, 100, false);
                    inviso.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }

            base.OnUpdate();
        }

    }
}
