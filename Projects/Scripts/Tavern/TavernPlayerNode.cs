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

namespace Scripts.Tavern
{
    [ScriptAlias(nameof(TavernPlayerNode))]
    [Serializable]
    public class TavernPlayerNode : TechnoScriptable
    {
        public TavernPlayerNode(TechnoExt owner) : base(owner)
        {
        }

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
        /// 是否已经注册到GameManger
        /// </summary>
        private bool _registed = false;

        /// <summary>
        /// 基地等级
        /// </summary>
        public int BaseLevel { get; private set; } = 1;

        public override void OnUpdate()
        {
            if (!Register())
                return;
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
        #endregion

        /// <summary>
        /// 刷新卡池
        /// </summary>
        public void OnRefreshShop()
        {
            var enabledSlots = TavernShopSlots.Where(x => x.IsEnabled).ToList();
            var avaibleCards = TavernGameManager.Instance.GetAvailableCardPools(BaseLevel);
            
            var result = new HashSet<int>();
            var rng = MathEx.Random;

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

                //显示升本花费的资金
                TavernGameManager.Instance.ShowFlyingTextAt($"-${1000}", Owner.OwnerObject.Ref.Base.Base.GetCoords() + new PatcherYRpp.CoordStruct(0, 0, 500), 1);
            }
        }
    }
}
