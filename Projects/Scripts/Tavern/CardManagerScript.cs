using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scripts.Tavern;
using PatcherYRpp;
using Newtonsoft.Json;
using DynamicPatcher;

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
            bool deploy = false;

            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if(mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);
                deploy = true;
            }

            if (PlayerNode is null)
                return;

            if (deploy)
            {
                PlayerNode.OnRefreshShop();
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(pTarget.CastToTechno(out var ptechno))
            {
                var ext = TechnoExt.ExtMap.Find(ptechno);
                if (ext.IsNullOrExpired())
                    return;

                var shopSlot = ext.GameObject.GetComponent<TavernShopSlot>();
                if (shopSlot is not null)
                {
                    var temp = PlayerNode.TavernTempSlots.Where(x => x.CurrentCard == null).FirstOrDefault();
                    if(temp is not null)
                    {
                        if (shopSlot.CurrentCard != null)
                        {
                            var cardType = shopSlot.TakeCard();

                            temp.AddCard(cardType);

                            //显示购买卡牌消耗的资金
                            TavernGameManager.Instance.ShowFlyingTextAt($"-${300}", pTarget.Ref.GetCoords() + new PatcherYRpp.CoordStruct(0, 0, 200), 1);
                        }
                    }
                }
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
