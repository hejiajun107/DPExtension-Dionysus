
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

namespace Scripts
{

    [Serializable]
    public class Wellka : TechnoScriptable
    {
        public Wellka(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(10);
        }

        private ManaCounter _manaCounter;


        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> heroWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("WellkaIronRingHeroWh");
        static Pointer<WarheadTypeClass> unitWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("WellkaIronRingUnitWh");



        private int battleFrame = 0;

        //每隔多少帧检测一次
        private int rof = 0;


        public override void OnUpdate()
        {
            _manaCounter.OnUpdate(Owner);
            //检测是否处于战斗状态
            if (battleFrame > 0)
            {
                if (rof <= 0)
                {
                    if (_manaCounter.Cost(10))
                    {
                        //施加一个铁幕光环
                        Pointer<BulletClass> pBullet1 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, heroWh, 100, false);
                        pBullet1.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

                        Pointer<BulletClass> pBullet2 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, unitWh, 100, false);
                        pBullet2.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    }
                    rof = 50;
                }
                else
                {
                    rof--;
                }
            }

            battleFrame--;
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            battleFrame = 500;
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
             Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            battleFrame = 500;
        }
    }
}