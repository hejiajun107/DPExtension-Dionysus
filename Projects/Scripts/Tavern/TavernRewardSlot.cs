using Extension.Ext;
using Extension.Script;
using Scripts.Cards;
using Scripts.Tavern;
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

        public CardComponent CurrentCard { get; private set; }

        public CardScript CurrentScript { get; private set; }


        private bool _registered = false;

        public bool Register()
        {
            if (_registered)
                return true;

            if (TavernGameManager.Instance is null)
                return false;

            var node = TavernGameManager.Instance.FindPlayerNodeByHouse(Owner.OwnerObject.Ref.Owner);
            
            if (node is null)
                return false;

            node.RegisterRewardSlot(this);
            _registered = true;
            return true;
        }
    }
}
