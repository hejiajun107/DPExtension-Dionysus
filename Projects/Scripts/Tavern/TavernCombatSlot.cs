using DynamicPatcher;
using Extension.EventSystems;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using Scripts.Tavern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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


        public bool IsEnabled { get; set; } = true;


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
          


            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if(mission.Ref.CurrentMission == Mission.Selling)
            {
                mission.Ref.ForceMission(Mission.Stop);
                if (TavernGameManager.Instance is null)
                    return;
                //仅允许准备阶段变卖
                if (TavernGameManager.Instance.GameStatus == GameStatus.Ready)
                {
                    OnSell(true, true);
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

            node.RegisterCombatSlot(this);
            _registered = true;

            return true;
        }

        public void ChangeCard(CardType cardType,bool destroyOldCards,bool destroyBuffCards) 
        {
            if(CurrentCardType is null)
            {
                CurrentCardType = cardType;

                if (destroyOldCards)
                {
                    CardRecords.RemoveAll(x => !x.IsPersist);
                }

                CardLevel = 1;
            }
            else
            {
                if (CardLevel < 3)
                {
                    CardLevel++;

                    if (CardLevel == 3)
                    {
                        //todo 获得三连奖励
                    }
                }
            }

            foreach(var techno in cardType.Technos)
            {
                for (var i = 0; i < techno.Count; i++) 
                {
                    CardRecords.Add(new CardRecord()
                    {
                        CardType = cardType,
                        IsPersist = techno.IsPersist,
                        Techno = techno.Key
                    });
                }
            }

            RefreshAggregates();
        }

        public void OnSell(bool destroyOldCards, bool destroyBuffCards)
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

                TavernGameManager.Instance.ShowFlyingTextAt("+$100",Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0,0,200));
            }    
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (pAttacker.IsNull)
                return;

            if (!IsEnabled)
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
                    if (CurrentCardType is not null && CurrentCardType.Key != tempSlot.CurrentCard.Key && CardLevel < 3)
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
            _aggregates = CardRecords.GroupBy(x => x.Techno).Select(x => new CardAggregate
            {
                Techno = x.Key,
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

                if(CurrentCardType != null)
                {
                    GameUtil.RenderCameo(CurrentCardType.Cameo, Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(-250, 0, 50));

                    //绘制三连，三连
                    if (CardLevel > 1)
                    {
                        if (FileSystem.TyrLoadSHPFile("combo.shp", out Pointer<SHPStruct> pCustomSHP))
                        {
                            Pointer<Surface> pSurface = Surface.Current;
                            RectangleStruct rect = pSurface.Ref.GetRect();
                            Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(-250, 0, 50));
                            pSurface.Ref.DrawSHP(FileSystem.UNITx_PAL, pCustomSHP, CardLevel == 2 ? 0 : 1, point, rect.GetThisPointer(), BlitterFlags.None);
                        }
                    }
                }

                int zCount = 0;
                foreach(var card in _aggregates)
                {
                    var tCameo = TavernGameManager.Instance.GetTechnoCameo(card.Techno);
                    if (!string.IsNullOrWhiteSpace(tCameo))
                    {
                        GameUtil.RenderCameo(tCameo,Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(-250, 0, 50 + 450 + zCount * 100));
                    }
                    DrawNumber(card.Count.ToString(), -240, 0, 50 + 450 + zCount * 100);
                    zCount++;
                }
            }
        }

        private void DrawNumber(string txt, int offsetX, int offsetY, int offsetZ)
        {
            Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(offsetX, offsetY, offsetZ));
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

        public string Techno { get; set; }

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
        //public CardType CardType { get; set; }
        public string Techno { get; set; }

        /// <summary>
        /// 计数
        /// </summary>
        public int Count { get; set; }
    }

  

}
