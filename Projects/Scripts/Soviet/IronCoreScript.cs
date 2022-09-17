using DpLib.Scripts.Scrin;
using DynamicPatcher;
using Extension.Decorators;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.Soviet
{
   

    [Serializable]
    [ScriptAlias(nameof(IronCoreScript))]

    public class IronCoreScript : TechnoScriptable
    {
        public IronCoreScript(TechnoExt owner) : base(owner) { }
     
        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ICTKCoreEffectWh");
        static Pointer<WarheadTypeClass> ironEffectWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ICTKGiveIronWh");


        static Pointer<WarheadTypeClass> ironWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ICTKCoreIronWh");


        //static Pointer<BulletTypeClass> bulletPoint => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("ICTKPointerBullet");
        //static Pointer<WarheadTypeClass> warheadPoint => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ICTKCorePointWh");


        



        //TechnoExt protectedTechno;



        private int duration = 120;

        private int currentFrame = 0;

        private int immnueCoolDown = 0;

        private int checkTargetRof = 250;

        public override void OnUpdate()
        {
            if (currentFrame <= duration)
            {
                currentFrame++;
            }
            else
            {
                currentFrame = 0;
                var pTechno = Owner.OwnerObject;
                Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, warhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(pTechno.Ref.Base.Base.GetCoords());

                Pointer<BulletClass> pBullet2 = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, ironEffectWh, 100, false);
                pBullet2.Ref.DetonateAndUnInit(pTechno.Ref.Base.Base.GetCoords());
            }

            if (immnueCoolDown > 0)
            {
                immnueCoolDown--;
            }
            else
            {
                Owner.OwnerObject.Ref.Ammo = 1;
            }

          
            //if (checkTargetRof <= 0)
            //{
            //    if (protectedTechno.Get() != null)
            //    {
            //        var target = protectedTechno.Get().OwnerObject;
            //        Pointer<BulletClass> pBullet = bulletPoint.Ref.CreateBullet(target.Convert<AbstractClass>(), Owner.OwnerObject, 1, warheadPoint, 100, false);
            //        pBullet.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 500), new BulletVelocity(0, 0, 0));
            //        Owner.OwnerObject.Ref.Ammo = 0;
            //        //if (MapClass.Instance.TryGetCellAt(target.Ref.Base.Base.GetCoords(), out Pointer<CellClass> pCell))
            //        //{
            //        //    pBullet.Ref.SetTarget(pCell.Convert<AbstractClass>());
            //        //}
            //    }
            //    else
            //    {
            //        Owner.OwnerObject.Ref.Ammo = 1;
            //    }
            //    checkTargetRof = 250;
            //}
            //else
            //{
            //    checkTargetRof--;
            //}

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 0)
            {
               
                //if (pTarget.CastToTechno(out Pointer<TechnoClass> pTechno))
                //{
                //    TechnoExt pTargetExt = TechnoExt.ExtMap.Find(pTechno);
                //    if (pTechno.Ref.Owner.Ref.ArrayIndex == Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex || Owner.OwnerObject.Ref.Owner.Ref.IsAlliedWith(pTechno.Ref.Owner.Ref.ArrayIndex))
                //    {
                //        if (pTargetExt.Get(IonCoreProtection.ID) == null)
                //        {
                //            if (protectedTechno.TryGet(out TechnoExt oldTechnoExt))
                //            {
                //                //释放掉当前的单位
                //                var decorator = oldTechnoExt.Get(IonCoreProtection.ID);
                //                if (decorator != null)
                //                {
                //                    oldTechnoExt.Remove(IonCoreProtection.ID);
                //                }
                //            }

                //            pTargetExt.CreateDecorator<IonCoreProtection>(IonCoreProtection.ID, "IonCoreProtection Decorator", this, pTargetExt);
                //            protectedTechno.Set(pTargetExt);
                //            Owner.OwnerObject.Ref.Ammo = 0;
                //        }
                //    }
                //}
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
            if (immnueCoolDown <= 0 && pDamage.Ref > 20)
            {
                Owner.OwnerObject.Ref.Ammo = 0;
                immnueCoolDown = 2500;
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, ironWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(currentLocation);
            }

        }

         

    }



    [Serializable]
    [ScriptAlias(nameof(IonCoreProtection))]

    public class IonCoreProtection : TechnoScriptable
    {
        public static int ID = 214001;
        public IonCoreProtection(IronCoreScript owner, TechnoExt self) : base(self)
        {
            Owner=(owner.Owner);
            Self=(self);
        }

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> ironWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ICTKCoreIronOtherWh");

        static Pointer<WarheadTypeClass> breakMindWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BreakMindControlWh");


        TechnoExt Owner;
        TechnoExt Self;

        private int immnueCoolDown = 0;

        private int startTime = 500;

        private int initHouse = 0;

        public override void OnUpdate()
        {
            if (startTime > 0)
            {
                startTime--;
            }

            if (immnueCoolDown > 0)
            {
                immnueCoolDown--;
            }
            if (Owner.IsNullOrExpired() || Self.IsNullOrExpired())
            {
                DetachFromParent();
                return;
            }

            if (initHouse != 0)
            {
                Pointer<TechnoClass> pTechno = Self.OwnerObject;
                if (!pTechno.Ref.Owner.IsNull)
                {
                    initHouse = pTechno.Ref.Owner.Ref.ArrayIndex;
                }
            }
           
            
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
       Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (startTime > 0)
                return;

           if(pAttackingHouse.IsNull)
            {
                return;
            }

            if (!Self.IsNullOrExpired())
            {
                Pointer<TechnoClass> pTechno = Self.OwnerObject;
                if (pAttackingHouse.Ref.ArrayIndex == pTechno.Ref.Owner.Ref.ArrayIndex || pTechno.Ref.Owner.Ref.IsAlliedWith(pAttackingHouse))
                    return;

                

                if ((immnueCoolDown <= 0 && pDamage.Ref > 20))
                {
                    CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();

                    if (initHouse != 0 && !pTechno.Ref.Owner.IsNull)
                    {
                        if (pTechno.Ref.Owner.Ref.ArrayIndex != initHouse)
                        {
                            Pointer<BulletClass> pBullet2 = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Self.OwnerObject, 1, breakMindWarhead, 100, false);
                            pBullet2.Ref.DetonateAndUnInit(currentLocation);
                        }   
                    }
                    immnueCoolDown = 1200;
                    Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Self.OwnerObject, 1, ironWarhead, 100, false);
                    pBullet.Ref.DetonateAndUnInit(currentLocation);
                }
            }
          
        }

    }

}
