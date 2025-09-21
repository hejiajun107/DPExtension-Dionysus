using DynamicPatcher;
using Extension.INI;
using Scripts.Tavern;
using System;
using System.Collections;
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

                var trigger = new CommonCardTrigger();

                trigger.Event = (CommonCardEvent)Enum.Parse(typeof(CommonCardEvent), evt);
                trigger.Action = (CommonCardAction)(Enum.Parse(typeof(CommonCardAction), (dtype.GetField($"Action{i}").GetValue(ini.Data)) as string));
                trigger.AffectRange = (CommonAffectRange)(Enum.Parse(typeof(CommonAffectRange), (dtype.GetField($"Action{i}AffectRange").GetValue(ini.Data)) as string));
                trigger.ActionFilter = (dtype.GetField($"Action{i}Filter").GetValue(ini.Data)) as string;
                trigger.ActionCardFilter = (dtype.GetField($"Action{i}CardFilter").GetValue(ini.Data)) as string;
                trigger.ActionTechnoResult = (dtype.GetField($"Action{i}TechnoResult").GetValue(ini.Data)) as string;
                trigger.ActionCardResult = (dtype.GetField($"Action{i}CardResult").GetValue(ini.Data)) as string;
                trigger.ActionTechnoResultCount = int.Parse(dtype.GetField($"Action{i}TechnoResultCount").GetValue(ini.Data).ToString());

                Triggers.Add(trigger);
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
            var triggers = Triggers.Where(x => x.Event == CommonCardEvent.OnRoundEnd).ToList();
            if(Slot is TavernCombatSlot currentSlot)
            {
                foreach (var trigger in triggers)
                {
                    var slots = GetAffectSlots(currentSlot, trigger.AffectRange, 1);

                    foreach (var slot in slots)
                    {
                        for (var i = 0; i < trigger.ActionTechnoResultCount; i++)
                        {
                            if(slot.IsEnabled && slot.CurrentCardType is not null)
                            {
                                slot.CardRecords.Add(new CardRecord() { Techno = trigger.ActionTechnoResult, CardType = TavernGameManager.Instance.CardTypes[trigger.ActionCardResult], IsPersist = false });
                            }
                        }
                        slot.RefreshAggregates();
                    }
                }
            }
         
        }


        private List<TavernCombatSlot> GetAffectSlots(TavernCombatSlot current,CommonAffectRange range,int spread)
        {
            var slots = new List<TavernCombatSlot>();

            var node = Player;

            var currentIndex = Player.TavernCombatSlots.IndexOf(current);

            switch (range)
            {
                case CommonAffectRange.Self:
                    {
                        slots.Add(current);
                        break;
                    }
                case CommonAffectRange.LeftSide:
                    {
                        var start = currentIndex - spread;
                        if (start < 0)
                            start = 0;

                        if(start<currentIndex)
                        {
                            slots.AddRange(node.TavernCombatSlots.Skip(start).Take(currentIndex - start).ToList());
                        }

                        break;
                    }
                case CommonAffectRange.RightSide:
                    {
                        var end = currentIndex + spread;
                        
                        if(end > node.TavernCombatSlots.Count() - 1)
                        {
                            end = node.TavernCombatSlots.Count() - 1;
                        }

                        if(end>currentIndex)
                        {
                            slots.AddRange(node.TavernCombatSlots.Skip(currentIndex).Take(end - currentIndex).ToList());
                        }
                        break;
                    }
                case CommonAffectRange.BothSide:
                    {
                        var start = currentIndex - spread;
                        var end = currentIndex + spread;

                        if (start < 0)
                        {
                            start = 0;
                        }

                        if (end > node.TavernCombatSlots.Count() - 1)
                        {
                            end = node.TavernCombatSlots.Count() - 1;
                        }

                        if (end > start)
                        {
                            slots.AddRange(node.TavernCombatSlots.Skip(start).Take(end - start + 1).Where(x => x != current).ToList());
                        }

                        break;
                    }
                case CommonAffectRange.AllExceptSelf:
                    {
                        slots.AddRange(node.TavernCombatSlots.Where(x => x != current).ToList());
                        break;
                    }
                case CommonAffectRange.All:
                    {
                        slots.AddRange(node.TavernCombatSlots);
                        break;
                    }
                default:
                    break;
            }


            return slots;
        }

    }

    [Serializable]

    public class CommonCardTrigger
    {
        public CommonCardEvent Event { get; set; }

        public CommonCardAction Action { get; set; }

        public CommonAffectRange AffectRange { get; set; }

        public string ActionFilter { get; set; }

        public string ActionCardFilter { get; set; }

        

        public string ActionTechnoResult { get; set; }
        public int ActionTechnoResultCount { get; set; } = 1;
        public string ActionCardResult { get; set; }


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
        /// 限定生效的卡牌，可以是Key,Tag
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1CardFilter")]
        public string Action1CardFilter;

        /// <summary>
        /// 响应对象的范围，对应CommonAffectRange
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1FilterRange")]
        public string Action1FilterRange = "";
        /// <summary>
        /// 响应的结果，对应CommonCardAction
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1TechnoResult")]
        public string Action1TechnoResult = "";
        [INIField(Key = "CommonCardScript.Action1TechnoResultCount")]
        public int Action1TechnoResultCount = 1;
        /// <summary>
        /// 响应的结果所属卡面
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1CardResult")]
        public string Action1CardResult = "";

        /// <summary>
        /// 响应结果作用于哪对应CommonAffectRange
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1AffectRange")]
        public string Action1AffectRange = "Self";
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
