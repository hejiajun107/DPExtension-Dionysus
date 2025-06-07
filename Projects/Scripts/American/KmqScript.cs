using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Linq;

namespace DpLib.Scripts.American
{
    [Serializable]
    [ScriptAlias(nameof(KmqScript))]
    public class KmqScript : TechnoScriptable
    {
        public KmqScript(TechnoExt owner) : base(owner)
        {
        }

        private Pointer<WeaponTypeClass> weapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("KmqGuideWeapon");

        private int delay = 180;
        public override void OnUpdate()
        {
            if (delay > 0)
            {
                delay--;
            }
            base.OnUpdate();
        }


        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 0)
            {
                if (delay <= 0)
                {
                    delay = 180;

                    var wh = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("KqmAnimWh");
                    var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

                    var bullet = pInviso.Ref.CreateBullet(pTarget, Owner.OwnerObject, 1, wh, 100, false);
                    bullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());

                    var technos = ObjectFinder.FindTechnosNear(pTarget.Ref.GetCoords(), Game.CellSize * 6).Select(x => x.Convert<TechnoClass>()).Where(x => !x.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner) && !x.Ref.Base.InLimbo && x.Ref.Base.Base.GetCoords() != pTarget.Ref.GetCoords() && MapClass.GetTotalDamage(10000,wh,x.Ref.Type.Ref.Base.Armor,0)>0).ToList()
                        .OrderBy(x=>x.Ref.Base.Base.GetCoords().BigDistanceForm(pTarget.Ref.GetCoords())).Take(2).ToList();

                    if (technos.Count > 0) { 
                        foreach(var techno in technos)
                        {
                            var bullet1 = pInviso.Ref.CreateBullet(techno.Convert<AbstractClass>(), Owner.OwnerObject, 1, wh, 100, false);
                            bullet1.Ref.DetonateAndUnInit(techno.Ref.Base.Base.GetCoords());
                        }
                    }

                    //var bullet = weapon.Ref.Projectile.Ref.CreateBullet(pTarget, Owner.OwnerObject, 1, weapon.Ref.Warhead, 100, false);
                    //bullet.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 100), new BulletVelocity(0, 0, 0));
                    //bullet.Ref.SetTarget(pTarget);
                }
            }
        }
    }
}
