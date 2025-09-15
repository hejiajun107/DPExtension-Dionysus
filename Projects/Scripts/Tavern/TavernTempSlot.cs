using Extension.EventSystems;
using Extension.Ext;
using Extension.Script;
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
    [ScriptAlias(nameof(TavernTempSlot))]
    [Serializable]
    public class TavernTempSlot : TechnoScriptable
    {
        public TavernTempSlot(TechnoExt owner) : base(owner)
        {
        }

        public CardType CurrentCard { get; private set; } = null;
        public CardScript CurrentScript { get; private set; }


        public bool IsEnabled { get; set; } = true;

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Selling)
            {
                mission.Ref.ForceMission(Mission.Stop);

                if (TavernGameManager.Instance is null)
                    return;
                //仅允许准备阶段变卖
                if (TavernGameManager.Instance.GameStatus == GameStatus.Ready)
                {
                    OnSell();
                }
            }

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

            node.RegisterTempSlot(this);
            _registered = true;
            return true;
        }

        public void AddCard(CardType card)
        {
            var script = ScriptManager.GetScript(nameof(CardComponent));
            var scriptComponent = ScriptManager.CreateScriptableTo(Owner.GameObject, script, Owner);
            if (scriptComponent is CardComponent cardComponent)
            {
                cardComponent.CardType = card;
                CurrentCard = card;
            }
        }

        public void OnSell()
        {
            this.RemoveCard();
        }

        /// <summary>
        /// 移除当前卡牌
        /// </summary>
        /// <returns></returns>
        public CardType RemoveCard() {
            if(this.CurrentCard is not null)
            {
                var card = CurrentCard;
                var old = GameObject.GetComponent<CardComponent>();
                if (old is not null)
                {
                    old.DetachFromParent();
                    old.RelaseCompnent();
                    old = null;
                }
                CurrentCard = null;
                return card;
            }
            return null;
        }


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

                //绘制禁用标识
                if (!IsEnabled)
                {
                    if (FileSystem.TyrLoadSHPFile("banned.shp", out Pointer<SHPStruct> pCustomSHP))
                    {
                        Pointer<Surface> pSurface = Surface.Current;
                        RectangleStruct rect = pSurface.Ref.GetRect();
                        Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(-250, 0, 50));
                        pSurface.Ref.DrawSHP(FileSystem.UNITx_PAL, pCustomSHP, 0, point, rect.GetThisPointer(), BlitterFlags.None);
                    }
                }
            }
        }
    }
}
