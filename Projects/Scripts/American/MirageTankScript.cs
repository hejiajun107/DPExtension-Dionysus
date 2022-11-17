using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.American
{
    [Serializable]
    [ScriptAlias(nameof(MirageTankScript))]
    public class MirageTankScript : TechnoScriptable
    {
        public MirageTankScript(TechnoExt owner) : base(owner) { }

        //贴上mk2的buff
        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");
        static Pointer<WarheadTypeClass> cloackWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MGTKCloakWh");

        static Pointer<WarheadTypeClass> areaCloackWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MGTKCloakAreaWh");

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");


        private bool IsMkIIUpdated = false;

        private int delay = 0;

        private int rof = 0;

        public override void OnUpdate()
        {
            if (!IsMkIIUpdated)
            {
                return;
            }

            if (delay > 0)
            {
                delay--;
                return;
            }

            if (rof > 0)
            {
                rof--;
                return;
            }

            rof = 100;

            Pointer<BulletClass> cloacAreakbullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, areaCloackWh, 100, false);
            cloacAreakbullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            delay = 400;
            base.OnFire(pTarget, weaponIndex);
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
             Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (!pAttackingHouse.IsNull)
            {
                var ownerHouse = Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex;
                if (!pAttackingHouse.Ref.IsAlliedWith(ownerHouse) && pAttackingHouse.Ref.ArrayIndex != ownerHouse)
                {
                    delay = 400;
                }
            }

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

                    Pointer<BulletClass> cloackbullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, cloackWarhead, 100, false);
                    cloackbullet.Ref.DetonateAndUnInit(currentLocation);
                }
            }
        }




    }
}
