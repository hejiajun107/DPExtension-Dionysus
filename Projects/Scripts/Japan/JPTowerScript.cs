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

            bool giveExp = false;

            if(pTarget.CastToTechno(out var pVimctim))
            {
                if (!pVimctim.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                {
                    var houseName = pVimctim.Ref.Owner.Ref.Type.Ref.Base.ID.ToString();
                    if(houseName != "Special" && houseName != "Neutral")
                    {
                        giveExp = true;
                    }
                }
            }

            List<int> levels = new List<int>();

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

                    if (ptechno.Ref.Veterancy.IsElite())
                    {
                        levels.Add(2);
                    }
                    else
                    {
                        levels.Add(1);
                    }

                    if (giveExp)
                    {
                        ptechno.Ref.Veterancy.Add(0.01);
                    }
                    
                    count++;
                    ptechno.Ref.Fire_NotVirtual(Owner.OwnerObject.Convert<AbstractClass>(), 0);
                }
            }

            if (count > 0)
            {
                var level = levels.OrderByDescending(x => x).Sum();
                var pBullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                var pWh = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ETowrBuffWh");
                if(level == 2)
                {
                    pWh = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ETowrBuffWh2");
                }else if (level == 3)
                {
                    pWh = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ETowrBuffWh3");
                }else if (level >= 4)
                {
                    pWh = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ETowrBuffWh4");
                }

                var bullet = pBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 0, pWh, 100, false);
                bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                delay = 50;
            }
        }
    }
}
