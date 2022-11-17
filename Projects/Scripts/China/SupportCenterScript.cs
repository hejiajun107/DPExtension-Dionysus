using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(SupportCenterScript))]

    public class SupportCenterScript : TechnoScriptable
    {
        public SupportCenterScript(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<TechnoTypeClass> targetType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("GATP2");


        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            base.OnPut(coord, faceDir);

            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;

            if (Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
            {
                var exts = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, t => t.Ref.Type.Ref.Base.Base.ID == "E9" && t.Ref.IsInPlayfield == true, FindRange.Owner);

                foreach (var ext in exts)
                {
                    if (!ext.IsNullOrExpired())
                    {
                        ext.OwnerObject.Convert<InfantryClass>().Ref.Type = targetType.Convert<InfantryTypeClass>();
                    }
                }
            }
        }
    }
}
