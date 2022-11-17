using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.Soviet
{
    [Serializable]
    [ScriptAlias(nameof(ZepScript))]
    public class ZepScript : TechnoScriptable
    {
        public ZepScript(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<BulletTypeClass> pBullet => new Pointer<BulletTypeClass>();

        private static Pointer<WarheadTypeClass> pDamgeWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChaosDamageWh");
        //private static Pointer<TechnoTypeClass> NoramlType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("ZEP");

        //private static Pointer<TechnoTypeClass> SpeedType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("ZEPFAST");

        //private int type = 0;

        public override void OnUpdate()
        {

        }


        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 1)
            {
                var dBullet = pBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1000, pDamgeWarhead, 100, false);
                dBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }
        }



    }
}
