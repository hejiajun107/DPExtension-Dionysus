using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using Scripts.Tavern;

namespace Scripts.Tavern
{
    [ScriptAlias(nameof(RefreshSWScript))]
    [Serializable]
    public class RefreshSWScript : SuperWeaponScriptable
    {
        public RefreshSWScript(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            if(TavernGameManager.Instance is not null)
            {
                var node = TavernGameManager.Instance.FindPlayerNodeByHouse(Owner.OwnerObject.Ref.Owner);
                if(node is not null)
                {
                    node.OnRefreshShop();
                }
            }
        }
    }
}
