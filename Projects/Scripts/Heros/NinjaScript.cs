using Extension.Ext;
using Extension.Ext4CW;
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
            _manaCounter = new ManaCounter(owner);
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


        private int currentRainFrame = 0;


        public override void Awake()
        {
            base.Awake();
            if (Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID.ToString().EndsWith("AI"))
            {
                Owner.GameObject.GetTechnoGlobalComponent().IsAiEdition = true;
            }
        }

        public override void OnUpdate()
        {
            if(NanoShieldEnabled)
                NanoUpdate();

            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                if (NanoShieldEnabled)
                    return;

                if (_manaCounter.Cost(100))
                {
                    CreateNano();
                }
            }


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


        private int shieldMax = 2000;

        private int _shieldValue = 2000;
        private bool _nanoShieldEnabled = false;


        public int ShieldValue
        {
            get
            {
                return _shieldValue;
            }
            private set
            {
                _shieldValue = value;
                if(_shieldValue <= 0)
                {
                    NanoShieldEnabled = false;
                }
            }
        }


        private int nanoDuration = 1000;

        private int nanoCheckRof = 20;


        public bool NanoShieldEnabled
        {
            get
            {
                return _nanoShieldEnabled;
            }
            private set
            {
                if(value == false)
                {
                    _manaCounter.Resume();
                }
                else
                {
                    _manaCounter.Pause();
                }
                _nanoShieldEnabled = value;
            }
        }

        private void NanoUpdate()
        {
            if (nanoDuration-- < 0)
            {
                NanoShieldEnabled = false;
                return;
            }

            if (ShieldValue <= 0)
            {
                NanoShieldEnabled = false;
                return;
            }

            if (nanoCheckRof-- > 0)
                return;

            nanoCheckRof = 20;

            var pInvisoType = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
            var bullet1 = pInvisoType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("NanoLinkEffectWH"), 100, false);
            bullet1.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            //
            var bullet2 = pInvisoType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("NanoLinkDebuffWH"), 100, false);
            bullet2.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

        }

        public void CostShield(int value)
        {
            ShieldValue -= value;
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
            //if (weaponIndex == 1)
            //{
            //    //if (!IsRainning)
            //    //{
            //    if (_manaCounter.Cost(100))
            //    {
            //        CreateNano();
            //        //IsRainning = true;CreateNano
            //        //rainRof = 0;
            //        //currentRainFrame = 0;
            //        //center = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            //    }
            //    //}
            //}
        }


        private void CreateNano()
        {
            NanoShieldEnabled = true;
            nanoDuration = 1000;
            ShieldValue = shieldMax;
            //Pointer<TechnoClass> pTechno = Owner.OwnerObject;
            //Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
            //Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(swNano);
            //CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            //pSuper.Ref.IsCharged = true;
            //pSuper.Ref.Launch(targetCell, true);
            //pSuper.Ref.IsCharged = false;
        }

    }


    [ScriptAlias(nameof(NanoLinkAttachEffectScript))]
    [Serializable]
    public class NanoLinkAttachEffectScript : AttachEffectScriptable
    {
        public NanoLinkAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private TechnoExt Protector;

        public override void OnUpdate()
        {
            base.OnUpdate();
        }



        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            Duration = duration;

            if (!pAttacker.IsNull)
            {
                if(pAttacker.CastToTechno(out var ptechno))
                {
                    Protector = TechnoExt.ExtMap.Find(ptechno);
                }
            }

            base.OnAttachEffectRecieveNew(duration, pDamage, pWH, pAttacker, pAttackingHouse);
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (!Protector.IsNullOrExpired())
            {
                if (!Owner.IsNullOrExpired())
                {
                    if (pAttackingHouse.IsNull)
                    {
                        return;
                    }
          
                    if (pDamage.Ref <= 1)
                    {
                        return;
                    }


                    var realDamage = MapClass.GetTotalDamage(pDamage.Ref, pWH, Owner.OwnerObject.Ref.Type.Ref.Base.Armor, DistanceFromEpicenter) / 2;

                    if (!Protector.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                    {
                        realDamage *= 2;
                    }

                    var script = Protector.GameObject.GetComponent<NinjaScript>();
                    if (script != null)
                    {
                        if (script.NanoShieldEnabled)
                        {
                            script.CostShield(realDamage);
                        }
                        YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("NanoLinkDMG"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    }


                }
            }

            base.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
        }






    }
}