using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Yuri
{
    [ScriptAlias(nameof(SuperBruteScript))]
    [Serializable]
    public class SuperBruteScript : TechnoScriptable
    {
        public SuperBruteScript(TechnoExt owner) : base(owner)
        {

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);
        }
    }

    [ScriptAlias(nameof(PushedByBruteEffect))]
    [Serializable]
    public class PushedByBruteEffect : TechnoScriptable
    {
        public PushedByBruteEffect(TechnoExt owner) : base(owner)
        {
        }
    }
}
