using DynamicPatcher;
using Extension.EventSystems;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using Scripts.Tavern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Scripts.Tavern
{
    [ScriptAlias(nameof(TavernCombatSlot))]
    [Serializable]
    public class TavernCombatSlot : TechnoScriptable
    {
        public TavernCombatSlot(TechnoExt owner) : base(owner)
        {
        }

        public List<CardRecord> CardRecords { get; private set; } = new List<CardRecord>();

        private List<CardAggregate> _aggregates = new List<CardAggregate>();

        public CardType CurrentCardType { get; private set; }

        /// <summary>
        /// 1 2 3连
        /// </summary>
        public int CardLevel { get; private set; } = 1;

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

        public void ChangeCard(CardType cardType,bool destroyOldCards,bool destroyBuffCards) 
        {
            if(CurrentCardType is null)
            {
                CurrentCardType = cardType;
            }
            else
            {
                //三连
            }

            if (destroyOldCards)
            {
                CardRecords.RemoveAll(x => !x.IsPersist);
            }

            CardRecords.Add(new CardRecord()
            {
                CardType = cardType,
                IsPersist = false,
            });

            CardRecords.Add(new CardRecord()
            {
                CardType = TavernGameManager.Instance.CardTypes["MTNK"],
                IsPersist = false,
            });

            CardRecords.Add(new CardRecord()
            {
                CardType = TavernGameManager.Instance.CardTypes["MTNK"],
                IsPersist = false,
            });

            RefreshAggregates();
        }

        public void OnSale(bool destroyOldCards, bool destroyBuffCards)
        {
            if(CurrentCardType is not null)
            {
                CurrentCardType = null;
                CardLevel = 0;
                if(destroyOldCards)
                {
                    CardRecords.RemoveAll(x => !x.IsPersist);
                    RefreshAggregates();
                }
            }    
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

                //从暂存区来的卡牌
                var tempSlot = ext.GameObject.GetComponent<TavernTempSlot>();
                if (tempSlot is not null)
                {
                    if (tempSlot.CurrentCard is null)
                    {
                        return;
                    }

                    //需要当前卡槽为空或者是相同卡牌
                    if (CurrentCardType is not null && CurrentCardType.Key != tempSlot.CurrentCard.Key)
                    {
                        return;
                    }

                    var card = tempSlot.RemoveCard();
                    ChangeCard(card, true, true);
                    return;
                }

            }
        }

        /// <summary>
        /// 重新计算卡牌计数
        /// </summary>
        public void RefreshAggregates()
        {
            _aggregates = CardRecords.GroupBy(x => x.CardType.Key).Select(x => new CardAggregate
            {
                CardType = x.FirstOrDefault().CardType,
                Count = x.Count()
            }).ToList();
        }

        public void OnGScreenRender(object sender, EventArgs args)
        {
            if (args is GScreenEventArgs gScreenEvtArgs)
            {
                if (!gScreenEvtArgs.IsLateRender)
                {
                    return;
                }

                if(CurrentCardType != null)
                {
                    RenderPCX(CurrentCardType.Cameo, -250, 0, 50);
                }

                int zCount = 0;
                foreach(var card in _aggregates)
                {
                    RenderPCX(card.CardType.Cameo, -250, 0, 50 + 450 + zCount * 100);
                    DrawNumber(card.Count.ToString(), -240, 0, 50 + 450 + zCount * 100);
                    zCount++;
                }
            }
        }

        private void RenderPCX(string pcxName,int offsetX,int offsetY, int offsetZ)
        {
            var loaded = PCX.Instance.LoadFile(pcxName);
            var pcx = PCX.Instance.GetSurface(pcxName, Pointer<BytePalette>.Zero);
            RectangleStruct pcxBounds = new RectangleStruct(0, 0, pcx.Ref.Base.Base.Width, pcx.Ref.Base.Base.Height);
            Pointer<Surface> pSurface = Surface.Current;
            RectangleStruct rect = pSurface.Ref.GetRect();
            Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.BaseAbstract.GetCoords() + new CoordStruct(offsetX, offsetY, offsetZ));
            var source = new RectangleStruct(point.X, point.Y, pcx.Ref.Base.Base.Width, pcx.Ref.Base.Base.Height);
            PCX.Instance.BlitToSurface(source.GetThisPointer(), pSurface.Convert<DSurface>(), pcx);
        }

        private void DrawNumber(string txt, int offsetX, int offsetY, int offsetZ)
        {
            Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.BaseAbstract.GetCoords() + new CoordStruct(offsetX, offsetY, offsetZ));
            var source = new RectangleStruct(point.X, point.Y, 60, 48);
            Pointer<Surface> pSurface = Surface.Current;
            var point2 = new Point2D(2, 32);
            pSurface.Ref.DrawText(txt, source.GetThisPointer(), point2.GetThisPointer(), new ColorStruct(0, 255, 0));
        }
    }

    [Serializable]
    public class CardRecord
    {
        /// <summary>
        /// 类型
        /// </summary>
        public CardType CardType { get; set; }

        /// <summary>
        /// 是否是永久卡
        /// </summary>
        public bool IsPersist { get; set; }
    }

    [Serializable]
    public class CardAggregate
    {
        /// <summary>
        /// 类型
        /// </summary>
        public CardType CardType { get; set; }

        /// <summary>
        /// 计数
        /// </summary>
        public int Count { get; set; }
    }
}
