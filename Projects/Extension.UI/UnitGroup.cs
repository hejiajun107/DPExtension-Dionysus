using DynamicPatcher;
using Extension.INI;
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

        // 单位 ID → 教程 PCX 列表 缓存（空列表表示未配置，已查过）
        private static readonly Dictionary<string, List<string>> TutorialCache =
            new Dictionary<string, List<string>>();

        // 无教程时的共享空列表（避免每帧分配）
        private static readonly List<string> EmptyTutorialList = new List<string>();

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
        //     获取单位类型的 ID（TypeClass 的 ID 字符串）。取不到时返回空字符串。
        public static string GetTypeId(Pointer<TechnoTypeClass> pType)
        {
            if (pType.IsNull)
            {
                return string.Empty;
            }
            string id = pType.Ref.BaseAbstractType.ID.ToString();
            return id ?? string.Empty;
        }

        //
        // 摘要:
        //     获取单位类型的教程图 PCX 列表（rules [单位ID] TutorialPCX=，逗号分隔，顺序即页序）。
        //     未配置 / 解析为空时返回空列表（该单位没有教程）。
        //     结果按单位 ID 缓存，不每帧读 INI。
        public static List<string> GetTutorialPCX(Pointer<TechnoTypeClass> pType)
        {
            if (pType.IsNull)
            {
                return EmptyTutorialList;
            }
            string id = pType.Ref.BaseAbstractType.ID.ToString();
            if (string.IsNullOrEmpty(id))
            {
                return EmptyTutorialList;
            }
            if (TutorialCache.TryGetValue(id, out List<string> cached))
            {
                return cached;
            }

            List<string> list;
            try
            {
                list = ParsePCXList(ReadRulesString(id, "TutorialPCX"));
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
                list = EmptyTutorialList;
            }
            TutorialCache[id] = list;
            return list;
        }

        //
        // 摘要:
        //     解析逗号分隔的 PCX 列表：去空白、去空项、缺 .pcx 后缀补上、去重。
        //     INI 的 ReadString 不会自动截断 ';' 注释，这里手动截掉。
        private static List<string> ParsePCXList(string raw)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(raw))
            {
                return list;
            }
            int comment = raw.IndexOf(';');
            if (comment >= 0)
            {
                raw = raw.Substring(0, comment);
            }
            foreach (string part in raw.Split(','))
            {
                string name = part.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                if (!name.EndsWith(".pcx", StringComparison.OrdinalIgnoreCase))
                {
                    name += ".pcx";
                }
                if (!list.Contains(name))
                {
                    list.Add(name);
                }
            }
            return list;
        }

        //
        // 摘要:
        //     取当前选中集合里**第一个**本地玩家的单位类型（按 CurrentObjects 顺序）。
        //     教程按钮的显示与打开都以这个类型为准：它没教程就不显示，即使后面还有别的类型配了教程。
        public static bool TryGetFirstLocalSelectedType(out Pointer<TechnoTypeClass> pType)
        {
            pType = Pointer<TechnoTypeClass>.Zero;
            try
            {
                var currentObjs = ObjectClass.CurrentObjects;
                for (int i = 0; i < currentObjs.Count; i++)
                {
                    var pObj = currentObjs[i];
                    if (pObj.IsNull)
                    {
                        continue;
                    }
                    var pTechno = pObj.Convert<TechnoClass>();
                    if (pTechno.IsNull)
                    {
                        continue;
                    }
                    // 只取本地玩家自己的对象：CurrentObjects 里可能混入其他玩家/中立对象
                    if (pTechno.Ref.Owner != HouseClass.Player)
                    {
                        continue;
                    }
                    var pSelectedType = pTechno.Ref.Type;
                    if (pSelectedType.IsNull)
                    {
                        continue;
                    }
                    pType = pSelectedType;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
            return false;
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
        //     从合并后的 rules 读取 [section] key=：地图附加 INI → 游戏模式 INI → rulesmd.ini
        //     （前面的优先）。这样写在地图 INI 里的标签也能生效，与引擎/Ares 的地图覆盖行为一致。
        //     合并读取失败时回退到引擎全局 rules INI。
        public static string ReadRulesString(string section, string key)
        {
            try
            {
                string dependency = GetMergedRulesDependency();
                string value = Ini.Get<string>(dependency, section, key, null);
                if (value != null)
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
            return ReadFromIni(CCINIClass.INI_Rules, section, key);
        }

        //
        // 摘要:
        //     取合并 rules 的依赖链（如 "地图.map->游戏模式.ini->rulesmd.ini"），
        //     并滤掉尚未就绪的空文件名，避免拼出 "->->" 这种空路径。
        private static string GetMergedRulesDependency()
        {
            string[] parts = Ini.SplitDependency(Ini.RulesDependency);
            var valid = new List<string>(parts.Length);
            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    valid.Add(part);
                }
            }
            return string.Join("->", valid);
        }

        //
        // 摘要:
        //     清空所有按单位 ID 的缓存。场景切换（换了地图）后必须调用，
        //     否则地图 INI 里的覆盖值会沿用上一张地图的结果。
        public static void ResetCaches()
        {
            Cache.Clear();
            CameoPCXCache.Clear();
            ArtSectionCache.Clear();
            HiddenCache.Clear();
            TutorialCache.Clear();
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
