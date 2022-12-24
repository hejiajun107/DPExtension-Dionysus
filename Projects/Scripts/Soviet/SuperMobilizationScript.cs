using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{

    [Serializable]
    [ScriptAlias(nameof(SuperMobilizationScript))]
    public class SuperMobilizationScript : TechnoScriptable
    {

        public SuperMobilizationScript(TechnoExt owner) : base(owner) { }


        private bool canRefresh = true;

        private int lifeTime = 1350;

        public override void OnUpdate()
        {
            Pointer<TechnoClass> ownerTechno = Owner.OwnerObject;
            Pointer<HouseClass> ownerHouse = ownerTechno.Ref.Owner;


            if (canRefresh)
            {
             
                Pointer<SuperClass> super1 = ownerHouse.Ref.FindSuperWeapon(SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("TeslaRushSpecial"));
                super1.Ref.IsCharged = true;

                Pointer<SuperClass> super2 = ownerHouse.Ref.FindSuperWeapon(SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SovietParaTankSpecial"));
                super2.Ref.IsCharged = true;
             
                ownerHouse.Ref.NumAirpads += 5;//机场数量
                ownerHouse.Ref.NumBarracks += 8;//兵营数量
                ownerHouse.Ref.NumWarFactories += 8;//重工数量
                ownerHouse.Ref.NumConYards += 5;//建造厂数量
                ownerHouse.Ref.NumShipyards += 5;//船厂数量
          
                canRefresh = false;
            }

            if (lifeTime-- <= 0)
            {
                ownerHouse.Ref.NumAirpads -= 5;//空军建造
                ownerHouse.Ref.NumBarracks -= 8;//兵营建造
                ownerHouse.Ref.NumWarFactories -= 8;//重工建造
                ownerHouse.Ref.NumConYards -= 5;//建筑建造
                ownerHouse.Ref.NumShipyards -= 5;//船厂建造


                ownerTechno.Ref.Base.Remove();
                ownerTechno.Ref.Base.UnInit();
            }
            //base.OnUpdate();
        }
    }
}
