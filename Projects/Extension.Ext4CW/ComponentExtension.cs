using Extension.Components;
using Extension.CW;
using Extension.Ext;
using Extension.Script;
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

        public static HouseGlobalExtension GetHouseGlobalExtension(this TechnoExt technoExt)
        {
            //注册单位
            var house = HouseExt.ExtMap.Find(technoExt.OwnerObject.Ref.Owner);
            if (house == null)
            {
                return null;
            }
            var component = house.GameObject.GetComponent<HouseGlobalExtension>();
            return component;
        }

        public static bool TryGetHouseGlobalExtension(this TechnoExt technoExt,out HouseGlobalExtension houseGlobalExtension)
        {
            //注册单位
            var house = HouseExt.ExtMap.Find(technoExt.OwnerObject.Ref.Owner);

            if (house == null)
            {
                houseGlobalExtension = null;
                return false;
            }

            var component = house.GameObject.GetComponent<HouseGlobalExtension>();
            if(component !=null)
            {
                houseGlobalExtension = component;
                return true;
            }

            houseGlobalExtension = null;
            return false;
        }
    }
}
