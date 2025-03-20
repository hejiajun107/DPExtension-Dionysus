
using DynamicPatcher;
using Extension.EventSystems;
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(BadunScript))]
    public class BadunScript : TechnoScriptable
    {
        public BadunScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner);
        }

        private ManaCounter _manaCounter;

        static BadunScript()
        {
          
        }

        Random random = new Random(110512);

        static ColorStruct innerColor = new ColorStruct(255, 255, 255);
        static ColorStruct outerColor = new ColorStruct(255, 0, 0);
        static ColorStruct outerSpread = new ColorStruct(255, 0, 0);


        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> pWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BaidunAOEffect");
        static Pointer<WarheadTypeClass> sWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BaidunLaserReportWh");

        static Pointer<WarheadTypeClass> showWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BaidunAnimEFWh");

        static Pointer<WeaponTypeClass> bdlaser => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("BadunLaser");



        private int burstCount = 12;

        private bool isBurst = false;

        private int currentBurst = 0;

        private CoordStruct target;

        private int delay = 2;

        private int currentInterval = 0;

        private int burstRate = 10;

        //private int needBurstCount = 10;

        private bool reloaded = false;

        private int currentDuration = 0;

        private int filp = 1;

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                StartBurst();
            }

            int maxKeep = 350;


            if (reloaded)
            {
                if (currentDuration >= maxKeep)
                {
                    EndBurst();
                }
                else
                {
                    currentDuration++;
                    var ammo = 6 - (int)((currentDuration / (double)maxKeep) * 6);


                    if (ammo < 0)
                    {
                        ammo = 0;
                    }
                    Owner.OwnerObject.Ref.Ammo = ammo;
                }
            }
            else
            {
                Owner.OwnerObject.Ref.Ammo = 0;
            }

            if (isBurst)
            {
                if (currentInterval >= delay)
                {
                    if (currentBurst < burstCount)
                    {
                        currentBurst++;

                        var pTechno = Owner.OwnerObject;
                        var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                        filp = -filp;
                        var launchLocation = ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, new CoordStruct(100, -15, 115), false, filp);
                        //new CoordStruct(currentLocation.X, currentLocation.Y, currentLocation.Z + 100);

                        var ntarget = new CoordStruct(target.X + random.Next(-360, 360), target.Y + random.Next(-360, 360), target.Z);


                        //Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(launchLocation, ntarget, innerColor, outerColor, outerSpread, 10);
                        //pLaser.Ref.Thickness = 10;
                        //pLaser.Ref.IsHouseColor = false;

                        Pointer<BulletClass> pBullets = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, sWH, 100, true);
                        pBullets.Ref.DetonateAndUnInit(ntarget);

                        Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 16, pWH, 100, true);
                        pBullet.Ref.Base.SetLocation(ntarget);
                        Owner.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, bdlaser, launchLocation);
                        pBullet.Ref.DetonateAndUnInit(ntarget);
                    }
                    else
                    {
                        isBurst = false;
                        currentBurst = 0;
                    }
                    currentInterval = 0;
                }
                currentInterval++;
            }
        }

        //public override void OnRender()
        //{
        //    _manaCounter.OnRender(Owner);
        //}

    
        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            bool controlledByAi = false;

            if (!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                    controlledByAi = true;
            }

            if (controlledByAi && !reloaded)
            {
                if (!Owner.OwnerObject.Ref.Base.InLimbo)
                    StartBurst();
            }

            if (weaponIndex == 0)
            {
                if (isBurst == false)
                {
                    if (random.Next(100) <= burstRate)
                    {
                        isBurst = true;
                        currentBurst = 0;
                        target = pTarget.Ref.GetCoords();
                    }
                }
            }
            //else
            //{

            //}
        }


        private void StartBurst()
        {
            if (_manaCounter.Cost(100))
            {
                var pTechno = Owner.OwnerObject;

                //Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, showWH, 100, true);
                //pBullet.Ref.DetonateAndUnInit(pTechno.Ref.Base.Base.GetCoords());
                //var panim = YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("DYRELOADEDEFFECT"), pTechno.Ref.Base.Base.GetCoords());
                //panim.Ref.SetOwnerObject(pTechno.Convert<ObjectClass>());

                if (Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                {
                    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BDBurstDWWH"), 100, false);
                    pBullet.Ref.DetonateAndUnInit(pTechno.Ref.Base.Base.GetCoords());
                }

                reloaded = true;
                currentDuration = 0;
                burstRate = 60;

                EventSystem.General.AddTemporaryHandler(EventSystem.General.LogicClassUpdateEvent, AutoFireUpdate);
            }
        }


        private void EndBurst()
        {
            reloaded = false;
            burstRate = 10;
            currentDuration = 0;
            EventSystem.General.RemoveTemporaryHandler(EventSystem.General.LogicClassUpdateEvent, AutoFireUpdate);
        }


        //private int delay = 0;

        private TechnoExt currentTarget;

        private int autoFireRof = 0;

        public void AutoFireUpdate(object sender, EventArgs args) 
        {
            if(args is LogicClassUpdateEventArgs largs)
            {
                if (!largs.IsLateUpdate)
                    return;
            }
            else
            {
                return;
            }
            if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
            {
                return;
            }


            if(Owner.OwnerObject.Ref.Target.IsNotNull)
            {
                if(Owner.OwnerObject.Ref.Target.CastToTechno(out var targetTechno))
                {
                    if (currentTarget.IsNullOrExpired())
                    {
                        currentTarget = TechnoExt.ExtMap.Find(targetTechno);
                    }
                    else
                    {
                        if(currentTarget.OwnerObject != targetTechno)
                        {
                            currentTarget = TechnoExt.ExtMap.Find(targetTechno);
                        }
                    }
                }
            }

            if (currentTarget.IsNullOrExpired())
            {
                var picked = FindTarget(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                if (picked.IsNotNull)
                {
                    currentTarget = TechnoExt.ExtMap.Find(picked.Convert<TechnoClass>());
                }
            }



            if (!currentTarget.IsNullOrExpired())
            {
                if (Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(currentTarget.OwnerObject.Ref.Base.Base.GetCoords()) > 7 * Game.CellSize)
                {
                    currentTarget = null;
                    return;
                }

                if (Owner.OwnerObject.CastToFoot(out Pointer<FootClass> pfoot))
                {
                    var dir = GameUtil.Point2Dir(Owner.OwnerObject.Ref.Base.Base.GetCoords(), currentTarget.OwnerObject.Ref.Base.Base.GetCoords());
                    var tdir = new DirStruct(16, (short)DirStruct.TranslateFixedPoint(3, 16, (uint)dir));
                    Owner.OwnerObject.Ref.Facing.set(tdir);

                    if (autoFireRof > 0)
                    {
                        autoFireRof--;
                        return;
                    }
                    Owner.OwnerObject.Ref.Fire_NotVirtual(currentTarget.OwnerObject.Convert<AbstractClass>(), 0);
                    autoFireRof = Owner.OwnerObject.Ref.Veterancy.IsElite() ? 2 : 4;
                }
            }
        }

        private int findRate = 0;

        private Pointer<AbstractClass> FindTarget(CoordStruct coord)
        {
            if (findRate-- <= 0)
            {
                findRate = 3;
            }

            var zhongli = new[]
            {
                "Special",
                "Neutral"
            };

            List<Pointer<ObjectClass>> list = ObjectFinder.FindTechnosNear(coord, 7 * Game.CellSize)
                .Where(x => !x.Ref.Base.GetOwningHouse().Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner) && !zhongli.Contains(x.Ref.Base.GetOwningHouse().Ref.Type.Ref.Base.ID))
                .Where(x => Owner.OwnerObject.Ref.CanAttack(x))
                .OrderBy(x => x.Ref.Base.GetCoords().DistanceFrom(coord))
                .ToList();

            if (list.Count > 0)
            {
                return list.FirstOrDefault().Convert<AbstractClass>();
            }

            return Pointer<AbstractClass>.Zero;
        }

        public override void OnDestroy()
        {
            EventSystem.General.RemoveTemporaryHandler(EventSystem.General.LogicClassUpdateEvent, AutoFireUpdate);
        }

    }
}