using Extension.INI;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.CW
{
    public partial class TechnoGlobalExtension
    {
        public bool IsDeployedFrom { get; set; } = false;


        public void ConvertSyncStatus(Pointer<TechnoClass> from,Pointer<TechnoClass> to)
        {
            if (Data.ConvertShareAmmo == true)
            {
                to.Ref.Ammo = from.Ref.Ammo;
            }
        }
    }

    public partial class TechnoGlobalTypeExt
    {

        [INIField(Key = "Convert.ShareAmmo")]
        public bool ConvertShareAmmo = false;
    }
}
