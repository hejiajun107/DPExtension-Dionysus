using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Soviet
{
    [ScriptAlias(nameof(TechnoScriptable))]
    [Serializable]
    public class SovietRocketScript : TechnoScriptable
    {
        public SovietRocketScript(TechnoExt owner) : base(owner)
        {
        }

        private int rof = 100;

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
                rof = 100;
                for (var i = 0; i < 2; i++)
                {
                    var pbolt = Owner.OwnerObject.Ref.Electric_Zap(Owner.OwnerObject.Convert<AbstractClass>(), WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ChargedElec1"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    var eSource = ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, new CoordStruct(-152, 64 * (i == 0 ? 1 : -1), 120), false);
                    var eTarget = ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, new CoordStruct(-88, 64 * (i == 0 ? 1 : -1), 120), false);
                    pbolt.Ref.Point1 = eSource + new CoordStruct(MathEx.Random.Next(-50, 50), MathEx.Random.Next(-50, 50), 0);
                    pbolt.Ref.Point2 = eTarget + new CoordStruct(MathEx.Random.Next(-50, 50), MathEx.Random.Next(-50, 50), 0);
                }
            }
        }
    }
}
