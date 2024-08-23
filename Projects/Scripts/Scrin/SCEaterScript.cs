using Extension.Ext;
using Extension.Script;
using PatcherYRpp.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(SCEaterScript))]
    public class SCEaterScript : TechnoScriptable
    {
        public SCEaterScript(TechnoExt owner) : base(owner)
        {
        }

        private int checkDelay = 20;

        public override void OnUpdate()
        {

            if (checkDelay > 0)
            {
                checkDelay--;
                return;
            }

            if (Owner.OwnerObject.Ref.Ammo >= 5)
                return;

            if (checkDelay <= 0)
            {
                checkDelay = 20;
                SeekTibrium();
            }

            base.OnUpdate();
        }


        private void SeekTibrium()
        {
            //获取脚下的矿
            var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            if (MapClass.Instance.TryGetCellAt(coord, out var pCell))
            {
                var value = pCell.Ref.GetContainedTiberiumValue();
                if (value > 0)
                {
                    var index = pCell.Ref.GetContainedTiberiumIndex();

                    var ammo = Owner.OwnerObject.Ref.Ammo;
                    ammo = ammo + ((index == 0 || index == 2) ? 1 : 2);
                    ammo = ammo > 5 ? 5 : ammo;

                    Owner.OwnerObject.Ref.Ammo = ammo;
                       
                    pCell.Ref.ReduceTiberium(1);
                    YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SCAbsorbRay"), coord + new CoordStruct(0,0,50));
                }
            }
        }
    }
}
