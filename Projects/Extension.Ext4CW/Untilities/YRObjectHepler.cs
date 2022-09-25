using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Ext4CW.Untilities
{
    public static class YRObjectHepler
    {
        public static CoordStruct ToCoordStruct(this SingleVector3D vector3D)
        {
            return new CoordStruct((int)vector3D.X, (int)vector3D.Y, (int)vector3D.Z);
        }

        public static SingleVector3D ToSingleVector3D(this CoordStruct coord)
        {
            return new SingleVector3D(coord.X, coord.Y, coord.Z);
        }
    }
}
