using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using Extension.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{
    [Serializable]
    public class CComand : TechnoScriptable
    {
        public CComand(TechnoExt owner) : base(owner) {
            _manaCounter = new ManaCounter();
        }

   
        private ManaCounter _manaCounter;

        static Pointer<WeaponTypeClass> Weapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("FakeWeaponTimeStop");
        //static Pointer<WeaponTypeClass> Weapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("RedEye2");
        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        //static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("TimeStopPro");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChronoTimeStopWH");

        private int delay = 0;


        public override void OnUpdate()
        {
            _manaCounter.OnUpdate(Owner);
            if (delay <= 100)
            {
                delay++;
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            Pointer<TechnoClass> pTechno = Owner.OwnerObject;

            if (delay >= 100)
            {
                if (_manaCounter.Cost(50))
                {
                    Pointer<BulletClass> pBullet = Weapon.Ref.Projectile.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 10, Weapon.Ref.Warhead, Weapon.Ref.Speed, false);
                    CoordStruct where = pTechno.Ref.Base.Base.GetCoords();
                    BulletVelocity velocity = new BulletVelocity(0, 0, 0);

                    pBullet.Ref.MoveTo(where + new CoordStruct(0, 0, 1000), velocity);
                    pBullet.Ref.SetTarget(pTarget);

                    delay = 0;
                    //pBullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
                }
            }
           
        }



    }
   
}
