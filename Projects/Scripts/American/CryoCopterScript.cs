﻿using Extension.Decorators;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.American
{
    [Serializable]
    public class CryoCopterScript : TechnoScriptable
    {

        public CryoCopterScript(TechnoExt owner) : base(owner) { }

        ExtensionReference<TechnoExt> pTargetRef;

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            pTargetRef.Set(TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Target.Convert<TechnoClass>()));
            if (pTargetRef.TryGet(out TechnoExt target))
            {
                if (target.GameObject.GetComponent(FreezingDecorator.ID) == null)
                {
                    target.GameObject.CreateScriptComponent(nameof(FreezingDecorator),FreezingDecorator.ID, "FreezingDecorator Decorator", target);
                }

            }
        }
    }


    [Serializable]
    public class FreezingDecorator : TechnoScriptable
    {
        public static int ID = 114001;
        public FreezingDecorator(TechnoExt self):base(self)
        {
            Owner.Set(self);

            initTemperature = 300 + (int)(self.OwnerObject.Ref.Base.Health * 5 / 100);
        }

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisibleAll");

        static Pointer<WarheadTypeClass> warhead0 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("CryoFrozenWH0");
        
        static Pointer<WarheadTypeClass> warhead1 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("CryoFrozenWH1");
        static Pointer<WarheadTypeClass> warhead2 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("CryoFrozenWH2");
        static Pointer<WarheadTypeClass> warhead3 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("CryoFrozenWH3");
        static Pointer<WarheadTypeClass> warhead4 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("CryoFrozenWH4");


        ExtensionReference<TechnoExt> Owner;

        int temperature = 300;

        int initTemperature = 300;


        public override void OnUpdate()
        {
            if (Owner.Get() == null || temperature >= initTemperature + 200)
            {
                DetachFromParent();
                return;
            }
            temperature++;
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (pWH.IsNull)
            {
                return;
            }

            var technoExt = Owner.Get();

            if (technoExt == null)
            {
                return;
            }

            if (pWH.Ref.Base.ID == "FreezingBeamWH")
            {
                temperature -= 18;
                if (temperature < -50)
                {
                    temperature = -50;
                }
            }

            Pointer<WarheadTypeClass> warhead;


            if (temperature > 180)
            {
                warhead = warhead0;
            }
            else if (temperature>= 150 && temperature <= 180)
            {
                //第一阶段的冰冻
                warhead = warhead1;
            }
            else if(temperature > 100 && temperature < 150)
            {
                //第二阶段的冰冻
                warhead = warhead2;
            }
            else if (temperature > 0 && temperature <= 100)
            {
                //第三阶段的冰冻
                warhead = warhead3;
            }
            else
            {
                //第四阶段的冰冻
                warhead = warhead4;
            }

            Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(technoExt.OwnerObject.Convert<AbstractClass>(), technoExt.OwnerObject, 1, warhead, 100, false);
            pBullet.Ref.DetonateAndUnInit(technoExt.OwnerObject.Ref.Base.Base.GetCoords());
        }
    }






}
