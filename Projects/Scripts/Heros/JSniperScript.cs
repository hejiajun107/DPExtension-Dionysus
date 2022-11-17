using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(JSniperScript))]
    public class JSniperScript : TechnoScriptable
    {
        public JSniperScript(TechnoExt owner) : base(owner)
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
            if (weaponIndex == 1)
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
        public VirusSpreadDecorator(TechnoExt owner, TechnoExt self, int lifetime) : base(self)
        {
            Owner = (owner);
            Self = (self);
            _lifetime = lifetime;
            _startTime = lifetime - 100;
        }

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> pWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("JVirusSpreadWH");

        TechnoExt Owner;
        TechnoExt Self;

        int _lifetime = 1000;

        //开始传播的延迟
        private int _startTime = 100;

        int rof = 2;

        public override void OnUpdate()
        {
            if (Owner.IsNullOrExpired() || Self.IsNullOrExpired() || _lifetime <= 0)
            {
                DetachFromParent();
                return;
            }

            if (rof-- > 0 && --_lifetime > 0)
            {
                return;
            }
            rof = 5;

            var owner = Owner;


            if (_lifetime < _startTime)
            {
                var current = Self.OwnerObject.Ref.Base.Base.GetCoords();
                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(owner.OwnerObject.Convert<AbstractClass>(), owner.OwnerObject, 1, pWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(current);
            }


        }



    }

}