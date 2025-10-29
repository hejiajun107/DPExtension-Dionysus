using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Linq;

namespace DpLib.Scripts.Japan
{
    [Serializable]
    [ScriptAlias(nameof(EKingScript))]
    public class EKingScript : TechnoScriptable
    {
        public EKingScript(TechnoExt owner) : base(owner) { }
        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");

        static Pointer<BulletTypeClass> shootBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("EkingSeeker");
        static Pointer<WarheadTypeClass> shootWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EkingHealShWh");

        static Pointer<WarheadTypeClass> damageWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("EkingDamageAP");

        static Pointer<WarheadTypeClass> healthWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("KOGHEALWarhead");


        private bool IsMkIIUpdated = false;

        private int FireKeepTime = 0;

        private int feedBackRecoverRof = 100;

        private int feedBackCount = 1;

        private int rof = 0;

        public override void OnUpdate()
        {
            if (feedBackRecoverRof > 0)
            {
                feedBackRecoverRof--;
            }
            else
            {
                if (feedBackCount < 3)
                {
                    feedBackRecoverRof = 100;
                    feedBackCount += 1;
                }
            }

            Owner.OwnerObject.Ref.Ammo = feedBackCount;

            if (!IsMkIIUpdated)
            {
                return;
            }

            if (FireKeepTime <= 0)
            {
                return;
            }

            FireKeepTime--;

            if (rof-- > 0)
            {
                return;
            }

            rof = 100;

            int count = 0;
            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var currentCell = CellClass.Coord2Cell(location);


            //var ptechnos = ObjectFinder.FindTechnosNear(location, 4 * Game.CellSize).ToList();

            var ptechnos = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), 4 * Game.CellSize).OrderBy(x=>x.Ref.Base.GetCoords().DistanceFrom(location));

            foreach (var pobj in ptechnos)
            {
                if (count >= 2)
                    break;

                if(pobj.CastToTechno(out var ptechno))
                {
                    if (ptechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex))
                        continue;

                    if (ptechno.Ref.Base.InLimbo == true)
                    {
                        continue;
                    }

                    Pointer<BulletClass> dbullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, (int)(60 * Owner.OwnerObject.Ref.FirepowerMultiplier), damageWarhead, 30, true);
                    dbullet.Ref.DetonateAndUnInit(ptechno.Ref.Base.Base.GetCoords());

                    Pointer<BulletClass> bullet = shootBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, (int)(60 * Owner.OwnerObject.Ref.FirepowerMultiplier), shootWarhead, 30, true);
                    bullet.Ref.MoveTo(ptechno.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 150), new BulletVelocity(0, 0, 0));
                    bullet.Ref.SetTarget(Owner.OwnerObject.Convert<AbstractClass>());
                    count++;

                }
            }
            

              
 
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 0 && (pTarget.Ref.WhatAmI() == AbstractType.Unit || pTarget.Ref.WhatAmI() == AbstractType.Infantry || pTarget.Ref.WhatAmI() == AbstractType.Building))
            {
                if (pTarget.CastToTechno(out var ptech))
                {
                    var bullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), ptech, (int)(60 * Owner.OwnerObject.Ref.FirepowerMultiplier), healthWarhead, 100, false);
                    bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }
            if (IsMkIIUpdated)
            {
                FireKeepTime = 250;
            }
            base.OnFire(pTarget, weaponIndex);
        }


        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
          Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (feedBackCount > 0)
            {
                if (pAttackingHouse.IsNotNull && pAttacker.IsNotNull)
                {
                    if (pAttackingHouse.Ref.ArrayIndex != Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                    {
                        var selfLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                        var targetLocation = pAttacker.Ref.Base.GetCoords();

                        var distance = selfLocation.BigDistanceForm(targetLocation);

                        if (distance <= Game.CellSize * 7)
                        {
                            if(pWH != healthWarhead)
                            {
                                feedBackCount--;
                                Pointer<BulletClass> dbullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 50, damageWarhead, 30, true);
                                dbullet.Ref.DetonateAndUnInit(pAttacker.Ref.Base.GetCoords());

                                Pointer<BulletClass> bullet = shootBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 50, shootWarhead, 30, true);
                                bullet.Ref.MoveTo(pAttacker.Ref.Base.GetCoords() + new CoordStruct(0, 0, 150), new BulletVelocity(0, 0, 0));
                                bullet.Ref.SetTarget(Owner.OwnerObject.Convert<AbstractClass>());
                            }
                        }
                    }
                }
            }
            
           


            if (IsMkIIUpdated == false)
            {
                //判断是否来自升级弹头
                if (pWH.Ref.Base.ID.ToString() == "MarkIISpWh")
                {
                    IsMkIIUpdated = true;
                    Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                    CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                    Pointer<BulletClass> mk2bullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, mk2Warhead, 50, false);
                    mk2bullet.Ref.DetonateAndUnInit(currentLocation);

                }
            }
            else
            {
                FireKeepTime = 200;
            }
        }

    }
}