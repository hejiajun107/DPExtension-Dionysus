using DynamicPatcher;
using Extension.INI;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.CW
{
    public partial class TechnoGlobalExtension
    {
        public void InfiltratedBy(Pointer<HouseClass> enterer)
        {
            if(Data.UseSpyEffect)
            {
                if (Owner.OwnerObject.Ref.Owner == enterer || Owner.OwnerObject.Ref.Owner.Ref.IsAlliedWith(enterer))
                    return;

                if (!string.IsNullOrEmpty(Data.VictimSuperWeapon))
                {
                    var swType = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.VictimSuperWeapon);
                    if(!swType.IsNull)
                    {
                        Pointer<HouseClass> pOwner = Owner.OwnerObject.Ref.Owner;
                        Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(swType);
                        pSuper.Ref.IsCharged = true;
                        pSuper.Ref.Launch(CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords()), true);
                        pSuper.Ref.IsCharged = false;
                    }
                }

                if (!string.IsNullOrEmpty(Data.InfiltratorSuperWeapon))
                {
                    var swType = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.InfiltratorSuperWeapon);
                    if (!swType.IsNull)
                    {
                        Pointer<HouseClass> pOwner = enterer;
                        Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(swType);
                        pSuper.Ref.IsCharged = true;
                        pSuper.Ref.Launch(CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords()), true);
                        pSuper.Ref.IsCharged = false;
                    }
                }
            }
        }
    }

    public partial class TechnoGlobalTypeExt
    {
        [INIField(Key = "CW.SpyEffect.Custom")]
        public bool UseSpyEffect = false;

        [INIField(Key = "CW.SpyEffect.VictimSuperWeapon")]
        public string VictimSuperWeapon;

        [INIField(Key = "CW.SpyEffect.InfiltratorSuperWeapon")]
        public string InfiltratorSuperWeapon;
    }
}
