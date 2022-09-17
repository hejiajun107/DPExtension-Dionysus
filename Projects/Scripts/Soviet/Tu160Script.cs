using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.Soviet
{
    [Serializable]
    [ScriptAlias(nameof(Tu160Script))]
    public class Tu160Script : TechnoScriptable
    {
        public Tu160Script(TechnoExt owner) : base(owner) { }

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> ironWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Tu160IronWh");

        static Pointer<BulletTypeClass> boomberMissile => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("TuExpMissile");
        static Pointer<WarheadTypeClass> expWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("TuExpHE");



        private int immnueCoolDown = 0;

        private int bombing = 0;

        private int delay = 0;

        public override void OnUpdate()
        {
            if (immnueCoolDown > 0)
            {
                immnueCoolDown--;
            }

            if(bombing > 0)
            {
                if(delay<=0)
                {
                    delay = 20;

                    var ptechno = Owner.OwnerObject;

                 
                    var targetLocation = ptechno.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, -ptechno.Ref.Base.GetHeight());

                    var cell = CellClass.Coord2Cell(targetLocation);
                    if (MapClass.Instance.TryGetCellAt(cell, out Pointer<CellClass> pcell))
                    {
                        var bullet = boomberMissile.Ref.CreateBullet(ptechno.Convert<AbstractClass>(), ptechno, 100, expWarhead, 50, false);
                        bullet.Ref.SetTarget(pcell.Convert<AbstractClass>());
                        bullet.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords() - new CoordStruct(0, 0, 10), new BulletVelocity(0, 0, 0));
                    }

                }

                bombing--;
                delay--;
            }

        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
    Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (pAttackingHouse.IsNull)
            {
                return;
            }
            if (pAttackingHouse.Ref.ArrayIndex == Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex || Owner.OwnerObject.Ref.Owner.Ref.IsAlliedWith(pAttackingHouse))
            {
                return;
            }
            if (immnueCoolDown <= 0 && pDamage.Ref > 5)
            {
                immnueCoolDown = 1200;
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, ironWarhead, 80, false);
                pBullet.Ref.DetonateAndUnInit(currentLocation);
            }

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);
            bombing = 100;
        }

    }
}
