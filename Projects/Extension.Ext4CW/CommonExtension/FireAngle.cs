using Extension.INI;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.CW
{
    public partial class TechnoGlobalExtension
    {
        public partial bool CanFire(Pointer<AbstractClass> pTarget, Pointer<WeaponTypeClass> pWeapon)
        {
            if(!INI.Data.FixedFiringAngle)
                return true;

            var myFacing = Owner.OwnerObject.Ref.Facing.target().GetValue();

            var targetCoord = pTarget.Ref.GetCoords();
            var myCoord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var dy = Math.Abs(targetCoord.Y - myCoord.Y);
            var dl = targetCoord.BigDistanceForm(myCoord);
            if (dl == 0)
                return true;
            var angle = (180/3.14 * Math.Asin(dy / dl));
            return Math.Abs(((myFacing) / ((short.MaxValue - short.MinValue) / 360)) - angle) <= INI.Data.FixedAnlgeRange;
        }
    }

    public partial class TechnoGlobalTypeExt
    {
        [INIField(Key = "Firing.FixedAngle")]
        public bool FixedFiringAngle = false;

        [INIField(Key = "Firing.FixedAnlgeRange")]
        public int FixedAnlgeRange = 0;
    }
}
