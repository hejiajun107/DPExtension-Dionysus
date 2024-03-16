using DynamicPatcher;
using Extension.CW;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;

namespace Scripts.Soviet
{
    [Serializable]
    [ScriptAlias(nameof(ThunderBirdScript))]
    public class ThunderBirdScript : TechnoScriptable
    {
        public ThunderBirdScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnRemove()
        {
            var house = HouseExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
            if (house != null)
            {
                var component = house.GameObject.GetComponent<HouseGlobalExtension>();
                if (component != null)
                {
                    component.NeedRecalcNumOfThunderBird = true;
                }
            }
        }

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            var house = HouseExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
            if (house != null)
            {
                var component = house.GameObject.GetComponent<HouseGlobalExtension>();
                if (component != null)
                {
                    component.NeedRecalcNumOfThunderBird = true;
                }
            }
        }

        private int delay = 100;

        private int elecLevel = 0;

        public override void OnUpdate()
        {
            if (delay-- > 0)
                return;

            delay = 100;

            var house = HouseExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
            if (house != null)
            {
                var component = house.GameObject.GetComponent<HouseGlobalExtension>();
                var powerDrain = house.OwnerObject.Ref.PowerDrain + 40 * component.NumOfThunderBird;
                var powerOutput = house.OwnerObject.Ref.PowerOutput;

                var dPower = powerOutput - powerDrain;
                if (dPower < 0)
                {
                    elecLevel = 0;
                    var pWarhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ORCABCharge0Wh");
                    var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                    var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, pWarhead, 100, false);
                    pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    //缺电
                }
                else if (dPower >= 0 && dPower <= 20 * component.NumOfThunderBird)
                {
                    elecLevel = 1;
                    //无事发生
                }
                else if (dPower > 0 && dPower <= 50 * component.NumOfThunderBird)
                {
                    elecLevel = 2;
                    var pWarhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ORCABCharge1Wh");
                    var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                    var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, pWarhead, 100, false);
                    pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    //中等电量
                }
                else
                {
                    elecLevel = 3;
                    var pWarhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ORCABCharge2Wh");
                    var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                    var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, pWarhead, 100, false);
                    pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    //电力充足
                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(elecLevel == 2)
            {
                var coord = ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, new CoordStruct(180, 0, -30), false);
                Owner.OwnerObject.Ref.Electric_Zap(pTarget, WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ChargedElec1"), coord);
            }
            else if(elecLevel == 3)
            {
                var coord = ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, new CoordStruct(180, 0, -30), false);
                Owner.OwnerObject.Ref.Electric_Zap(pTarget, WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ChargedElec2"), coord);
            }
        }
    }
}
