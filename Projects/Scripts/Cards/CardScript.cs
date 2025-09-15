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

        public CardScript(CardType type)
        {
            Type = type;
        }

        public object Slot { get; private set; }
        
        public TavernPlayerNode Player { get; private set; }

        public virtual void OnBought()
        {

        }

        public virtual void OnSelled()
        {

        }

        public virtual void OnPutToCombatSlot(TavernCombatSlot tavernCombatSlot)
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
