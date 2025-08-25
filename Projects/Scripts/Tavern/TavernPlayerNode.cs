using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scripts.Tavern;

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
        /// 是否已经注册到GameManger
        /// </summary>
        private bool _registed = false;

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
            TavernRewardSlots = TavernRewardSlots.OrderByDescending(x => x.Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords())).ToList();
        }

        public void RegisterCombatSlot(TavernCombatSlot slot)
        {
            TavernCombatSlots.Add(slot);
            TavernCombatSlots = TavernCombatSlots.OrderByDescending(x => x.Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords())).ToList();
        }

        public void RegisterTempSlot(TavernTempSlot slot)
        {
            TavernTempSlots.Add(slot);
            TavernTempSlots = TavernTempSlots.OrderByDescending(x => x.Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords())).ToList();
        }
        #endregion

        /// <summary>
        /// 刷新卡池
        /// </summary>
        public void OnRefreshCardPool()
        {

        }

        /// <summary>
        /// 升级酒馆
        /// </summary>
        public void OnUpgrade()
        {

        }
    }
}
