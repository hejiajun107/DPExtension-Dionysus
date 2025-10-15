using DynamicPatcher;
using Extension.Coroutines;
using Extension.CW;
using Extension.EventSystems;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Newtonsoft.Json;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Scripts.Cards;
using Scripts.Tavern;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Scripts.Tavern
{
    [ScriptAlias(nameof(TavernGameManager))]
    [Serializable]
    public class TavernGameManager : TechnoScriptable
    {
        /// <summary>
        /// 全局唯一Instance
        /// </summary>
        public static TavernGameManager Instance { get; private set; } = null;

        public List<TavernPlayerNode> PlayerNodes { get; private set; } = new List<TavernPlayerNode>();

        private int maxPlayer = 2;

        private static Dictionary<string, Type> RegisteredCardScripts = new Dictionary<string, Type>();

        public Dictionary<string, CardType> CardTypes { get; private set; } = new Dictionary<string, CardType>();
        public Dictionary<string, TechnoMetaData> TechnoMetaDatas { get; private set; } = new Dictionary<string, TechnoMetaData>();


        public List<string> CardPool { get; private set; } = new List<string>();

        //public List<string> CommanderPrerequisites { get; private set; } = new List<string> { "CTanyBD", "CBorisBD", "CYuriBD" };

        public List<CommanderData> CommanderTypes { get; private set; } = new List<CommanderData>();
        public List<CommanderData> CommanderPool { get; private set; } = new List<CommanderData>();


        public int BaseMaxLevel { get; private set; }

        private List<FlyingText> _flyingTexts = new List<FlyingText>();


        private static Dictionary<string, string> CameoCached = new Dictionary<string, string>();
        private INIComponentWith<TechnoData> rulesIni;
        private INIComponentWith<ArtData> artIni;

        private GameStatusMachine gameStatusMachine = new GameStatusMachine();

        public GameStatus GameStatus { 
            get 
            {
                return gameStatusMachine.CurrentStatus;
            } 
        }

        /// <summary>
        /// 当前回合数
        /// </summary>
        public int CurrentRound { get; private set; } = 1;

        /// <summary>
        /// 准备阶段的计时器
        /// </summary>
        public int ReadyStatusTick { get; private set; } = 500;

        /// <summary>
        /// 战斗阶段的计时器
        /// </summary>
        public int BattleEndTicks { get; private set; } = 500;

        /// <summary>
        /// 指挥官选择的计时器
        /// </summary>
        public int ComandderSelectTicks { get; private set; } = 500;



        #region rules
        /// <summary>
        /// 刷新价格
        /// </summary>
        public int RulesRefreshPrice { 
            get 
            {
                return ini.Data.RefreshPrice;
            } 
        }

        /// <summary>
        /// 购买卡牌价格
        /// </summary>
        public int RulesBuyCardPrice
        {
            get
            {
                return ini.Data.BuyCardPrice;
            }
        }


        /// <summary>
        /// 出售卡牌价格
        /// </summary>
        public int RulesSellCardPrice
        {
            get
            {
                return ini.Data.SellCardPrice;
            }
        }


        /// <summary>
        /// 获得升级酒馆的费用
        /// </summary>
        /// <returns></returns>
        public int GetUpgradeBaseCost(Pointer<HouseClass> house)
        {
            var node = FindPlayerNodeByHouse(house);
            if (node.BaseLevel >= ini.Data.BaseMaxLevel)
                return 0;

            var cost = ini.Data.InitUpgradeCost + (ini.Data.BaseMaxLevel - 1) * ini.Data.BaseMaxLevel;
            cost = cost - node.RoundAfterUpgrade * ini.Data.UpgradeCostDecrease;

            if(cost>ini.Data.UpgradeCostMin)
            {
                cost = ini.Data.UpgradeCostMin;
            }

            return cost;
        }
        #endregion

        public TavernGameManager(TechnoExt owner) : base(owner)
        {
        }

        private INIComponentWith<GameManagerSetting> ini;

        private bool inited = false;

        //用于流程比较长的协程的锁定，避免重复触发
        private bool coroutineLock = false;

        public override void Awake()
        {
            ini = Owner.GameObject.CreateRulesIniComponentWith<GameManagerSetting>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            rulesIni = this.CreateRulesIniComponentWith<TechnoData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            artIni = this.CreateArtIniComponentWith<ArtData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            //导入所有卡配置
            LoadConfig();
            LoadCardTypes();
            InitCardPools();
            LoadCommanderTypes();
            EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
        }

        public override void OnDestroy()
        {
            EventSystem.GScreen.RemoveTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
        }

        public override void OnUpdate()
        {
            //注册全局实例
            if (Instance == null) 
            {
                Instance = this;
            }

            //未全部注册
            //if (PlayerNodes.Count < maxPlayer)
            //{
            //    return;
            //}

            if (!inited) 
            {
                inited = true;
                GameObject.StartCoroutine(DoInit());
            }

            if(GameStatus == GameStatus.ChooseCommander)
            {
                if (ComandderSelectTicks > 0)
                {
                    ComandderSelectTicks--;
                }

                if(ComandderSelectTicks == 0 || !PlayerNodes.Where(x=>x.CommanderSlot.Commander == null).Any())
                {
                    if(!coroutineLock)
                    {
                        coroutineLock = true;
                        //全部准备好以后进入下一个阶段
                        GameObject.StartCoroutine(ReadyToStart());
                    }
                }
            }

            if(GameStatus == GameStatus.Ready)
            {
                if (ReadyStatusTick > 0)
                {
                    ReadyStatusTick--;
                }
                 
                if(!PlayerNodes.Where(x => !x.IsReady).Any() || ReadyStatusTick == 0)
                {
                    if (!coroutineLock)
                    {
                        coroutineLock = true;
                        GameObject.StartCoroutine(ReadyToBattle());
                    }
                }
            }

            if (GameStatus == GameStatus.Battle) 
            {
                if (BattleEndTicks > 0)
                {
                    BattleEndTicks--;
                }

                if (!PlayerNodes.Where(x => !x.VoteSkiped).Any() || BattleEndTicks == 0)
                {
                    if (!coroutineLock)
                    {
                        //结束战斗
                        coroutineLock = true;
                        //全部准备好以后进入下一个阶段
                        GameObject.StartCoroutine(EndBattle());
                    }
                }
            }

            UpdateFlyingTexts();
        }

        public void OnGScreenRender(object sender, EventArgs args)
        {
            if (args is GScreenEventArgs gScreenEvtArgs)
            {
                if (gScreenEvtArgs.IsLateRender)
                {
                    DrawingFlyingText();

                    if (GameStatus == GameStatus.Battle)
                    {
                        Pointer<Surface> pSurface = Surface.Current;
                        var source = pSurface.Ref.GetRect();
                        Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(-100, 0, 600));
                        var point2 = new Point2D(point.X, point.Y);
                        pSurface.Ref.DrawText(BattleEndTicks.ToString(), source.GetThisPointer(), point2.GetThisPointer(), new ColorStruct(0, 255, 0));
                    }
                }
            }
        }

        public void ShowFlyingTextAt(string text, CoordStruct location, int color = 0)
        {
            _flyingTexts.Add(new FlyingText()
            {
                Text = text,
                Duration = 25,
                Location = location,
                Color = color
            });
        }

        private void DrawingFlyingText()
        {
            foreach(var text in _flyingTexts)
            {
                if (text.Duration > 0)
                {
                    Point2D point = TacticalClass.Instance.Ref.CoordsToClient(text.Location);
                    Pointer<Surface> pSurface = Surface.Current;
                    var source = pSurface.Ref.GetRect();
                    var point2 = new Point2D(point.X, point.Y);
                    pSurface.Ref.DrawText(text.Text, source.GetThisPointer(), point2.GetThisPointer(), text.Color == 0 ? new ColorStruct(0, 255, 0) : new ColorStruct(255, 0, 0));
                }
            }
        }

        private void UpdateFlyingTexts()
        {
            _flyingTexts.RemoveAll(x => x.Duration <= 0);
            foreach (var flyingText in _flyingTexts)
            {
                if (flyingText.Duration > 0)
                {
                    flyingText.Duration--;
                    flyingText.Location = flyingText.Location + new CoordStruct(0, 0, 5);
                }
            }
        }

        #region 流程控制

        IEnumerator DoInit()
        {
            yield return new WaitForFrames(5);
            foreach (var node in PlayerNodes)
            {
                node.InitRandom();

                //初始节点数
                for (var i = 0; i < node.TavernCombatSlots.Count(); i++)
                {
                    if (i < ini.Data.InitCombatSlots)
                    {
                        node.TavernCombatSlots[i].IsEnabled = true;
                    }
                    else
                    {
                        node.TavernCombatSlots[i].IsEnabled = false;
                    }
                }

                for (var i = 0; i < node.TavernShopSlots.Count(); i++)
                {
                    if (i < ini.Data.InitShopSlots)
                    {
                        node.TavernShopSlots[i].IsEnabled = true;
                    }
                    else
                    {
                        node.TavernShopSlots[i].IsEnabled = false;
                    }
                }

                for (var i = 0; i < node.TavernTempSlots.Count(); i++)
                {
                    if (i < ini.Data.InitTempSlots)
                    {
                        node.TavernTempSlots[i].IsEnabled = true;
                    }
                    else
                    {
                        node.TavernTempSlots[i].IsEnabled = false;
                    }
                }

                for(var i = 0; i < ini.Data.ChooseCommanderOptions; i++)
                {
                    var idx = 0;

                    if (CommanderPool.Count() > 0)
                    {
                        idx = node.NRandom.Next(0, CommanderPool.Count());
                    }

                    var selecedCommander = CommanderPool[idx];
                    var cmdbd = selecedCommander.Prerequisites;
                    CommanderPool.Remove(selecedCommander);
                    node.CommanderPool.Add(selecedCommander.Techno);
                    TechnoPlacer.PlaceTechnoNear(TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(cmdbd), node.Owner.OwnerObject.Ref.Owner, CellClass.Coord2Cell(node.Owner.OwnerObject.Ref.Base.Base.GetCoords()));
                }

                if (!node.Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman()) 
                {
                    //AI直接选择，或者直接选用指定指挥官
                    var prerequisites = TavernGameManager.Instance.CommanderTypes.Select(x => x.Prerequisites).ToList();
                    //删除所有提供建造前提的建筑
                    var buildings = ObjectFinder.FindTechnosNear(node.CommanderSlot.Owner.OwnerObject.Ref.Base.Base.GetCoords(), 10 * Game.CellSize).Select(x => x.Convert<TechnoClass>())
                        .Where(x => x.Ref.Owner == node.Owner.OwnerObject.Ref.Owner && prerequisites.Contains(x.Ref.Type.Ref.Base.Base.ID)).ToList();
                    foreach (var building in buildings)
                    {
                        building.Ref.Base.Remove();
                        building.Ref.Base.UnInit();
                    }
                    if (node.CommanderSlot.Commander is null)
                    {
                        node.CommanderSlot.InitComander(node.CommanderPool[node.NRandom.Next(0, node.CommanderPool.Count)]);
                    }
                }
                //初始化指挥官选择
            
            }
            yield return new WaitForFrames(1);


            gameStatusMachine.OnCommanderSelectStart += ComanderSelectStart;
            gameStatusMachine.OnRoundStart += RoundStart;
            gameStatusMachine.OnPrepared += Prepared;
            gameStatusMachine.OnBattleStart += BattleStart;
            gameStatusMachine.OnBattleEnd += BattleEnded;
            gameStatusMachine.Init();
        }

        IEnumerator ReadyToStart()
        {
            foreach (var node in PlayerNodes)
            {
                var prerequisites = TavernGameManager.Instance.CommanderTypes.Select(x => x.Prerequisites).ToList();
                //删除所有提供建造前提的建筑
                var buildings = ObjectFinder.FindTechnosNear(node.CommanderSlot.Owner.OwnerObject.Ref.Base.Base.GetCoords(), 10 * Game.CellSize).Select(x => x.Convert<TechnoClass>())
                    .Where(x => x.Ref.Owner == node.Owner.OwnerObject.Ref.Owner && prerequisites.Contains(x.Ref.Type.Ref.Base.Base.ID)).ToList();
                foreach (var building in buildings)
                {
                    building.Ref.Base.Remove();
                    building.Ref.Base.UnInit();
                }
            }
            yield return new WaitForFrames(10);
            foreach (var node in PlayerNodes)
            {
               if(node.CommanderSlot.Commander is null)
               {
                    node.CommanderSlot.InitComander(node.CommanderPool[node.NRandom.Next(0, node.CommanderPool.Count)]);
               }
            }
            //5
            yield return new WaitForFrames(20);
            //4
            yield return new WaitForFrames(20);
            //3
            yield return new WaitForFrames(20);
            //2
            yield return new WaitForFrames(20);
            //1
            gameStatusMachine.CommandSelected();
            coroutineLock = false;
        }


        IEnumerator ReadyToBattle()
        {
            yield return new WaitForFrames(20);
            //5
            yield return new WaitForFrames(20);
            //4
            yield return new WaitForFrames(20);
            //3
            yield return new WaitForFrames(20);
            //2
            yield return new WaitForFrames(20);
            //1
            gameStatusMachine.Next();
            coroutineLock = false;
        }

        IEnumerator EndBattle()
        {
            yield return new WaitForFrames(20);
            //5
            yield return new WaitForFrames(20);
            //4
            yield return new WaitForFrames(20);
            //3
            yield return new WaitForFrames(20);
            //2
            yield return new WaitForFrames(20);
            //1

            var warhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Super");

            //消灭所有刷出来的单位
            var count = TechnoClass.Array.Count();
            for(var i = count - 1; i >= 0; i--)
            {
                var techno = TechnoClass.Array[i];

                var ext = TechnoExt.ExtMap.Find(techno);

                if (ext.IsNullOrExpired())
                    continue;

                var gext = ext.GameObject.FastGetScript1 as TechnoGlobalExtension;

                if (gext is null)
                    continue;

                if (gext.IsTavernBattleUnit)
                {
                    techno.Ref.Base.TakeDamage(10000, warhead, true);
                }
            }

            gameStatusMachine.Next();
            coroutineLock = false;


        }

        /// <summary>
        /// 战斗结束后的事件，回合数+1,进入下一轮准备
        /// </summary>
        private void BattleEnded()
        {
            CurrentRound++;

            foreach(var node in PlayerNodes)
            {
                if(node.Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                {
                    //重置上轮的准备和投票跳过
                    node.IsReady = false;
                    node.VoteSkiped = false;
                }
            }

            gameStatusMachine.Next();
        }


        private void ComanderSelectStart()
        {
            ComandderSelectTicks = ini.Data.ChooseCommanderTime;

            foreach (var node in PlayerNodes)
            {
                //清除玩家金钱
                var money = node.Owner.OwnerObject.Ref.Owner.Ref.Available_Money();
                if (money > 0)
                {
                    node.Owner.OwnerObject.Ref.Owner.Ref.TransactMoney(-money);
                }
            }


            
        }

        /// <summary>
        /// 回合开始
        /// </summary>
        private void RoundStart()
        {
            //免费刷新一次所有未锁定的商店
            foreach (var node in PlayerNodes)
            {
                //清除玩家金钱
                var money = node.Owner.OwnerObject.Ref.Owner.Ref.Available_Money();
                if (money > 0)
                {
                    node.Owner.OwnerObject.Ref.Owner.Ref.TransactMoney(-money);
                }

                if (!node.IsLocked)
                {
                    node.OnRefreshShop(true);
                }

                //玩家获得回合金钱
                var giveMoney = ini.Data.InitMoney + (CurrentRound - 1) * ini.Data.RoundMoney;
                if (giveMoney > ini.Data.MaxMoney)
                {
                    giveMoney = ini.Data.MaxMoney;
                }
                node.Owner.OwnerObject.Ref.Owner.Ref.TransactMoney(giveMoney);

                node.IsLocked = false;
                node.VoteSkiped = false;

                //所有AI节点直接准备就绪
                if (!node.Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                {
                    node.IsReady = true;
                }

                node.CurrentRoundSellRecords.RemoveAll(x => true);

                //todo触发所有卡牌的回合开始效果
                foreach (var item in node.TavernCombatSlots)
                {
                    item.CardScript?.OnRoundStarted();
                }


                if (!string.IsNullOrWhiteSpace(ini.Data.RoundStartLaunchSW))
                {
                    var psw = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(ini.Data.RoundStartLaunchSW);
                    var sw = node.Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(psw);
                    sw.Ref.Launch(CellClass.Coord2Cell(node.Owner.OwnerObject.Ref.Base.Base.GetCoords()), false);
                }
             

            }

            //给与准备时间
            var ticks = ini.Data.ReadyStatusInitTime + (CurrentRound - 1) * ini.Data.ReadyStatusRoundTime;
            if (ticks > ini.Data.ReadyStatusMaxTime)
            {
                ticks = ini.Data.ReadyStatusMaxTime;
            }
            ReadyStatusTick = ticks;

            //投放回合开始超武

        }

        private void Prepared()
        {
            var voteSkipSW = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(ini.Data.VoteSkipSW);
            var voteTicks = ini.Data.VoteSkipInitTime + (CurrentRound - 1) * ini.Data.VoteSkipRoundTime;
            if(voteTicks > ini.Data.VoteSkipMaxTime)
            {
                voteTicks = ini.Data.VoteSkipMaxTime;
            }

            foreach (var node in PlayerNodes)
            {
                //清除玩家金钱
                var money = node.Owner.OwnerObject.Ref.Owner.Ref.Available_Money();
                if (money > 0)
                {
                    node.Owner.OwnerObject.Ref.Owner.Ref.TransactMoney(-money);
                }

                node.OnRoundEnded();

                node.VoteSkiped = false;

                //所有AI节点准备跳过
                if (!node.Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                {
                    node.VoteSkiped = true;
                }

                var psw = node.Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(voteSkipSW);
                psw.Ref.IsCharged = false;
                psw.Ref.RechargeTimer.Start(voteTicks);

                //todo触发所有卡牌的回合结束效果
                foreach (var item in node.TavernCombatSlots)
                {
                    item.CardScript?.OnRoundEnded();
                }


                //释放回合结束需要释放的超武
                if (!string.IsNullOrWhiteSpace(ini.Data.RoundEndLaunchSW))
                {
                    var pendSW = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(ini.Data.RoundEndLaunchSW);
                    var endsw = node.Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(pendSW);
                    endsw.Ref.Launch(CellClass.Coord2Cell(node.Owner.OwnerObject.Ref.Base.Base.GetCoords()), false);
                }

            }


            //设置战斗开始倒计时
            var ticks = ini.Data.BattleEndInitTime + (CurrentRound - 1) * ini.Data.BattleEndRoundTime;
            if (ticks > ini.Data.BattleEndMaxTime)
            {
                ticks = ini.Data.BattleEndMaxTime;
            }
            BattleEndTicks = ticks;


            gameStatusMachine.Next();
        }

        private void BattleStart()
        {

            //todo 刷兵
            foreach(var node in PlayerNodes)
            {
                foreach(var slot in node.TavernCombatSlots)
                {
                    if(slot.CurrentCardType is not null)
                    {
                        var spawn = slot.Owner.OwnerObject.Ref.Base.Base.GetCoords();
                        foreach(var techno in slot.CardRecords)
                        {
                            var ptech = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(techno.Techno).Ref.Base.CreateObject(node.Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();
                            if (ptech.IsNull)
                                continue;

                            Game.IKnowWhatImDoing++;
                            ptech.Ref.Base.Put(spawn, Direction.N);
                            Game.IKnowWhatImDoing--;
                            ptech.Ref.Base.Scatter(spawn, true, true);

                            var ext = TechnoExt.ExtMap.Find(ptech);
                            if(ext is not null)
                            {
                                var globalExt = ext.GameObject.FastGetScript1 as TechnoGlobalExtension;
                                globalExt.IsTavernBattleUnit = true;

                                ext.GameObject.CreateScriptComponent(nameof(TavernAutoBattleScript), nameof(TavernAutoBattleScript), ext);
                            }
                        }
                    }
                }
            }


        }

        #endregion

        public TavernPlayerNode FindPlayerNodeByHouse(Pointer<HouseClass> house)
        {
            return PlayerNodes.Where(x=>x.Owner.OwnerObject.Ref.Owner ==  house).FirstOrDefault();
        }

        public List<string> GetAvailableCardPools(int baseLevel)
        {
            return CardPool.Where(x=> CardTypes[x].Level <= baseLevel).ToList();
        }

        public void RegisterNode(TavernPlayerNode node)
        {
            PlayerNodes.Add(node);
        }


        private void LoadConfig()
        {
            if (ini != null)
            {
                BaseMaxLevel = ini.Data.BaseMaxLevel;
            }
        }


        private void LoadCardTypes()
        {
            if (ini != null) {
                var technos = new HashSet<string>();

                var metaReader = GameObject.CreateRulesIniComponentWith<TechnoExtendData>("Special");

                var fileConfig = ini.Data.CardConfigFiles;
                var files = fileConfig.Split(',').ToList();
                var types = new List<CardType>();
                foreach (var file in files) 
                {
                    using (StreamReader sr = new StreamReader(file)) 
                    {
                        var json = sr.ReadToEnd();
                        var cardTypes = JsonConvert.DeserializeObject<List<CardType>>(json);
                        types.AddRange(cardTypes);
                    }
                }
                CardTypes = types.ToDictionary(x => x.Key, x => x);

                foreach(var cardtype in CardTypes)
                {
                    foreach(var techno in cardtype.Value.Technos)
                    {
                        technos.Add(techno.Key);
                    }
                }

                foreach(var techno in technos)
                {
                    metaReader.Section = techno;
                    TechnoMetaDatas.Add(techno, new TechnoMetaData()
                    {
                        Tags = string.IsNullOrWhiteSpace(metaReader.Data.Tags) ? new List<string>() : metaReader.Data.Tags.Split(',').ToList()
                    });
                }

            }
        }

        private void LoadCommanderTypes()
        {
            if (ini != null)
            {
                var file= ini.Data.CommanderConfigFile;
                var types = new List<CommanderData>();
              
                using (StreamReader sr = new StreamReader(file))
                {
                    var json = sr.ReadToEnd();
                    types = JsonConvert.DeserializeObject<List<CommanderData>>(json);
                    CommanderTypes = types;

                    foreach (var type in types)
                    {
                        CommanderPool.Add(type);
                    }
                }
            }
        }


        private void InitCardPools()
        {
            var pool = new List<string>();
            foreach(var cardType in CardTypes)
            {
                var type = cardType.Value;
                for(var i = 0; i < type.Amount; i++)
                {
                    pool.Add(type.Key);
                }
            }
            CardPool = pool;
        }

        #region Util

        /// <summary>
        /// 根据单位注册名获取图标
        /// </summary>
        /// <param name="technoType"></param>
        /// <returns></returns>
        public string GetTechnoCameo(string technoType)
        {
            if (CameoCached.ContainsKey(technoType))
                return CameoCached[technoType];

            rulesIni.Section = technoType;
            string artKey = technoType;
            if (!string.IsNullOrEmpty(rulesIni.Data.Image))
            {
                artKey = rulesIni.Data.Image;
            }
            artIni.Section = artKey;
            var cameo = string.Empty;
            if (!string.IsNullOrEmpty(artIni.Data.CameoPCX))
            {
                cameo = artIni.Data.CameoPCX;
            }
            else
            {
                cameo = artIni.Data.Cameo;
            }

            CameoCached.Add(technoType, cameo);
            return cameo;
        }

        public CardScript CreateCardScript(CardType type,TavernPlayerNode player)
        {
            if (string.IsNullOrWhiteSpace(type.Scripts))
            {
                return new EmptyCardScript(type,player);
            }

            TryRegisterCardScript();

            if(RegisteredCardScripts.TryGetValue(type.Scripts, out var cardScriptType))
            {
                object instance = Activator.CreateInstance(cardScriptType, type, player); 
                var script = instance as CardScript;
                if(script is not null)
                {
                    script.OnAwake();
                }
                return script;
            }

            return new EmptyCardScript(type, player);
        }

        private void TryRegisterCardScript()
        {
            if (RegisteredCardScripts.Count() <= 0)
            {
                var baseType = typeof(CardScript);
                var scriptTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => baseType.IsAssignableFrom(t) && t != baseType).ToList();
                foreach(var scriptType in scriptTypes)
                {
                    RegisteredCardScripts.Add(scriptType.Name, scriptType);
                }
            }
            else
            {
                return;
            }
        }

        #endregion


        /// <summary>
        /// 播放没钱的提示
        /// </summary>
        public void SoundNoMoney()
        {

        }
    }

    [Serializable]
    public class GameStatusMachine
    {
        public GameStatus CurrentStatus { get; private set; } = GameStatus.NoInited;

        public void Init()
        {
            CurrentStatus = GameStatus.ChooseCommander;
            OnCommanderSelectStart?.Invoke();
        }

        public void CommandSelected()
        {
            CurrentStatus = GameStatus.Ready;
            OnRoundStart?.Invoke();
        }

    
        public void Next()
        {
            if(CurrentStatus == GameStatus.Ready)
            {
                CurrentStatus = GameStatus.Prepared;
                OnPrepared?.Invoke();
            }
            else if(CurrentStatus == GameStatus.Prepared)
            {
                CurrentStatus = GameStatus.Battle;
                OnBattleStart?.Invoke();
            }
            else if (CurrentStatus == GameStatus.Battle)
            {
                CurrentStatus = GameStatus.BattleEnd;
                OnBattleEnd?.Invoke();
            }
            else if (CurrentStatus == GameStatus.BattleEnd)
            {
                CurrentStatus = GameStatus.Ready;
                OnRoundStart?.Invoke();
            }
        }

        public event Action OnRoundStart;
        public event Action OnPrepared;
        public event Action OnBattleStart;
        public event Action OnBattleEnd;
        public event Action OnCommanderSelectStart;


    }

    public enum GameStatus
    {
        NoInited,
        ChooseCommander,
        Ready,
        Prepared,
        Battle,
        BattleEnd
    }

    public class GameManagerSetting : INIAutoConfig
    {
        /// <summary>
        /// 卡牌配置文件路径，可以多个以,隔开
        /// </summary>
        [INIField(Key = "CardConfigFiles")]
        public string CardConfigFiles = "";

        /// <summary>
        /// 指挥官配置文件路径，可以多个以,隔开
        /// </summary>
        [INIField(Key = "CommanderConfigFile")]
        public string CommanderConfigFile = "";

        /// <summary>
        /// 基地最高等级
        /// </summary>
        [INIField(Key = "BaseMaxLevel")]
        public int BaseMaxLevel = 5;
        /// <summary>
        /// 购买卡牌需要的金钱
        /// </summary>
        [INIField(Key = "BuyCardPrice")]
        public int BuyCardPrice = 300;
        /// <summary>
        /// 出售卡牌获得的金钱
        /// </summary>
        [INIField(Key = "SellCardPrice")]
        public int SellCardPrice = 100;
        /// <summary>
        /// 刷新需要的金钱
        /// </summary>
        [INIField(Key = "RefreshPrice")]
        public int RefreshPrice = 100;
        /// <summary>
        /// 初始暂存区数量
        /// </summary>
        [INIField(Key = "InitTempSlots")]
        public int InitTempSlots = 3;
        /// <summary>
        /// 初始上场区数量
        /// </summary>
        [INIField(Key = "InitCombatSlots")]
        public int InitCombatSlots = 3;
        /// <summary>
        /// 初始商店区数量
        /// </summary>
        [INIField(Key = "InitShopSlots")]
        public int InitShopSlots = 3;


        /// <summary>
        /// 初始金钱
        /// </summary>
        [INIField(Key = "InitMoney")]
        public int InitMoney = 300;
        /// <summary>
        /// 随回合数每回合增加的金钱
        /// </summary>
        [INIField(Key = "RoundMoney")]
        public int RoundMoney = 300;
        /// <summary>
        /// 每回合获得的最大金钱
        /// </summary>
        [INIField(Key = "MaxMoney")]
        public int MaxMoney = 5000;


        /// <summary>
        /// 初始升级酒馆等级所需要的金钱
        /// </summary>
        [INIField(Key = "InitUpgradeCost")]
        public int InitUpgradeCost = 500;

        /// <summary>
        /// 每级升级酒馆额外增加的金钱
        /// </summary>
        [INIField(Key = "UpgradeExtraCost")]
        public int UpgradeExtraCost = 500;

        /// <summary>
        /// 每回合升级酒馆降低的金额
        /// </summary>
        [INIField(Key = "UpgradeCostDecrease")]
        public int UpgradeCostDecrease = 100;

        /// <summary>
        /// 升级酒馆的最低金额
        /// </summary>
        [INIField(Key = "UpgradeCostMin")]
        public int UpgradeCostMin = 500;

        /// <summary>
        /// 选择指挥官的时间（帧数）
        /// </summary>
        [INIField(Key = "ChooseCommanderTime")]
        public int ChooseCommanderTime = 3000;

        /// <summary>
        /// 指挥官的选项数
        /// </summary>
        [INIField(Key = "ChooseCommanderOptions")]
        public int ChooseCommanderOptions = 2;

        /// <summary>
        /// 准备阶段初始时间（帧数）
        /// </summary>
        [INIField(Key = "ReadyStatusInitTime")]
        public int ReadyStatusInitTime = 1500;
        /// <summary>
        /// 每轮额外增加的准备时间数
        /// </summary>
        [INIField(Key = "ReadyStatusRoundTime")]
        public int ReadyStatusRoundTime = 500;
        /// <summary>
        /// 最大准备时间数
        /// </summary>
        [INIField(Key = "ReadyStatusMaxTime")]
        public int ReadyStatusMaxTime = 3000;


        /// <summary>
        /// 投票跳过战斗阶段的超武
        /// </summary>
        [INIField(Key = "VoteSkipSW")]

        public string VoteSkipSW = "BRVoteSW";

        /// <summary>
        /// 允许投票跳过的初始时间（帧数）
        /// </summary>
        [INIField(Key = "VoteSkipInitTime")]
        public int VoteSkipInitTime = 1200;

        /// <summary>
        /// 投票跳过的每回合递增时间
        /// </summary>
        [INIField(Key = "VoteSkipRoundTime")]
        public int VoteSkipRoundTime = 300;

        /// <summary>
        /// 投票跳过的最大时间
        /// </summary>
        [INIField(Key = "VoteSkipMaxTime")]
        public int VoteSkipMaxTime = 2000;
        /// <summary>
        /// 战斗结束初始时间
        /// </summary>
        [INIField(Key = "BattleEndInitTime")]
        public int BattleEndInitTime = 2000;
        [INIField(Key = "BattleEndRoundTime")]
        /// <summary>
        /// 战斗结束每回合递增时间
        /// </summary>
        public int BattleEndRoundTime = 300;
        /// <summary>
        /// 战斗结束最大时间
        /// </summary>
        [INIField(Key = "BattleEndMaxTime")]
        public int BattleEndMaxTime = 5000;

        /// <summary>
        /// 回合开始发射的超武（用于授予投票准备完毕，刷新等操作的,移除战斗时使用的超武）
        /// </summary>
        [INIField(Key = "RoundStartLaunchSW")]
        /// <summary>
        /// 回合开始发射的超武（用于移除投票准备完毕，刷新等操作的,授予战斗时使用的超武如投票跳过）
        /// </summary>
        public string RoundStartLaunchSW = "";
        [INIField(Key = "RoundEndLaunchSW")]

        public string RoundEndLaunchSW = "";

        /// <summary>
        /// 自动变更所属到最近的出生点所属方，而不要通过触发变更所属
        /// </summary>
        [INIField(Key = "AutoSetHouseEnabled")]
        public bool AutoSetHouseEnabled = false;

    }


    [Serializable]
    public class FlyingText
    {
        public string Text { get; set; }

        public int Duration { get; set; }

        public CoordStruct Location { get; set; }

        /// <summary>
        /// 0 green 1 red
        /// </summary>
        public int Color = 0;
    }

    public class TechnoData : INIAutoConfig
    {
        [INIField(Key = "Image")]
        public string Image = "";
    }

    public class ArtData : INIAutoConfig
    {
        [INIField(Key = "Cameo")]
        public string Cameo = string.Empty;
        [INIField(Key = "CameoPCX")]
        public string CameoPCX = string.Empty;
    }

}
