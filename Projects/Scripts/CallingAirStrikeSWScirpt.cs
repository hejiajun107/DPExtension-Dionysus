using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    [ScriptAlias(nameof(CallingAirStrikeSWScirpt))]
    public class CallingAirStrikeSWScirpt : SuperWeaponScriptable
    {
        public CallingAirStrikeSWScirpt(SuperWeaponExt owner) : base(owner)
        {
        }

        private Random rd = new Random(114514);

        static Pointer<SuperWeaponTypeClass> airStrike => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("A5AirstrikeSpecial");


        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            var coord = CellClass.Cell2Coord(cell);
            for (var i = 0; i < 10; i++)
            {
                CallAirStrike(new CoordStruct(coord.X + rd.Next(-800, 800), coord.Y + rd.Next(-800, 800), coord.Z));
            }

            for (var i = 0; i < 10; i++)
            {
                CallAirStrike(new CoordStruct(coord.X + rd.Next(-1500, 1500), coord.Y + rd.Next(-1500, 1500), coord.Z));
            }

            for (var i = 0; i < 10; i++)
            {
                CallAirStrike(new CoordStruct(coord.X + rd.Next(-2560, 2560), coord.Y + rd.Next(-2560, 2560), coord.Z));
            }

            for (var i = 0; i < 10; i++)
            {
                CallAirStrike(new CoordStruct(coord.X + rd.Next(-3560, 3560), coord.Y + rd.Next(-3560, 3560), coord.Z));
            }
        }

        private void CallAirStrike(CoordStruct target, int count = 1)
        {
            Pointer<SuperClass> pSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(airStrike);
            CellStruct targetCell = CellClass.Coord2Cell(target);
            pSuper.Ref.IsCharged = true;
            pSuper.Ref.Launch(targetCell, true);
            pSuper.Ref.IsCharged = false;
        }
    }
}
