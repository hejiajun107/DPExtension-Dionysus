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
    [ScriptAlias(nameof(TeslaTankScript))]
    public class TeslaTankScript : TechnoScriptable
    {
        public TeslaTankScript(TechnoExt owner) : base(owner) { }

        //贴上mk2的buff
        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");

        static Pointer<WarheadTypeClass> teslaWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("TankElectric");


        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<BulletTypeClass> eBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Electricbounce");



        private bool IsMkIIUpdated = false;

        private int SleepDuration = 0;

        public override void OnUpdate()
        {
            if(IsMkIIUpdated && SleepDuration < 500)
            {
                SleepDuration++;
            }
            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (IsMkIIUpdated && weaponIndex == 0)
            {
                if (SleepDuration > 30)
                {
                    var extraDamage = (int)((SleepDuration / 100d) * 30);
                    if (SleepDuration <= 300)
                    {
                        Pointer<BulletClass> plv1 = pBulletType.Ref.CreateBullet(pTarget, Owner.OwnerObject, extraDamage, teslaWarhead, 100, true);
                        plv1.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords(), new BulletVelocity(0, 0, 0));
                        plv1.Ref.SetTarget(pTarget);
                    }
                    else
                    {
                        Pointer<BulletClass> plv2 = eBullet.Ref.CreateBullet(pTarget, Owner.OwnerObject, extraDamage, teslaWarhead, 100, true);
                        plv2.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords(), new BulletVelocity(0, 0, 0));
                        plv2.Ref.SetTarget(pTarget);
                    }
                }
                SleepDuration = 0;
            }
        }


        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
             Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (IsMkIIUpdated == false)
            {
                //判断是否来自升级弹头
                if (pWH.Ref.Base.ID.ToString() == "MarkIISpWh")
                {
                    IsMkIIUpdated = true;
                    Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                    CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                    Pointer<BulletClass> mk2bullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, mk2Warhead, 100, false);
                    mk2bullet.Ref.DetonateAndUnInit(currentLocation);
                }
            }
        }

    }
}
