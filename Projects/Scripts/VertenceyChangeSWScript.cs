using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
	[ScriptAlias(nameof(VertenceyChangeSWScript))]
	[Serializable]
	public class VertenceyChangeSWScript : SuperWeaponScriptable
	{
		public VertenceyChangeSWScript(SuperWeaponExt owner) : base(owner)
		{
		}

		public override void OnLaunch(CellStruct cell, bool isPlayer)
		{
			var technos =ObjectFinder.FindTechnosNear(CellClass.Cell2Coord(cell), 5 * Game.CellSize);
			foreach (var techno in technos)
			{
				if(techno.CastToTechno(out var ptechno))
				{
					if (ptechno.Ref.Veterancy.IsRookie())
					{
						ptechno.Ref.Veterancy.SetVeteran(true);
					}else if(ptechno.Ref.Veterancy.IsVeteran())
					{
						ptechno.Ref.Veterancy.SetElite(true);
					}
				}
			}
			base.OnLaunch(cell, isPlayer);
		}
	}
}
