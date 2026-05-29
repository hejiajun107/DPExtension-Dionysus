using Extension.EventSystems;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Yuri
{
    [ScriptAlias(nameof(ReaperScript))]
    [Serializable]
    public class ReaperScript : TechnoScriptable
    {
        public ReaperScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);
            }
            base.OnUpdate();
        }

        public void TrySpa()
        {
            if(Owner.OwnerObject.Ref.Ammo >= 1)
            {
                Owner.OwnerObject.Ref.Ammo = 0;
                //
                EventSystem.General.AddTemporaryHandler(EventSystem.General.LogicClassUpdateEvent, Spa);
            }
        }

        public void Spa(object sender, EventArgs args)
        {
            if (args is LogicClassUpdateEventArgs largs)
            {
                if (!largs.IsLateUpdate)
                    return;
            }
            else
            {
                return;
            }
            if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
            {
                return;
            }

            //if (Owner.OwnerObject.CastToFoot(out Pointer<FootClass> pfoot))
            //{
            //    var dir = GameUtil.Point2Dir(Owner.OwnerObject.Ref.Base.Base.GetCoords(), currentTarget.OwnerObject.Ref.Base.Base.GetCoords());
            //    var tdir = new DirStruct(16, (short)DirStruct.TranslateFixedPoint(3, 16, (uint)dir));
            //    Owner.OwnerObject.Ref.Facing.set(tdir);

            //    if (autoFireRof > 0)
            //    {
            //        autoFireRof--;
            //        return;
            //    }
            //    Owner.OwnerObject.Ref.Fire_NotVirtual(currentTarget.OwnerObject.Convert<AbstractClass>(), 1);
            //    autoFireRof = Owner.OwnerObject.Ref.Veterancy.IsElite() ? 2 : 4;
            //}


        }

        public override void OnDestroy()
        {
            EventSystem.General.RemoveTemporaryHandler(EventSystem.General.LogicClassUpdateEvent, Spa);
        }


    }
}
