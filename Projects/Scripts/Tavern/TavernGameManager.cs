using DynamicPatcher;
using Extension.Coroutines;
using Extension.EventSystems;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Newtonsoft.Json;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Scripts.Tavern;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

        public Dictionary<string, CardType> CardTypes { get; private set; } = new Dictionary<string, CardType>();

        public List<string> CardPool { get; private set; } = new List<string>();

        public List<string> CommanderPrerequisites { get; private set; } = new List<string> { "CTanyBD", "CBorisBD", "CYuriBD" };

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
        /// 获得升级酒馆的费用
        /// </summary>
        /// <returns></returns>
        public int GetUpgradeBaseCost(Pointer<HouseClass> house)
        {
            var node = FindPlayerNodeByHouse(house);
            if (node.BaseLevel >= ini.Data.BaseMaxLevel)
                return 0;

            return ini.Data.InitUpgradeCost + (ini.Data.BaseMaxLevel-1) * ini.Data.BaseMaxLevel;
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
                        //全部准备好以后进入下一个阶段
                        GameObject.StartCoroutine(ReadyToBattle());
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
                }
                else
                {

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

        IEnumerator DoInit()
        {
            yield return new WaitForFrames(5);
            foreach (var node in PlayerNodes)
            {
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

                //初始化指挥官选择
                foreach(var cmdbd in CommanderPrerequisites)
                {
                    TechnoPlacer.PlaceTechnoNear(TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(cmdbd), node.Owner.OwnerObject.Ref.Owner, CellClass.Coord2Cell(node.Owner.OwnerObject.Ref.Base.Base.GetCoords()));
                }
            }
            yield return new WaitForFrames(1);
          


            gameStatusMachine.OnRoundStart += RoundStart;
            gameStatusMachine.OnPrepared += () =>
            {
                Logger.Log("准备完了");
            };
            gameStatusMachine.OnBattleStart += () =>
            {
                Logger.Log("战斗开始");
            };
            gameStatusMachine.OnBattleEnd += () =>
            {
                Logger.Log("战斗结束");
            };
            gameStatusMachine.Init();
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

                //todo触发所有卡牌的回合开始效果
            }

            //给与准备时间
            var ticks = ini.Data.ReadyStatusInitTime + (CurrentRound - 1) * ini.Data.ReadyStatusRoundTime;
            if (ticks > ini.Data.ReadyStatusMaxTime)
            {
                ticks = ini.Data.ReadyStatusMaxTime;
            }
            ReadyStatusTick = ticks;
        }




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

    }

    public enum GameStatus
    {
        NoInited,
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
        public int MaxMoney = 300;


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
        /// 准备阶段初始时间（帧数）
        /// </summary>
        [INIField(Key = "ReadyStatusInitTime")]
        public int ReadyStatusInitTime = 900;
        /// <summary>
        /// 每轮额外增加的准备时间数
        /// </summary>
        [INIField(Key = "ReadyStatusRoundTime")]
        public int ReadyStatusRoundTime = 300;
        /// <summary>
        /// 最大准备时间数
        /// </summary>
        [INIField(Key = "ReadyStatusMaxTime")]
        public int ReadyStatusMaxTime = 3000;
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
