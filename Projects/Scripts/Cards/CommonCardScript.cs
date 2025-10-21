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
        public Dictionary<string, int> Variables { get; private set; } = new Dictionary<string, int>();

        public CommonCardScript(CardType type, TavernPlayerNode player) : base(type, player)
        {
        }

        public List<CommonCardTrigger> Triggers { get; private set; } = new List<CommonCardTrigger>();

        private int RoundCounter = 1;

        private int CurrentCount = 1;

        public string DoubleSection { get; private set; } = string.Empty;

        public string TripleSection { get; private set; } = string.Empty;

        public override void OnAwake()
        {
            var ini = TavernGameManager.Instance.CreateRulesIniComponentWith<CommonCardScriptData>(Type.Key);
            InitTriggers(ini, true);
        }

        private void InitTriggers(INIComponentWith<CommonCardScriptData> ini,bool first)
        {
            var dtype = typeof(CommonCardScriptData);

            RoundCounter = ini.Data.RoundCounter;

            if (first)
            {
                CurrentCount = ini.Data.RoundCounter;
                DoubleSection = ini.Data.RookieSection;
                TripleSection = ini.Data.EliteSection;
            }

            Triggers = new List<CommonCardTrigger>();

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
                trigger.ActionCheckTechnoKeywords = (dtype.GetField($"Action{i}CheckTechnoKeywords").GetValue(ini.Data)) as string;
                trigger.ActionAffectKeywords = (dtype.GetField($"Action{i}AffectKeywords").GetValue(ini.Data)) as string;
                trigger.ActionCheckCellSpread = int.Parse(dtype.GetField($"Action{i}CheckCellSpread").GetValue(ini.Data).ToString());
                trigger.ActionAffectCellSpread = int.Parse(dtype.GetField($"Action{i}AffectCellSpread").GetValue(ini.Data).ToString());
                trigger.ActionConvertRate = int.Parse(dtype.GetField($"Action{i}ConvertRate").GetValue(ini.Data).ToString());
                trigger.ActionTechnoResult = (dtype.GetField($"Action{i}TechnoResult").GetValue(ini.Data)) as string;
                trigger.ActionCardResult = (dtype.GetField($"Action{i}CardResult").GetValue(ini.Data)) as string;
                trigger.ActionTechnoResultCount = (dtype.GetField($"Action{i}TechnoResultCount").GetValue(ini.Data).ToString());
                if (!string.IsNullOrWhiteSpace(trigger.ActionTechnoResultCount))
                {
                    trigger.ActionTechnoResultCount = trigger.ActionTechnoResultCount.Replace("&semi", ";");
                }
                trigger.ActionInvokeScript = (dtype.GetField($"Action{i}InvokeScript").GetValue(ini.Data).ToString());
                if (!string.IsNullOrWhiteSpace(trigger.ActionInvokeScript))
                {
                    if (trigger.ActionInvokeScript.StartsWith("b64:"))
                    {
                        var str = trigger.ActionInvokeScript.Replace("b64:", "");
                        var jsBytes = Convert.FromBase64String(str);
                        trigger.ActionInvokeScript = Encoding.UTF8.GetString(jsBytes);
                    }
                    else
                    {
                        trigger.ActionInvokeScript = trigger.ActionInvokeScript.Replace("<<", ";");
                        if (trigger.ActionInvokeScript.StartsWith("\"") && trigger.ActionInvokeScript.EndsWith("\""))
                        {
                            trigger.ActionInvokeScript.Substring(1, trigger.ActionInvokeScript.Length - 1);
                        }
                    }
                }

                Triggers.Add(trigger);
            }
        }

        public override void OnBought()
        {
            base.OnBought();
        }

        public override void OnPlaceToCombatSlot(TavernCombatSlot tavernCombatSlot)
        {
            if (RoundCounter > 0)
            {
                tavernCombatSlot.Owner.OwnerObject.Ref.Ammo = CurrentCount;
            }
            var triggers = Triggers.Where(x => x.Event == CommonCardEvent.OnCombatPut).ToList();
            ExcuteTrigger(triggers, Slot);
        }

        public override void OnRoundStarted()
        {
            if (RoundCounter > 1)
            {
                if (CurrentCount > 0)
                {
                    CurrentCount--;
                    if(Slot is TavernCombatSlot combatSlot)
                    {
                        combatSlot.Owner.OwnerObject.Ref.Ammo = CurrentCount;
                    }
                    return;
                }
            }

            var triggers = Triggers.Where(x => x.Event == CommonCardEvent.OnRoundStart).ToList();
            ExcuteTrigger(triggers,Slot);
        }

        public override void OnBaseUpgrade()
        {
            var triggers = Triggers.Where(x => x.Event == CommonCardEvent.OnBaseUpgrade).ToList();
            ExcuteTrigger(triggers, Slot);
        }

        public override void OnCardDouble()
        {
            if (!string.IsNullOrEmpty(DoubleSection))
            {
                var ini = TavernGameManager.Instance.CreateRulesIniComponentWith<CommonCardScriptData>(Type.Key);
                InitTriggers(ini, false);
            }
        }

        public override void OnCardTriple()
        {
            var triggers = Triggers.Where(x => x.Event == CommonCardEvent.OnCardTriple).ToList();
            ExcuteTrigger(triggers, Slot);

            if (!string.IsNullOrEmpty(TripleSection))
            {
                var ini = TavernGameManager.Instance.CreateRulesIniComponentWith<CommonCardScriptData>(Type.Key);
                InitTriggers(ini, false);
            }
        }

        public override void OnRoundEnded()
        {
            if (RoundCounter > 1)
            {
                if (CurrentCount > 0)
                    return;
            }

            var triggers = Triggers.Where(x => x.Event == CommonCardEvent.OnRoundEnd).ToList();
            ExcuteTrigger(triggers,Slot);

            if (CurrentCount <= 0 && RoundCounter>0)
            {
                CurrentCount = RoundCounter;
                if (Slot is TavernCombatSlot combatSlot)
                {
                    combatSlot.Owner.OwnerObject.Ref.Ammo = CurrentCount;
                }
            }
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
                                    slot.CardRecords.Add(new CardRecord() { Techno = trigger.ActionTechnoResult, CardType = TavernGameManager.Instance.CardTypes[trigger.ActionCardResult], IsPersist = trigger.Action == CommonCardAction.AddPermanent, Tags = TavernGameManager.Instance.TechnoMetaDatas[trigger.ActionTechnoResult]?.Tags });
                                }
                            }
                        }
                        else if(trigger.Action == CommonCardAction.Convert || trigger.Action == CommonCardAction.ConvertPermanent)
                        {
                            var count = ParseCountExpression(trigger.ActionTechnoResultCount, new CombatSlotsJSInvokeEntry(filterSlots), new CombatSlotJSInvokeEntry(slot));
                            var keywords = !string.IsNullOrWhiteSpace(trigger.ActionCheckTechnoKeywords) ? trigger.ActionCheckTechnoKeywords.Split(',') : new string[0];
                            var query = slot.CardRecords.Where(x => true);
                            if (keywords.Any())
                            {
                                query = query.Where(x => keywords.Contains(x.Techno) || x.Tags.Intersect(keywords).Any());
                            }

                            var technoCount = query.Count();
                            var num = (int)Math.Floor((double)technoCount / trigger.ActionConvertRate);

                            var taked = query.Take(num * trigger.ActionConvertRate).ToList();

                            slot.CardRecords.RemoveAll(x => taked.Contains(x));

                            for(var i = 0; i < num; i++)
                            {
                                slot.CardRecords.Add(new CardRecord() { Techno = trigger.ActionTechnoResult, CardType = TavernGameManager.Instance.CardTypes[trigger.ActionCardResult], IsPersist = trigger.Action == CommonCardAction.ConvertPermanent, Tags = TavernGameManager.Instance.TechnoMetaDatas[trigger.ActionTechnoResult]?.Tags });
                            }
                        }
                        else if(trigger.Action == CommonCardAction.Move || trigger.Action == CommonCardAction.MovePermanent)
                        {
                            var keywords = !string.IsNullOrWhiteSpace(trigger.ActionCheckTechnoKeywords) ? trigger.ActionCheckTechnoKeywords.Split(',') : new string[0];
                            var count = ParseCountExpression(trigger.ActionTechnoResultCount, new CombatSlotsJSInvokeEntry(filterSlots), new CombatSlotJSInvokeEntry(slot));
                            foreach (var fslot in filterSlots)
                            {
                                var query = fslot.CardRecords.Where(x => true);
                                if (keywords.Any())
                                {
                                    query = query.Where(x => keywords.Contains(x.Techno) || x.Tags.Intersect(keywords).Any());
                                }

                                var taked = query.Take(count).ToList();

                                fslot.CardRecords.RemoveAll(x => taked.Contains(x));

                                taked.ForEach(x => x.IsPersist = trigger.Action == CommonCardAction.ConvertPermanent);

                                slot.CardRecords.AddRange(taked);
                                fslot.RefreshAggregates();
                            }
                        }

                        if (engine is not null)
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

        public string ActionCheckTechnoKeywords { get; set; }


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

        public int ActionConvertRate { get; set; } = 1;


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
        /// <summary>
        /// 对指定卡槽攻击
        /// </summary>
        OnFire,
        /// <summary>
        /// 当酒馆升级时
        /// </summary>
        OnBaseUpgrade,
        /// <summary>
        /// 当发生三连时
        /// </summary>
        OnCardTriple,
    }

    public enum CommonCardAction
    {
        None,
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
        //[INIField(Key = "CommonCardScript.Event1")]
        [INIField(Key = "事件1")]
        public string Event1 = "";
        /// <summary>
        /// 触发时的动作对应CommonCardAction
        /// </summary>
        //[INIField(Key = "CommonCardScript.Action1")]
        [INIField(Key = "动作1")]
        public string Action1 = "";

        /// <summary>
        /// 响应对象的范围关键词（卡牌），支持key,tag,以,隔开多个
        /// </summary>
        //[INIField(Key = "CommonCardScript.Action1CheckKeywords")]
        [INIField(Key = "动作1检查关键词")]
        public string Action1CheckKeywords = "";

        /// <summary>
        /// 响应对象的范围，对应CommonAffectRange
        /// </summary>
        //[INIField(Key = "CommonCardScript.Action1CheckRange")]
        [INIField(Key = "动作1检查范围")]
        public string Action1CheckRange = "";
        /// <summary>
        /// 响应的结果，对应CommonCardAction
        /// </summary>
        //[INIField(Key = "CommonCardScript.Action1TechnoResult")]
        [INIField(Key = "动作1单位结果")]
        public string Action1TechnoResult = "";

        /// <summary>
        /// 转化或移动时作用单位的关键词，支持key,tag,以,隔开多个
        /// </summary>
        //[INIField(Key = "CommonCardScript.Action1CheckTechnoKeywords")]
        [INIField(Key = "动作1检查单位关键词")]
        public string Action1CheckTechnoKeywords = "";

        /// <summary>
        /// 响应结果的数量，支持表达式
        /// </summary>
        //[INIField(Key = "CommonCardScript.Action1TechnoResultCount")]
        [INIField(Key = "动作1单位结果数量")]
        public string Action1TechnoResultCount = "1";
        /// <summary>
        /// 响应的结果所属卡面
        /// </summary>
        //[INIField(Key = "CommonCardScript.Action1CardResult")]
        [INIField(Key = "动作1卡牌结果")]
        public string Action1CardResult = "";

        /// <summary>
        /// 响应结果作用于哪对应CommonAffectRange
        /// </summary>
        //[INIField(Key = "CommonCardScript.Action1AffectRange")]
        [INIField(Key = "动作1响应范围")]

        public string Action1AffectRange = "Self";

        /// <summary>
        /// 响应结果的范围关键词，支持key,tag,以,隔开多个
        /// </summary>
        //[INIField(Key = "CommonCardScript.Action1AffectKeywords")]
        [INIField(Key = "动作1响应结果关键词")]
        public string Action1AffectKeywords = "";

        /// <summary>
        /// 当效果为Convert时，转化率，默认为1
        /// </summary>
        //[INIField(Key = "CommonCardScript.Action1ConvertRate")]
        [INIField(Key = "动作1转化率")]
        public int Action1ConvertRate = 1;

        /// <summary>
        /// 范围1
        /// </summary>
        //[INIField(Key = "CommonCardScript.Action1CheckCellSpread")]
        [INIField(Key = "动作1检查距离")]
        public int Action1CheckCellSpread = 1;
        /// <summary>
        /// 范围2
        /// </summary>
        //[INIField(Key = "CommonCardScript.Action1AffectCellSpread")]
        [INIField(Key = "动作1响应距离")]
        public int Action1AffectCellSpread = 1;

        /// <summary>
        /// 结果脚本
        /// </summary>
        //[INIField(Key = "CommonCardScript.Action1InvokeScript")]
        [INIField(Key = "动作1结果脚本")]
        public string Action1InvokeScript = "";

        //[INIField(Key = "CommonCardScript.RoundCounter")]
        [INIField(Key = "回合计数器")]

        public int RoundCounter = 1;

        /// <summary>
        /// 二连读取的效果section
        /// </summary>
        [INIField(Key = "二连")]
        public string RookieSection = "";
        /// <summary>
        /// 三联读取的效果section
        /// </summary>
        [INIField(Key = "三连")]
        public string EliteSection = "";
       
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
                Tags = x.Tags ?? new List<string>()
            });

            if (types is null || !types.Any())
                return results.Count();

            return results.Where(x=> types.Contains(x.Key) || types.Intersect(x.Tags).Any()).Count();
        }

        public CombatSlotsJSInvokeEntry TriggerEvent(string evt)
        {
            if(Enum.TryParse<CommonCardEvent>(evt,out var cardEvent))
            {
                foreach (var slot in Slots)
                {
                    switch (cardEvent)
                    {
                        case CommonCardEvent.OnBought:
                            {
                                slot?.CardScript?.OnBought();
                                break;
                            }
                        case CommonCardEvent.OnCombatPut:
                            {
                                slot?.CardScript?.OnPlaceToCombatSlot(slot);
                                break;
                            }
                        case CommonCardEvent.OnRoundStart:
                            {
                                slot?.CardScript?.OnRoundStarted();
                                break;
                            }
                        case CommonCardEvent.OnRoundEnd:
                            {
                                slot?.CardScript?.OnRoundEnded();
                                break;
                            }
                        case CommonCardEvent.OnSelledCombat:
                            {
                                slot?.CardScript?.OnSelledCombat(TavernGameManager.Instance.RulesSellCardPrice);
                                break;
                            }
                        case CommonCardEvent.OnSelled:
                            {
                                slot?.CardScript?.OnSelledCombat(TavernGameManager.Instance.RulesSellCardPrice);
                                break;
                            }
                        case CommonCardEvent.OnFire:
                            break;
                        case CommonCardEvent.OnBaseUpgrade:
                            {
                                slot?.CardScript?.OnBaseUpgrade();
                                break;
                            }
                        case CommonCardEvent.OnCardTriple:
                            {
                                slot?.CardScript?.OnCardTriple();
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
            
            return this;
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
                Tags = x.Tags ?? new List<string>()
            });

            if (types is null || !types.Any())
                return results.Count();

            return results.Where(x => types.Contains(x.Key) || types.Intersect(x.Tags).Any()).Count();
        }

        public void ReplaceCard(string card,bool overwrite)
        {
            if (Slot.CurrentCardType is not null && !overwrite)
                return;

            Slot.ChangeCard(TavernGameManager.Instance.CardTypes[card], true, true);
        }

        public CombatSlotJSInvokeEntry TriggerEvent(string evt)
        {
            if (Enum.TryParse<CommonCardEvent>(evt, out var cardEvent))
            {
                switch (cardEvent)
                {
                    case CommonCardEvent.OnBought:
                        {
                            Slot?.CardScript?.OnBought();
                            break;
                        }
                    case CommonCardEvent.OnCombatPut:
                        {
                        Slot?.CardScript?.OnPlaceToCombatSlot(Slot);
                            break;
                        }
                    case CommonCardEvent.OnRoundStart:
                        {
                            Slot?.CardScript?.OnRoundStarted();
                            break;
                        }
                    case CommonCardEvent.OnRoundEnd:
                        {
                            Slot?.CardScript?.OnRoundEnded();
                            break;
                        }
                    case CommonCardEvent.OnSelledCombat:
                        {
                            Slot?.CardScript?.OnSelledCombat(TavernGameManager.Instance.RulesSellCardPrice);
                            break;
                        }
                    case CommonCardEvent.OnSelled:
                        {
                            Slot?.CardScript?.OnSelledCombat(TavernGameManager.Instance.RulesSellCardPrice);
                            break;
                        }
                    case CommonCardEvent.OnFire:
                        break;
                    case CommonCardEvent.OnBaseUpgrade:
                        {
                            Slot?.CardScript?.OnBaseUpgrade();
                            break;
                        }
                    case CommonCardEvent.OnCardTriple:
                        {
                            Slot?.CardScript?.OnCardTriple();
                            break;
                        }
                    default:
                        break;
                }
            }

            return this;
        }

        public int GetVariable(string name)
        {
            if(Slot.CardScript is CommonCardScript cs)
            {
                if (cs.Variables.ContainsKey(name))
                {
                    return cs.Variables[name];
                }
                return 0;
            }
            return 0;
        }

        public int SetVariable(string name,int value)
        {
            if (Slot.CardScript is CommonCardScript cs)
            {
                if (cs.Variables.ContainsKey(name))
                {
                    return cs.Variables[name] = value;
                }
                else
                {
                    cs.Variables.Add(name, value);
                }
                return value;
            }
            return 0;
        }

        public int ClearVariable(string name)
        {
            if (Slot.CardScript is CommonCardScript cs)
            {
                if (cs.Variables.ContainsKey(name))
                {
                    var result = cs.Variables[name];
                    cs.Variables.Remove(name);
                    return result;
                }
                return 0;
            }
            return 0;
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

        public int BaseLevel()
        {
            return Player.BaseLevel;
        }

        public int Money()
        {
            return Player.Owner.OwnerObject.Ref.Owner.Ref.Available_Money() ;
        }

        public PlayerJSInvokeEntry ClearSelled(params string[] types)
        {
            if(types is null || !types.Any())
            {
                Player.SellRecords.RemoveAll(x => true);
            }
            else
            {
                Player.SellRecords.RemoveAll(x => types.Contains(x.Key) || types.Intersect(x.Card.Tags).Any());
            }

            return this;
        }

        public PlayerJSInvokeEntry ClearCurrentSelled(params string[] types)
        {
            if (types is null || !types.Any())
            {
                Player.CurrentRoundSellRecords.RemoveAll(x => true);
            }
            else
            {
                Player.CurrentRoundSellRecords.RemoveAll(x => types.Contains(x.Key) || types.Intersect(x.Card.Tags).Any());
            }
            return this;
        }

        public int TakeCardsFromSelled(string[] types, int group = 1, bool clear = true)
        {
            var query = Player.SellRecords.AsQueryable();
            if (types is not null && types.Any())
            {
                query = query.Where(x => types.Contains(x.Key) || types.Intersect(x.Card.Tags).Any());
            }

            var totalCount = query.Count();

            var count = totalCount / group;

            if (count > 0 && clear)
            {
                var taked = query.Take(count * group).ToList();
                Player.SellRecords.RemoveAll(x => taked.Contains(x));
            }
           
            return count;
        }

        public int TakeCardsFromCurrentSelled(string[] types, int group = 1, bool clear= true)
        {
            var query = Player.CurrentRoundSellRecords.AsQueryable();
            if (types is not null && types.Any())
            {
                query = query.Where(x => types.Contains(x.Key) || types.Intersect(x.Card.Tags).Any());
            }

            var totalCount = query.Count();

            var count = totalCount / group;

            if (count > 0 && clear)
            {
                var taked = query.Take(count * group).ToList();
                Player.CurrentRoundSellRecords.RemoveAll(x => taked.Contains(x));
            }

            return count;
        }


        public PlayerJSInvokeEntry GiveMoney(int amount)
        {
            
            Player.Owner.OwnerObject.Ref.Owner.Ref.GiveMoney(amount);
            TavernGameManager.Instance.ShowFlyingTextAt($"{(amount > 0 ? "+" : "-")}${Math.Abs(amount)}", Player.Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 200));
            return this;
        }

        public PlayerJSInvokeEntry GiveCardTemp(string card,int count)
        {
            for(var i = 0; i < count; i++)
            {
                Player.CardCacheQueue.Enqueue(TavernGameManager.Instance.CardTypes[card]);
            }
            return this;
        }

        public PlayerJSInvokeEntry UnlockTempSlot()
        {
            var temp = Player.TavernTempSlots.Where(x => !x.IsEnabled).FirstOrDefault();

            if (temp is not null)
            {
                temp.IsEnabled = true;
            }
        
            return this;
        }

        public PlayerJSInvokeEntry UnlockCombatSlot()
        {
            var combat = Player.TavernCombatSlots.Where(x => !x.IsEnabled).FirstOrDefault();

            if (combat is not null)
            {
                combat.IsEnabled = true;
            }

            return this;
        }

        public PlayerJSInvokeEntry UnlockShopSlot()
        {
            var shop = Player.TavernShopSlots.Where(x => !x.IsEnabled).FirstOrDefault();

            if (shop is not null)
            {
                shop.IsEnabled = true;
            }

            return this;
        }

        public PlayerJSInvokeEntry UpgradeBase()
        {
            Player.OnUpgrade();
            return this;
        }

        public int Random(int min, int max)
        {
            return Player.NRandom.Next(min, max);
        }





        public int GetVariable(string name)
        {
            if (Player.Variables.ContainsKey(name))
            {
                return Player.Variables[name];
            }
            return 0;
        }

        public int SetVariable(string name, int value)
        {
            if (Player.Variables.ContainsKey(name))
            {
                return Player.Variables[name] = value;
            }
            else
            {
                Player.Variables.Add(name, value);
            }
            return value;
        }

        public int ClearVariable(string name)
        {
            if (Player.Variables.ContainsKey(name))
            {
                var result = Player.Variables[name];
                Player.Variables.Remove(name);
                return result;
            }
            return 0;
        }






    }
    #endregion
}
