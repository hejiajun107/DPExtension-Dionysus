using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void RegisterNode(TavernPlayerNode node)
        {
            PlayerNodes.Add(node);
        }
    }


}
