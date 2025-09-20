using DynamicPatcher;
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
    /// <summary>
    /// 商店区
    /// </summary>
    [ScriptAlias(nameof(TavernShopSlot))]
    [Serializable]
    public class TavernShopSlot : TechnoScriptable
    {
        public TavernShopSlot(TechnoExt owner) : base(owner)
        {
        }

        public bool IsEnabled { get; set; } = true;

        public CardType CurrentCard { get; private set; }

        public CardScript CardScript { get; private set; }

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

            node.RegisterShopSlot(this);
            _registered = true;
            return true;
        }

        public void ChangeCard(CardType cardType)
        {
            var old = GameObject.GetComponent<CardComponent>();
            if(old is not null)
            {
                old.DetachFromParent();
                old.RelaseCompnent();
                old = null;
                CardScript = null;
            }
            CurrentCard = cardType;
            var script = ScriptManager.GetScript(nameof(CardComponent));
            var scriptComponent = ScriptManager.CreateScriptableTo(Owner.GameObject, script,Owner);
            if(scriptComponent is CardComponent cardComponent)
            {
                cardComponent.CardType = cardType;
            }

            CardScript = TavernGameManager.Instance.CreateCardScript(cardType, TavernGameManager.Instance.FindPlayerNodeByHouse(Owner.OwnerObject.Ref.Owner));
            CardScript.Slot = this;
        }

        public CardType TakeCard(bool free = false)
        {
            var type = CurrentCard;
            var component = GameObject.GetComponent<CardComponent>();
            component.DetachFromParent();
            component.RelaseCompnent();
            component = null;
            CurrentCard = null;
            CardScript = null;
            return type;
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
