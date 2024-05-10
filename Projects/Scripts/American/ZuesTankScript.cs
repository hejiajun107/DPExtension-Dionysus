using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Linq;

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


        //范围 CellSpread*256
        private int spread = 2560;

        //伤害
        private int damage = 180;

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {

            if (weaponIndex == 1)
            {
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();


                for (var i = 0; i < 3; i++)
                {
                    var target = new CoordStruct(location.X + MathEx.Random.Next(-spread, spread), location.Y + MathEx.Random.Next(-spread, spread), location.Z - Owner.OwnerObject.Ref.Base.GetHeight());
                    var targetsNearBy = ObjectFinder.FindTechnosNear(target, 5 * Game.CellSize).Select(x => x.Convert<TechnoClass>()).Where(x=>!x.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner)).OrderByDescending(x => x.Ref.Base.Base.GetCoords().DistanceFrom(target)).ToList();
                    var nearyby = targetsNearBy.FirstOrDefault();

                    if (nearyby != null && nearyby.IsNotNull)
                    {
                        target = nearyby.Ref.Base.Base.GetCoords();
                    }

                    var bullet1 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, warhead, 100, true);
                    bullet1.Ref.DetonateAndUnInit(target);

                    if (MathEx.Random.Next(100) > 50)
                    {
                        var target2 = new CoordStruct(location.X + MathEx.Random.Next(-spread / 2, spread / 2), location.Y + MathEx.Random.Next(-spread / 2, spread / 2), location.Z - Owner.OwnerObject.Ref.Base.GetHeight());
                        var bullet2 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, warhead, 100, true);
                        bullet2.Ref.DetonateAndUnInit(target2);
                    }

                    if (MathEx.Random.Next(100) > 50)
                    {
                        var target3 = new CoordStruct(location.X + MathEx.Random.Next(-spread, spread), location.Y + MathEx.Random.Next(-spread, spread), location.Z - Owner.OwnerObject.Ref.Base.GetHeight());
                        var bullet3 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, warhead, 100, true);
                        bullet3.Ref.DetonateAndUnInit(target3);
                    }
                }
            }
            base.OnFire(pTarget, weaponIndex);
        }

    }
}
