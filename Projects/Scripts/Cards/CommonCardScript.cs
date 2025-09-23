using DynamicPatcher;
using Extension.INI;
using Jint;
using PatcherYRpp;
using Scripts.Tavern;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
                trigger.ActionCheckRange = (CommonAffectRange)(Enum.Parse(typeof(CommonAffectRange), (dtype.GetField($"Action{i}CheckRange").GetValue(ini.Data)) as string));
                trigger.ActionCheckKeywords = (dtype.GetField($"Action{i}CheckKeywords").GetValue(ini.Data)) as string;
                trigger.ActionAffectKeywords = (dtype.GetField($"Action{i}AffectKeywords").GetValue(ini.Data)) as string;
                trigger.ActionCheckCellSpread = int.Parse(dtype.GetField($"Action{i}CheckCellSpread").GetValue(ini.Data).ToString());
                trigger.ActionAffectCellSpread = int.Parse(dtype.GetField($"Action{i}AffectCellSpread").GetValue(ini.Data).ToString());


                trigger.ActionTechnoResult = (dtype.GetField($"Action{i}TechnoResult").GetValue(ini.Data)) as string;
                trigger.ActionCardResult = (dtype.GetField($"Action{i}CardResult").GetValue(ini.Data)) as string;
                trigger.ActionTechnoResultCount = (dtype.GetField($"Action{i}TechnoResultCount").GetValue(ini.Data).ToString());
                trigger.ActionInvokeScript = (dtype.GetField($"Action{i}InvokeScript").GetValue(ini.Data).ToString());


                Triggers.Add(trigger);
            }
        }

        public override void OnBought()
        {
            base.OnBought();
        }

        public override void OnPlaceToCombatSlot(TavernCombatSlot tavernCombatSlot)
        {
            var triggers = Triggers.Where(x => x.Event == CommonCardEvent.OnCombatPut).ToList();
            ExcuteTrigger(triggers, Slot);
        }

        public override void OnRoundStarted()
        {
            var triggers = Triggers.Where(x => x.Event == CommonCardEvent.OnRoundStart).ToList();
            ExcuteTrigger(triggers,Slot);
        }

        public override void OnRoundEnded()
        {
            var triggers = Triggers.Where(x => x.Event == CommonCardEvent.OnRoundEnd).ToList();
            ExcuteTrigger(triggers,Slot);
        }

        public override int OnSelledCombat(int price)
        {
            var triggers = Triggers.Where(x => x.Event == CommonCardEvent.OnSelledCombat).ToList();
            ExcuteTrigger(triggers,Slot);
            return base.OnSelledCombat(price);
        }

        private void ExcuteTrigger(List<CommonCardTrigger> triggers,object eventSlot)
        {
            if (eventSlot is TavernCombatSlot currentSlot)
            {
                foreach (var trigger in triggers)
                {
                    var slots = GetAffectSlots(currentSlot, trigger.AffectRange, trigger.ActionAffectKeywords, trigger.ActionAffectCellSpread);
                    var filterSlots = GetAffectSlots(currentSlot, trigger.ActionCheckRange, trigger.ActionCheckKeywords, trigger.ActionCheckCellSpread);

                    Engine engine = null;
                    if (!string.IsNullOrWhiteSpace(trigger.ActionInvokeScript))
                    {
                        engine = new Engine();
                        engine.Execute($"function doAction(matched,current,player) {{ {trigger.ActionInvokeScript}; }}");
                    }

                    foreach (var slot in slots)
                    {
                        if (trigger.Action == CommonCardAction.Add || trigger.Action == CommonCardAction.AddPermanent)
                        {
                            var count = ParseCountExpression(trigger.ActionTechnoResultCount, new CombatSlotsJSInvokeEntry(filterSlots), new CombatSlotJSInvokeEntry(slot));
                            for (var i = 0; i < count; i++)
                            {
                                if (slot.IsEnabled && slot.CurrentCardType is not null)
                                {
                                    slot.CardRecords.Add(new CardRecord() { Techno = trigger.ActionTechnoResult, CardType = TavernGameManager.Instance.CardTypes[trigger.ActionCardResult], IsPersist = trigger.Action == CommonCardAction.AddPermanent });
                                }
                            }
                        }
                        if(engine is not null)
                        {
                            engine.Invoke("doAction", new CombatSlotsJSInvokeEntry(filterSlots), new CombatSlotJSInvokeEntry(slot), new PlayerJSInvokeEntry(Player));
                        }
                        slot.RefreshAggregates();
                    }
                }
            }
        }


        private int ParseCountExpression(string count, CombatSlotsJSInvokeEntry slots, CombatSlotJSInvokeEntry slot)
        {
            if(int.TryParse(count, out var countValue)) 
                return countValue;

            using var engine = new Engine();
            var result = engine
               .Execute($"function calc(matched, current , player) {{ return {count}; }}")
               .Invoke("calc", slots, slot,new PlayerJSInvokeEntry(Player));

            return (int)result.AsNumber();
        }

        private List<TavernCombatSlot> GetAffectSlots(TavernCombatSlot current,CommonAffectRange range,string keywords,int spread)
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

            if (!string.IsNullOrWhiteSpace(keywords))
            {
                var arrayKeywords = keywords.Split(',');
                slots.Where(x => x.CurrentCardType != null && (arrayKeywords.Contains(x.CurrentCardType.Key) || x.CurrentCardType.Tags.Intersect(arrayKeywords).Any()));
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

        public CommonAffectRange ActionCheckRange { get; set; }

        public string ActionCheckKeywords { get; set; }

        public string ActionAffectKeywords { get; set; }

        

        public string ActionTechnoResult { get; set; }
        public string ActionTechnoResultCount { get; set; } = "1";
        public string ActionCardResult { get; set; }

        public int ActionCheckCellSpread { get; set; } = 1;
        public int ActionAffectCellSpread { get; set; } = 1;


        /// <summary>
        /// 触发动作调用的脚本
        /// </summary>
        public string ActionInvokeScript { get; set; } = string.Empty;

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
        AddPermanent,

        /// <summary>
        /// 转化指定卡
        /// </summary>
        Convert,
        ConvertPermanent,

        /// <summary>
        /// 夺取指定卡
        /// </summary>
        Move,
        MovePermanent
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
        /// 响应对象的范围关键词，支持key,tag,以,隔开多个
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1CheckKeywords")]
        public string Action1CheckKeywords = "";

        /// <summary>
        /// 响应对象的范围，对应CommonAffectRange
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1CheckRange")]
        public string Action1CheckRange = "";
        /// <summary>
        /// 响应的结果，对应CommonCardAction
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1TechnoResult")]
        public string Action1TechnoResult = "";

        /// <summary>
        /// 响应结果的数量，支持表达式
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1TechnoResultCount")]
        public string Action1TechnoResultCount = "1";
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
        /// 响应结果的范围关键词，支持key,tag,以,隔开多个
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1AffectKeywords")]
        public string Action1AffectKeywords = "";


        /// <summary>
        /// 范围1
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1CheckCellSpread")]
        public int Action1CheckCellSpread = 1;
        /// <summary>
        /// 范围2
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1AffectCellSpread")]
        public int Action1AffectCellSpread = 1;

        /// <summary>
        /// 结果脚本
        /// </summary>
        [INIField(Key = "CommonCardScript.Action1InvokeScript")]
        public int Action1InvokeScript = 1;
    }


    #region 给JS公式用的对象
    [Serializable]
    public class CombatSlotsJSInvokeEntry
    {
     
        public CombatSlotsJSInvokeEntry(List<TavernCombatSlot> slots) 
        {
            Slots = slots;
        }

        public List<TavernCombatSlot> Slots { get; private set; }

        public int CountCard(params string[] types)
        {
            if (types is null || !types.Any())
                return Slots.Count();

            foreach(var slot in Slots)
            {
                if (slot.CurrentCardType == null) continue;
            }


            return Slots.Where(x => x.CurrentCardType != null && (types.Contains(x.CurrentCardType.Key) || types.Intersect(x.CurrentCardType.Tags).Any())).Count();
        }

        public int CountTechno(params string[] types)
        {
            var records = new List<CardRecord>();

            foreach (var slot in Slots)
            {
                records.AddRange(slot.CardRecords);
            }

            var results = records.Select(x => new
            {
                Key = x.Techno,
                Tags = x.CardType.Tags ?? new List<string>()
            });

            if (types is null || !types.Any())
                return results.Count();

            return results.Where(x=> types.Contains(x.Key) || types.Intersect(x.Tags).Any()).Count();
        }
    }

    [Serializable]
    public class CombatSlotJSInvokeEntry
    {
        public CombatSlotJSInvokeEntry(TavernCombatSlot slot)
        {
            Slot = slot;
        }

        public TavernCombatSlot Slot { get; private set; }

        public int CountCard(params string[] types)
        {
            if (types is null || !types.Any())
                return 1;
            return Slot.CurrentCardType != null && (types.Intersect(Slot.CurrentCardType.Tags).Any() || types.Contains(Slot.CurrentCardType.Key)) ? 1 : 0;
        }

        public int CountTechno(params string[] types)
        {
            var results = Slot.CardRecords.Select(x => new
            {
                Key = x.Techno,
                Tags = x.CardType.Tags ?? new List<string>()
            });

            if (types is null || !types.Any())
                return results.Count();

            return results.Where(x => types.Contains(x.Key) || types.Intersect(x.Tags).Any()).Count();
        }
    }

    [Serializable]
    public class PlayerJSInvokeEntry
    {
        public PlayerJSInvokeEntry(TavernPlayerNode player)
        {
            Player = player;
        }

        public TavernPlayerNode Player { get; private set; }

        public int CountSelled(params string[] types)
        {
            if (types is null || !types.Any())
                return Player.SellRecords.Count();

            return Player.SellRecords.Where(x => types.Contains(x.Key) || types.Intersect(x.Card.Tags).Any()).Count();
        }

        public int CountCurrentSelled(params string[] types)
        {
            if (types is null || !types.Any())
                return Player.CurrentRoundSellRecords.Count();

            return Player.CurrentRoundSellRecords.Where(x => types.Contains(x.Key) || types.Intersect(x.Card.Tags).Any()).Count();
        }

        public PlayerJSInvokeEntry ClearCurrentSelled()
        {
            Player.CurrentRoundSellRecords.RemoveAll(x => true);
            return this;
        }

        public PlayerJSInvokeEntry GiveMoney(int amount)
        {
            Player.Owner.OwnerObject.Ref.Owner.Ref.GiveMoney(amount);
            TavernGameManager.Instance.ShowFlyingTextAt($"+${amount}", Player.Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 200));
            return this;
        }
    }
    #endregion
}
