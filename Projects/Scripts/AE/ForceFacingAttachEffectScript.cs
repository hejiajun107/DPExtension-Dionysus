using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(ForceFacingAttachEffectScript))]
    public class ForceFacingAttachEffectScript : AttachEffectScriptable
    {
        public ForceFacingAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        DirStruct dir;

        public override void OnUpdate()
        {
            Logger.Log("UPDATE");
            Owner.OwnerObject.Ref.Facing.ROT = new DirStruct(256);
            if (dir == null)
            {
                dir = Owner.OwnerObject.Ref.Facing.Value;
            }
            else
            {
                Owner.OwnerObject.Ref.Facing.set(dir);
            }

            //if (Owner.OwnerObject.CastToFoot(out var pfoot))
            //{
            //    //pfoot.Ref..
            //}
        }
    }
}
