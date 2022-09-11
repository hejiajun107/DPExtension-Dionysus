using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{
    [Serializable]
    public class DeptScript : TechnoScriptable
    {

        public DeptScript(TechnoExt owner) : base(owner) { }


        static Pointer<WarheadTypeClass> repairWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DEPTRepDpWh");
        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private int rof = 0;
        public override void OnUpdate()
        {
            if (rof-- >  0)
            {
                return;
            }
            rof = 120;

            if(Owner.OwnerObject.Ref.Base.IsAlive && Owner.OwnerObject.Ref.Base.IsOnMap && Owner.OwnerObject.Ref.Base.InLimbo==false)
            {
                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 40, repairWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }
        }
    }
}
