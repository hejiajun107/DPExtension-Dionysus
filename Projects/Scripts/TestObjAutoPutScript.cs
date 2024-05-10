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
    public class TestObjAutoPutScript : TechnoScriptable
    {
        public TestObjAutoPutScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            //var side = Owner.OwnerObject.Ref.Type.Ref.
            //base.OnPut(coord, faceDir);
        }
    }
}
