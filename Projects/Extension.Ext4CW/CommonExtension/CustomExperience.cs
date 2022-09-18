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
        [INIField(Key = "Experience.GiveMultiple")]
        public double GiveExperienceMultiple = 1.0;

        [INIField(Key = "Experience.GainMultiple")]
        public double GainExperienceMultiple = 1.0;
    }
}
