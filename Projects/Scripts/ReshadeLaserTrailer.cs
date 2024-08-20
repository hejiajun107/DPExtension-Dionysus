using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    [ScriptAlias(nameof(ReshadeLaserTrail))]
    public class ReshadeLaserTrail : BulletScriptable
    {
        public ReshadeLaserTrail(BulletExt owner) : base(owner)
        {
        }

        public override void Awake()
        {
            var ini = this.CreateRulesIniComponentWith<ReshadeLaserTrailerData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            weapon = ini.Data.Weapon;
            distance = ini.Data.Distance;
        }

        private string weapon;
        private int distance;

        CoordStruct lastLocation;
        public override void OnUpdate()
        {
            if(lastLocation == null)
            {
                lastLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            }

            var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            if(currentLocation.DistanceFrom(lastLocation) > distance)
            {
                if (Owner.OwnerObject.Ref.Owner.IsNotNull)
                {
                    Owner.OwnerObject.Ref.Owner.Ref.CreateLaser(Owner.OwnerObject.Convert<ObjectClass>(), 0, WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weapon), lastLocation);
                }

                lastLocation = currentLocation;
            }
        }
    }


    public class ReshadeLaserTrailerData : INIAutoConfig
    {
        [INIField(Key = "ReshadeLaserTrail.Weapon")]
        public string Weapon;
        [INIField(Key = "ReshadeLaserTrail.Distance")]
        public int Distance;

    }
}
