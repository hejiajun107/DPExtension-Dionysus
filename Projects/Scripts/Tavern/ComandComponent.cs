using Extension.Ext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Tavern
{
    /// <summary>
    /// 基类
    /// </summary>
    [Serializable]
    public class ComandComponent
    {
        public string Key { get; set; }

        public TechnoExt Owner { get; set; }

        public virtual void OnInit()
        {

        }
    }

    [Serializable]
    public class CommanderData
    {
        public string Prerequisites { get; set; }

        public string Techno { get; set; }
    }
}
