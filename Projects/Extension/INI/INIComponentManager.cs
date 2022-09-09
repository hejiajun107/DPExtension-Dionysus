﻿using Extension.EventSystems;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Extension.INI
{
    internal static class INIComponentManager
    {
        public static string GetDependency(string iniName)
        {
            if (iniName.Equals(INIConstant.RulesName, StringComparison.OrdinalIgnoreCase))
            {
                if (SessionClass.Instance.GameMode == GameMode.Campaign)
                {
                    return GetMergedName(INIConstant.MapName, INIConstant.RulesName);
                }

                return GetMergedName(INIConstant.MapName, INIConstant.GameModeName, INIConstant.RulesName);
            }

            return iniName;
        }

        private static string GetMergedName(params string[] names)
        {
            return string.Join("->", names);
        }


        static INIComponentManager()
        {
            EventSystem.General.AddPermanentHandler(EventSystem.General.ScenarioClearClassesEvent, ScenarioClearClassesEventHandler);
        }

        private static void ScenarioClearClassesEventHandler(object sender, EventArgs e)
        {
            ClearBuffer();
        }

        private static Dictionary<string, INIFileBuffer> s_File = new();

        private static INIFileBuffer FindFile(string name)
        {
            if (!s_File.TryGetValue(name, out INIFileBuffer buffer))
            {
                buffer = new INIFileBuffer(name);

                s_File[name] = buffer;
            }

            return buffer;
        }

        internal static INIBuffer FindBuffer(string name, string section)
        {
            return FindFile(name).GetSection(section);
        }


        private static Dictionary<(string dependency, string section), INILinkedBuffer> s_LinkedBuffer = new();
        private static Dictionary<INILinkedBuffer, INIConfig> s_Config = new();

        internal static INILinkedBuffer FindLinkedBuffer(string dependency, string section)
        {
            if (!s_LinkedBuffer.TryGetValue((dependency, section), out INILinkedBuffer linkedBuffer))
            {
                string[] names = dependency.Replace("->", "+").Split('+');
                foreach (string name in names.Reverse())
                {
                    var buffer = FindBuffer(name, section);
                    linkedBuffer = new INILinkedBuffer(buffer, linkedBuffer);
                }

                s_LinkedBuffer[(dependency, section)] = linkedBuffer;
            }
            return linkedBuffer;
        }

        internal static T FindConfig<T>(INILinkedBuffer linkedBuffer, INIBufferReader reader) where T : INIConfig, new()
        {
            if (!s_Config.TryGetValue(linkedBuffer, out INIConfig config))
            {
                config = new T();
                config.Read(reader);

                s_Config[linkedBuffer] = config;
            }

            return (T)config;
        }

        /// <summary>
        /// clear all parsed and unparsed buffer
        /// </summary>
        public static void ClearBuffer()
        {
            foreach (INILinkedBuffer linkedBuffer in s_LinkedBuffer.Values)
            {
                linkedBuffer.Expired = true;
            }

            s_File.Clear();
            s_LinkedBuffer.Clear();
            s_Config.Clear();
        }
    }
}
