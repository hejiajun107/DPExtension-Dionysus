using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(WaveEnergyMechScript))]
    [Serializable]
    public class WaveEnergyMechScript : TechnoScriptable
    {
        public WaveEnergyMechScript(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<WarheadTypeClass> pWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BLMCKExpWH");

        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private static Pointer<AnimTypeClass> pAnimLow => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("1202SPARK");

        private static Pointer<AnimTypeClass> pAnimHigh => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("1201SPARK");

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 2)
            {
                var tLocation = pTarget.Ref.GetCoords();
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                var distance = tLocation.DistanceFrom(location);
                if(distance == double.NaN)
                {
                    distance = 2560;
                }
                distance = distance > 2560 ? 2560 : distance;

                var baseDamage = 30;
                var extraDamage = ((2560d + 256d - distance) / 2560) * (300 + (distance <= 760 ? 150 : 0));

                var pBullet = pInviso.Ref.CreateBullet(pTarget, Owner.OwnerObject, (int)(baseDamage + extraDamage), pWarhead, 100, true);
                pBullet.Ref.DetonateAndUnInit(tLocation);
                YRMemory.Create<AnimClass>((extraDamage >= 220 ? pAnimHigh : pAnimLow), tLocation);
            }
        }

    }
}
