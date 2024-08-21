using DynamicPatcher;
using Extension.Coroutines;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(TiberiumGrowthTargetScript))]
    public class TiberiumGrowthTargetScript : TechnoScriptable
    {
        public TiberiumGrowthTargetScript(TechnoExt owner) : base(owner)
        {
        }

        private bool inited = false;
        public override void OnUpdate()
        {
            if(inited) return;

            inited = true;

            var center = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            Owner.GameObject.StartCoroutine(Growth(center));        
        }

        
        IEnumerator Growth(CoordStruct center)
        {
            YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SCTiberiumGrow"), center);
            yield return new WaitForFrames(1);
            IncreseTiberium(2, center, false);
            yield return new WaitForFrames(3);
            IncreseTiberium(3, center, false);
            yield return new WaitForFrames(3);
            IncreseTiberium(3, center, true);
            yield return new WaitForFrames(3);
            IncreseTiberium(4, center, true);
            yield return new WaitForFrames(3);
            IncreseTiberium(5, center, true);
            Owner.OwnerObject.Ref.Base.Remove();
            Owner.OwnerObject.Ref.Base.UnInit();
        }

        private void IncreseTiberium(uint spread,CoordStruct center,bool growUp)
        {

            var enumerator = new CellSpreadEnumerator(spread);
            var currentCell = CellClass.Coord2Cell(center);

            GenTiberiumAt(center, growUp);

            foreach (CellStruct offset in enumerator)
            {
                CoordStruct where = CellClass.Cell2Coord(currentCell + offset, center.Z);

                GenTiberiumAt(where, growUp);
            }
        }

        private void GenTiberiumAt(CoordStruct where,bool growUp)
        {
            if (MapClass.Instance.TryGetCellAt(where, out var pCell))
            {
                var value = pCell.Ref.GetContainedTiberiumValue();

                if (value > 0)
                {
                    if (pCell.Ref.GetContainedTiberiumIndex() == 1)
                    {
                        if (growUp)
                        {
                            var currentAmount = value / 50;
                            if (currentAmount < 12)
                            {
                                pCell.Ref.ReduceTiberium(currentAmount);
                                pCell.Ref.IncreaseTiberium(1, ++currentAmount);
                            }
                        }
                    }
                    else
                    {
                        if (pCell.Ref.GetContainedTiberiumIndex() == 0)
                        {
                            var currentAmount = value / 25;
                            pCell.Ref.ReduceTiberium(currentAmount);
                            pCell.Ref.IncreaseTiberium(1, currentAmount);
                        }
                        else if(pCell.Ref.GetContainedTiberiumIndex() == 2)
                        {
                            var currentAmount = value / 25;
                            pCell.Ref.ReduceTiberium(currentAmount);
                            pCell.Ref.IncreaseTiberium(1, currentAmount);
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
}
