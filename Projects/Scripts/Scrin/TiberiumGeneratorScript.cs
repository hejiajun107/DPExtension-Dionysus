using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Scrin
{
    [ScriptAlias(nameof(TiberiumGeneratorScript))]
    [Serializable]
    public class TiberiumGeneratorScript : TechnoScriptable
    {
        public TiberiumGeneratorScript(TechnoExt owner) : base(owner)
        {
        }

        public int rof = 1800;

        private uint range = 3;

        private int loop = 1;

        public override void OnUpdate()
        {
            if (!CanWork())
                return;

            if (rof-- > 0)
                return;

            rof = 1800;

            var currentCoord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            var currentCell = CellClass.Coord2Cell(currentCoord);

            var enumerator = new CellSpreadEnumerator(3);

            foreach (CellStruct offset in enumerator)
            {
                CoordStruct where = CellClass.Cell2Coord(currentCell + offset, currentCoord.Z);

                if (MapClass.Instance.TryGetCellAt(where, out var pCell))
                {
                    var value = pCell.Ref.GetContainedTiberiumValue();

                    if(value > 0)
                    {
                        if(pCell.Ref.GetContainedTiberiumIndex() == 1)
                        {
                            var currentAmount = value / 50;
                            if (currentAmount < 8)
                            {
                                pCell.Ref.ReduceTiberium(currentAmount);
                                pCell.Ref.IncreaseTiberium(1, ++currentAmount);
                            }
                        }
                    }
                    else
                    {
                        pCell.Ref.IncreaseTiberium(1, 1);
                    }

                }
            }
        }

        public bool CanWork()
        {
            double powerP = Owner.OwnerObject.Ref.Owner.Ref.GetPowerPercentage();
            return Owner.OwnerObject.Ref.IsPowerOnline() & (powerP >= 1);
        }
    }
}
