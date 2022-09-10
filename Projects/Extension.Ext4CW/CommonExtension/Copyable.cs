using Extension.INI;
using Extension.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.CW
{
    public partial class TechnoGlobalTypeExt
    {
        [INIField(Key = "Copyable")]
        public bool Copyable = true;
    }

}
