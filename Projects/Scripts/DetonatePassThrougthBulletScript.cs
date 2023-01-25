using DynamicPatcher;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [ScriptAlias(nameof(DetonatePassThrougthBulletScript))]
    [Serializable]
    public class DetonatePassThrougthBulletScript : BulletScriptable
    {
        public DetonatePassThrougthBulletScript(BulletExt owner) : base(owner)
        {
        }

        private INIComponentWith<DetonatePassThrougthBulletData> ini;

        public override void Awake()
        {
            ini = GameObject.CreateRulesIniComponentWith<DetonatePassThrougthBulletData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            base.Awake();
        }

        private bool exploded = false;
        public override void OnUpdate()
        {
            if (!exploded)
            {
                exploded = true;

                if (Owner.OwnerObject.Ref.Owner.IsNull)
                    return;

                var start = Owner.OwnerObject.Ref.SourceCoords;
                var target = Owner.OwnerObject.Ref.TargetCoords;
                var weapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(ini.Data.PassThrougthWeapon);
                if (weapon.IsNull)
                    return;

                var s = weapon.Ref.Range;

                var flipX = target.X > start.X ? 1 : -1;
                var flipY = target.Y > start.Y ? 1 : -1;

                var cita = Math.Atan(Math.Abs((target.Y - start.Y) / (target.X - start.X)));

                for (var i = 1; i <= s / Game.CellSize; i++)
                {
                    var cs = i * Game.CellSize;
                    var dest = new CoordStruct((start.X + (int)(cs * Math.Cos(cita) * flipX)), start.Y + (int)(cs * Math.Sin(cita)) * flipY, start.Z);
                    var bullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisble").Ref.CreateBullet(Owner.OwnerObject.Ref.Owner.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner, weapon.Ref.Damage, weapon.Ref.Warhead, 100, false);
                    bullet.Ref.DetonateAndUnInit(dest);
                }
            }
        }

    }

    [Serializable]
    public class DetonatePassThrougthBulletData : INIAutoConfig
    {
        [INIField(Key = "PassThrougth.Weapon")]
        public string PassThrougthWeapon;

    }
}
