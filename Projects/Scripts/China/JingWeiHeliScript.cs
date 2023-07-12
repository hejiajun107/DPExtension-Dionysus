using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(JingWeiHeliScript))]
    public class JingWeiHeliScript : TechnoScriptable
    {
        public JingWeiHeliScript(TechnoExt owner) : base(owner) { }


        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");

        static Pointer<WarheadTypeClass> powrWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("JWPowrWH");
        

        static Pointer<WarheadTypeClass> healWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ORCACARAOEHelWH");

        static Pointer<WarheadTypeClass> empWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ORCACARAOEWH");

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private bool IsMkIIUpdated = false;


        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (IsMkIIUpdated)
            {
                var target = pTarget.Ref.GetCoords();
                var pHeal = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 30, healWarhead, 100, false);
                var pEmp = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, empWh, 100, false);
                pHeal.Ref.DetonateAndUnInit(target);
                pEmp.Ref.DetonateAndUnInit(target);
            }

            if(pTarget.CastToTechno(out var ptechno))
            {
                if (ptechno.Ref.Owner.IsNull)
                    return;
                if (!ptechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                    return;
                if (ptechno.Ref.Base.Health < ptechno.Ref.Type.Ref.Base.Strength)
                    return;
                var target = pTarget.Ref.GetCoords();
                var pPowr = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, powrWarhead, 100, false);
                pPowr.Ref.DetonateAndUnInit(target);
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
