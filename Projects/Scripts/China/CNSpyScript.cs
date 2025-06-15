using Extension.EventSystems;
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.China
{
    [ScriptAlias(nameof(CNSpyScript))]
    [Serializable]
    public class CNSpyScript : TechnoScriptable
    {
        public CNSpyScript(TechnoExt owner) : base(owner)
        {
            _voc = new VocExtensionComponent(owner);
        }

        private VocExtensionComponent _voc;

        public override void Awake()
        {
            _voc.Awake();
        }

        private int delay = 800;

        public override void OnUpdate()
        {
            if (Owner.OwnerObject.Ref.Ammo <= 0)
            {
                delay--;
                if (delay == 0)
                {
                    delay = 800;
                    Owner.OwnerObject.Ref.Ammo = 1;
                }
            }
            
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                if(Owner.OwnerObject.Ref.Ammo > 0)
                {
                    Owner.OwnerObject.Ref.Ammo = 0;
                    _voc.PlaySpecialVoice(1, true);

                    var warhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("CNSpyCloackWh");
                    var inviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                    var pBullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, warhead, 100, false);
                    pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

                    var faketype = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("CNSPYM");
                    var techno = faketype.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();

                    if(TechnoPlacer.PlaceTechnoNear(techno, CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords())))
                    {
                        techno.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                        var tmission = techno.Convert<MissionClass>();
                        techno.Ref.Base.Scatter(Owner.OwnerObject.Ref.Base.Base.GetCoords(), true, true);
                    }
                }
            }
        }


    }
}
