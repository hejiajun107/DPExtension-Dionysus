using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Encryption
{
    public class PackageEntry
    {
        public uint Hash { get; set; }
        public uint Offset { get; set; }
        public uint Length { get; set; }

        public MixHeaderData CastToHeader()
        {
            return new MixHeaderData()
            {
                ID = Hash,
                Offset = Offset,
                Size = Length
            };
        }
    }


}
