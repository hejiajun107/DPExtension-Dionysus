using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.American
{
    [Serializable]
    [ScriptAlias(nameof(AuroraScript))]
    public class AuroraScript : TechnoScriptable
    {
        public AuroraScript(TechnoExt owner) : base(owner) { }

        private int checkRof = 0;

        TechnoExt pTargetRef;

        private int duration = 0;

        private int coolDown = 500;

        private int takedDamage = 0;

        private bool isInShield = false;

        static Pointer<WarheadTypeClass> immnueWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("AuroraImmnueWh");

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        public override void OnUpdate()
        {
            base.OnUpdate();



            if (checkRof-- <= 0)
            {
                isInShield = false;
                checkRof = 20;

                if (Owner.OwnerObject.Ref.Ammo > 0 && duration <= 600 && takedDamage <= 800)
                {
                    pTargetRef = (TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Target.Convert<TechnoClass>()));
                    if (!pTargetRef.IsNullOrExpired())
                    {
                        var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                        var targetLocation = pTargetRef.OwnerObject.Ref.Base.Base.GetCoords();
                        var distance = currentLocation.DistanceFrom(targetLocation);
                        if (distance <= 30 * 256)
                        {
                            isInShield = true;

                            //施加免疫效果
                            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, immnueWarhead, 100, false);
                            pBullet.Ref.DetonateAndUnInit(currentLocation);
                            duration += 20;
                        }
                    }
                }
                else
                {
                    coolDown -= 20;
                    if (coolDown <= 0)
                    {
                        coolDown = 500;
                        duration = 0;
                        takedDamage = 0;
                    }

                }
            }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if(isInShield)
            {
                var damage = MapClass.GetTotalDamage(pDamage.Ref, pWH, Owner.OwnerObject.Ref.Type.Ref.Base.Armor, DistanceFromEpicenter);
                if (damage > 0)
                {
                    takedDamage += damage;
                }
            }
        }

    }
}
