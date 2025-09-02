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


        public TavernGameManager(TechnoExt owner) : base(owner)
        {
        }

        private INIComponentWith<GameManagerSetting> ini;

        private bool inited = false;

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
            foreach (var node in PlayerNodes)
            {
                node.OnRefreshShop();
            }
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
        [INIField(Key = "BuyCardPrice")]
        public int BuyCardPrice = 300;
        [INIField(Key = "SellCardPrice")]
        public int SellCardPrice = 100;
        [INIField(Key = "RefreshPrice")]
        public int RefreshPrice = 100;
        [INIField(Key = "InitTempSlots")]
        public int InitTempSlots = 3;
        [INIField(Key = "InitCombatSlots")]
        public int InitCombatSlots = 3;
        [INIField(Key = "InitShopSlots")]
        public int InitShopSlots = 3;
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
