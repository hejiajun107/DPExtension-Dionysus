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

        public TavernGameManager(TechnoExt owner) : base(owner)
        {
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

        public void RegisterNode(TavernPlayerNode node)
        {
            PlayerNodes.Add(node);
        }
    }


    //public class GameManagerSetting : INIAutoConfig
    //{
    //    public bool Reverse { get; set; }
    //}

}
