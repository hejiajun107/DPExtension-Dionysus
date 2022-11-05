using Extension.INI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.CW
{
    public partial class TechnoGlobalTypeExt
    {
        [INIField(Key = "Firestorm.Wall")]
        public bool IsFirestormWall = false;

        [INIField(Key = "IsTrench")]
        public string IsTrench = null;
    }
}
