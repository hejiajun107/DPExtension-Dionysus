using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(ScrabScript))]
    public class ScrabScript : TechnoScriptable
    {
        public ScrabScript(TechnoExt owner) : base(owner)
        {
        }

        private int checkRof;

        static Pointer<SuperWeaponTypeClass> swJump => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ScarabJumpSpecial");


        public override void OnUpdate()
        {
            base.OnUpdate();

            if (checkRof-- >= 0)
                return;
            checkRof = 20;
            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;
            if (Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                return;
            if (Owner.OwnerObject.Ref.Target.IsNull)
                return;
            if (Owner.OwnerObject.Ref.Target.Ref.GetCoords().DistanceFrom(Owner.OwnerObject.Ref.Base.Base.GetCoords()) <= 5000)
            {
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
                Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(swJump);
                CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Target.Ref.GetCoords());

                if (pSuper.Ref.IsCharged == true)
                {
                    pSuper.Ref.Launch(targetCell, true);
                    pSuper.Ref.IsCharged = false;
                    pSuper.Ref.RechargeTimer.Start(1000);
                }
            }
        }
    }
}
