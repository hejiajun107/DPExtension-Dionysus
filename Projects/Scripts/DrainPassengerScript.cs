using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(DrainPassenger))]

    public class DrainPassenger : TechnoScriptable
    {
        public DrainPassenger(TechnoExt owner) : base(owner) 
        { 

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {

            var my = Owner.OwnerObject.Ref;

            if (pTarget.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                //var total = my.Passengers.GetTotalSize();
                //if (my.Passengers.NumPassengers >= 2)
                //{
                //    my.Passengers.RemoveFirstPassenger();
                //}
                my.Passengers.AddPassenger(new Pointer<FootClass>(pTechno));
            }
            
        }

    }
}
