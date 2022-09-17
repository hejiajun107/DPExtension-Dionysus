
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Script;
using System.Threading.Tasks;
using System.Linq;
using Extension.Shared;
using DpLib.Scripts;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(SFZSScript))]

    public class SFZSScript : TechnoScriptable
    {
        public SFZSScript(TechnoExt owner) : base(owner) {
        }


        static SFZS()
        {
            
        }



        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> pWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SFZSSlashWH");


        




        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(weaponIndex==0)
            {
                var targetLocation = pTarget.Ref.GetCoords();
                var selfLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                // Logger.Log(targetLocation.DistanceFrom(selfLocation));
                if (targetLocation.DistanceFrom(selfLocation) <= 400)
                {
                    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 80, pWH, 100, false);
                    pBullet.Ref.DetonateAndUnInit(targetLocation);
                }
            }
        }
    }
}