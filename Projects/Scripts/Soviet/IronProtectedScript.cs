using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.Soviet
{

    [Serializable]
    [ScriptAlias(nameof(IronProtectedScript))]
    public class IronProtectedScript : TechnoScriptable
    {
        public IronProtectedScript(TechnoExt owner) : base(owner) { }

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> ironWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ICTKCoreIronOtherWh");

        private int immnueCoolDown = 0;

        public override void OnUpdate()
        {
            if (immnueCoolDown > 0)
            {
                immnueCoolDown--;
            }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
      Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (pAttackingHouse.IsNull)
            {
                return;
            }
            if (pAttackingHouse.Ref.ArrayIndex == Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex || Owner.OwnerObject.Ref.Owner.Ref.IsAlliedWith(pAttackingHouse))
            {
                return;
            }
            if (immnueCoolDown <= 0 && pDamage.Ref > 20)
            {
                immnueCoolDown = 1200;
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, ironWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(currentLocation);
            }

        }

    }
}
