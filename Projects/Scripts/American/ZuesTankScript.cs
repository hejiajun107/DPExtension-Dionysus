using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.American
{
    [Serializable]
    [ScriptAlias(nameof(ZuesTankScript))]
    public class ZuesTankScript : TechnoScriptable
    {
        public ZuesTankScript(TechnoExt owner) : base(owner) { }

        //引爆闪电的弹头
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ZeusTnkWh");

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisibleHigh");

        private Random random = new Random(12135);

        //范围 CellSpread*256
        private int spread = 2560;

        //伤害
        private int damage = 180;

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 1)
            {
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                var target = new CoordStruct(location.X + random.Next(-spread, spread), location.Y + random.Next(-spread, spread), location.Z - Owner.OwnerObject.Ref.Base.GetHeight());

                for (var i = 0; i < 3; i++)
                {
                    var bullet1 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, warhead, 100, true);
                    bullet1.Ref.DetonateAndUnInit(target);

                    if (random.Next(100) > 50)
                    {
                        var target2 = new CoordStruct(location.X + random.Next(-spread / 2, spread / 2), location.Y + random.Next(-spread / 2, spread / 2), location.Z - Owner.OwnerObject.Ref.Base.GetHeight());
                        var bullet2 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, warhead, 100, true);
                        bullet2.Ref.DetonateAndUnInit(target2);
                    }

                    if (random.Next(100) > 50)
                    {
                        var target3 = new CoordStruct(location.X + random.Next(-spread, spread), location.Y + random.Next(-spread, spread), location.Z - Owner.OwnerObject.Ref.Base.GetHeight());
                        var bullet3 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, warhead, 100, true);
                        bullet3.Ref.DetonateAndUnInit(target3);
                    }
                }
            }
            base.OnFire(pTarget, weaponIndex);
        }

    }
}
