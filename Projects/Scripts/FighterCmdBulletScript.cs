using Extension.Utilities;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;

namespace DpLib.Scripts
{
    public class FighterCmdBulletScript : BulletScriptable
    {
        public FighterCmdBulletScript(BulletExt owner) : base(owner) { }

        private bool isActived = false;

        private List<string> registeredFighters = new List<string>()
        {
            "MQWZDJ","ICWG","J10","MIG","JALC","RCROS","J20","WindRider"
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
                     if (registeredFighters.Contains(id) &&  (x.Ref.Target.IsNull || x.Ref.Base.GetCurrentMission() != Mission.Attack)) //&& x.Ref.Base.GetCurrentMission() == (Mission.Guard | Mission.Sleep) ；was x.Ref.Ammo > 0 &&
                     {
                         return true;
                     }
                     return false;
                 }, FindRange.Owner);

                if(fighters.Count()==0)
                {
                    return;
                }

                var targets = Finder.FindTechno(house, x =>
                {
                    var coords = x.Ref.Base.Base.GetCoords();
                    var height = x.Ref.Base.GetHeight();
                    var type = x.Ref.Base.Base.WhatAmI();

                    if (x.Ref.Base.InLimbo)
                    {
                        return false;
                    }

                    var bounsRange = 0;
                    if(coords.Z > location.Z)
                    {
                        if(coords.Z - location.Z > 300)
                        {
                            //空中目标奖励距离
                            bounsRange = 1024;
                        }
                    }

                    if ((coords - new CoordStruct(0, 0, height)).DistanceFrom(location)  <= (1536 + bounsRange) && type != AbstractType.Building)
                    {
                        return true;
                    }
                    return false;
                }, FindRange.Enermy);

                if (targets.Count() == 0)
                {
                    return;
                }

                var index = 0;

                foreach (var fighter in fighters)
                {
                    //循环分配攻击
                    if (fighter.TryGet(out TechnoExt fighterExt))
                    {
                        if (index >= targets.Count())
                        {
                            index = 0;
                        }

                        if (fighterExt.OwnerObject.Ref.Ammo == 0 && fighterExt.OwnerObject.Ref.Base.GetCurrentMission() != Mission.Sleep)
                        {
                            var mission = fighterExt.OwnerObject.Convert<MissionClass>();
                            mission.Ref.ForceMission(Mission.Enter);
                            continue;
                        }

                        var target = targets[index];
                        if (target.TryGet(out TechnoExt targetExt))
                        {
                            fighterExt.OwnerObject.Ref.SetTarget(targetExt.OwnerObject.Convert<AbstractClass>());
                            var mission = fighterExt.OwnerObject.Convert<MissionClass>();
                            //mission.Ref.QueueMission(Mission.Guard, false);
                            //mission.Ref.NextMission();
                            //mission.Ref.QueueMission(Mission.Attack, false);
                            mission.Ref.ForceMission(Mission.Stop);
                            mission.Ref.ForceMission(Mission.Attack);
                            //mission.Ref.NextMission();
                            //mission.Ref.QueueMission(Mission.Stop, false);

                            index++;
                        }
                    }
                }
            }
        }
    }
}
