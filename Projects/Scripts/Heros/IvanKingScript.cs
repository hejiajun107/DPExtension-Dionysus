﻿
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(IvanKingScript))]
    public class IvanKingScript : TechnoScriptable
    {
        public IvanKingScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner, 8);
        }

        private ManaCounter _manaCounter;

        Random random = new Random(114545);

        private int maxStrenth = 650;


        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("IVANKINGWH");



        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                ReleaseDrone();
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {

        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
    Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            try
            {
                if(!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                {
                    ReleaseDrone();
                }
                if (pAttackingHouse.IsNull)
                {
                    return;
                }
                if (pAttackingHouse.Ref.ArrayIndex != Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                {
                    var selfLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                    var targetLocation = pAttacker.Ref.Base.GetCoords();

                    var health = Owner.OwnerObject.Ref.Base.Health;

                    var distance = selfLocation.DistanceFrom(targetLocation);

                    if (distance <= 1400)
                    {
                        int rate = 30;
                        if (health / (double)maxStrenth < 0.4)
                        {
                            rate = 40;
                        }

                        var rd = random.Next(100);

                        if (rd <= rate)
                        {
                            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 70, warhead, 100, false);
                            pBullet.Ref.DetonateAndUnInit(targetLocation);

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //可能是因为爆炸产生的碎片之类的伤害导致获取不到攻击者
                ;
            }
        }

        private void ReleaseDrone()
        {
            if(_manaCounter.Cost(100))
            {
                TechnoPlacer.PlaceTechnoNear(TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("ODRON"), Owner.OwnerObject.Ref.Owner, CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords()));
            }
        }
    }
}