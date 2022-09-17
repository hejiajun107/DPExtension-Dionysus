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
    [ScriptAlias(nameof(JumpWithBulletScript))]
    public class JumpWithBulletScript:BulletScriptable
    {
        public JumpWithBulletScript(BulletExt owner) : base(owner)
        {
            
        }

        public override void OnUpdate()
        {
            if(!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                var pTechno = Owner.OwnerObject.Ref.Owner;

                var mission = pTechno.Convert<MissionClass>();
                mission.Ref.ForceMission(Mission.Stop);

                pTechno.Ref.SetTarget(default);
                pTechno.Ref.SetDestination(default, false);
                //位置
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                pTechno.Ref.Base.SetLocation(location);
            }
          
        }

    }
}
