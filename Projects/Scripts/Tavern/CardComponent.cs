using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scripts.Tavern;
using PatcherYRpp;

namespace Scripts.Tavern
{
    [ScriptAlias(nameof(CardComponent))]
    [Serializable]
    public class CardComponent : TechnoScriptable
    {
        public CardComponent(TechnoExt owner) : base(owner)
        {
        }

        public CardType CardType { get; set; }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            
        }

       
    }

    [Serializable]
    public class CardType
    {
        /// <summary>
        /// 卡牌注册名，需要唯一，可与单位注册名一致
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 对应单位的注册名
        /// </summary>
        public string TechnoType { get; set; }
    }
}
