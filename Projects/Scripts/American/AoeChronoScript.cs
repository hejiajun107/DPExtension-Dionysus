using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.American
{
    [Serializable]
    [ScriptAlias(nameof(AoeChronoScript))]
    public class AoeChronoScript : TechnoScriptable
    {
        public AoeChronoScript(TechnoExt owner) : base(owner)
        {
        }

        private int delay = 500;

        private bool inited = false;

        private static Pointer<TechnoTypeClass> AttackerType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("AoeChronoAttacker");

        public override void OnUpdate()
        {
            if (delay >= 0)
            {
                delay--;
            }
            else
            {
                Owner.OwnerObject.Ref.Base.UnInit();
            }

            if (inited == false)
            {
                inited = true;

                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                var targets =
                  Finder.FindTechno(Owner.OwnerObject.Ref.Owner, x =>
                  {
                      var id = x.Ref.Type.Ref.Base.Base.ID.ToString();
                      if (x.Ref.Base.Base.GetCoords().DistanceFrom(location) < 2048 && x.Ref.Base.IsOnMap && !x.Ref.Base.InLimbo) //&& x.Ref.Base.GetCurrentMission() == (Mission.Guard | Mission.Sleep) ；was x.Ref.Ammo > 0 &&
                      {
                          return true;
                      }
                      return false;
                  }, FindRange.All);

                base.OnUpdate();


                foreach (var target in targets)
                {
                    if (!target.IsNullOrExpired())
                    {
                        var ptechno = target.OwnerObject;

                        var attacker = AttackerType.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();
                        if (attacker == null)
                            continue;

                        attacker.Ref.Base.SetLocation(location);

                        if (attacker.Ref.Base.Put(ptechno.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 200), Direction.N))
                        {
                            attacker.Ref.Fire(ptechno.Convert<AbstractClass>(), 0);
                            //attacker.Ref.SetTarget(ptechno.Convert<AbstractClass>());
                            //var mission = attacker.Convert<MissionClass>();
                            //mission.Ref.ForceMission(Mission.Attack);
                        }
                        else
                        {
                            attacker.Ref.Base.UnInit();
                        }
                    }
                }


            }


        }
    }
}
