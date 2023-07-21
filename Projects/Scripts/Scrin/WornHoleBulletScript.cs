using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(WornHoleBulletScript))]
    public class WornHoleBulletScript : BulletScriptable
    {
        public WornHoleBulletScript(BulletExt owner) : base(owner)
        {

        }

        private bool actived = false;

        private bool stoped = false;

        static Pointer<SuperWeaponTypeClass> powerDownSW => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("WornHolePDSpecial");

        static Pointer<BulletTypeClass> pBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<WarheadTypeClass> aWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("WornActiveWH");
        static Pointer<WarheadTypeClass> eWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("WornEffectWH");

        static List<string> AiTypes = new List<string>()
        {
            "PlanetShip",
            "SCDISK",
            "SCDISK",
            "SCRINAA",
            "SCRINAA",
            "SCRINAA",
            "SCSeeker",
            "SCSeeker",
            "SCSeeker",
            "SCSeeker",
            "SCSeeker",
            "SCARAB4AI",
            "Corruptor4AI",
            "SCINF",
            "SCINF",
            "SCINF",
            "SCINF",
            "SCINF",
            "SCINF",
            "SCINF",
            "SCINF",
            "SCINF",
            "SCINF",
            "SCAAINF",
            "SCAAINF",
            "SCAAINF",
            "SCAAINF",
            "SCAAINF",
        };

        public override void Start()
        {
            INI = this.CreateRulesIniComponentWith<WornHoleAISetting>("WornHoleSpecial");
        }


        List<TechnoExt> targets = new List<TechnoExt>();

        private INIComponentWith<WornHoleAISetting> INI;

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!actived)
            {
                actived = true;

                if (Owner.OwnerObject.Ref.Owner.IsNull)
                    return;
                
                Pointer<TechnoClass> pTechno = Owner.OwnerObject.Ref.Owner;

                var currentLocation = pTechno.Ref.Base.Base.GetCoords();

                if (pTechno.Ref.Owner.IsNull)
                    return;

                var target = Owner.OwnerObject.Ref.TargetCoords;

                Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
                Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(powerDownSW);
                CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.TargetCoords);
                pSuper.Ref.IsCharged = true;
                pSuper.Ref.Launch(targetCell, true);
                pSuper.Ref.IsCharged = false;


                //播放动画
                var activeBullet = pBullet.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), pTechno, 1, aWarhead, 100, false);
                activeBullet.Ref.DetonateAndUnInit(target);
                //寻找空地

                //寻找附近的空点
                var currentCell = CellClass.Coord2Cell(target);

                CellSpreadEnumerator enumeratorTarget = new CellSpreadEnumerator(5);

                List<Pointer<CellClass>> emptyCells = new List<Pointer<CellClass>>();

                foreach (CellStruct offset in enumeratorTarget)
                {
                    CoordStruct where = CellClass.Cell2Coord(currentCell + offset, target.Z);

                    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                    {
                        if (pCell.IsNull)
                        {
                            continue;
                        }

                        Point2D p2d = new Point2D(60, 60);
                        Pointer<TechnoClass> ptargetTechno = pCell.Ref.FindTechnoNearestTo(p2d, false, pTechno);

                        if (TechnoExt.ExtMap.Find(ptargetTechno) == null)
                        {
                            emptyCells.Add(pCell);
                            continue;
                        }
                    }
                }

                //放置单位
                var index = 0;

                

                if (emptyCells.Count > 0)
                {
                    targets = new List<TechnoExt>();

                    if (Owner.OwnerObject.Ref.Owner.IsNotNull)
                    {
                        if(Owner.OwnerObject.Ref.Owner.Ref.Owner.Ref.ControlledByHuman() || INI.Data.EnableAIBehavior )
                        {
                            //寻找虫洞附近的友方单位
                            targets = Finder.FindTechno(pTechno.Ref.Owner, techno =>
                            {
                                return (techno.Ref.Base.Base.GetCoords().DistanceFrom(currentLocation) <= 5 * 256 || (techno.Ref.Base.Base.GetCoords().Z - currentLocation.Z > 100 && techno.Ref.Base.Base.GetCoords().DistanceFrom(currentLocation) <= 8 * 256)) && techno.Ref.Base.IsOnMap && !techno.Ref.Base.InLimbo && (techno.Ref.Base.Base.WhatAmI() == AbstractType.Unit || techno.Ref.Base.Base.WhatAmI() == AbstractType.Infantry);
                            }, FindRange.Owner).Take(emptyCells.Count()).ToList();
                        }


                        if (!Owner.OwnerObject.Ref.Owner.Ref.Owner.Ref.ControlledByHuman())
                        {
                            var takeCount = emptyCells.Count() > 8 ? 8 : emptyCells.Count() - targets.Count;
                            if (takeCount <= 0)
                                takeCount = 0;
                            var rd = new Random(Owner.OwnerObject.Ref.TargetCoords.X + Owner.OwnerObject.Ref.TargetCoords.Y + Owner.OwnerObject.Ref.TargetCoords.Z);
                            for(var i = 0; i < takeCount; i++)
                            {
                                var stype = AiTypes[rd.Next(0, AiTypes.Count())];
                                var pType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(stype);
                                if (pType.IsNotNull)
                                {
                                    var techno = pType.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner.Ref.Owner).Convert<TechnoClass>();
                                    if (techno == null)
                                        continue;
                                       
                                    if(TechnoPlacer.PlaceTechnoNear(techno, CellClass.Coord2Cell(Owner.OwnerObject.Ref.SourceCoords)))
                                    {
                                        targets.Add(TechnoExt.ExtMap.Find(techno));
                                    }
                                }
                            }
                            
                        }
                    }
                    else
                    {
                        targets = new List<TechnoExt>();
                    }
                  
                }

                foreach (var refertTechno in targets)
                {
                    if (index + 1 >= emptyCells.Count()) break;
                    if (!refertTechno.IsNullOrExpired())
                    {
                        refertTechno.OwnerObject.Ref.Base.Remove();
                        var lastLocation = refertTechno.OwnerObject.Ref.Base.Base.GetCoords();
                        var targetLocation = CellClass.Cell2Coord(emptyCells[index].Ref.MapCoords);
                        if (!refertTechno.OwnerObject.Ref.Base.Put(targetLocation, Direction.N))
                        {
                            if (!refertTechno.OwnerObject.Ref.Base.Put(lastLocation, Direction.N))
                                refertTechno.OwnerObject.Ref.Base.UnInit();
                        }
                        else
                        {
                            if (!refertTechno.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                            {
                                var mission = refertTechno.OwnerObject.Convert<MissionClass>();
                                mission.Ref.ForceMission(Mission.Hunt);
                            }
                        }
                        index++;
                    }
                }


                return;
            }

            if (!stoped)
            {
                stoped = true;
                foreach (var tref in targets)
                {
                    if (!tref.IsNullOrExpired())
                    {
                        var pTechno = tref.OwnerObject;
                        var lastLocation = pTechno.Ref.Base.Base.GetCoords();

                        if (pTechno.Ref.Base.IsOnMap)
                        {
                            //pTechno.Ref.SetDestination(default, false);
                            var mission = pTechno.Convert<MissionClass>();
                            mission.Ref.ForceMission(Mission.Stop);

                            var color = new ColorStruct(64, 0, 128);
                            Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(pTechno.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 2000), pTechno.Ref.Base.Base.GetCoords(), color, color, color, 100);
                            pLaser.Ref.Thickness = 10;
                            pLaser.Ref.IsHouseColor = true;

                            var effectBullet = pBullet.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), pTechno, 1, eWarhead, 100, false);
                            effectBullet.Ref.DetonateAndUnInit(pTechno.Ref.Base.Base.GetCoords());
                        }
                    }
                }

                return;
            }
        }

        private bool TrySetLocation(Pointer<TechnoClass> techno, CoordStruct location)
        {
            if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
            {
                var occFlags = pCell.Ref.OccupationFlags;
                techno.Ref.Base.SetLocation(location);
                pCell.Ref.OccupationFlags = occFlags;
                return true;
            }
            return false;
        }

    }

    public class WornHoleAISetting : INIAutoConfig
    {
        [INIField(Key = "WornHole.AIBehavior")]
        public bool EnableAIBehavior = false;
    }
}
