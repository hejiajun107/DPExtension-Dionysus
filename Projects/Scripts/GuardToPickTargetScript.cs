using DynamicPatcher;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    [ScriptAlias(nameof(GuardToPickTargetScript))]
    public class GuardToPickTargetScript : TechnoScriptable
    {
        public GuardToPickTargetScript(TechnoExt owner) : base(owner)
        {

        }

        INIComponentWith<GuardToPickTargetData> settingINI;

        const string Special = "Special";
        const string Neutral = "Neutral";

        public override void Awake()
        {
            settingINI = this.CreateRulesIniComponentWith<GuardToPickTargetData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
        }


        public override void OnUpdate()
        {
            if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                return;
           
            var mission = Owner.OwnerObject.Cast<MissionClass>();
            if(mission.Ref.CurrentMission == Mission.Area_Guard)
            {
                var technos = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), settingINI.Data.PickRange * Game.CellSize).Where(x=>
                {
                    if(x.CastToTechno(out var techno))
                    {
                        if (techno.Ref.Base.InLimbo)
                            return false;
                        if (!techno.Ref.Base.IsOnMap)
                            return false;
                        var houseName = Owner.OwnerObject.Ref.Owner.Ref.Type.Ref.Base.ID;
                        if (houseName == Special || houseName == Neutral)
                            return false;
                        if (Owner.OwnerObject.Ref.Owner.Ref.IsAlliedWith(techno.Ref.Owner))
                            return false;
                        if (!Owner.OwnerObject.Ref.IsCloseEnoughToAttack(techno.Convert<AbstractClass>()))
                            return false;
                        if (!Owner.OwnerObject.Ref.CanAttack(techno.Convert<AbstractClass>()))
                            return false;
                        return true;
                    }
                    return false;
                }).ToList();

                Pointer<AbstractClass> pTarget = Pointer<AbstractClass>.Zero;

                if(technos.Count()==1)
                {
                    pTarget = technos[0].Convert<AbstractClass>();
                }else if(technos.Count>1)
                {
                    pTarget = technos[MathEx.Random.Next(0, technos.Count())].Convert<AbstractClass>();
                }

                if (pTarget.IsNotNull)
                {
                    Owner.OwnerObject.Ref.Attack(pTarget);
                }
            }


        }
    }


    public class GuardToPickTargetData : INIAutoConfig
    {
        [INIField(Key = "GuardToPickTarget.Range")]
        public int PickRange = 5;
    }


}
