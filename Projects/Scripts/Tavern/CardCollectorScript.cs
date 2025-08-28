using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using Scripts.Tavern;

namespace Scripts.Tavern
{
    [ScriptAlias(nameof(CardCollectorScript))]
    [Serializable]
    public class CardCollectorScript : TechnoScriptable
    {
        public CardCollectorScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (pAttacker.IsNull)
                return;

            if (pAttacker.CastToTechno(out var ptechno))
            {
                var ext = TechnoExt.ExtMap.Find(ptechno);
                if (ext.IsNullOrExpired())
                    return;
            }
        }

        public TavernPlayerNode PlayerNode
        {
            get
            {
                if (_playerNode is null)
                {
                    if (TavernGameManager.Instance is not null)
                    {
                        _playerNode = TavernGameManager.Instance.FindPlayerNodeByHouse(Owner.OwnerObject.Ref.Owner);
                    }
                }
                return _playerNode;
            }
        }

        private TavernPlayerNode _playerNode = null;
    }
}
