using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    [ScriptAlias(nameof(LaserTrailerBulletScirpt))]
    public class LaserTrailerBulletScirpt : BulletScriptable
    {
        public LaserTrailerBulletScirpt(BulletExt owner) : base(owner)
        {
        }

        public CoordStruct? lastLocation;

        public INIComponentWith<LaserTrailerBulletData> ini;

        public override void Awake()
        {
            ini = GameObject.CreateRulesIniComponentWith<LaserTrailerBulletData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
        }

        public override void OnUpdate()
        {
            var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            if (lastLocation is null)
            {
                lastLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            }

            var distance = ini.Data.IgnoreVertical ? 
                (int)Math.Round(Math.Sqrt(Math.Pow(currentLocation.X - lastLocation.Value.X, 2) + Math.Pow(currentLocation.Y - lastLocation.Value.Y, 2))) : 
                (int)Math.Round(currentLocation.BigDistanceForm(lastLocation.Value));

            if (distance >= ini.Data.SegmentLength)
            {
                var techno = Owner.OwnerObject.Ref.Owner;

                //Owner挂了的话随便抓个壮丁
                if(techno.IsNull && techno.Ref.Base.IsAlive)
                {
                    techno = TechnoClass.Array.FirstOrDefault();
                }

                if(techno.IsNull || !techno.Ref.Base.IsAlive)
                {
                    //AI说可能会出现单位狗带但是数组里还有的情况
                    techno = TechnoClass.Array.FirstOrDefault(t =>
                       t.IsNotNull && t.Ref.Base.IsAlive
                   );
                }

                if(techno.IsNotNull)
                {
                    if (lastLocation.Value.X == 0 && lastLocation.Value.Y == 0 && lastLocation.Value.Z == 0)
                    {
                        lastLocation = new CoordStruct(1, 1, 0);
                    }
                    techno.Ref.CreateLaser(Owner.OwnerObject.Convert<ObjectClass>(), 0, ini.Data.Weapon, lastLocation.Value);
                }

                lastLocation = currentLocation;
            }
        }
    }

    public class LaserTrailerBulletData : INIAutoConfig
    {
        [INIField(Key = "LaserTrailer.Weapon")]
        public Pointer<WeaponTypeClass> Weapon;
        [INIField(Key = "LaserTrailer.SegmentLength")]
        public int SegmentLength;
        [INIField(Key = "LaserTrailer.IgnoreVertical")]
        public bool IgnoreVertical = false;
    }
}
