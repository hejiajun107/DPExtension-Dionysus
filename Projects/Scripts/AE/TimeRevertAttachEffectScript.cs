using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(TimeRevertAttachEffectScript))]
    public class TimeRevertAttachEffectScript : AttachEffectScriptable
    {
        public TimeRevertAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private int heath = 0;
        private CoordStruct coord;



        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            heath = Owner.OwnerObject.Ref.Base.Health;
            coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
        }


        public override void OnAttachEffectRemove()
        {
            if (Owner.OwnerObject.Ref.Base.Health <= 0)
            {
                base.OnAttachEffectRemove();
                return;
            }
            //移除时触发时间倒流
            var pTechno = Owner.OwnerObject;

            var animType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("CHRONOEXPMINI");

            var pAnim1 = YRMemory.Create<AnimClass>(animType, pTechno.Ref.Base.Base.GetCoords());

            if (pTechno.CastToFoot(out var pfoot))
            {
                if (MapClass.Instance.TryGetCellAt(coord, out var pCell))
                {
                    var source = pTechno.Ref.Base.Base.GetCoords();
                    pfoot.Ref.Locomotor.Mark_All_Occupation_Bits(0);
                    pfoot.Ref.Locomotor.Force_Track(-1, source);
                    pTechno.Ref.Base.UnmarkAllOccupationBits(source);
                    var pLocal = pCell.Ref.Base.GetCoords();
                    pTechno.Ref.Base.SetLocation(pLocal);
                    pTechno.Ref.Base.UnmarkAllOccupationBits(pLocal);
                }
            }

            var pAnim2 = YRMemory.Create<AnimClass>(animType, coord);

            pTechno.Ref.Base.Health = heath;

        }

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            //持续状态中阻止接收新的AE效果
            return;
        }
    }
}
