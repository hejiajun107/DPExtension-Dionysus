using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [ScriptAlias(nameof(CallIronCurtainSpecialScript))]
    [Serializable]
    public class CallIronCurtainSpecialScript : SuperWeaponScriptable
    {
        public CallIronCurtainSpecialScript(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            foreach(var house in HouseClass.Array)
            {
                var curtain = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("IronCurtainSpecial");

                if(!house.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                {
                    continue;
                }

                if(house == Owner.OwnerObject.Ref.Owner)
                {
                    continue;
                }

                var sw = house.Ref.FindSuperWeapon(curtain);

                if (sw.IsNull)
                {
                    continue;
                }

                if (!sw.Ref.IsCharged)
                {
                    continue;
                }

                sw.Ref.Launch(cell, false);
                sw.Ref.IsCharged = false;
                sw.Ref.Reset();
                return;
            }
        }
    }
}
