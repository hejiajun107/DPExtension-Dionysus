using Extension.CW;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DpLib.Scripts.Heros
{
    [Serializable]
    [ScriptAlias(nameof(ScMandoScript))]

    public class ScMandoScript : TechnoScriptable
    {
        public ScMandoScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner,10);
        }

        private ManaCounter _manaCounter;

        private List<MandoUnitRecord> unitRecords = new List<MandoUnitRecord>();

        private int checkDelay = 20;

        private int listCheckDelay = 5;

        private int lifeTime = 60;

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                Respawn();
            }

            //检索附近单位，记录进数据
            if (checkDelay-- <= 0)
            {
                checkDelay = 20;
                if (_manaCounter.Current > 50)
                {
                    //todo检索单位
                    var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                    //var currentCell = CellClass.Coord2Cell(location);

                    var technos = ObjectFinder.FindTechnosNear(location, 6 * Game.CellSize);

                    foreach (var tehcno in technos)
                    {
                        if(tehcno.CastToTechno(out var ptechno))
                        {
                            var tref = TechnoExt.ExtMap.Find(ptechno);
                            if (!tref.IsNullOrExpired())
                            {
                                if (tref.OwnerObject.Ref.Owner.IsNull)
                                    continue;
                                //if (!Owner.OwnerObject.Ref.Owner.Ref.IsAlliedWith(ptechno.OwnerObject.Ref.Owner))
                                //    continue;
                                if (Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex != tref.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                                    continue;
                                if (Owner.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Unit && Owner.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Infantry)
                                    continue;

                                var gext = tref.GameObject.GetTechnoGlobalComponent();
                                if (gext == null)
                                    continue;

                                if (gext.Data.IsHero || gext.Data.IsEpicUnit)
                                    continue;

                                var respawnTagScript = tref.GameObject.GetComponent<RespawnRecorderScript>();
                                if (respawnTagScript == null)
                                {
                                    tref.GameObject.CreateScriptComponent(nameof(RespawnRecorderScript), RespawnRecorderScript.UniqueId, "RespawnRecorderScript", tref, Owner);
                                }
                                else
                                {
                                    respawnTagScript.Duration = 200;
                                }
                            }
                        }
                    }
                }
            }

            if (listCheckDelay-- <= 0)
            {
                listCheckDelay = 20;
                //todo检索单位，挑选活着的单位，并清理死亡的单位
                foreach (var unit in unitRecords)
                {
                    unit.LifeTime = unit.LifeTime - 1;
                    if (unit.LifeTime < 0)
                    {
                        //清除
                        continue;
                    }
                }
                unitRecords.RemoveAll(x => x.LifeTime <= 0);

            }

            base.OnUpdate();
        }
        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            //if (weaponIndex == 1)
            //{
            //    Respawn();
            //}
            //base.OnFire(pTarget, weaponIndex);
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            base.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
            if (pAttacker.IsNull)
                return;
            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;
            if (Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                return;

            if (unitRecords.Count() >= 2)
            {
                Respawn();
            }

        }


        private void Respawn()
        {
            if (_manaCounter.Cost(100))
            {
                var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var animType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("ScRespawn");
                YRMemory.Create<AnimClass>(animType, currentLocation);


                foreach (var unit in unitRecords)
                {
                  
                    if (currentLocation.DistanceFrom(unit.Location) <= 2560)
                    {
                        var pType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(unit.Type);
                        var techno = pType.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();

                        if (techno.Ref.Base.Put(unit.Location, Direction.N))
                        {
                            YRMemory.Create<AnimClass>(animType, unit.Location);
                        }
                        else
                        {
                            techno.Ref.Base.UnInit();
                        }
                    }
                }

                //删除所有记录
                unitRecords.Clear();
            }
        }

        public void RegisterDeath(string type,CoordStruct location)
        {
            var record = new MandoUnitRecord()
            {
                Type = type,
                Location = location,
                LifeTime = lifeTime
            };

            unitRecords.Add(record);
        }
    }

    [Serializable]
    public class MandoUnitRecord
    {

        public string Type { get; set; }

        public CoordStruct Location { get; set; }

        public int LifeTime { get; set; }
    }

    [Serializable]
    [ScriptAlias(nameof(RespawnRecorderScript))]
    public class RespawnRecorderScript : TechnoScriptable
    {
        public const int UniqueId = 202302281;

        public int Duration = 200;

        TechnoExt respawner;

        public RespawnRecorderScript(TechnoExt owner,TechnoExt Respawner) : base(owner)
        {
            respawner = Respawner;
        }



        public override void OnUpdate()
        {
            if (Duration-- <= 0)
            {
                DetachFromParent();
            }

            if(respawner.IsNullOrExpired())
            {
                DetachFromParent();
            }
        }

        public override void OnRemove()
        {
            DetachFromParent();
            if (Owner.OwnerObject.Ref.Base.Health <= 0)
            {
                if (!respawner.IsNullOrExpired())
                {
                    var script = respawner.GameObject.GetComponent<ScMandoScript>();
                    if (script != null)
                    {
                        script.RegisterDeath(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID,Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    }
                }
            }
           
        }

    }
}
