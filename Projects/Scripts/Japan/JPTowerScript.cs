using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(JPTowerScript))]
    [Serializable]
    public class JPTowerScript : TechnoScriptable
    {
        public JPTowerScript(TechnoExt owner) : base(owner)
        {
        }

        private int delay = 0;

        public override void OnUpdate()
        {
            if (delay > 0)
            {
                delay--;
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (delay > 0)
                return;
            var objs = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), 5 * Game.CellSize);
            int count = 0;
            foreach(var obj in objs)
            {
                if (obj.Ref.GetTechnoType().Ref.Base.Base.ID != "JPETOWR")
                    continue;

                if(obj.CastToTechno(out var ptechno))
                {
                    if (!ptechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                        continue;

                    var mission = ptechno.Convert<MissionClass>();
                    if (mission.Ref.CurrentMission == Mission.Selling || mission.Ref.CurrentMission == Mission.Construction)
                        continue;

                    count++;
                    ptechno.Ref.Fire_NotVirtual(Owner.OwnerObject.Convert<AbstractClass>(), 0);
                }
            }

            if (count > 0)
            {
                var pBullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                var pWh = count > 1 ? WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ETowrBuffWh2") : WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ETowrBuffWh");
                var bullet = pBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 0, pWh, 100, false);
                bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                delay = 50;
            }
        }
    }
}
