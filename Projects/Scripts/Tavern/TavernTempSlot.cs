using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using Scripts.Tavern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Tavern
{
    [ScriptAlias(nameof(TavernTempSlot))]
    [Serializable]
    public class TavernTempSlot : TechnoScriptable
    {
        public TavernTempSlot(TechnoExt owner) : base(owner)
        {
        }

        public CardType CurrentCard { get; private set; } = null;

        public bool IsEnabled { get; private set; } = true;

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Selling)
            {
                mission.Ref.ForceMission(Mission.Stop);
                OnSell();
            }

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

            node.RegisterTempSlot(this);
            _registered = true;
            return true;
        }

        public void AddCard(CardType card)
        {
            var script = ScriptManager.GetScript(nameof(CardComponent));
            var scriptComponent = ScriptManager.CreateScriptableTo(Owner.GameObject, script, Owner);
            if (scriptComponent is CardComponent cardComponent)
            {
                cardComponent.CardType = card;
                CurrentCard = card;
            }
        }

        public void OnSell()
        {
            this.RemoveCard();
        }

        /// <summary>
        /// 移除当前卡牌
        /// </summary>
        /// <returns></returns>
        public CardType RemoveCard() {
            if(this.CurrentCard is not null)
            {
                var card = CurrentCard;
                var old = GameObject.GetComponent<CardComponent>();
                if (old is not null)
                {
                    old.DetachFromParent();
                    old.RelaseCompnent();
                    old = null;
                }
                CurrentCard = null;
                return card;
            }
            return null;
        }
    }
}
