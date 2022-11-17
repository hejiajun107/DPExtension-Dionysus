using Extension.Ext;
using Extension.Script;
using System;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(AdditionWeaponBulletScript))]
    public class AdditionWeaponBulletScript : BulletScriptable
    {
        public AdditionWeaponBulletScript(BulletExt owner) : base(owner)
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}
