
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using System;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(YuriGuardScript))]

    public class YuriGuardScript : TechnoScriptable
    {
        public YuriGuardScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner);
        }

        private ManaCounter _manaCounter;


        private int ghostWalkingDuration = 0;
        //Random random = new Random(113156);

        static Pointer<SuperWeaponTypeClass> sw => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("UnitDeliveryYuriGuard");



        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);
                if (_manaCounter.Cost(100))
                {
                    ghostWalkingDuration = 200;
                }
            }

            if (ghostWalkingDuration > 0) 
            {
                if (ghostWalkingDuration % 20 == 0) 
                {
                    var bullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RPCloackWh"), 100, false);
                    bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
                ghostWalkingDuration--;
            }

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(weaponIndex == 0)
            {
                if(ghostWalkingDuration > 0)
                {
                    var bullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(pTarget, Owner.OwnerObject, Owner.OwnerObject.Ref.Veterancy.IsElite() ? 160 : 80, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("AKVHitWH"), 100, false);
                    bullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
                }
            }

            if (_manaCounter.Cost(45))
            {
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
                Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(sw);
                CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                pSuper.Ref.IsCharged = true;
                pSuper.Ref.Launch(targetCell, true);
                pSuper.Ref.IsCharged = false;
            }

            ghostWalkingDuration = 0;
        }





    }
}

