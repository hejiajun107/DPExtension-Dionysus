using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(CorruptorScript))]
    public class CorruptorScript : TechnoScriptable
    {

        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private static Pointer<BulletTypeClass> pBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("CorrExBullet");

        private static Pointer<WarheadTypeClass> attachWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("GiveUnitShiedsWh");

        private static Pointer<WarheadTypeClass> removeWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RemoveUnitShiedsWh");

        private static Pointer<WarheadTypeClass> damageWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("CorruptorWh2");


        private bool enginerred = false;

        private int delay = 100;

        public CorruptorScript(TechnoExt owner) : base(owner)
        {

        }

        public override void OnUpdate()
        {
            if (delay-- > 0)
            {
                return;
            }

            delay = 100;

            if (Owner.OwnerObject.Ref.Passengers.NumPassengers > 0)
            {
                if (!enginerred)
                {
                    enginerred = true;
                    var inviso = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, attachWarhead, 100, false);
                    inviso.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }
            else
            {
                if (enginerred)
                {
                    enginerred = false;
                    var inviso = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, removeWarhead, 100, false);
                    inviso.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }

            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);

            if (enginerred)
            {
                var bullet = pBullet.Ref.CreateBullet(pTarget, Owner.OwnerObject, 40, damageWarhead, 100, false);
                bullet.Ref.MoveTo(pTarget.Ref.GetCoords() + new CoordStruct(0, 0, 10), new BulletVelocity(0, 0, 0));
                bullet.Ref.SetTarget(pTarget);
            }
        }


    }
}
