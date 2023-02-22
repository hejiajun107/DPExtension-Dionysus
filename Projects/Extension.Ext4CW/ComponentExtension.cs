using Extension.Components;
using Extension.CW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Ext4CW
{
    public static class ComponentExtension
    {
        public static TechnoGlobalExtension GetTechnoGlobalComponent(this Component component)
        {
            if (component.FastGetScript1 == null)
            {
                return null;
            }

            return (TechnoGlobalExtension)component.FastGetScript1;
        }
    }
}
