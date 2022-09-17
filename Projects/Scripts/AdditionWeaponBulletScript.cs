using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
