
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
    public class IvanKing : TechnoScriptable
    {
        public IvanKing(TechnoExt owner) : base(owner)
        {

        }


        Random random = new Random(114545);

        private int maxStrenth = 650;


        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("IVANKINGWH");



        public override void OnUpdate()
        {

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {

        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
    Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            try
            {
                if (pAttackingHouse.IsNull)
                {
                    return;
                }
                if (pAttackingHouse.Ref.ArrayIndex != Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                {
                    var selfLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                    var targetLocation = pAttacker.Ref.Base.GetCoords();

                    var health = Owner.OwnerObject.Ref.Base.Health;

                    var distance = selfLocation.DistanceFrom(targetLocation);

                    if (distance <= 1400)
                    {
                        int rate = 30;
                        if (health / (double)maxStrenth < 0.4)
                        {
                            rate = 40;
                        }

                        var rd = random.Next(100);

                        if (rd <= rate)
                        {
                            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 70, warhead, 100, false);
                            pBullet.Ref.DetonateAndUnInit(targetLocation);

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //可能是因为爆炸产生的碎片之类的伤害导致获取不到攻击者
                ;
            }
        }
    }
}