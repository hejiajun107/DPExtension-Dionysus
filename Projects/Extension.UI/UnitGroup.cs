using DynamicPatcher;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extension.UI
{
    //
    // 摘要:
    //     单位的 Ares 扩展 INI 属性读取工具（GroupAs / CameoPCX）。
    //     这些属性不在原版 TechnoTypeClass 内存布局中，只能从合并后的 rules INI 读取。
    //     GroupAs 语义：未显式指定时默认为单位自身 ID。
    //     CameoPCX 语义：非空时优先于原版 SHP cameo，用指定 PCX 作为单位图标。
    //     结果按单位 ID 缓存，避免每帧重复读 INI。
    //     联机安全：读取的是确定的游戏配置数据，所有机器一致。
    public static class UnitGroup
    {
        // 单位 ID → GroupAs 分组键 缓存
        private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>();

        // 单位 ID → CameoPCX 文件名 缓存（空字符串表示未配置，已查过）
        private static readonly Dictionary<string, string> CameoPCXCache = new Dictionary<string, string>();

        // 单位 ID → 解析后的 art 节名 缓存（rules [ID] Image=，缺省为 ID 本身）
        private static readonly Dictionary<string, string> ArtSectionCache = new Dictionary<string, string>();

        // 单位 ID → 是否应隐藏 缓存（Insignificant/DontScore）
        private static readonly Dictionary<string, bool> HiddenCache = new Dictionary<string, bool>();

        // 读 INI 用的复用缓冲区（游戏逻辑单线程，可复用静态缓冲区）
        private static readonly byte[] ReadBuffer = new byte[2048];

        //
        // 摘要:
        //     获取单位类型的分组键：
        //       1. 从合并 rules INI 读 [单位ID] GroupAs=，有值用该值；
        //       2. 无值回退单位自身 ID（Ares 默认行为）。
        //     ID 读不到时退化用 ArrayIndex 保证键唯一。
        public static string GetGroupKey(Pointer<TechnoTypeClass> pType)
        {
            if (pType.IsNull)
            {
                return string.Empty;
            }
            string id = pType.Ref.BaseAbstractType.ID.ToString();
            if (string.IsNullOrEmpty(id))
            {
                int idx = pType.Convert<AbstractClass>().Ref.GetArrayIndex();
                return "idx:" + idx;
            }
            if (Cache.TryGetValue(id, out string cached))
            {
                return cached;
            }

            string groupAs = ReadGroupAs(id);
            string key = string.IsNullOrEmpty(groupAs) ? id : groupAs;
            Cache[id] = key;
            return key;
        }

        //
        // 摘要:
        //     获取单位类型的 CameoPCX 文件名（Ares 扩展）。
        //     注意：CameoPCX 是 art INI 的键，位于 [Image] 节（artmd.ini），不在 rules：
        //       1. 从 rules [单位ID] Image= 取 art 节名（缺省为单位 ID 本身）；
        //       2. 从 art INI [art节名] CameoPCX= 取文件名。
        //     未配置时返回空字符串（回退 SHP cameo）。
        public static string GetCameoPCX(Pointer<TechnoTypeClass> pType)
        {
            string id;
            string artSection;
            if (!TryGetIdAndArtSection(pType, out id, out artSection))
            {
                return string.Empty;
            }
            if (CameoPCXCache.TryGetValue(id, out string cached))
            {
                return cached;
            }

            string pcx = ReadArtString(artSection, "CameoPCX");
            CameoPCXCache[id] = pcx;
            return pcx;
        }

        //
        // 摘要:
        //     是否应把该单位类型隐藏（不显示在 UnitFilterPanel）。
        //     从 rules INI 读 [单位ID] Insignificant= / DontScore=，任一为真即隐藏。
        //     这两个键在项目里没有内存字段（尤其 DontScore），统一从配置读取，避免猜偏移导致崩溃。
        public static bool IsHidden(Pointer<TechnoTypeClass> pType)
        {
            if (pType.IsNull)
            {
                return false;
            }
            string id = pType.Ref.BaseAbstractType.ID.ToString();
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            if (HiddenCache.TryGetValue(id, out bool cached))
            {
                return cached;
            }

            bool hidden = IsTruthy(ReadRulesString(id, "Insignificant"))
                       || IsTruthy(ReadRulesString(id, "DontScore"));
            HiddenCache[id] = hidden;
            return hidden;
        }

        //
        // 摘要:
        //     解析 INI 布尔值：yes/true/1 为真，其余（含空值）为假。
        private static bool IsTruthy(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            value = value.Trim();
            return string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                || value == "1";
        }

        //
        // 摘要:
        //     解析单位的 art 节名：rules [ID] Image= 优先，缺省用 ID。
        //     pType 为空或无 ID 时返回 false。
        public static bool TryGetIdAndArtSection(Pointer<TechnoTypeClass> pType, out string id, out string artSection)
        {
            id = string.Empty;
            artSection = string.Empty;
            if (pType.IsNull)
            {
                return false;
            }
            id = pType.Ref.BaseAbstractType.ID.ToString();
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            if (ArtSectionCache.TryGetValue(id, out artSection))
            {
                return true;
            }

            artSection = ReadRulesString(id, "Image");
            if (string.IsNullOrEmpty(artSection))
            {
                artSection = id;
            }
            ArtSectionCache[id] = artSection;
            return true;
        }

        //
        // 摘要:
        //     从 art INI（artmd.ini 合并结果）读取 [section] key=。
        public static string ReadArtString(string section, string key)
        {
            return ReadFromIni(CCINIClass.INI_Art, section, key);
        }

        //
        // 摘要:
        //     从 rules INI 读取 [section] key=。
        public static string ReadRulesString(string section, string key)
        {
            return ReadFromIni(CCINIClass.INI_Rules, section, key);
        }

        private static string ReadFromIni(Pointer<CCINIClass> pINI, string section, string key)
        {
            try
            {
                if (pINI.IsNull)
                {
                    return string.Empty;
                }
                int ret = pINI.Ref.ReadString(section, key, "", ReadBuffer, ReadBuffer.Length);
                if (ret <= 0)
                {
                    return string.Empty;
                }
                string str = Encoding.Default.GetString(ReadBuffer);
                int end = str.IndexOf('\0');
                if (end >= 0)
                {
                    str = str.Substring(0, end);
                }
                return str.Trim();
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
                return string.Empty;
            }
        }

        //
        // 摘要:
        //     从合并后的 rules INI 读取 [section] GroupAs=。
        private static string ReadGroupAs(string section)
        {
            return ReadRulesString(section, "GroupAs");
        }
    }
}
