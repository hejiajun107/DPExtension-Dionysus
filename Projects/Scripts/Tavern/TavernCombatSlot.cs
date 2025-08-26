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
    [ScriptAlias(nameof(TavernCombatSlot))]
    [Serializable]
    public class TavernCombatSlot : TechnoScriptable
    {
        public TavernCombatSlot(TechnoExt owner) : base(owner)
        {
        }

        public CardComponent CurrentCard { get; private set; }

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

            node.RegisterCombatSlot(this);
            _registered = true;

            return true;
        }
    }
}
