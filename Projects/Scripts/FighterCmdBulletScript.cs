using Extension.CW;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(FighterCmdBulletScript))]

    public class FighterCmdBulletScript : BulletScriptable
    {
        public FighterCmdBulletScript(BulletExt owner) : base(owner) { }

        private bool isActived = false;

        private List<string> registeredFighters = new List<string>()
        {
            "MQWZDJ","ICWG","J10","MIG","JALC","RCROS","J20","WindRider","MIG_AI1"
        };

        public override void OnUpdate()
        {
            if (isActived == false)
            {
                isActived = true;

                var house = Owner.OwnerObject.Ref.Owner.Ref.Owner;

                var location = Owner.OwnerObject.Ref.TargetCoords;

                //找到己方所有的战机单位
                var fighters =
                Finder.FindTechno(house, x =>
                 {
                     var id = x.Ref.Type.Ref.Base.Base.ID.ToString();
                     if (registeredFighters.Contains(id) && (x.Ref.Target.IsNull || x.Ref.Base.GetCurrentMission() != Mission.Attack)) //&& x.Ref.Base.GetCurrentMission() == (Mission.Guard | Mission.Sleep) ；was x.Ref.Ammo > 0 &&
                     {
                         return true;
                     }
                     return false;
                 }, FindRange.Owner);

                if (fighters.Count() == 0)
                {
                    return;
                }

                var fighter1 = fighters.First();


                var objects = ObjectFinder.FindTechnosNear(location, (int)(Game.CellSize * 6.5 + Game.CellSize * 4));

                var targets = objects.Where(o =>
                {
                    if (o.CastToTechno(out var x))
                    {
                        var coords = x.Ref.Base.Base.GetCoords();
                        var height = x.Ref.Base.GetHeight();
                        var type = x.Ref.Base.Base.WhatAmI();

                        if (x.Ref.Base.InLimbo)
                        {
                            return false;
                        }

                        if (x.Ref.Owner.Ref.IsAlliedWith(house))
                        {
                            return false;
                        }

                        if(x.Ref.Type.Ref.Cost<100)
                        {
                            return false;
                        }

                        var bounsRange = 0;
                        if (coords.Z > location.Z)
                        {
                            if (coords.Z - location.Z > 300)
                            {
                                //空中目标奖励距离
                                bounsRange = 1024;
                            }
                        }

                        if (!GameUtil.CanAffectTarget(fighter1.OwnerObject, x))
                        {
                            return false;
                        }

                        //if (x.Ref.Base.IsDisguised())
                        //{
                        //    return false;
                        //    //var fakeHouse = x.Ref.Base.GetDisguiseHouse(true);
                        //    //if (!fakeHouse.IsNull)
                        //    //{
                        //    //    if (fakeHouse.Ref.IsAlliedWith(house))
                        //    //    {
                        //    //        return false;
                        //    //    }
                        //    //}
                        //}

                        if ((coords - new CoordStruct(0, 0, height)).DistanceFrom(location) <= (1536 + bounsRange) && type != AbstractType.Building)
                        {
                            return true;
                        }

                        return false;
                    }
                    return false;
                }).Select(x=>TechnoExt.ExtMap.Find(x.Convert<TechnoClass>())).ToList();

                if (targets.Count() == 0)
                {
                    return;
                }

                List<FighetCmdTargetInfo> targetsRecords = new List<FighetCmdTargetInfo>();
               
                foreach(var target in targets)
                {
                    targetsRecords.Add(new FighetCmdTargetInfo()
                    {
                        CurrentDamage = 0,
                        TechnoExt = target,
                        TotalHealth = Owner.OwnerObject.Ref.Base.Health
                    });
                }

                var index = 0;

                foreach (var fighter in fighters)
                {
                    //循环分配攻击
                    if (!fighter.IsNullOrExpired())
                    {
                        if (index >= targets.Count())
                        {
                            index = 0;
                        }

                        if (fighter.OwnerObject.Ref.Ammo == 0 && fighter.OwnerObject.Ref.Base.GetCurrentMission() != Mission.Sleep)
                        {
                            var mission = fighter.OwnerObject.Convert<MissionClass>();
                            mission.Ref.ForceMission(Mission.Enter);
                            continue;
                        }

                        var target = targets[index];

                        var record = targetsRecords[index];
                        if (record.CurrentDamage >= record.TotalHealth)
                        {
                            var nextRecord = targetsRecords.Where(x => x.CurrentDamage < x.TotalHealth).FirstOrDefault();
                            if (nextRecord != null)
                            {
                                target = nextRecord.TechnoExt;
                                record = nextRecord;
                            }
                        }

                        if (!target.IsNullOrExpired())
                        {
                            fighter.OwnerObject.Ref.SetTarget(target.OwnerObject.Convert<AbstractClass>());
                            var mission = fighter.OwnerObject.Convert<MissionClass>();
                            //mission.Ref.QueueMission(Mission.Guard, false);
                            //mission.Ref.NextMission();
                            //mission.Ref.QueueMission(Mission.Attack, false);
                            mission.Ref.ForceMission(Mission.Stop);
                            mission.Ref.ForceMission(Mission.Attack);
                            //mission.Ref.NextMission();
                            //mission.Ref.QueueMission(Mission.Stop, false);
                            
                            var gext = fighter.GameObject.GetTechnoGlobalComponent();
                            if(gext!=null)
                            {
                                gext.HandleAirCommand = true;
                            }

                            //预估伤害
                            var estimateDamage = GameUtil.GetEstimateDamage(fighter.OwnerObject, target.OwnerObject, true);
                            record.CurrentDamage += estimateDamage;

                            index++;
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class FighetCmdTargetInfo
    {
        public int CurrentDamage { get; set; } = 0;

        public int TotalHealth { get; set; } = 0;

        public TechnoExt TechnoExt { get; set; }
    }
}
