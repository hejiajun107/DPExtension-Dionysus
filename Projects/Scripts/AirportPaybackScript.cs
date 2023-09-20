using Extension.CW;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [ScriptAlias(nameof(AirportPaybackScript))]
    [Serializable]
    public class AirportPaybackScript : TechnoScriptable
    {
        public AirportPaybackScript(TechnoExt owner) : base(owner)
        {
        }

        static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> pWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("AirRtMoneyWh");

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            var house = HouseExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
            if (house == null)
            {
                return;
            }
            var component = house.GameObject.GetComponent<HouseGlobalExtension>();
            if (component == null)
            {
                return;
            }

            if (component.AirportPaybackTime > 0)
            {
                return;
            }

            component.AirportPaybackTime++;

            Pointer<BulletClass> pBullets = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, pWH, 100, false);
            pBullets.Ref.DetonateAndUnInit(coord + new CoordStruct(0, 0, 200));
        }
    }
}
