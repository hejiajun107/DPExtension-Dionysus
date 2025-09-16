using Extension.INI;
using Scripts.Tavern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Cards
{
    [Serializable]
    public class CommonCardScript : CardScript
    {
        public CommonCardScript(CardType type, TavernPlayerNode player) : base(type, player)
        {
        }

        public override void OnAwake()
        {
            TavernGameManager.Instance.CreateRulesIniComponentWith<CommonCardScriptData>(Type.Key);
        }


    }

    public class CommonCardScriptData : INIAutoConfig
    {
        public string Event1 = "";

        public string Action1 = "";


    }

    
}
