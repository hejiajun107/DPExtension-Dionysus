using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [ScriptAlias(nameof(VerticalLauncherScriptable))]
    [Serializable]
    public class VerticalLauncherScriptable : BulletScriptable
    {
        public VerticalLauncherScriptable(BulletExt owner) : base(owner)
        {
        }

        private bool inited = false;
        private int initeHeight = 0;
        private bool over = false;

        public override void OnLateUpdate()
        {
            if (inited == false)
            {
                inited = true;
                initeHeight = Owner.OwnerObject.Ref.Base.GetHeight();
            }

            if(!over)
            {
                if (Owner.OwnerObject.Ref.Base.GetHeight() < initeHeight + 1200)
                {
                    var velocity = Owner.OwnerObject.Ref.Velocity;


                    velocity.Z = Owner.OwnerObject.Ref.Speed;

                    velocity.X = 0;
                    velocity.Y = 0;

                    Owner.OwnerObject.Ref.Velocity = velocity;
                }
                else
                {
                    over = true;
                }
            }
            

            base.OnUpdate();
        }
    }
}
