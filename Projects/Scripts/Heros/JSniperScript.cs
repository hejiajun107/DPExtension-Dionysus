using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using System.Threading.Tasks;
using System.Linq;
using Extension.Shared;
using Extension.Decorators;
using Extension.Utilities;

namespace Scripts
{

    [Serializable]
    public class JSniper : TechnoScriptable
    {
        public JSniper(TechnoExt owner) : base(owner)
        {
        }

 

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");


        public override void OnUpdate()
        {
           
        }



        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 0)
            {
               
            }
            if(weaponIndex == 1)
            {
                if (pTarget.CastToTechno(out Pointer<TechnoClass> pTechno))
                {
                    TechnoExt pTargetExt = TechnoExt.ExtMap.Find(pTechno);
                  
                    if (pTargetExt.GameObject.GetComponent(VirusSpreadDecorator.ID) == null)
                    {
                        pTargetExt.GameObject.CreateScriptComponent(nameof(VirusSpreadDecorator), VirusSpreadDecorator.ID, "VirusSpread Decorator", Owner, pTargetExt, 1000);
                    }
                }
            }
        }
    }




    [Serializable]
    public class VirusSpreadDecorator : TechnoScriptable
    {
        public static int ID = 514022;
        public VirusSpreadDecorator(TechnoExt owner, TechnoExt self, int lifetime):base(self)
        {
            Owner.Set(owner);
            Self.Set(self);
            _lifetime = lifetime;
            _startTime = lifetime - 100;
        }

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> pWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("JVirusSpreadWH");

        ExtensionReference<TechnoExt> Owner;
        ExtensionReference<TechnoExt> Self;

        int _lifetime = 1000;

        //开始传播的延迟
        private int _startTime = 100;

        int rof = 2;

        public override void OnUpdate()
        {
            if (Owner.Get() == null || Self.Get() == null || _lifetime <= 0)
            {
                DetachFromParent();
                return;
            }

            if (rof-- > 0 && --_lifetime > 0)
            {
                return;
            }
            rof = 5;

            var owner = Owner.Get();


            if(_lifetime< _startTime)
            {
                var current = Self.Get().OwnerObject.Ref.Base.Base.GetCoords();
                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(owner.OwnerObject.Convert<AbstractClass>(), owner.OwnerObject, 1, pWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(current);
            }    
       

        }



    }

}