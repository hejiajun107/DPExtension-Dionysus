using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scripts.Tavern;
using PatcherYRpp.Utilities;
using DynamicPatcher;
using PatcherYRpp;

namespace Scripts.Tavern
{
    [ScriptAlias(nameof(TavernPlayerNode))]
    [Serializable]
    public class TavernPlayerNode : TechnoScriptable
    {
        public TavernPlayerNode(TechnoExt owner) : base(owner)
        {
        }

        public TechnoExt CardManager { get; private set; }

        /// <summary>
        /// 奖励区
        /// </summary>
        public List<TavernRewardSlot> TavernRewardSlots { get; private set; } = new List<TavernRewardSlot>();
        /// <summary>
        /// 暂存区
        /// </summary>
        public List<TavernTempSlot> TavernTempSlots { get; private set; } = new List<TavernTempSlot>();
        /// <summary>
        /// 上场区
        /// </summary>
        public List<TavernCombatSlot> TavernCombatSlots { get; private set; } = new List<TavernCombatSlot>();

        /// <summary>
        /// 商店区
        /// </summary>
        public List<TavernShopSlot> TavernShopSlots { get; private set; } = new List<TavernShopSlot>();
        /// <summary>
        /// 指挥官区
        /// </summary>
        public TavernCommanderSlot CommanderSlot { get; private set; } = null;

        /// <summary>
        /// 是否已经注册到GameManger
        /// </summary>
        private bool _registed = false;

        /// <summary>
        /// 基地等级
        /// </summary>
        public int BaseLevel { get; private set; } = 1;

        /// <summary>
        /// 准备状态
        /// </summary>
        public bool IsReady {get;set;} = false;

        /// <summary>
        /// 跳过状态
        /// </summary>
        public bool VoteSkiped { get; set; } = false;


        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// 候选池
        /// </summary>
        public List<string> CommanderPool { get; set; } = new List<string>();

        public Random NRandom { get; private set; }

        public Queue<CardType> CardCacheQueue { get; set; } = new Queue<CardType>();


        public List<SellRecord> SellRecords { get; set; } = new List<SellRecord>();
        public List<SellRecord> CurrentRoundSellRecords { get; set; } = new List<SellRecord>();

        /// <summary>
        /// 距离上次上级x回合
        /// </summary>
        public int RoundAfterUpgrade { get; private set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public void OnRoundEnded()
        {
            RoundAfterUpgrade++;
        }


        public override void OnUpdate()
        {
            if (!Register())
                return;

            //从缓冲区加入暂存区
            if(CardCacheQueue.Count() > 0)
            {
                var tempSlot = TavernTempSlots.Where(x => x.IsEnabled && x.CurrentCard == null).FirstOrDefault();
                if(tempSlot is not null)
                {
                    tempSlot.AddCard(CardCacheQueue.Dequeue());
                }
            }
        }

        public void InitRandom()
        {
            if(NRandom is null)
            {
                CardManager.OwnerObject.Ref.Base.Scatter(CardManager.OwnerObject.Ref.Base.Base.GetCoords(),true,true);
                var pfoot = CardManager.OwnerObject.Convert<FootClass>();
                var seed = 0;
                if(pfoot.Ref.Destination.IsNotNull)
                {
                    seed = pfoot.Ref.Destination.Ref.GetCoords().X + pfoot.Ref.Destination.Ref.GetCoords().Y + pfoot.Ref.Destination.Ref.GetCoords().Z;
                }
                NRandom = new Random(seed);
            }
        }

        /// <summary>
        /// 将当前节点注册到GameManager
        /// </summary>
        private bool Register()
        {
            if (_registed)
                return true;

            if(TavernGameManager.Instance is not null)
            {
                TavernGameManager.Instance.RegisterNode(this);
                _registed = true;
                return true;
            }
            return false;
        }



        #region 注册初始区域
        public void RegisterRewardSlot(TavernRewardSlot slot)
        {
            TavernRewardSlots.Add(slot);
            TavernRewardSlots = TavernRewardSlots.OrderBy(x => x.Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords())).ToList();
        }

        public void RegisterCombatSlot(TavernCombatSlot slot)
        {
            TavernCombatSlots.Add(slot);
            TavernCombatSlots = TavernCombatSlots.OrderBy(x => x.Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords())).ToList();
        }

        public void RegisterTempSlot(TavernTempSlot slot)
        {
            TavernTempSlots.Add(slot);
            TavernTempSlots = TavernTempSlots.OrderBy(x => x.Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords())).ToList();
        }

        public void RegisterShopSlot(TavernShopSlot slot)
        {
            TavernShopSlots.Add(slot);
            TavernShopSlots = TavernShopSlots.OrderBy(x => x.Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords())).ToList();
        }

        public void RegisterCommanderSlot(TavernCommanderSlot slot)
        {
            CommanderSlot = slot;
        }


        public void RegisterCardManager(TechnoExt cardManager)
        {
            CardManager = cardManager;
        }
        #endregion


        /// <summary>
        /// 刷新卡池
        /// </summary>
        public void OnRefreshShop(bool free = false)
        {
            if (!free)
            {
                if(Owner.OwnerObject.Ref.Owner.Ref.Available_Money() < TavernGameManager.Instance.RulesRefreshPrice)
                {
                    TavernGameManager.Instance.SoundNoMoney();
                    //提示钱不够
                    return;
                }
                else
                {
                    Owner.OwnerObject.Ref.Owner.Ref.TransactMoney(-TavernGameManager.Instance.RulesRefreshPrice);
                    TavernGameManager.Instance.ShowFlyingTextAt($"-${TavernGameManager.Instance.RulesRefreshPrice}", Owner.OwnerObject.Ref.Base.Base.GetCoords() + new PatcherYRpp.CoordStruct(0, 0, 500), 1);
                }
            }
                  
            var enabledSlots = TavernShopSlots.Where(x => x.IsEnabled).ToList();
            var avaibleCards = TavernGameManager.Instance.GetAvailableCardPools(BaseLevel);
            
            var result = new HashSet<int>();
            var rng = NRandom;

            while (result.Count < enabledSlots.Count())
            {
                int num = rng.Next(avaibleCards.Count);
                result.Add(num);
            }

            var idx = 0;
            var resultArr = result.ToArray();

            foreach(var slot in enabledSlots)
            {
                slot.ChangeCard(TavernGameManager.Instance.CardTypes[avaibleCards[resultArr[idx]]]);
                idx++;
            }
        }

        /// <summary>
        /// 升级酒馆
        /// </summary>
        public void OnUpgrade()
        {
            if(BaseLevel < TavernGameManager.Instance.BaseMaxLevel)
            {
                BaseLevel++;
                //解锁槽位
                var temp = TavernTempSlots.Where(x => !x.IsEnabled).FirstOrDefault();

                if (temp is not null) 
                {
                    temp.IsEnabled = true;
                }
               
                var combat = TavernCombatSlots.Where(x => !x.IsEnabled).FirstOrDefault();

                if (combat is not null)
                {
                    combat.IsEnabled = true;
                }

                var shop = TavernShopSlots.Where(x => !x.IsEnabled).FirstOrDefault();

                if (shop is not null)
                {
                    shop.IsEnabled = true;
                }

                foreach(var slot in TavernCombatSlots)
                {
                    slot.CardScript?.OnBaseUpgrade() ;
                }

                RoundAfterUpgrade = 0;
            }
        }
    }

    [Serializable]
    public class SellRecord
    {
        public string Key { get; set; }

        public CardType Card { get; set; }
    }
}
