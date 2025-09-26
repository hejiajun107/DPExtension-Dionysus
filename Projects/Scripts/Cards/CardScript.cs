using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scripts.Cards;
using Scripts.Tavern;

namespace Scripts.Cards
{
    [Serializable]
    public class CardScript
    {
        public CardType Type { get; set; }

        public CardScript(CardType type, TavernPlayerNode player)
        {
            Type = type;
            Player = player;
        }

        public object Slot { get; set; }
        
        public TavernPlayerNode Player { get; private set; }

        public virtual void OnAwake()
        {

        }

        public virtual void OnBought()
        {

        }

        /// <summary>
        /// 出出售时触发，无论是否在场上，返回最终价格
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public virtual int OnSelled(int price)
        {
            return price;
        }

        /// <summary>
        /// 在场上的单位出售时触发，返回最终价格
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public virtual int OnSelledCombat(int price)
        {
            return price;
        }


        public virtual void OnPlaceToCombatSlot(TavernCombatSlot tavernCombatSlot)
        {

        }

        public virtual void OnBattleRoundStarted()
        {

        }
        
        public virtual void OnRoundEnded() 
        {
        
        }

        public virtual void OnRoundStarted()
        {

        }

        public virtual void OnBaseUpgrade()
        {

        }

        public virtual void OnCardTriple()
        {

        }


    }

    [Serializable]
    public class EmptyCardScript : CardScript
    {
        public EmptyCardScript(CardType type, TavernPlayerNode player) : base(type, player)
        {
        }
    }
}
