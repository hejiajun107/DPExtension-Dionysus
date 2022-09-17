using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{
    [Serializable]
    public class ArmyFlagScript : TechnoScriptable
    {
        public ArmyFlagScript(TechnoExt owner) : base(owner)
        {
        }

        private int Max_Strength = 10000;

        static Pointer<WarheadTypeClass> peaceWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("PeaceKillWh");

        static Pointer<WarheadTypeClass> pup1wh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("FLGPOWRUP1");
        static Pointer<WarheadTypeClass> pup2wh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("FLGPOWRUP2");
        static Pointer<WarheadTypeClass> pdownwh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("FLGPOWRDN");

        static Pointer<BulletTypeClass> pBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<SuperWeaponTypeClass> sw => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("InspiredSpecial");


        private int rof = 200;

        private int scoreCheckRof = 50;

        private int lastScore = 0;

        public override void OnUpdate()
        {
            if (scoreCheckRof-- <= 0)
            {
                scoreCheckRof = 50;

                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                Pointer<HouseClass> pOwner = pTechno.Ref.Owner;
                Pointer<SuperClass> pSuper = pOwner.Ref.FindSuperWeapon(sw);

                var currentScore = Owner.OwnerObject.Ref.Owner.Ref.SiloMoney;

                if (currentScore > lastScore)
                {
                    var deltaScore = currentScore - lastScore;
                    var cutDown = deltaScore / 2;
                    if (cutDown == 0)
                    {
                        cutDown = 1;
                    }

                    var timeLeft = pSuper.Ref.RechargeTimer.GetTimeLeft();

                    if (cutDown > timeLeft)
                    {
                        cutDown = timeLeft;
                    }
                    pSuper.Ref.RechargeTimer.Start(timeLeft - cutDown);
                }

                lastScore = currentScore;
            }

            if (rof-- <= 0)
            {
                rof = 200;
                var health = Owner.OwnerObject.Ref.Base.Health;
                if (health > 8000)
                {
                    var bullet = pBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, pup2wh, 100, false);
                    bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }else if(health>6000&& health<=8000)
                {
                    var bullet = pBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, pup1wh, 100, false);
                    bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }else if (health < 3000)
                {
                    var bullet = pBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, pdownwh, 100, false);
                    bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }
            base.OnUpdate();
        }


        public override void OnRemove()
        {
            //if (Owner.OwnerObject.Ref.Base.Health > 100)
            //{
            //    return;
            //}

            var technos = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, t =>
                    !t.Ref.Owner.IsNull
                    && t.Ref.Base.IsOnMap
                       , FindRange.Owner
                   );
            foreach (var techno in technos)
            {
                if (!techno.IsNullOrExpired())
                {
                    var ptechno = techno.OwnerObject;
                    ptechno.Ref.Base.TakeDamage(8000, peaceWarhead, true);
                }
            }
            base.OnRemove();
        }
    }
}
