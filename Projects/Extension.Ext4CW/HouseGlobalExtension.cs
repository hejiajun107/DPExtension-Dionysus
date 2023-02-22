using Extension.CWUtilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.CW
{

    [Serializable]
    [GlobalScriptable(typeof(HouseExt))]
    public partial class HouseGlobalExtension : HouseScriptable
    {
        public HouseGlobalExtension(HouseExt owner) : base(owner)
        {
        }

        public Dictionary<string, int> TechnoMaxRank = new Dictionary<string, int>();

        public int NatashaNukeCount = 0;



        /// <summary>
        /// 羲和阳炎攻击位置
        /// </summary>
        public CoordStruct XHSunstrikeTarget1 { get; set; }
        public CoordStruct XHSunstrikeTarget2 { get; set; }
        public CoordStruct XHSunstrikeTarget3 { get; set; }



    }


}
