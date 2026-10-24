using DynamicPatcher;
using PatcherYRpp;
using System;

namespace Extension.UI
{
    //
    // 摘要:
    //     教程面板的全局配置（rules INI [TutorialUI] 节）。
    //     全部懒加载 + 静态缓存；缺键或解析失败时使用默认值，不抛异常。
    //     联机安全：只读载入期配置，所有客户端一致。
    public static class TutorialConfig
    {
        private const string Section = "TutorialUI";

        // 默认值
        private const int DefaultTransitionFPS = 15;
        private const int DefaultButtonWidth = 24;
        private const int DefaultButtonHeight = 24;
        private const int DefaultFooterHeight = 32;

        private static bool _loaded;

        private static string _transitionSHP = string.Empty;
        private static int _transitionFPS = DefaultTransitionFPS;
        private static string _transitionPalette = string.Empty;
        private static string _prevPCX = string.Empty;
        private static string _nextPCX = string.Empty;
        private static string _closePCX = string.Empty;
        private static int _buttonWidth = DefaultButtonWidth;
        private static int _buttonHeight = DefaultButtonHeight;
        private static int _footerHeight = DefaultFooterHeight;

        // 过渡动画调色板（解析一次后缓存）
        private static Pointer<ConvertClass> _palette;
        private static bool _paletteResolved;

        //
        // 摘要:
        //     过渡动画 SHP 文件名（已补 .shp 后缀）。空表示跳过过渡动画。
        public static string TransitionSHP { get { EnsureLoaded(); return _transitionSHP; } }

        //
        // 摘要:
        //     过渡动画帧率（帧/秒）。
        public static int TransitionFPS { get { EnsureLoaded(); return _transitionFPS; } }

        //
        // 摘要:
        //     上一页 / 下一页 / 关闭 按钮的 PCX 文件名（已补 .pcx 后缀）。空表示用占位方块。
        public static string PrevPCX { get { EnsureLoaded(); return _prevPCX; } }
        public static string NextPCX { get { EnsureLoaded(); return _nextPCX; } }
        public static string ClosePCX { get { EnsureLoaded(); return _closePCX; } }

        //
        // 摘要:
        //     按钮尺寸与底部按钮条高度。
        public static int ButtonWidth { get { EnsureLoaded(); return _buttonWidth; } }
        public static int ButtonHeight { get { EnsureLoaded(); return _buttonHeight; } }
        public static int FooterHeight { get { EnsureLoaded(); return _footerHeight; } }

        //
        // 摘要:
        //     丢弃已读取的配置，下次访问时重新从合并 INI 读取。
        //     场景切换（换地图，地图 INI 里的 [TutorialUI] 可能不同）后必须调用。
        public static void Reset()
        {
            _loaded = false;
            _transitionSHP = string.Empty;
            _transitionFPS = DefaultTransitionFPS;
            _transitionPalette = string.Empty;
            _prevPCX = string.Empty;
            _nextPCX = string.Empty;
            _closePCX = string.Empty;
            _buttonWidth = DefaultButtonWidth;
            _buttonHeight = DefaultButtonHeight;
            _footerHeight = DefaultFooterHeight;
            _paletteResolved = false;
            _palette = Pointer<ConvertClass>.Zero;
        }

        //
        // 摘要:
        //     解析过渡动画调色板：留空或就是 cameo.pal 时直接用引擎已加载的 CAMEO_PAL，
        //     其余情况按文件名加载；加载失败回退 CAMEO_PAL。只解析一次。
        public static Pointer<ConvertClass> ResolveTransitionPalette()
        {
            if (_paletteResolved)
            {
                return _palette;
            }
            _paletteResolved = true;

            try
            {
                EnsureLoaded();
                if (string.IsNullOrEmpty(_transitionPalette)
                    || string.Equals(_transitionPalette, "cameo.pal", StringComparison.OrdinalIgnoreCase))
                {
                    _palette = FileSystem.CAMEO_PAL;
                    return _palette;
                }

                Pointer<ConvertClass> pPalette = ConvertClass.CreateFromFile(_transitionPalette);
                _palette = pPalette.IsNull ? FileSystem.CAMEO_PAL : pPalette;
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
                _palette = FileSystem.CAMEO_PAL;
            }
            return _palette;
        }

        //
        // 摘要:
        //     从 [TutorialUI] 读取全部配置。只读一次。
        private static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }
            _loaded = true;

            try
            {
                _transitionSHP = EnsureSuffix(ReadValue("TransitionSHP"), ".shp");
                _transitionFPS = ReadInt("TransitionFPS", DefaultTransitionFPS);
                if (_transitionFPS <= 0)
                {
                    _transitionFPS = DefaultTransitionFPS;
                }

                _transitionPalette = ReadValue("TransitionPalette");

                _prevPCX = EnsureSuffix(ReadValue("PrevPCX"), ".pcx");
                _nextPCX = EnsureSuffix(ReadValue("NextPCX"), ".pcx");
                _closePCX = EnsureSuffix(ReadValue("ClosePCX"), ".pcx");

                _buttonWidth = ReadInt("ButtonWidth", DefaultButtonWidth);
                _buttonHeight = ReadInt("ButtonHeight", DefaultButtonHeight);
                _footerHeight = ReadInt("FooterHeight", DefaultFooterHeight);
                if (_buttonWidth <= 0) _buttonWidth = DefaultButtonWidth;
                if (_buttonHeight <= 0) _buttonHeight = DefaultButtonHeight;
                if (_footerHeight < _buttonHeight) _footerHeight = _buttonHeight;
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     读取一个字符串值：去空白，截掉 ';' 行内注释（ReadString 不会自动截断）。
        private static string ReadValue(string key)
        {
            string value = UnitGroup.ReadRulesString(Section, key);
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            int comment = value.IndexOf(';');
            if (comment >= 0)
            {
                value = value.Substring(0, comment);
            }
            return value.Trim();
        }

        private static int ReadInt(string key, int defaultValue)
        {
            string value = ReadValue(key);
            return int.TryParse(value, out int parsed) ? parsed : defaultValue;
        }

        //
        // 摘要:
        //     补文件后缀（不区分大小写），已有则原样返回。
        private static string EnsureSuffix(string name, string suffix)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            return name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) ? name : name + suffix;
        }
    }
}
