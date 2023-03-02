
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripts
{
    [Serializable]
    public class HealthAndPostion
    {
        public int Health { get; set; }

        public CoordStruct Location { get; set; }
    }

    [Serializable]
    [ScriptAlias(nameof(TanyScript))]
    public class TanyScript : TechnoScriptable
    {
        public TanyScript(TechnoExt owner) : base(owner) {
            _manaCounter = new ManaCounter(owner, 2);
        }

        private ManaCounter _manaCounter;


        static TanyScript()
        {
            // Task.Run(() =>
            // {
            //     while (true)
            //     {
            //         Logger.Log("Ticked.");
            //         Thread.Sleep(1000);
            //     }
            // });
        }

        Random random = new Random(62521);

        static Pointer<AnimTypeClass> chroAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("CHRONOEXPMINI");

        //static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChronoBeamC");

        //static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        //static Pointer<WarheadTypeClass> pWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChronoBeamC");

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChronoBeamC");


        //��¼��ʷ��Ϣ������ֵ��λ�ã�
        private List<HealthAndPostion> healthAndPostionHistories = new List<HealthAndPostion>();

        //����¼��
        private int historyMaxCount = 100;

        //��¼��ʷ��Ϣ��Ƶ�ʣ�ÿN֡��¼һ��
        private int historyInterval = 2;

        private int currentInterval = 0;



        public override void OnUpdate()
        {

            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                if (_manaCounter.Cost(100))
                {
                    BackWrap(true);
                }
            }

            currentInterval++;

            if (currentInterval >= historyInterval)
            {
                currentInterval = 0;
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                TechnoTypeExt extType = Owner.Type;


                CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();

                var health = pTechno.Ref.Base.Health;

                var hal = new HealthAndPostion()
                {
                    Health = health,
                    Location = currentLocation
                };

                if (healthAndPostionHistories.Count < historyMaxCount)
                {
                    healthAndPostionHistories.Add(hal);
                }
                else
                {
                    healthAndPostionHistories.RemoveAt(0);
                    healthAndPostionHistories.Add(hal);
                }

            }




        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            // TechnoTypeExt extType = Owner.Type;
            // Pointer<SuperWeaponTypeClass> pSWType = extType.FireSuperWeapon;

            // if (pSWType.IsNull == false)
            // {
            //     Pointer<TechnoClass> pTechno = Owner.OwnerObject;
            //     Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
            //     Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(pSWType);

            //     CellStruct targetCell = CellClass.Coord2Cell(pTarget.Ref.GetCoords());
            //     //Logger.Log("FireSuperWeapon({2}):0x({3:X}) -> ({0}, {1})", targetCell.X, targetCell.Y, pSWType.Ref.Base.ID, (int)pSuper);
            //     pSuper.Ref.IsCharged = true;
            //     pSuper.Ref.Launch(targetCell, true);
            //     pSuper.Ref.IsCharged = false;
            // }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
             Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if(!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
            {
                int rate = random.Next(100);
                if (rate < 40)
                {
                    if(_manaCounter.Cost(100))
                    {
                        BackWrap(false);
                    }
                }
            }
          

        }

        public void BackWrap(bool force)
        {
            if (healthAndPostionHistories.Count == historyMaxCount)
            {
                var hal = healthAndPostionHistories.OrderByDescending(x => x.Health).FirstOrDefault();
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                if (pTechno.Ref.Base.Health < hal.Health || force)
                {
                    CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                    Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, -10, warhead, 100, false);
                    pBullet.Ref.DetonateAndUnInit(currentLocation);
                    //MapClass.FlashbangWarheadAt(1, warhead, currentLocation, false);
                    //MapClass.DamageArea(currentLocation, 1, pTechno, warhead, false, pTechno.Ref.owner);

                    //chroAnim.Ref.Base.SpawnAtMapCoords(CellClass.Coord2Cell(currentLocation), pTechno.Ref.Owner);
                    //var pAnim = YRMemory.Create<AnimClass>(chroAnim, currentLocation);
                    pTechno.Ref.Base.Health = hal.Health;
                    //pTechno.Ref.Base.SetLocation(hal.Location);
                    TrySetLocation(pTechno, hal.Location);

                    //pBullet.Ref.DetonateAndUnInit(hal.Location);

                    healthAndPostionHistories.RemoveAll(x => true);
                }
            }
        }


        private bool TrySetLocation(Pointer<TechnoClass> techno, CoordStruct location)
        {
            if (!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                var pTechno = Owner.OwnerObject;
                var mission = pTechno.Convert<MissionClass>();

                var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                //= O

                //位置
                if (pTechno.CastToFoot(out Pointer<FootClass> pfoot))
                {
                    if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
                    {
                        var source = pTechno.Ref.Base.Base.GetCoords();
                        pfoot.Ref.Locomotor.Mark_All_Occupation_Bits(0);
                        pfoot.Ref.Locomotor.Force_Track(-1, source);
                        pTechno.Ref.Base.UnmarkAllOccupationBits(source);
                        var cLocal = pCell.Ref.Base.GetCoords();
                        var pLocal = new CoordStruct(cLocal.X, cLocal.Y, location.Z);
                        pTechno.Ref.Base.SetLocation(pLocal);
                        pTechno.Ref.Base.UnmarkAllOccupationBits(pLocal);
                    }
                }

                return true;
            }


            return false;
        }

    }
}