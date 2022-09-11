using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.China
{
    [Serializable]
    class DTowercript : TechnoScriptable
    {
        public DTowercript(TechnoExt owner) : base(owner) { }


        private Random random = new Random(55212);

        static Pointer<BulletTypeClass> bullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("DrRaySeeker");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("OpRayWH");

        static Pointer<BulletTypeClass> Ibullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");


        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            var target = pTarget.Ref.GetCoords();
            var count = IsMkIIUpdated ? 5 : 3;
            for (var i = 0; i < count; i++)
            {
                var rdlocaton = target + new CoordStruct(random.Next(-700, 700), random.Next(-700, 700), 0);
                if (MapClass.Instance.TryGetCellAt(target, out Pointer<CellClass> cell))
                {
                    Pointer<BulletClass> pBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 55, warhead, IsMkIIUpdated ? 95 + i : 90, true);
                    pBullet.Ref.SetTarget(cell.Convert<AbstractClass>());
                    pBullet.Ref.MoveTo(rdlocaton + new CoordStruct(0, 0, 100), new BulletVelocity(0, 0, 0));
                }
            }
        }


        private bool IsMkIIUpdated = false;

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            base.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);

            if (IsMkIIUpdated == false)
            {
                //判断是否来自升级弹头
                if (pWH.Ref.Base.ID.ToString() == "MarkIISpWh")
                {
                    IsMkIIUpdated = true;
                    Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                    CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                    Pointer<BulletClass> mk2bullet = Ibullet.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, mk2Warhead, 100, false);
                    mk2bullet.Ref.DetonateAndUnInit(currentLocation);
                }
            }
        }


    }
}
