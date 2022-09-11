
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using System.Threading.Tasks;
using System.Linq;
using Extension.Shared;

namespace Scripts
{

    [Serializable]
    public class YuriGuard : TechnoScriptable
    {
        public YuriGuard(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter();
        }

        private ManaCounter _manaCounter;



        //Random random = new Random(113156);

        static Pointer<SuperWeaponTypeClass> sw => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("UnitDeliveryYuriGuard");

        public override void OnUpdate()
        {
            _manaCounter.OnUpdate(Owner);
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(_manaCounter.Cost(45))
            {
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
                Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(sw);
                CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                pSuper.Ref.IsCharged = true;
                pSuper.Ref.Launch(targetCell, true);
                pSuper.Ref.IsCharged = false;
            }
        }

 


       
    }
}

