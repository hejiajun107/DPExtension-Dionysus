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
    [ScriptAlias(nameof(CardComponentBattle))]
    [Serializable]
    public class CardComponentBattle : TechnoScriptable
    {
        public CardComponentBattle(TechnoExt owner) : base(owner)
        {
        }
    }
}
