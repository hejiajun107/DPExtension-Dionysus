using Extension.Coroutines;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [ScriptAlias(nameof(MMKSeekerScript))]
    [Serializable]
    public class MMKSeekerScript : BulletScriptable
    {
        public MMKSeekerScript(BulletExt owner) : base(owner)
        {
        }

        int i = 2;

        public override void Start()
        {
            GameObject.StartCoroutine(Seek());
        }

        IEnumerator Seek()
        {
            while(true)
            {
                yield return new WaitForFrames(10);

                if (!Owner.OwnerObject.Ref.Owner.IsNull)
                {
                    var bulletType = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("MMKSeeker");
                    var warhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("CenturionRailWH");

                    var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                    var pr = bulletType.Ref.CreateBullet(Owner.OwnerObject.Cast<AbstractClass>(), Owner.OwnerRef.Owner, 100, warhead, 30, true);
                    pr.Ref.DetonateAndUnInit(location + new CoordStruct(0, 0, -Owner.OwnerRef.Base.GetHeight()));

                    //i++;

                    //var technos = ObjectFinder.FindTechnosNear(location, i < 5 ? i : 5 * Game.CellSize);
                    //foreach (var tech in technos)
                    //{
                    //    if(tech.CastToTechno(out var ptech))
                    //    {
                    //        if(!ptech.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner.Ref.Owner))
                    //        {
                    //            var pr = bulletType.Ref.CreateBullet(ptech.Cast<AbstractClass>(), Owner.OwnerRef.Owner, 100, warhead, 30, true);
                    //            pr.Ref.MoveTo(location, new BulletVelocity(0, 0, 100));
                    //            pr.Ref.SetTarget(ptech.Cast<AbstractClass>());
                    //        }
                    //    }
                    //}

                }
            }
        }
    }
}
