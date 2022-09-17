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

namespace DpLib.Scripts.Japan
{
    [Serializable]
    [ScriptAlias(nameof(JEpicScript))]
    public class JEpicScript : TechnoScriptable
    {
        public JEpicScript(TechnoExt owner) : base(owner) { }

        private int delay = 40;

        private Random random = new Random(11123);

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> expWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EpicPoExpWh");

        static Pointer<SuperWeaponTypeClass> swVirus => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("UnitDeliveryForVirus");

        static Pointer<AnimTypeClass> anim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("PIFFPIFF");

        public override void OnUpdate()
        {
            base.OnUpdate();




            if (delay-- > 0) return;

            delay = 40;

            var currentAmount = Owner.OwnerObject.Ref.Tiberium.GetTotalAmount();

            if (currentAmount >= 120) return;

            //获取脚下的矿
            var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var currentCell = CellClass.Coord2Cell(coord);

            var enumerator = new CellSpreadEnumerator(1);

            foreach (CellStruct offset in enumerator)
            {
                currentAmount = (int)Owner.OwnerObject.Ref.Tiberium.GetTotalAmount();

                if (currentAmount >= 120) return;

                CoordStruct where = CellClass.Cell2Coord(currentCell + offset, coord.Z);

                if (MapClass.Instance.TryGetCellAt(where, out var pCell))
                {
                    var value = pCell.Ref.GetContainedTiberiumValue();
                    if (value > 0)
                    {

                        var index = pCell.Ref.GetContainedTiberiumIndex();
                        var amount = value / (index == 0 ? 25f : 50f);

                        amount = (currentAmount + amount > 120) ? (120 - currentAmount) : amount;


                        Owner.OwnerObject.Ref.Tiberium.AddAmount(amount, index);
                        pCell.Ref.ReduceTiberium((int)amount);
                        YRMemory.Create<AnimClass>(anim, where);
                    }
                }
            }

            if (Owner.OwnerObject.Ref.Tiberium.GetTotalAmount() >= 120) return;

            if (random.Next(100) >= 70)
            {
                Owner.OwnerObject.Ref.Tiberium.AddAmount(1, 1);
            }
            else
            {
                Owner.OwnerObject.Ref.Tiberium.AddAmount(1, 0);
            }

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);

            if (weaponIndex == 0)
            {
                var total = Owner.OwnerObject.Ref.Tiberium.GetTotalAmount();

                if (total >= 10)
                {
                    var yellowAmount = Owner.OwnerObject.Ref.Tiberium.GetAmount(0);
                    var colorfulAmount = Owner.OwnerObject.Ref.Tiberium.GetAmount(1);

                    var colorful = colorfulAmount > 10 ? 10 : colorfulAmount;
                    var yellow = 10 - colorful;

                    //每次消耗5个矿
                    Owner.OwnerObject.Ref.Tiberium.RemoveAmount(colorful, 1);
                    Owner.OwnerObject.Ref.Tiberium.RemoveAmount(yellow, 0);

                    //todo
                    var damage = (int)(yellowAmount * 15 + colorful * 30);
                    Pointer<BulletClass> expBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, expWarhead, 50, false);
                    expBullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());

                    //放置毒云
                    Pointer<HouseClass> pOwner = Owner.OwnerObject.Ref.Owner;
                    Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(swVirus);
                    CellStruct targetCell = CellClass.Coord2Cell(pTarget.Ref.GetCoords());
                    pSuper.Ref.IsCharged = true;
                    pSuper.Ref.Launch(targetCell, true);
                    pSuper.Ref.IsCharged = false;

                }
            }



        }

    }
}
