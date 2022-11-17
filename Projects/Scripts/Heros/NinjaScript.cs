using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using System;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(NinjaScript))]

    public class NinjaScript : TechnoScriptable
    {
        public NinjaScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter();
        }

        Random random = new Random(242641);

        private ManaCounter _manaCounter;

        //static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<BulletTypeClass> pAcidBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("AcidRainSeeker");
        static Pointer<WarheadTypeClass> pWHDamage => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("AcidDamageWh");
        static Pointer<WarheadTypeClass> pWHHealth => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("AcidHealthWh");

        static Pointer<SuperWeaponTypeClass> swNano => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("UnitDeliveryNanoLink");

        private bool IsRainning = false;

        private CoordStruct center;

        private int rainDuration = 650;

        private int currentRainFrame = 0;

        private int rainRof = 0;

        public override void OnUpdate()
        {
            _manaCounter.OnUpdate(Owner);

            //if (IsRainning && currentRainFrame<rainDuration)
            //{
            //    if(rainRof++ >=4)
            //    {
            //        for (int i = 0; i < 4; i++)
            //        {
            //            var ntarget = new CoordStruct(center.X + random.Next(-2500, 2500), center.Y + random.Next(-2500, 2500), 2000);
            //            var ntargetGround = new CoordStruct(ntarget.X, ntarget.Y, -center.Z);

            //            if (MapClass.Instance.TryGetCellAt(ntargetGround, out Pointer<CellClass> pCell))
            //            {
            //                var warhead = i % 2 == 0 ? pWHDamage : pWHHealth;
            //                Pointer <BulletClass> pBullet = pAcidBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 20, warhead, 50, true);
            //                BulletVelocity velocity = new BulletVelocity(0, 0, 0);
            //                pBullet.Ref.MoveTo(ntarget, velocity);
            //                pBullet.Ref.SetTarget(pCell.Convert<AbstractClass>());
            //            }
            //        }
            //        rainRof = 0;
            //    }

            //    currentRainFrame++;
            //}
            //else
            //{
            //    IsRainning = false;
            //    currentRainFrame = 0;
            //}
        }



        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 0)
            {
                if (!Owner.OwnerObject.Ref.Owner.IsNull)
                {
                    if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                    {
                        if (_manaCounter.Cost(100))
                        {
                            CreateNano();
                        }
                    }
                }
            }
            if (weaponIndex == 1)
            {
                //if (!IsRainning)
                //{
                if (_manaCounter.Cost(100))
                {
                    CreateNano();
                    //IsRainning = true;CreateNano
                    //rainRof = 0;
                    //currentRainFrame = 0;
                    //center = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                }
                //}
            }
        }


        private void CreateNano()
        {
            Pointer<TechnoClass> pTechno = Owner.OwnerObject;
            Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
            Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(swNano);
            CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            pSuper.Ref.IsCharged = true;
            pSuper.Ref.Launch(targetCell, true);
            pSuper.Ref.IsCharged = false;
        }

    }
}