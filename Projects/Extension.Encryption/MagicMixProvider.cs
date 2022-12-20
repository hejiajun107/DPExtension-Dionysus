using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;

namespace Extension.Encryption
{
    public class MagicMixProvider
    {

        static MagicMixProvider()
        {
            string keyPath = "./gamemd.key";
            if (File.Exists(keyPath))
            {
                using(StreamReader sr = new StreamReader(keyPath))
                {
                    var txt = sr.ReadToEnd();
                    encryptHeaders = JsonConvert.DeserializeObject<Dictionary<string, List<PackageEntry>>>(txt);
                }
            }
        }

        private static Dictionary<string, List<PackageEntry>> encryptHeaders = new Dictionary<string, List<PackageEntry>>();

        public static bool IsEncrypted(string fileName)
        {
            return encryptHeaders.ContainsKey(fileName.ToUpper());
        }

        public static List<PackageEntry> GetHeaders(string fileName)
        {
            if(encryptHeaders.ContainsKey(fileName.ToUpper()))
            {
                return encryptHeaders[fileName.ToUpper()];
            }
            else
            {
                return new List<PackageEntry>();
            }
        }
    }
}
