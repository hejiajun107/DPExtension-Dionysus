using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.CW;
using Extension.Ext4CW;

namespace Scripts.Yuri
{
    [ScriptAlias(nameof(SMNKScript))]
    [Serializable]
    public class SMNKScript : TechnoScriptable
    {
        private static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SMNKSSB");

        private static Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");


        public SMNKScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (pTarget.IsNull)
                return;

            if (pTarget.Ref.WhatAmI() == AbstractType.Building)
                return;

            if (pTarget.CastToTechno(out var ptechno))
            {
                var technoExt = TechnoExt.ExtMap.Find(ptechno);

                if (technoExt.IsNullOrExpired())
                    return;

                var gext = technoExt.GameObject.GetTechnoGlobalComponent();

                if ((ptechno.Ref.IsMindControlled() || ptechno.Ref.Type.Ref.ImmuneToPsionics) && !gext.Data.IsEpicUnit && !gext.Data.IsHero)
                {
                    var damage = Owner.OwnerObject.Ref.Veterancy.IsElite() ? 80 : 40;
                    var pbullet = inviso.Ref.CreateBullet(pTarget, Owner.OwnerObject, damage, warhead, 100, true);
                    pbullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
                }
            }
        }
    }
}
