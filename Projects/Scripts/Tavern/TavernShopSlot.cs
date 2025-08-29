using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scripts.Tavern;

namespace Scripts.Tavern
{
    /// <summary>
    /// 商店去
    /// </summary>
    [ScriptAlias(nameof(TavernShopSlot))]
    [Serializable]
    public class TavernShopSlot : TechnoScriptable
    {
        public TavernShopSlot(TechnoExt owner) : base(owner)
        {
        }

        public bool Enabled { get; set; }

        public CardType CurrentCard { get; private set; }

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

            var node = TavernGameManager.Instance.FindPlayerNodeByHouse(Owner.OwnerObject.Ref.Owner);

            if (node is null)

                return false;

            node.RegisterShopSlot(this);
            _registered = true;
            return true;
        }

        public void ChangeCard(CardType cardType)
        {
            var old = GameObject.GetComponent<CardComponent>();
            if(old is not null)
            {
                old.DetachFromParent();
                old = null;
            }
            CurrentCard = cardType;
            var script = ScriptManager.GetScript(nameof(CardComponent));
            var scriptComponent = ScriptManager.CreateScriptableTo(Owner.GameObject, script);
            if(scriptComponent is CardComponent cardComponent)
            {
                cardComponent.CardType = cardType;
            }
        }

        public CardComponent TakeCard()
        {
            var component = GameObject.GetComponent<CardComponent>();
            component.DetachFromParent();
            CurrentCard = null;
            return component;
        }
    }
}
