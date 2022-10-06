using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(BunkerWallScript))]
    [Serializable]
    public class BunkerWallScript : TechnoScriptable
    {
        public BunkerWallScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            var building = Owner.OwnerObject.Convert<BuildingClass>();
            //Logger.Log("wall:" + building.Ref.GetFWFlags());
        }
    }
}
 