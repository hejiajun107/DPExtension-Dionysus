using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Scrin
{
    [ScriptAlias(nameof(TriMechScript))]
    [Serializable]
    public class TriMechScript : TechnoScriptable
    {
        public TriMechScript(TechnoExt owner) : base(owner)
        {
        }

        public int warpDelay = 500;

        public int inBattle = 1000;

        public override void OnUpdate()
        {
            if(warpDelay >= 0)
            {
                warpDelay--;
            }

            if (inBattle >= 0)
            {
                inBattle--;
            }

            if(inBattle > 0 || warpDelay > 0)
            {
                Owner.OwnerObject.Ref.Ammo = 0;
                return;
            }
            else
            {
                Owner.OwnerObject.Ref.Ammo = 1;
            }

            var mission = Owner.OwnerObject.Convert<MissionClass>();

            if(mission.Ref.CurrentMission == Mission.Move)
            {
                mission.Ref.ForceMission(Mission.Stop);

                var pfoot = Owner.OwnerObject.Convert<FootClass>();

                if (pfoot.Ref.Destination.IsNull)
                    return;

                if (Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(pfoot.Ref.Destination.Ref.GetCoords()) < 10 * Game.CellSize)
                    return;

                warpDelay = 500;
                TrySetLocation(Owner.OwnerObject, pfoot.Ref.Destination.Ref.GetCoords());
            }
        }

        private bool TrySetLocation(Pointer<TechnoClass> techno, CoordStruct location)
        {
            if (!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                var pTechno = Owner.OwnerObject;
                var mission = pTechno.Convert<MissionClass>();

                var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                //= O

                //位置
                if (pTechno.CastToFoot(out Pointer<FootClass> pfoot))
                {
                    if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
                    {
                        var transAnim = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("TRIWARP");
                        var source = pTechno.Ref.Base.Base.GetCoords();
                        YRMemory.Create<AnimClass>(transAnim, source);
                        pfoot.Ref.Locomotor.Mark_All_Occupation_Bits(0);
                        pfoot.Ref.Locomotor.Force_Track(-1, source);
                        pTechno.Ref.Base.UnmarkAllOccupationBits(source);
                        var cLocal = pCell.Ref.Base.GetCoords();
                        var pLocal = new CoordStruct(cLocal.X, cLocal.Y, location.Z);
                        pTechno.Ref.Base.SetLocation(pLocal);
                        pTechno.Ref.Base.UnmarkAllOccupationBits(pLocal);
                        YRMemory.Create<AnimClass>(transAnim, pLocal);
                    }
                }

                return true;
            }
            return false;
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            inBattle = 1000;
            base.OnFire(pTarget, weaponIndex);
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (pAttackingHouse.IsNull)
                return;

            if (pAttackingHouse.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                return;

            inBattle = 1000;

        }
    }
}
