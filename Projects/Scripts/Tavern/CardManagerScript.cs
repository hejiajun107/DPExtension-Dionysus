using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scripts.Tavern;
using PatcherYRpp;

namespace Scripts.Tavern
{
    /// <summary>
    /// 发牌员
    /// </summary>
    [ScriptAlias(nameof(CardManagerScript))]
    [Serializable]
    public class CardManagerScript : TechnoScriptable
    {
        public CardManagerScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnUpdate()
        {
            if (PlayerNode is null)
                return;
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(pTarget.CastToTechno(out var ptechno))
            {
                var ext = TechnoExt.ExtMap.Find(ptechno);
                if (ext.IsNullOrExpired())
                    return;


            }
        }

        public TavernPlayerNode PlayerNode { get {
                if (_playerNode is null) {
                    if (TavernGameManager.Instance is not null) {
                        _playerNode = TavernGameManager.Instance.FindPlayerNodeByHouse(Owner.OwnerObject.Ref.Owner);
                    }
                }
                return _playerNode;
            } }

        private TavernPlayerNode _playerNode = null;
    }
}
