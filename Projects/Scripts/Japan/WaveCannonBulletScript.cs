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
    [ScriptAlias(nameof(WaveCannonBulletScript))]
    [Serializable]
    public class WaveCannonBulletScript : BulletScriptable
    {
        private static Pointer<WarheadTypeClass> pWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BLMCKExpWH");

        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private static Pointer<AnimTypeClass> pAnimLow => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("1202SPARK");

        private static Pointer<AnimTypeClass> pAnimHigh => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("1201SPARK");

        public WaveCannonBulletScript(BulletExt owner) : base(owner)
        {
        }

        private bool inited = false;

        public override void OnUpdate()
        {
            if(!inited)
            {
                inited = true;

                if(!Owner.OwnerObject.Ref.Owner.IsNull)
                {
                    var tLocation = Owner.OwnerObject.Ref.Target.Ref.GetCoords();
                    var location = Owner.OwnerObject.Ref.SourceCoords;

                    var distance = tLocation.DistanceFrom(location);
                    if (distance == double.NaN)
                    {
                        distance = 2560;
                    }
                    distance = distance > 2560 ? 2560 : distance;

                    var baseDamage = 40;
                    var extraDamage = ((2560d + 256d - distance) / 2560) * (320 + (distance <= 760 ? 160 : 0));

                    var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Ref.Target, Owner.OwnerObject.Ref.Owner, (int)(baseDamage + extraDamage), pWarhead, 100, true);
                    pBullet.Ref.DetonateAndUnInit(tLocation);
                    YRMemory.Create<AnimClass>((extraDamage >= 220 ? pAnimHigh : pAnimLow), tLocation);
                }
                
             
            }
        }


    }
}
