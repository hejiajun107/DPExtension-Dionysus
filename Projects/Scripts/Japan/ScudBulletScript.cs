using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(ScudBulletScript))]
    [Serializable]
    public class ScudBulletScript : BulletScriptable
    {
        public ScudBulletScript(BulletExt owner) : base(owner)
        {
        }

         bool isRising { get; set; } = true;
         bool inited { get; set; } = false;

         CoordStruct originTarget = default;

        public override void OnUpdate()
        {
            if (!inited)
            {
                inited = true;
                var offset = (int)(Math.Floor(((double)MathEx.Random.Next(80, 200) / 100d) * Game.CellSize));
                originTarget = new CoordStruct(Owner.OwnerObject.Ref.TargetCoords.X, Owner.OwnerObject.Ref.TargetCoords.Y, Owner.OwnerObject.Ref.TargetCoords.Z);
                var target = Owner.OwnerObject.Ref.TargetCoords + new CoordStruct(offset * (MathEx.Random.Next(100) > 50 ? 1 : -1), offset * (MathEx.Random.Next(100) > 50 ? 1 : -1), 0);
                if (MapClass.Instance.TryGetCellAt(target, out var pcell)){
                    Owner.OwnerObject.Ref.SetTarget(pcell.Convert<AbstractClass>());
                    Owner.OwnerObject.Ref.TargetCoords = target;
                }
            }

            if(isRising)
            {
                if(Owner.OwnerObject.Ref.Base.Base.GetCoords().Z >= Owner.OwnerObject.Ref.SourceCoords.Z + 3000)
                {
                    isRising = false;
                    //Owner.OwnerObject.Ref.MoveTo(Owner.OwnerObject.Ref.TargetCoords + new CoordStruct(0, 0, 3000),new BulletVelocity(0,0,-100));
                    Owner.OwnerObject.Ref.Base.SetLocation(originTarget + new CoordStruct(0, 0, 3000));
                    Owner.OwnerObject.Ref.Velocity.Z = -100;
                    Owner.OwnerObject.Ref.Velocity.X = MathEx.Random.Next(-50, 50);
                    Owner.OwnerObject.Ref.Velocity.Y = MathEx.Random.Next(-50, 50);
                }
                else
                {
                    Owner.OwnerObject.Ref.Velocity.X = 0;
                    Owner.OwnerObject.Ref.Velocity.Y = 0;
                    Owner.OwnerObject.Ref.Velocity.Z = 80;
                }
            }
            base.OnUpdate();
        }
    }
}
