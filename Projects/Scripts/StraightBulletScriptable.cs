using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [ScriptAlias(nameof(StraightBulletScriptable))]
    [Serializable]
    public class StraightBulletScriptable : BulletScriptable
    {
        public StraightBulletScriptable(BulletExt owner) : base(owner)
        {
        }


        public override void OnUpdate()
        {
            var sourceCoord = Owner.OwnerObject.Ref.SourceCoords;
            var targetCoord = Owner.OwnerObject.Ref.TargetCoords;
            var distance = targetCoord.DistanceFrom(sourceCoord);
            if(!double.IsNaN(distance))
            {
                var t = distance / Owner.OwnerObject.Ref.Speed;
                Owner.OwnerObject.Ref.Velocity.X = (Owner.OwnerObject.Ref.TargetCoords.X - Owner.OwnerObject.Ref.SourceCoords.X) / t;
                Owner.OwnerObject.Ref.Velocity.Y = (Owner.OwnerObject.Ref.TargetCoords.Y - Owner.OwnerObject.Ref.SourceCoords.Y) / t;
                Owner.OwnerObject.Ref.Velocity.Z = (Owner.OwnerObject.Ref.TargetCoords.Z - Owner.OwnerObject.Ref.SourceCoords.Z) / t;
            }
        }
    }
}
