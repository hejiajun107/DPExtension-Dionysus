using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scripts.Tavern;
using Extension.INI;
using System.IO;
using Newtonsoft.Json;

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

        public TavernGameManager(TechnoExt owner) : base(owner)
        {
        }

        private INIComponentWith<GameManagerSetting> ini;

        public override void Awake()
        {
            ini = Owner.GameObject.CreateRulesIniComponentWith<GameManagerSetting>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            //导入所有卡配置
            LoadCardTypes();
            InitCardPools();
        }

        public override void OnUpdate()
        {
            //注册全局实例
            if (Instance == null) 
            {
                Instance = this;
            }

            //未全部注册
            if (PlayerNodes.Count < maxPlayer)
            {
                return;
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
    }


    public class GameManagerSetting : INIAutoConfig
    {
        /// <summary>
        /// 卡牌配置文件路径，可以多个以,隔开
        /// </summary>
        [INIField(Key = "CardConfigFiles")]
        public string CardConfigFiles = "";


    }

}
