using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Scrin
{
	[ScriptAlias(nameof(SCVanishScript))]
	[Serializable]
	public class SCVanishScript : TechnoScriptable
	{
		public SCVanishScript(TechnoExt owner) : base(owner)
		{
		}

		public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
		{
			if (weaponIndex != 1)
			{
				return;
			}

			if (pTarget.CastToTechno(out var ptechno))
			{
                if (!ptechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                {
					return;
                }
                TrySetLocation(ptechno, pTarget.Ref.GetCoords());
			}
		}


		private bool TrySetLocation(Pointer<TechnoClass> techno, CoordStruct location)
		{
			if (!Owner.OwnerObject.Ref.Owner.IsNull)
			{
				var pTechno = Owner.OwnerObject;
				var mission = pTechno.Convert<MissionClass>();
				mission.Ref.ForceMission(Mission.Stop);
				var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();

				//= O

				//位置
				if (pTechno.CastToFoot(out Pointer<FootClass> pfoot))
				{
					if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
					{
						//源动画
						var source = pTechno.Ref.Base.Base.GetCoords();

						YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("QGROCX03"), source);

						pfoot.Ref.Locomotor.Mark_All_Occupation_Bits(0);
						pfoot.Ref.Locomotor.Force_Track(-1, source);
						pTechno.Ref.Base.UnmarkAllOccupationBits(source);
						//var cLocal = pCell.Ref.Base.GetCoords();
						//var pLocal = new CoordStruct(cLocal.X, cLocal.Y, location.Z);
						pTechno.Ref.Base.SetLocation(location);
						pTechno.Ref.Base.UnmarkAllOccupationBits(location);

						YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("QGROCX03"), location);

						pTechno.Ref.Base.Scatter(location, true, true);

						//目标动画
					}
				}

				return true;
			}


			return false;
		}
	}
}
