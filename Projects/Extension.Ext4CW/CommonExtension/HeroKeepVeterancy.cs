using DynamicPatcher;
using Extension.CWUtilities;
using Extension.Ext;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.CW
{
    public partial class TechnoGlobalExtension
    {
        [RemoveAction]
        public void TechnoClass_Remove_HeroRespawn()
        {
            if (Data.IsHero)
            {
                var houseExtension = GetHouseExtension();
                var type = Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID;
                if (houseExtension != null)
                {
                    var maxRank = 0;
                    if (houseExtension.TechnoMaxRank.ContainsKey(type))
                    {
                        maxRank = houseExtension.TechnoMaxRank[type];
                    }

                    var currentRank = VeterancyToInt(Owner.OwnerObject.Ref.Veterancy);
                    if (currentRank > maxRank)
                    {
                        houseExtension.TechnoMaxRank[type] = currentRank;
                    }

                }
            }
        }

        [PutAction]
        public void TechnoClass_Put_HeroRespawn(CoordStruct coord, Direction faceDir)
        {
            if (Data.IsHero)
            {
                var houseExtension = GetHouseExtension();
                var type = Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID;
                if (houseExtension != null)
                {
                    if (houseExtension.TechnoMaxRank.ContainsKey(type))
                    {
                        var rank = houseExtension.TechnoMaxRank[type];

                        var currentRank = VeterancyToInt(Owner.OwnerObject.Ref.Veterancy);

                        if (rank > currentRank)
                        {
                            SetVerancy(Owner.OwnerObject, rank);
                        }
                    }
                }
            }
        }

        private HouseGlobalExtension GetHouseExtension()
        {
            var house = Owner.OwnerObject.Ref.Owner;
            if (house != null)
            {
                var houseExt = HouseExt.ExtMap.Find(house);
                if (!houseExt.IsNullOrExpired())
                {
                    return houseExt.GameObject.GetComponent<HouseGlobalExtension>();
                }
            }
            return null;
        }

        private int VeterancyToInt(VeterancyStruct veterancy)
        {
            if (veterancy.IsRookie())
            {
                return 0;
            }
            else if (veterancy.IsVeteran())
            {
                return 1;
            }
            else if (veterancy.IsElite())
            {
                return 2;
            }
            return 0;
        }

        private void SetVerancy(Pointer<TechnoClass> ptechno,int rank)
        {
            if (rank == 1)
            {
                ptechno.Ref.Veterancy.SetVeteran(true);
            }
            else if (rank == 2)
            {
                ptechno.Ref.Veterancy.SetElite(true);
            }
        }
    }
}
