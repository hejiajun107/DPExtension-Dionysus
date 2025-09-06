using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scripts.Tavern;

namespace Scripts.Tavern
{
    /// <summary>
    /// 用于自动战斗的逻辑
    /// </summary>
    [ScriptAlias(nameof(TavernAutoBattleScript))]
    [Serializable]
    public class TavernAutoBattleScript : TechnoScriptable
    {
        public TavernAutoBattleScript(TechnoExt owner) : base(owner)
        {
        }

        int delay = 10;

        //寻敌间隔
        private int rof = 1;

        private bool inited = false;

        public override void Awake()
        {
            //随机延迟避免在同一帧寻敌;
            delay = MathEx.Random.Next(10, 20);
            base.Awake();
        }

        public override void OnUpdate()
        {
            if (delay > 0)
            {
                delay--;
                return;
            }

            if (rof-- > 0)
            {
                return;
            }

            var mission = Owner.OwnerObject.Convert<MissionClass>();

            if (!inited) 
            {
                inited = true;
                mission.Ref.ForceMission(Mission.Hunt);
            }

            rof = 10;


            if (mission.Ref.CurrentMission == Mission.Hunt || mission.Ref.CurrentMission == Mission.Area_Guard)
            {
                if (Owner.OwnerObject.Ref.Target.IsNull)
                {
                    Owner.OwnerObject.CastToFoot(out Pointer<FootClass> pFoot);
                    if (!pFoot.Ref.Base.CanAttackMove())
                        return;

                    if (mission.Ref.CurrentMission != Mission.Area_Guard)
                        mission.Ref.ForceMission(Mission.Area_Guard);

                    var targets = Owner.OwnerObject.Ref.FindAttackTechnos(100 * Game.CellSize).Where(x => x.Ref.Owner.Ref.Type.Ref.Base.ID != "Special" && x.Ref.Owner.Ref.Type.Ref.Base.ID != "Neutral").OrderBy(x => Owner.OwnerObject.Ref.GetDistanceToTarget(x.Convert<AbstractClass>())).ToList();
                    if (targets.Count() > 0)
                    {
                        var target = targets.First();
                        if (MapClass.Instance.TryGetCellAt(CellClass.Coord2Cell(target.Ref.Base.Base.GetCoords()), out var pcell))
                        {
                            pFoot.Ref.AttackMove(target.Convert<AbstractClass>(), pcell);
                        }
                    }
                    else
                    {
                        rof = 100;
                    }
                }
            }
        }
    }
}
