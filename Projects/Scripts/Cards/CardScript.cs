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

        public virtual void OnSelled()
        {

        }

        public virtual void OnSelledCombat()
        {

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


    }
}
