using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script
{

    [GlobalScriptable(typeof(TechnoExt))]
    public partial class TechnoGlobalExtension : TechnoScriptable
    {
        public TechnoGlobalExtension(TechnoExt owner) : base(owner)
        {
        }
    }
}
