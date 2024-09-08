using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [ScriptAlias(nameof(AttachAnimScript))]
    [Serializable]
    public class AttachAnimScript : TechnoScriptable
    {
        public AttachAnimScript(TechnoExt owner) : base(owner)
        {
        }
    }

    [Serializable]
    public class AttachAnimConfig
    {
        public int Duration { get; set; }

        public bool OnlyVisibleToOwner { get; set; }

        public CoordStruct Location { get; set; }
    }
}
