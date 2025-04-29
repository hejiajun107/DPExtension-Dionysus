
using DynamicPatcher;
using Extension.Coroutines;
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections;
using System.Linq;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(BorisScript))]

    public class BorisScript : TechnoScriptable
    {
        public BorisScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner);
        }

        private ManaCounter _manaCounter;

        public TechnoExt Related { get; set; }


        Random random = new Random(113542);

        public bool Locked { get; set; } = false;


        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("BorisParaBullet");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BorisParaDropperWH");


        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                if(Related.IsNullOrExpired())
                {
                    if (_manaCounter.Cost(100))
                    {
                        CreateParadDrop(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    }
                }
            }

            if (_manaCounter.IsPaused() && !Locked && Related.IsNullOrExpired())
            {
                _manaCounter.Resume();
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
                    if (pAttackingHouse.IsNull)
                    {
                        return;
                    }
                    if (pAttackingHouse.Ref.ArrayIndex != Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                    {
                        if (Related.IsNullOrExpired())
                        {
                            if (_manaCounter.Cost(100))
                            {
                                CreateParadDrop(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                            }
                        }
                         
                    }
                }
            }
            catch (Exception) {; }
        }


        private void CreateParadDrop(CoordStruct center)
        {
            //var count = Owner.OwnerObject.Ref.Veterancy.IsElite() ? 4 : 2;
            //for (int i = 0; i < count; i++)
            //{
                var ntarget = new CoordStruct(center.X + random.Next(-500, 500), center.Y + random.Next(-500, 500), 3500);
                var ntargetGround = new CoordStruct(ntarget.X, ntarget.Y, center.Z);

                if (MapClass.Instance.TryGetCellAt(ntargetGround, out Pointer<CellClass> pCell))
                {
                    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 200, warhead, 60, true);
                    BulletVelocity velocity = new BulletVelocity(0, 0, 0);
                    pBullet.Ref.MoveTo(ntarget, velocity);
                    pBullet.Ref.SetTarget(pCell.Convert<AbstractClass>());
                }
            //}

            Locked = true;
            _manaCounter.Pause();
            Owner.GameObject.StartCoroutine(Recover());
        }

         IEnumerator Recover()
        {
            yield return new WaitForFrames(100);
            Locked = false;
        }


        
        public void PutSlaveNear(CoordStruct targetCoord)
        {
            Owner.GameObject.StartCoroutine(PutSalve(targetCoord));
        }

        IEnumerator PutSalve(CoordStruct targetCoord)
        {
            yield return new WaitForFrames(2);
            var hshk = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("HSHK").Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();

            if (TechnoPlacer.PlaceTechnoNear(hshk, CellClass.Coord2Cell(targetCoord)))
            {
                var slaveExt = TechnoExt.ExtMap.Find(hshk);
                if (!slaveExt.IsNullOrExpired())
                {
                    Related = slaveExt;
                    slaveExt.GameObject.CreateScriptComponent(nameof(BorisDroppedSHKScript), BorisDroppedSHKScript.UniqueId, nameof(BorisDroppedSHKScript), slaveExt, Owner);
                }
            }
        }

    }


    [Serializable]
    [ScriptAlias(nameof(BorisDroppedSHKScript))]
    public class BorisDroppedSHKScript : TechnoScriptable
    {
        public static int UniqueId = 2025033019;

        public TechnoExt Master { get; private set; }

        public BorisDroppedSHKScript(TechnoExt owner, TechnoExt master) : base(owner)
        {
            Master = master;
        }

        private int rof = 30;

        public override void OnUpdate()
        {
            if(Master.IsNullOrExpired())
            {
                Owner.OwnerObject.Ref.Base.TakeDamage(1000, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Super"), false);
            }
            else
            {
                Owner.OwnerObject.Ref.Veterancy.Veterancy = Master.OwnerObject.Ref.Veterancy.Veterancy;
                if (rof-- <= 0)
                {
                    rof = 30;
                    if(!Owner.OwnerObject.Ref.Base.InLimbo)
                    {
                        if (Master.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords()) < 5 * Game.CellSize)
                        {
                            var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("HSHKPUPWh"), 100, false);
                            pInviso.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    [ScriptAlias(nameof(BorisDropeddSHKBulletScript))]
    public class BorisDropeddSHKBulletScript : BulletScriptable
    {
        public BorisDropeddSHKBulletScript(BulletExt owner) : base(owner)
        {

        }

        private bool inited = false;

        public override void OnUpdate()
        {
            if (!inited)
            {
                inited = true;
                if (Owner.OwnerObject.Ref.Owner.IsNotNull)
                {
                    var techno = TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
                    if (techno != null)
                    {
                        var salveScript = techno.GameObject.GetComponent<BorisDroppedSHKScript>();
                        if (salveScript != null)
                        {
                            if (!salveScript.Master.IsNullOrExpired())
                            {
                                Owner.OwnerObject.Ref.Owner = salveScript.Master.OwnerObject;
                            }
                        }
                    }
                }
            }
        }
    }




    [Serializable]
    [ScriptAlias(nameof(BorisAirDropBulletScript))]

    public class BorisAirDropBulletScript : BulletScriptable
    {
        public BorisAirDropBulletScript(BulletExt owner) : base(owner)
        {
        }

        public override void OnDetonate(Pointer<CoordStruct> pCoords)
        {
            if (Owner.OwnerObject.Ref.Base.GetHeight() <= Game.CellSize)
            {
                if(Owner.OwnerObject.Ref.Owner.IsNull)
                    return;

                var technoExt = TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);

                if (technoExt.IsNullOrExpired())
                    return;

                var borisScript = technoExt.GameObject.GetComponent<BorisScript>();

                if (borisScript == null)
                    return;

                borisScript.PutSlaveNear(pCoords.Ref);

            }
          
            base.OnDetonate(pCoords);
        }


    }

}