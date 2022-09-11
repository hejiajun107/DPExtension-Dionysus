using Extension.CW;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{
    [Serializable]
    public class ConvertWhenBornScript : TechnoScriptable
    {
        public ConvertWhenBornScript(TechnoExt owner) : base(owner)
        {
            var gext = owner.GameObject.GetComponent<TechnoGlobalExtension>();
            if (gext != null)
            {
                targetType = gext.Data.ScriptArgs;
            }
        }

        private string targetType;

        bool placed = false;

        private int delay = 30;

        public override void OnUpdate()
        {
            //delay防止还没出来就put,put失败这个写法不建议参考，这是种摆烂的解决方式
            if (delay-- > 0)
                return;

            if (placed)
                return;

            if (!Owner.OwnerObject.Ref.Base.IsOnMap)
                return;

            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            var destnation = Owner.OwnerObject.Ref.Base.Base.GetDestination();

            if(Owner.OwnerObject.CastToFoot(out var pofoot))
            {
                var pdest = pofoot.Ref.Destination;


                var mission = Owner.OwnerObject.Convert<MissionClass>();

                var currentMission = mission.Ref.CurrentMission;

                var type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(targetType);

                var techno = type.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();
                if (techno != null)
                {
                    Owner.OwnerObject.Ref.Base.Remove();

                    if (techno.Ref.Base.Put(location, Direction.S))
                    {
                       
                            if (techno.CastToFoot(out var pFoot))
                            {
                                var tmission = techno.Convert<MissionClass>();
                                if(!pdest.IsNull)
                                {
                                    pFoot.Ref.MoveTo(pdest.Ref.GetCoords());
                                }
                                else
                                {
                                    pFoot.Ref.MoveTo(location);
                                }
                                tmission.Ref.QueueMission(Mission.Move,false);
                            }

                        //var tmission = techno.Convert<MissionClass>();
                        //if(!tmission.IsNull)
                        //{
                        //    tmission.Ref.QueueMission(currentMission, false);
                        //    tmission.Ref.NextMission();
                        //}
                    }
                    else
                    {
                        techno.Ref.Base.UnInit();
                    }

                }

                Owner.OwnerObject.Ref.Base.UnInit();
            }

          

            base.OnUpdate();
        }

    }
}
