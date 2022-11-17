using Extension.CW;
using Extension.Ext;
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
            _manaCounter = new ManaCounter(10);
        }

        private ManaCounter _manaCounter;

        private Dictionary<string, MandoUnitRecord> unitRecords = new Dictionary<string, MandoUnitRecord>();

        private int checkDelay = 20;

        private int listCheckDelay = 5;

        private int lifeTime = 60;

        public override void OnUpdate()
        {
            _manaCounter.OnUpdate(Owner);

            //检索附近单位，记录进数据
            if (checkDelay-- <= 0)
            {
                checkDelay = 20;
                if (_manaCounter.Current > 50)
                {
                    //todo检索单位
                    var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                    var currentCell = CellClass.Coord2Cell(location);
                    CellSpreadEnumerator enumerator = new CellSpreadEnumerator(6);

                    List<int> codes = new List<int>();

                    foreach (CellStruct offset in enumerator)
                    {

                        CoordStruct where = CellClass.Cell2Coord(currentCell + offset, location.Z);

                        if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                        {
                            if (pCell.IsNull)
                            {
                                continue;
                            }

                            Point2D p2d = new Point2D(60, 60);
                            Pointer<TechnoClass> target = pCell.Ref.FindTechnoNearestTo(p2d, false, Owner.OwnerObject);


                            if (TechnoExt.ExtMap.Find(target) == null)
                            {
                                continue;
                            }

                            TechnoExt tref = default;

                            tref = (TechnoExt.ExtMap.Find(target));

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
                                var hashCode = tref.OwnerObject.GetHashCode().ToString();


                                var gext = tref.GameObject.GetComponent<TechnoGlobalExtension>();
                                if (gext == null)
                                    continue;

                                if (gext.Data.IsHero || gext.Data.IsEpicUnit)
                                    continue;

                                var id = tref.Type.OwnerObject.Ref.Base.Base.ID.ToString();

                                if (unitRecords.ContainsKey(hashCode))
                                {
                                    unitRecords[hashCode].LifeTime = lifeTime;
                                    unitRecords[hashCode].Location = tref.OwnerObject.Ref.Base.Base.GetCoords();
                                    continue;
                                }
                                else
                                {
                                    var record = new MandoUnitRecord()
                                    {
                                        Techno = tref,
                                        Key = hashCode,
                                        LifeTime = lifeTime,
                                        Location = tref.OwnerObject.Ref.Base.Base.GetCoords(),
                                        Type = tref.OwnerObject.Ref.Type.Ref.Base.Base.ID,
                                        IsDead = false
                                    };

                                    unitRecords.Add(record.Key, record);
                                }
                            }
                        }

                    }

                }
            }

            if (listCheckDelay-- <= 0)
            {
                List<string> toRemove = new List<string>();
                listCheckDelay = 20;
                //todo检索单位，挑选活着的单位，并清理死亡的单位
                foreach (var kvp in unitRecords)
                {
                    var unit = kvp.Value;
                    unit.LifeTime = unit.LifeTime - 1;
                    if (unit.LifeTime < 0)
                    {
                        //清除
                        unit.Techno = null;
                        toRemove.Add(kvp.Key);
                        unit = null;
                        continue;
                    }

                    if (unit.IsDead == false)
                    {
                        if (!unit.Techno.IsNullOrExpired())
                        {
                            unit.Location = unit.Techno.OwnerObject.Ref.Base.Base.GetCoords();
                        }
                        else
                        {
                            //死亡状态重新开始计时
                            unit.IsDead = true;
                            unit.LifeTime = lifeTime;
                        }
                    }

                }

                foreach (var removeKey in toRemove)
                {
                    if (unitRecords.ContainsKey(removeKey))
                    {
                        unitRecords.Remove(removeKey);
                    }
                }
            }

            base.OnUpdate();
        }
        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 1)
            {
                Respawn();
            }
            base.OnFire(pTarget, weaponIndex);
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

            if (unitRecords.Where(t => t.Value.IsDead == true).Count() >= 2)
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


                foreach (var kvp in unitRecords)
                {
                    var unit = kvp.Value;
                    if (unit.IsDead == false)
                    {
                        if (unit.Techno.IsNullOrExpired())
                        {
                            unit.IsDead = true;
                        }
                    }


                    if (unit.IsDead == true && currentLocation.DistanceFrom(unit.Location) <= 2560)
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


    }

    [Serializable]
    public class MandoUnitRecord
    {
        public string Key { get; set; }

        public TechnoExt Techno;

        public string Type { get; set; }

        public CoordStruct Location { get; set; }

        public int LifeTime { get; set; }

        public bool IsDead { get; set; }
    }
}
