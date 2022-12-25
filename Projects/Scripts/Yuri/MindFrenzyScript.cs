using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(MindFrenzyScript))]
    public class MindFrenzyScript : AttachEffectScriptable
    {
        public MindFrenzyScript(TechnoExt owner) : base(owner) { }

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> chaosWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChaosWHA1");

        private int rof = 0;

        public override void OnUpdate()
        {
            if (rof > 0)
            {
                rof--;
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (rof <= 0)
            {
                rof = 40;
                Pointer<TechnoClass> ownerTechno = Owner.OwnerObject;
                CoordStruct ownerCoord = ownerTechno.Ref.Base.Base.GetCoords();

                Pointer<BulletClass> chaosBullet = bulletType.Ref.CreateBullet(ownerTechno.Convert<AbstractClass>(), Pointer<TechnoClass>.Zero, 60, chaosWarhead, 100, false);
                chaosBullet.Ref.DetonateAndUnInit(ownerCoord);
            }
        }
    }

}
