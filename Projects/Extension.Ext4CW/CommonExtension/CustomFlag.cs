﻿using Extension.INI;
using Extension.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.CW
{
    public partial class TechnoGlobalTypeExt
    {
        public bool isCustomFlagLoaded = false;

        [INIField(Key = "IsHero")]
        public bool IsHero = false;

        [INIField(Key = "IsEpicUnit")]
        public bool IsEpicUnit = false;

        [INIField(Key = "Harvester")]
        public bool IsHarvester = false;

        [INIField(Key = "CanMk2Update")]
        public bool CanMk2Update = false;

        [INIField(Key = "Script.Args")]
        public string ScriptArgs = "";

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="reader"></param>
        ///// <param name="section"></param>
        //private void ReadCustomFlagData(INIReader reader, string section)
        //{
        //    if (!isCustomFlagLoaded)
        //    {
        //        isCustomFlagLoaded = true;

        //        reader.ReadNormal(section, "IsHero", ref IsHero);

        //        reader.ReadNormal(section, "IsEpicUnit", ref IsEpicUnit);

        //        reader.ReadNormal(section, "Harvester", ref IsHarvester);

        //        reader.ReadNormal(section, "CanMk2Update", ref CanMk2Update);
        //    }

        //}
    }
}
