using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Scripts.AE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(NanoRobotScript))]
    [Serializable]
    public class NanoRobotScript : TechnoScriptable
    {
        public NanoRobotScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            var technos = ObjectFinder.FindTechnosNear(pTarget.Ref.GetCoords(), 5 * Game.CellSize);

            int additon = 1;
            if(pTarget.CastToTechno(out var pCurrent))
            {
                var ext = TechnoExt.ExtMap.Find(pCurrent);
                if (!ext.IsNullOrExpired())
                {
                    var scriptComponent = ext.GameObject.GetComponent<AttachEffectScriptExtension>();
                    if (scriptComponent != null)
                    {
                        if (scriptComponent.HasAttachEffect(nameof(NanoReparingAttachEffectScript)))
                        {
                            additon = 0;
                        }
                    }
                }
            }

            var targets = technos.Where(x =>
            {
                var ptechno = x.Convert<TechnoClass>();
                if (!ptechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                    return false;

                if (ptechno.Ref.Base.InLimbo)
                    return false;

                if (ptechno.Ref.Base.Base.IsInAir())
                    return false;

                if (!GameUtil.CanAffectTargetAllowNegative(Owner.OwnerObject, ptechno))
                    return false;

                if (ptechno.Ref.Type.Ref.Base.Base.ID == "SMECH")
                    return false;

                var ext = TechnoExt.ExtMap.Find(ptechno);
                if (ext.IsNullOrExpired())
                    return false;

                var scriptComponent = ext.GameObject.GetComponent<AttachEffectScriptExtension>();
                if (scriptComponent is null)
                    return false;

                if (scriptComponent.HasAttachEffect(nameof(NanoReparingAttachEffectScript)))
                    return false;


                return true;

            }).OrderByDescending(x=>x.Ref.Base.GetCoords().BigDistanceForm(pTarget.Ref.GetCoords())).Take(2 + additon).ToList();


            var lastCoord = pTarget.Ref.GetCoords();
            foreach(var target in targets)
            {
                if (target.Convert<AbstractClass>() == pTarget)
                    continue;

                var targetCoord = target.Ref.Base.GetCoords();

                var blue = new ColorStruct(0, 255, 255);

                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(lastCoord, targetCoord, blue, blue, blue, 3);
                pLaser.Ref.Thickness = 5;
                pLaser.Ref.IsHouseColor = false;

                var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(target.Convert<AbstractClass>(),Owner.OwnerObject,1,WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SMECHWarhead"),100,true);
                pInviso.Ref.DetonateAndUnInit(targetCoord);

                lastCoord = targetCoord;
            }
        }
    }
}
