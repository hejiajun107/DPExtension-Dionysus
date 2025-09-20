using DynamicPatcher;
using Extension.EventSystems;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using Newtonsoft.Json;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using Scripts.Cards;
using Scripts.Tavern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private bool _registered = false;

     
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

            if (!_registered)
            {
                _registered = true;
                PlayerNode.RegisterCardManager(Owner);
            }


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
                    if(Owner.OwnerObject.Ref.Owner.Ref.Available_Money()<TavernGameManager.Instance.RulesBuyCardPrice)
                    {
                        //提示金钱不足
                        TavernGameManager.Instance.SoundNoMoney();
                        return;
                    }



                    var temp = PlayerNode.TavernTempSlots.Where(x => x.CurrentCard == null).FirstOrDefault();
                    if(temp is not null)
                    {
                        if (shopSlot.CurrentCard != null)
                        {
                            var cardType = shopSlot.TakeCard();

                            temp.AddCard(cardType);
                            temp.CardScript?.OnBought();

                            Owner.OwnerObject.Ref.Owner.Ref.TransactMoney(-TavernGameManager.Instance.RulesBuyCardPrice);
                            //显示购买卡牌消耗的资金
                            TavernGameManager.Instance.ShowFlyingTextAt($"-${TavernGameManager.Instance.RulesBuyCardPrice}", pTarget.Ref.GetCoords() + new PatcherYRpp.CoordStruct(0, 0, 200), 1);
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

        public override void Awake()
        {
            EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
            base.Awake();
        }

        public override void OnDestroy()
        {
            EventSystem.GScreen.RemoveTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
            base.OnDestroy();
        }

        public void OnGScreenRender(object sender, EventArgs args)
        {
            if (args is GScreenEventArgs gScreenEvtArgs)
            {
                if (!gScreenEvtArgs.IsLateRender)
                {
                    return;
                }

                if (TavernGameManager.Instance is null)
                    return;

                if (TavernGameManager.Instance.GameStatus == GameStatus.Ready && Owner.OwnerObject.Ref.Owner == HouseClass.Player)
                {
                    DrawTicks(TavernGameManager.Instance.ReadyStatusTick.ToString(), 0, 0, -80);
                }
            }
        }

        private void DrawTicks(string txt, int offsetX, int offsetY, int offsetZ)
        {
            Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(offsetX, offsetY, offsetZ));
            var source = new RectangleStruct(point.X, point.Y, 60, 48);
            Pointer<Surface> pSurface = Surface.Current;
            var point2 = new Point2D(2, 32);
            pSurface.Ref.DrawText(txt, source.GetThisPointer(), point2.GetThisPointer(), new ColorStruct(0, 255, 0));
        }

    }

    
}
