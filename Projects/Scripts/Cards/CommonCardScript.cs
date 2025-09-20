using DynamicPatcher;
using Extension.INI;
using Scripts.Tavern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Cards
{
    [Serializable]
    public class CommonCardScript : CardScript
    {
        public CommonCardScript(CardType type, TavernPlayerNode player) : base(type, player)
        {
        }

        public List<CommonCardTrigger> Triggers { get; private set; } = new List<CommonCardTrigger>();

        public override void OnAwake()
        {
            var ini = TavernGameManager.Instance.CreateRulesIniComponentWith<CommonCardScriptData>(Type.Key);
          
            var dtype = typeof(CommonCardScriptData);

            for (var i = 1; i <= 1; i++) 
            {
                var evt = (dtype.GetField($"Event{i}").GetValue(ini.Data)) as string;
                if (string.IsNullOrWhiteSpace(evt))
                {
                    continue;
                }
            }
        }

        public override void OnBought()
        {
            base.OnBought();
        }

        public override void OnRoundStarted()
        {
            base.OnRoundStarted();
        }

        public override void OnRoundEnded()
        {
            base.OnRoundStarted();
        }

    }

    [Serializable]

    public class CommonCardTrigger
    {
        public CommonCardEvent Event { get; set; }

        public CommonCardAction Action { get; set; }


    }

    public enum CommonCardEvent
    {
        /// <summary>
        /// 卡牌被购买时触发
        /// </summary>
        OnBought,
        /// <summary>
        /// 卡牌上场时触发
        /// </summary>
        OnCombatPut,
        /// <summary>
        /// 回合开始时触发
        /// </summary>
        OnRoundStart,
        /// <summary>
        /// 回合结束时触发
        /// </summary>
        OnRoundEnd,
        /// <summary>
        /// 卡牌在场上被卖出时触发
        /// </summary>
        OnSelledCombat,
        /// <summary>
        /// 卡牌被卖出时触发
        /// </summary>
        OnSelled,
    }

    public enum CommonCardAction
    {
        /// <summary>
        /// 增加指定卡
        /// </summary>
        Add,
        /// <summary>
        /// 转化指定卡
        /// </summary>
        Convert,
        /// <summary>
        /// 夺取指定卡
        /// </summary>
        Rob
    }

    public enum CommonAffectRange
    {
        /// <summary>
        /// 自身卡槽
        /// </summary>
        Self,
        /// <summary>
        /// 左侧卡槽
        /// </summary>
        LeftSide,
        /// <summary>
        /// 右侧卡册
        /// </summary>
        RightSide,
        /// <summary>
        /// 两边卡槽
        /// </summary>
        BothSide,
        /// <summary>
        /// 除了自己外的所有卡槽
        /// </summary>
        AllExceptSelf,
        /// <summary>
        /// 所有卡槽
        /// </summary>
        All
    }

    

    public class CommonCardScriptData : INIAutoConfig
    {
        /// <summary>
        /// 什么时候触发，对应CommonCardEvent
        /// </summary>
        [INIField(Key = "CommonCardScript.Event1")]
        public string Event1 = "";
        /// <summary>
        /// 触发时的动作对应CommonCardAction
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1")]
        public string Action1 = "";
        /// <summary>
        /// 响应触发动作的对象，可以是Key,Tag,多个以,隔开
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1Filter")]
        public string Action1Filter = "";
        /// <summary>
        /// 响应对象的范围，对应CommonAffectRange
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1FilterRange")]
        public string Action1FilterRange = "";
        /// <summary>
        /// 响应的结果，对应CommonCardAction
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1Target")]
        public string Action1Target = "";
        /// <summary>
        /// 响应结果作用于哪对应CommonAffectRange
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1AffectRange")]
        public string Action1AffectRange = "";
        /// <summary>
        /// 响应结果的转化率，默认1比1
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1Rate")]
        public int Action1Rate = 1;

        [INIField(Key = "CommonCardScript.Action1FilterCellSpread")]
        public int Action1FilterCellSpread = 1;
        [INIField(Key = "CommonCardScript.Action1TargetCellSpread")]
        public int Action1TargetCellSpread = 1;
    }

    
}
