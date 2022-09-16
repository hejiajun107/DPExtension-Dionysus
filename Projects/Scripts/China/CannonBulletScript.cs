using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.China
{
    [Serializable]
    public class CannonBulletScript : BulletScriptable
    {
        public CannonBulletScript(BulletExt owner) : base(owner) { }

        private bool Inited = false;

        private List<TechnoExt> Technos;


        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!Inited)
            {
                Inited = true;

                if (!Owner.OwnerObject.Ref.Owner.IsNull)
                {
                    if (!Owner.OwnerObject.Ref.Owner.Ref.Owner.IsNull)
                    {
                        Technos = Finder.FindTechno(Owner.OwnerObject.Ref.Owner.Ref.Owner, t => t.Ref.Type.Ref.Base.Base.ID == "NCANNON", FindRange.Owner);
                    }
                }
                return;
            }

            if (Technos != null && Technos.Count() > 0)
            {
                var target = Owner.OwnerObject.Ref.Target;

                for (var i = Technos.Count - 1; i >= 0; i--)
                {
                    var techno = Technos[i];
                    if (techno.TryGet(out var technoExt))
                    {
                        var pTechno = technoExt.OwnerObject;
                        if (pTechno.Ref.GetFireErrorWithoutRange(target, 0) == FireError.OK)
                        {
                            FireViaPassenger(pTechno, target);
                            pTechno.Ref.SetTarget(default);
                            Technos.Remove(techno);
                            continue;
                        }
                        pTechno.Ref.SetTarget(target);
                    }
                    else
                    {
                        Technos.Remove(techno);
                    }
                }
            }
            else
            {
                Owner.OwnerObject.Ref.Base.Remove();
                Owner.OwnerObject.Ref.Base.UnInit();
            }
        }

        //public override void OnRemove()
        //{
        //    Technos = null;
        //    base.OnRemove();
        //}


        private void FireViaPassenger(Pointer<TechnoClass> pTechno,Pointer<AbstractClass> target)
        {
            var passenger = pTechno.Ref.Passengers.GetFirstPassenger();
            if (passenger.IsNull)  return; 
            if (passenger.Convert<AbstractClass>().CastToTechno(out Pointer<TechnoClass> pPassenger))
            {
                pPassenger.Ref.Fire(target, 0);
            }
        }

    }
}
