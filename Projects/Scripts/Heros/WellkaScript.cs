
using DynamicPatcher;
using Extension.CW;
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Net.Sockets;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.Script;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(WellkaScript))]
    public class WellkaScript : TechnoScriptable
    {
        public WellkaScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner, 10);
            _voc = new VocExtensionComponent(owner);
        }

        private ManaCounter _manaCounter;

        private VocExtensionComponent _voc;

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> heroWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("WellkaIronRingHeroWh");
        static Pointer<WarheadTypeClass> unitWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("WellkaIronRingUnitWh");



        private int battleFrame = 0;

        //每隔多少帧检测一次
        private int rof = 0;

        public override void Awake()
        {
            base.Awake();
            _voc.Awake();
        }


        public override void OnUpdate()
        {
            HouseGlobalExtension houseExt;
            if (!Owner.TryGetHouseGlobalExtension(out houseExt))
            {
                return;
            }
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);
                MakeNuke(houseExt);
            }

            if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
            {
                MakeNuke(houseExt);
            }


            if (aimDelay > 0)
            {
                aimDelay--;
                if (aimDelay <= 0)
                {
                    LoseTarget();
                    rate = 0;
                }
            }

            //battleFrame--;

          
           
            Owner.OwnerObject.Ref.Ammo = houseExt.NatashaNukeCount;
        }


        private void MakeNuke(HouseGlobalExtension houseExt)
        {
            if (_manaCounter.Current >= 100)
            {
                if (Owner.OwnerObject.Ref.Owner.Ref.Available_Money() > 1000)
                {
                    {
                        if (houseExt.NatashaNukeCount < 10)
                        {
                            if (_manaCounter.Cost(100))
                            {
                                houseExt.NatashaNukeCount = houseExt.NatashaNukeCount + 1;
                                var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                                var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BuyNukeWH"), 100, false);
                                pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                            }
                        }
                    }
                }
            }

        }



        public override void OnRemove()
        {
            rate = 0;
            LoseTarget();
            base.OnRemove();
        }

        private int rate = 0;
        private int aimDelay = 10;
        TechnoExt LastTarget;
        public BulletExt CurrentBullet;
        public bool IsLauching = false;

        public void Reset()
        {
            IsLauching = false;
            rate = 0;
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            //瞄准
            if (weaponIndex == 1)
            {
                if(pTarget.CastToTechno(out var techno))
                {
                    var technoExt = TechnoExt.ExtMap.Find(techno);
                    if (LastTarget.IsNullOrExpired())
                    {
                        LastTarget = technoExt;
                        rate = 0;
                        LoseTarget();
                    }
                    else
                    {
                        if(techno == LastTarget.OwnerObject)
                        {
                            rate++;
                        }
                        else
                        {
                            LastTarget = technoExt;
                            rate = 0;
                            LoseTarget();
                        }
                       
                    }

                    if (rate >= 15 && !IsLauching)
                    {
                        
                        var firer = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("NTLAUNCHB").Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();
                        if (TechnoPlacer.PlaceTechnoFromEdge(firer))
                        {
                            //YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("NataCallAir"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                            _voc.PlaySpecialVoice(1, false);
                            var firerExt = TechnoExt.ExtMap.Find(firer);
                            firerExt.GameObject.CreateScriptComponent(nameof(NatashLauncherScript), NatashLauncherScript.UniqueId, "Launcher Rocket Script Manager", firerExt, Owner);

                            var fireweapon = 0;
                            var house = HouseExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
                            if (house != null)
                            {
                                var component = house.GameObject.GetComponent<HouseGlobalExtension>();
                                if (component != null)
                                {
                                    if (component.NatashaNukeCount > 0)
                                    {
                                        //if (_manaCounter.Cost(100))
                                        //{
                                            component.NatashaNukeCount--;
                                            fireweapon = 1;
                                        //}
                                    }
                                }
                            }
                            firer.Ref.Fire_NotVirtual(pTarget, fireweapon);
                            IsLauching = true;
                        }
                        else
                        {
                            rate = 0;
                        }
                    }

                    aimDelay = 15;
                }
                else
                {
                    rate = 0;
                }
            }
        }

        private void LoseTarget()
        {
            if(!CurrentBullet.IsNullOrExpired())
            {
                var coord = CurrentBullet.OwnerObject.Ref.Target.Ref.GetCoords();
                if(MapClass.Instance.TryGetCellAt((coord),out var pcell))
                {
                    CurrentBullet.OwnerObject.Ref.SetTarget(pcell.Convert<AbstractClass>());
                }
            }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
             Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            //battleFrame = 500;
        }
    }

    [ScriptAlias(nameof(NatashLauncherScript))]
    [Serializable]
    public class NatashLauncherScript : TechnoScriptable
    {
        public static int UniqueId = 2022122218;

        public NatashLauncherScript(TechnoExt owner, TechnoExt parent ) : base(owner)
        {
            Parent = parent;
        }

        int time = 0;
        public override void OnUpdate()
        {
            if (time++ > 100)
            {
                DetachFromParent();
                Owner.OwnerObject.Ref.Base.Remove();
                Owner.OwnerObject.Ref.Base.UnInit();
            }
            base.OnUpdate();
        }

        public TechnoExt Parent;
    }

    [ScriptAlias(nameof(NatashAirBulletScript))]
    [Serializable]
    public class NatashAirBulletScript : BulletScriptable
    {

        public NatashAirBulletScript(BulletExt owner) : base(owner)
        {
        }

        private bool inited = false;

        TechnoExt Launcher;

        public override void OnUpdate()
        {
            if(!inited)
            {
                inited = true;
                var ownerExt = TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
                if(!ownerExt.IsNullOrExpired())
                {
                    var ownerScript = ownerExt.GameObject.GetComponent<NatashLauncherScript>();
                    if (ownerScript != null)
                    {

                        Launcher = ownerScript.Parent;

                        if(!Launcher.IsNullOrExpired())
                        {
                            Owner.OwnerRef.Owner = Launcher.OwnerObject;

                            var script = Launcher.GameObject.GetComponent<WellkaScript>();
                            if (script != null)
                            {
                                script.CurrentBullet = Owner;
                            }
                            
                        }

                    }
                }
            }


        }

        public override void OnDetonate(Pointer<CoordStruct> pCoords)
        {
          
        }

        public override void OnDestroy()
        {
            if (!Launcher.IsNullOrExpired())
            {
                var script = Launcher.GameObject.GetComponent<WellkaScript>();
                if (script != null)
                {
                    script.Reset();
                }
            }
        }
    }


    [Serializable]
    [ScriptAlias(nameof(NatashaChargingSWScript))]
    public class NatashaChargingSWScript : SuperWeaponScriptable
    {
        public NatashaChargingSWScript(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            var house = HouseExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
            if (house != null)
            {
                var component = house.GameObject.GetComponent<HouseGlobalExtension>();
                if (component != null)
                {
                    if (component.NatashaNukeCount < 10)
                    {
                        component.NatashaNukeCount = component.NatashaNukeCount + 1;
                    }
                }
            }
        }
    }

}
