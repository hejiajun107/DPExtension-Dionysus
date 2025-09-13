
using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Linq;

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

    [Serializable]
    [ScriptAlias(nameof(ZZBombScript))]
    public class ZZBombScript : TechnoScriptable
    {
        public ZZBombScript(TechnoExt owner) : base(owner)
        {
        }

        public TechnoExt techno { get; private set; }

        bool inited = false;
        public override void OnUpdate()
        {
            if (!inited) {
                inited = true;
                var hero = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), 5 * Game.CellSize).OrderBy(x => x.Ref.Base.GetCoords().BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords()))
                    .Where(x =>
                    {
                        var ptechno = x.Convert<TechnoClass>();
                        if (ptechno.Ref.Owner != Owner.OwnerObject.Ref.Owner)
                            return false;

                        if(ptechno.Ref.Type.Ref.Base.Base.ID != "IVANKING" && ptechno.Ref.Type.Ref.Base.Base.ID != "IVANKINGAI")
                            return false; 

                        return true;
                    }).ToList();

                if (hero.Count() > 0)
                {
                    var pFound = hero.First().Convert<TechnoClass>();
                    var ext = TechnoExt.ExtMap.Find(pFound);
                    techno = ext;
                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (techno.IsNullOrExpired())
                return;

            var wh = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Shock");

            var targets = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), 5 * Game.CellSize).OrderByDescending(x => x.Ref.Base.GetCoords().BigDistanceForm(pTarget.Ref.GetCoords()))
                   .Where(x =>
                   {
                       var ptechno = x.Convert<TechnoClass>();
                       if (ptechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                           return false;

                       if (ptechno.Ref.Base.InLimbo)
                           return false;

                       if (ptechno.Ref.Base.Base.IsInAir())
                           return false;

                       if (MapClass.GetTotalDamage(10000, wh, ptechno.Ref.Type.Ref.Base.Armor, 0) == 0)
                           return false;

                       return true;
                   }).ToList();

            if (targets.Count() > 0)
            {
                var pt = targets.First().Convert<TechnoClass>();
                var bTypoe = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("IVANKLinkPR");
                var damage = 80;
                Pointer<BulletClass> pBullet = bTypoe.Ref.CreateBullet(pt.Convert<AbstractClass>(), techno.OwnerObject, damage, wh, 50, true);
                BulletVelocity velocity = new BulletVelocity(0, 0, 0);
                pBullet.Ref.Base.SetLocation(pTarget.Ref.GetCoords() + new CoordStruct(0, 0, 50));
                pBullet.Ref.MoveTo(pTarget.Ref.GetCoords() + new CoordStruct(0,0, 50), velocity);
                pBullet.Ref.SetTarget(pt.Convert<AbstractClass>().Convert<AbstractClass>());
            }
        }
    }

    [Serializable]
    [ScriptAlias(nameof(ElecTranserBulletScript))]
    public class ElecTranserBulletScript : BulletScriptable
    {
        public ElecTranserBulletScript(BulletExt owner) : base(owner)
        {
        }

        public int Count { get; set; } = 1;

        private CoordStruct? coord;

        public override void OnUpdate()
        {
            if(coord is null)
            {
                coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            }
        }

        public override void OnDetonate(Pointer<CoordStruct> pCoords)
        {

            if (coord is null)
                return;

            var owner = Owner.OwnerObject.Ref.Owner;
            if(owner.IsNotNull)
            {
                var pbolt = owner.Ref.Electric_Zap(Owner.OwnerObject.Convert<AbstractClass>(), WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ElectricBolt"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                pbolt.Ref.Point1 = coord.Value;
                pbolt.Ref.Point2 = pCoords.Ref;
                VocClass.PlayAt(VocClass.FindIndex("TeslaTroopAttack"), coord.Value);

                if(Count < 5)
                {
                    var wh = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Shock");

                    var targets = ObjectFinder.FindTechnosNear(pCoords.Ref, 5 * Game.CellSize).OrderByDescending(x => x.Ref.Base.GetCoords().BigDistanceForm(pCoords.Ref))
                          .Where(x =>
                          {
                              var ptechno = x.Convert<TechnoClass>();
                              if (ptechno.Ref.Owner.Ref.IsAlliedWith(owner.Ref.Owner))
                                  return false;

                              if (ptechno.Ref.Base.InLimbo)
                                  return false;

                              if (ptechno.Ref.Base.Base.IsInAir())
                                  return false;

                              if (MapClass.GetTotalDamage(10000, wh, ptechno.Ref.Type.Ref.Base.Armor, 0) == 0)
                                  return false;

                              if (pCoords.Ref.BigDistanceForm(ptechno.Ref.Base.Base.GetCoords()) < 1 * Game.CellSize)
                                  return false;

                              return true;
                          }).ToList();

                    if (targets.Count() > 0)
                    {
                        var pt = targets.First().Convert<TechnoClass>();
                        //
                        var bTypoe = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("IVANKLinkPR");
                        var damage = 80;
                        for (var i = 0; i < Count; i++)
                        {
                            damage = (int)(damage * 0.8);
                        }
                        Pointer<BulletClass> pBullet = bTypoe.Ref.CreateBullet(pt.Convert<AbstractClass>(), owner, damage, wh, 50, true);

                        var ext = BulletExt.ExtMap.Find(pBullet);
                        if(ext != null)
                        {
                            var component = ext.GameObject.GetComponent<ElecTranserBulletScript>();
                            component.Count = Count + 1;
                        }

                        BulletVelocity velocity = new BulletVelocity(0, 0, 0);
                        pBullet.Ref.Base.SetLocation(pCoords.Ref + new CoordStruct(0, 0, 50));
                        pBullet.Ref.MoveTo(pCoords.Ref + new CoordStruct(0, 0, 50), velocity);
                        pBullet.Ref.SetTarget(pt.Convert<AbstractClass>());
                    }
                }

                //Owner.OwnerObject.Ref.
                //if(Owner.OwnerObject.Ref.)
            }
            //var pbolt = Owner.OwnerObject.Ref.Electric_Zap(Owner.OwnerObject.Convert<AbstractClass>(), WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ElectricBolt"), Owner.OwnerObject.Ref.Base.Base.GetCoords());

            base.OnDetonate(pCoords);
        }
    }
}