using DpLib.Scripts.AE;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(BlackHoleAttachEffectScript))]
    public class BlackHoleAttachEffectScript : AttachEffectScriptable
    {
        public BlackHoleAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        TechnoExt blackHole;

        private static Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        private static Pointer<WarheadTypeClass> whup => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SBHSpeedUpWh");
        private static Pointer<WarheadTypeClass> whdn => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SBHSpeedDownWh");

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if(pAttacker.CastToTechno(out var techno))
            {
                blackHole = TechnoExt.ExtMap.Find(techno);
                var lastDistance = Owner.OwnerObject.Ref.Base.Base.GetCoords().DistanceFrom(blackHole.OwnerObject.Ref.Base.Base.GetCoords());
                if (double.IsNaN(lastDistance))
                {
                    lastDistance = 5000;
                }
            }
        }

        private double lastDistance;

        public override void OnUpdate()
        {
            if(blackHole.IsNullOrExpired())
            {
                Duration = 0;
                return;
            }

            if (Owner.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Building)
            {
                var distance = Owner.OwnerObject.Ref.Base.Base.GetCoords().DistanceFrom(blackHole.OwnerObject.Ref.Base.Base.GetCoords());
                if(double.IsNaN(distance))
                {
                    return;
                }

                if (distance > lastDistance)
                {
                    var bullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), blackHole.OwnerObject, 0, whdn, 100, false);
                    bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
                else if (distance < lastDistance)
                {
                    var bullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), blackHole.OwnerObject, 0, whup, 100, false);
                    bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }

                lastDistance = distance;
            }
        }
    }
}
