using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Linq;

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

        private int healthCheckRof = 100;

        private int seekTibriumRof = 50;

        public override void OnUpdate()
        {
            if (delay > 0)
            {
                delay--;
            }
            else
            {
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
            }

            if(Owner.OwnerObject.Ref.Ammo > 0)
            {
                if (healthCheckRof > 0)
                {
                    healthCheckRof--;
                }
                else
                {
                    healthCheckRof = 100;
                    var doHealth = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), 2 * Game.CellSize).Select(x => x.Convert<TechnoClass>()).Where(x => x.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner) && !x.Ref.Base.Base.IsInAir() && !x.Ref.Base.InLimbo && x.Ref.Base.Health < 0.5 * x.Ref.Type.Ref.Base.Strength && x.Ref.Base.Base.WhatAmI() != AbstractType.Building).Any();

                    if (doHealth)
                    {
                        Owner.OwnerObject.Ref.Ammo--;
                        var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, (int)(120 * Owner.OwnerObject.Ref.FirepowerMultiplier), WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("CorruptorHealwh"), 100, false);
                        bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    }
                }
            }


            if (seekTibriumRof-- <= 0)
            {
                seekTibriumRof = 50;
                if (Owner.OwnerObject.Ref.Ammo < 3)
                {
                    SeekTibrium();
                }
            }

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);

            if (enginerred)
            {
                var bullet = pBullet.Ref.CreateBullet(pTarget, Owner.OwnerObject, 30, damageWarhead, 100, false);
                bullet.Ref.MoveTo(pTarget.Ref.GetCoords() + new CoordStruct(0, 0, 10), new BulletVelocity(0, 0, 0));
                bullet.Ref.SetTarget(pTarget);
            }
        }

        private void SeekTibrium()
        {
            //获取脚下的矿
            var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            if (MapClass.Instance.TryGetCellAt(coord, out var pCell))
            {
                var value = pCell.Ref.GetContainedTiberiumValue();
                if (value > 0)
                {
                    var index = pCell.Ref.GetContainedTiberiumIndex();

                    var ammo = Owner.OwnerObject.Ref.Ammo;
                    ammo = ammo + ((index == 0 || index == 2) ? 1 : 2);
                    ammo = ammo > 3 ? 3 : ammo;

                    Owner.OwnerObject.Ref.Ammo = ammo;

                    pCell.Ref.ReduceTiberium(1);
                    YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SCAbsorbRay"), coord + new CoordStruct(0, 0, 50));
                }
            }
        }
    }
}
