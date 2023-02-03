using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.China
{
    [ScriptAlias(nameof(DonfFengHaoDangScirpt))]
    [Serializable]
    public class DonfFengHaoDangScirpt : TechnoScriptable
    {
        public DonfFengHaoDangScirpt(TechnoExt owner) : base(owner)
        {
        }

        private Random rd = new Random(41);
        private List<CoordStruct> coords = new List<CoordStruct>();

        private static Pointer<AnimTypeClass> plock = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("DFLOCK");

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            var technos = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, t => ((t.Ref.Type.Ref.Base.Base.ID == "DF41BS") && t.Ref.Base.InLimbo == false), FindRange.Owner);

            foreach(var techno in technos)
            {
                if (!techno.IsNullOrExpired())
                {
                    techno.OwnerObject.Ref.Ammo = 1;
                    var mission = techno.OwnerObject.Convert<MissionClass>();
                    if(mission.Ref.CurrentMission!=Mission.Unload)
                    {
                        mission.Ref.ForceMission(Mission.Stop);

                        var tcoord = coord + new CoordStruct(rd.Next(-3000, 3000), rd.Next(-3000, 3000), 0);

                        if (MapClass.Instance.TryGetCellAt(tcoord, out var pcell))
                        {
                            techno.OwnerObject.Ref.SetTarget(pcell.Convert<AbstractClass>());
                            mission.Ref.ForceMission(Mission.Attack);
                            coords.Add(tcoord);
                        }
                    }
                }
            }
        }

        private int delay = 2;

        public override void OnUpdate()
        {
            if(delay--<=0)
            {
                delay = 2;

                if (!Owner.OwnerObject.Ref.Base.InLimbo)
                {
                    if (coords.Count > 0)
                    {
                        var target = coords[0];
                        coords.Remove(target);
                        YRMemory.Create<AnimClass>(plock, target);
                    }
                    else
                    {
                        Owner.OwnerObject.Ref.Base.UnInit();
                    }
                }
            }
        }
    }
}
