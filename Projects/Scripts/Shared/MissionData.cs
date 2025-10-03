using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Shared
{
    public class MissionDataHelper
    {
        public static void Save(MissionData data)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(MISSION_SAVE_PATH, false))
                {
                    var json = JsonConvert.SerializeObject(data);
                    var str = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
                    sw.Write(str);
                }
            }
            catch (Exception) { }   
        }

        private const string MISSION_SAVE_PATH = "./Client/mission_saved";

        public static MissionData Load()
        {
            if(File.Exists(MISSION_SAVE_PATH))
            {
                try
                {
                    using (StreamReader sw = new StreamReader(MISSION_SAVE_PATH))
                    {
                        var str = sw.ReadToEnd();
                        byte[] decodedBytes = Convert.FromBase64String(str);
                        string decodedString = Encoding.UTF8.GetString(decodedBytes);

                        return JsonConvert.DeserializeObject<MissionData>(decodedString) ?? new MissionData();
                    }
                }catch(Exception ex)
                {
                    return new MissionData();
                }
            }
            else
            {
                return new MissionData();
            }
        }
    }

    [Serializable]
    public class MissionData
    {
        public DataAll08 DataAll08 { get; set; } = new DataAll08();
    }

    [Serializable]
    public class DataAll08
    {
        /// <summary>
        /// 拥有金钱
        /// </summary>
        public int Cash { get; set; } = 1;

        /// <summary>
        /// 找到的小队数
        /// </summary>
        public int FindTeams { get; set; } = 0;

        /// <summary>
        /// 是否触发空中支援
        /// </summary>
        public bool AirForceActited { get; set; } = false;
    }
}
