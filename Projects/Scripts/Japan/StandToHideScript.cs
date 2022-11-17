using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(StandToHideScript))]
    [Serializable]
    public class StandToHideScript : TechnoScriptable
    {
        public StandToHideScript(TechnoExt owner) : base(owner)
        {
        }

        private CoordStruct lastCoord;

        private int keepStand = 0;

        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("invisible");

        private static Pointer<WarheadTypeClass> pCloack => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("JpCloakWh");

        public override void OnUpdate()
        {
            if (Owner.OwnerObject.Ref.Base.IsOnMap && !Owner.OwnerObject.Ref.Base.InLimbo)
            {
                var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                if (coord == lastCoord)
                {
                    keepStand++;
                }
                else
                {
                    lastCoord = coord;
                    keepStand = 0;
                }

                if (keepStand >= 200)
                {
                    keepStand = 0;
                    //隐身

                    //var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(),)

                }
            }
        }
    }
}
