using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Tavern
{
    [ScriptAlias(nameof(TavernRewardSlot))]
    [Serializable]
    public class TavernRewardSlot : TechnoScriptable
    {
        public TavernRewardSlot(TechnoExt owner) : base(owner)
        {
        }

        public override void OnUpdate()
        {
            if (!Register())
                return;
        }

        private bool _registered = false;

        public bool Register()
        {
            if (_registered)
                return true;

            if (TavernGameManager.Instance is null)
                return false;

            var node = TavernGameManager.Instance.PlayerNodes.Where(x => x.Owner.OwnerObject.Ref.Owner == Owner.OwnerObject.Ref.Owner).FirstOrDefault();
            
            if (node is null)
                return false;

            node.RegisterRewardSlot(this);
            _registered = true;
            return true;
        }
    }
}
