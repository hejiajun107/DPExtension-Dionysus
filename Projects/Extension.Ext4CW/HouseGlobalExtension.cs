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
        
    }

 
}
