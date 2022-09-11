using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.AI
{
    [Serializable]
    public class AntiAirGuidBuilding : TechnoScriptable
    {
        private int Side = 0;

        public SwizzleablePointer<SuperWeaponTypeClass> swTower = new SwizzleablePointer<SuperWeaponTypeClass>(IntPtr.Zero);

        private static readonly List<string> cnsts = new List<string>()
        {
            "GACNST","NACNST","YACNST","ZGJZC","JPCNST","RACNST",
        };

        public AntiAirGuidBuilding(TechnoExt owner) : base(owner)
        {
            var id = owner.Type.OwnerObject.Ref.Base.Base.ID.ToString();
            if (int.TryParse(id.Last().ToString(), out int s))
            {
                Side = s;
                swTower.Pointer = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("AntiAirDelivery" + Side);
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {

            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;

            ref DynamicVectorClass<Pointer<BuildingClass>> buildings = ref Owner.OwnerObject.Ref.Owner.Ref.Buildings;

            bool hasBuilding = false;

            for (int i = buildings.Count - 1; i >= 0; i--)
            {
                Pointer<BuildingClass> pBuilding = buildings.Get(i);

                if (cnsts.Contains(pBuilding.Ref.Type.Ref.Base.Base.Base.ID)   && pBuilding.Ref.Base.Base.IsOnMap == true && !pBuilding.Ref.Base.Base.InLimbo)
                {
                    hasBuilding = true;
                    break;
                }
            }

            if (!hasBuilding)
            {
                return;
            }

            if (Side > 0 && Side <= 4)
            {
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
                Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(swTower);
                CellStruct targetCell = CellClass.Coord2Cell(pTarget.Ref.GetCoords());
                pSuper.Ref.IsCharged = true;
                pSuper.Ref.Launch(targetCell, true);
                pSuper.Ref.IsCharged = false;
            }
        }




    }
}
