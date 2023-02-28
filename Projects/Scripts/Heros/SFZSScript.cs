
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(SFZSScript))]

    public class SFZSScript : TechnoScriptable
    {
        public SFZSScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner,15);
        }


        private ManaCounter _manaCounter;

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                if (_manaCounter.Cost(100))
                {
                    var selected = Owner.OwnerObject.Ref.Base.IsSelected;

                    var techno = ptype.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();

                    var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                    Owner.OwnerObject.Ref.Base.Remove();

                    if (TechnoPlacer.PlaceTechnoNear(techno, CellClass.Coord2Cell(location)))
                    {
                        techno.Ref.Veterancy.Veterancy = Owner.OwnerObject.Ref.Veterancy.Veterancy;
                        if (selected)
                        {
                            techno.Ref.Base.Select();
                        }
                        var ext = TechnoExt.ExtMap.Find(techno);
                        if (ext != null)
                        {
                            ext.GameObject.CreateScriptComponent(nameof(ExecutionerScript), ExecutionerScript.UniqueId, "ExecutionerScript", ext, Owner);
                        }
                        YRMemory.Create<AnimClass>(pAnim, location);
                    }
                    else
                    {
                        if (!TechnoPlacer.PlaceTechnoNear(Owner.OwnerObject, CellClass.Coord2Cell(location)))
                        {
                            Owner.OwnerObject.Ref.Base.UnInit();
                        }
                        techno.Ref.Base.UnInit();
                    }

                }

            }

            base.OnUpdate();
        }



        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> pWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SFZSSlashWH");

        private Pointer<TechnoTypeClass> ptype => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("Executioner");

        private Pointer<AnimTypeClass> pAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SFCallAnim");

        







        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 0)
            {
                //var targetLocation = pTarget.Ref.GetCoords();
                //var selfLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                //// Logger.Log(targetLocation.DistanceFrom(selfLocation));
                //if (targetLocation.DistanceFrom(selfLocation) <= 400)
                //{
                //    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 80, pWH, 100, false);
                //    pBullet.Ref.DetonateAndUnInit(targetLocation);
                //}
            }
            else
            {

            }
        }
    }


    [Serializable]
    [ScriptAlias(nameof(ExecutionerScript))]
    public class ExecutionerScript : TechnoScriptable
    {
        public ExecutionerScript(TechnoExt owner, TechnoExt caller) : base(owner)
        {
            Caller = caller;
        }

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> pWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ExecSwWh");

        static Pointer<BulletTypeClass> pDeloyBullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("ExecBeamPr");

        static Pointer<WarheadTypeClass> pBlast = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ExecBlastWh");

        private int Delay = 1500;

        private int coolDown = 0;

        TechnoExt Caller;

        public static int UniqueId = 1668829562;

        public override void OnDestroy()
        {
            if (!Caller.IsNullOrExpired())
            {
                if (TechnoPlacer.PlaceTechnoNear(Caller.OwnerObject, CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords())))
                {
                    if (Owner.OwnerObject.Ref.Base.IsSelected)
                    {
                        Caller.OwnerObject.Ref.Base.Select();
                    }

                    if (Owner.OwnerObject.Ref.Veterancy.Veterancy > Caller.OwnerObject.Ref.Veterancy.Veterancy)
                    {
                        Caller.OwnerObject.Ref.Veterancy.Veterancy = Owner.OwnerObject.Ref.Veterancy.Veterancy;
                    }
                }
                else
                {
                    Caller.OwnerObject.Ref.Base.UnInit();
                }
            }
        }

        public override void OnUpdate()
        {
            if (coolDown > 0)
            {
                coolDown--;
            }

            if (Delay-- <= 0)
            {
                Owner.OwnerObject.Ref.Base.UnInit();
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            var isElite = Owner.OwnerObject.Ref.Veterancy.IsElite();

            if (coolDown <= 0)
            {
                coolDown = 400;
                var center = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var blastRadius = 1280;
                for (var angle = -180; angle < 180; angle += 30)
                {
                    var pos = new CoordStruct(center.X + (int)(blastRadius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(blastRadius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z + 50);


                    if (MapClass.Instance.TryGetCellAt(pos, out var pCell))
                    {
                        if (!Owner.OwnerObject.IsNull)
                        {
                            var bullet = pDeloyBullet.Ref.CreateBullet(pCell.Convert<AbstractClass>(), Owner.OwnerObject, 50, pBlast, 5, false);
                            bullet.Ref.MoveTo(center + new CoordStruct(0, 0, 100), new BulletVelocity(0, 0, 0));
                            bullet.Ref.SetTarget(pCell.Convert<AbstractClass>());

                            var bext = BulletExt.ExtMap.Find(bullet);
                            if (bext != null)
                            {
                                bext.GameObject.CreateScriptComponent(nameof(ExecutionDetonateBulletScript), "ExecutionDetonateBulletScript", bext);
                                bext.GameObject.CreateScriptComponent(nameof(ExecutionFeekBackBulletScript), "ExecutionFeekBackBulletScript", bext);
                            }
                        }
                    }
                }
            }
            else
            {
                var bullet = pBulletType.Ref.CreateBullet(pTarget, Owner.OwnerObject, isElite ? 240 : 120, pWH, 100, false);
                bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }
         
        }
    }


    [ScriptAlias(nameof(ExecutionDetonateBulletScript))]
    [Serializable]
    public class ExecutionDetonateBulletScript : BulletScriptable
    {
        static Pointer<BulletTypeClass> pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<WarheadTypeClass> pWh = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ExecBlastWh");


        public static int UniqueId = 1668829589;


        public ExecutionDetonateBulletScript(BulletExt owner) : base(owner)
        {
        }

        private CoordStruct last;

        public override void OnUpdate()
        {
            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;

            var current = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            if (last == default)
            {
                last = current;
            }

            if (current.DistanceFrom(last) >= 300)
            {
                last = current;
                var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Ref.Owner.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner, 50, pWh, 5, false);
                bullet.Ref.DetonateAndUnInit(current);
            }

        }
    }

    [ScriptAlias(nameof(ExecutionFeekBackBulletScript))]
    [Serializable]
    public class ExecutionFeekBackBulletScript : BulletScriptable
    {
        public ExecutionFeekBackBulletScript(BulletExt owner) : base(owner)
        {
        }

        static Pointer<BulletTypeClass> pBullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("ExecBeamPr");

        static Pointer<WarheadTypeClass> pWh = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ExecBlastWh");

        public static int UniqueId = 1668829595;


        public override void OnDetonate(Pointer<CoordStruct> pCoords)
        {
            if(!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                var source = Owner.OwnerObject.Ref.SourceCoords;
                var target = pCoords.Ref;

                if (MapClass.Instance.TryGetCellAt(source, out var pCell))
                {
                    if (!Owner.OwnerObject.Ref.Owner.IsNull)
                    {
                        var bullet = pBullet.Ref.CreateBullet(Owner.OwnerObject.Ref.Owner.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner, 50, pWh, 5, false);
                        bullet.Ref.MoveTo(target + new CoordStruct(0, 0, 100), new BulletVelocity(0, 0, 0));
                        bullet.Ref.SetTarget(pCell.Convert<AbstractClass>());

                        var bext = BulletExt.ExtMap.Find(bullet);
                        if (bext != null)
                        {
                            bext.GameObject.CreateScriptComponent(nameof(ExecutionDetonateBulletScript), "ExecutionDetonateBulletScript", bext);
                        }
                    }
                }
            }
        }
    }









}