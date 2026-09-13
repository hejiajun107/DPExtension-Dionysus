using DynamicPatcher;
using Extension.UI.Core;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Extension.UI
{
    //
    // 摘要:
    //     超武面板容器。从 INI [SWPanel] 读取配置的超武 ID 列表，
    //     从当前玩家（HouseClass.Player）的超武列表中匹配，创建 SuperWeaponButton
    //     并横向排列在屏幕左侧（ViewBound 左边缘）、底部。
    public class SuperWeaponPanel : Panel
    {
        //
        // 摘要:
        //     配置的超武 ID 列表（来自 INI [SWPanel] SWList，逗号分隔）。
        private List<string> swIds = new List<string>();

        //
        // 摘要:
        //     ID -> Description 映射。
        //     填充顺序：
        //       1. 读 [SWID] UIDescription=（CSF 标签）→ 调 StringTable.LoadString 查到 desc 文本；
        //       2. 用 [SWPanel.SWID] Description= 作为覆盖（直接文本）。
        private Dictionary<string, string> descriptions = new Dictionary<string, string>();

        //
        // 摘要:
        //     ID -> SidebarPCX 文件名 映射（来自 INI [SWID] SidebarPCX=）。
        //     非空时按钮强制用此 PCX 绘制（覆盖 SHP）。
        private Dictionary<string, string> sidebarPCXs = new Dictionary<string, string>();

        // 按钮默认尺寸（与 SuperWeaponButton.DefaultWidth/DefaultHeight 一致）
        private const int ButtonWidth = 60;
        private const int ButtonHeight = 48;

        //
        // 摘要:
        //     上一次匹配到的超武指针列表，用于检测玩家超武增减。
        private List<Pointer<SuperClass>> _lastSupers = new List<Pointer<SuperClass>>();

        //
        // 摘要:
        //     上一次的 ViewBound，用于检测屏幕区域变化以重新定位。
        private RectangleStruct _lastViewBound;

        //
        // 摘要:
        //     INI 配置是否已加载（懒加载标志）。
        private bool _configLoaded;

        public SuperWeaponPanel()
        {
            Orientation = PanelOrientation.Horizontal;
            ClientRectangle = new RectangleStruct(0, 0, 0, ButtonHeight);
        }

        //
        // 摘要:
        //     从 INI 读取 [SWPanel] 配置：
        //       SWList    —— 逗号分隔的超武 ID 列表；
        //       Interval  —— 按钮间距，默认 2；
        //       [SWPanel.SWxxx] Description —— 单个超武的描述文本。
        //     读取完成后标记 _configLoaded，供 OnUpdate 懒加载使用。
        public void LoadFromINI(Pointer<CCINIClass> pINI)
        {
            swIds.Clear();
            descriptions.Clear();
            sidebarPCXs.Clear();

            if (pINI.IsNull)
            {
                return;
            }

            try
            {
                // 读取 SWList（逗号分隔的超武 ID）
                string swList = ReadIniString(pINI, "SWPanel", "SWList");
                if (!string.IsNullOrEmpty(swList))
                {
                    foreach (string part in swList.Split(','))
                    {
                        string id = part.Trim();
                        if (!string.IsNullOrEmpty(id) && !swIds.Contains(id))
                        {
                            swIds.Add(id);
                        }
                    }
                }
                // 读取 Interval（按钮间距，默认 2）
                string intervalStr = ReadIniString(pINI, "SWPanel", "Interval");
                int interval = 2;
                if (int.TryParse(intervalStr, out int parsed))
                {
                    interval = parsed;
                }
                Interval = interval;

                // 读取每个超武的 SidebarPCX / Description
                foreach (string id in swIds)
                {
                    // 1) SidebarPCX= 来自超武节，替换 %Side% 为当前阵营序号
                    string pcxName = ReadIniString(pINI, id, "SidebarPCX");
                    if (!string.IsNullOrEmpty(pcxName))
                    {
                        int sideIndex = HouseClass.Player.Ref.Type.Ref.SideIndex;
                        pcxName = pcxName.Replace("%Side%", sideIndex.ToString());
                    }
                    sidebarPCXs[id] = pcxName;

                    // 2) Description：
                    //    a. 先读 [SWID] UIDescription= → 视为 CSF 标签，调用 StringTable.LoadString 查真正 desc 文本
                    //    b. 再用 [SWPanel.SWID] Description= 覆盖（直接文本）
                    string desc = string.Empty;
                    string csfLabel = ReadIniString(pINI, id, "UIDescription");
                    if (!string.IsNullOrEmpty(csfLabel))
                    {
                        string resolved = LoadCSFString(csfLabel);
                        if (!string.IsNullOrEmpty(resolved))
                        {
                            desc = resolved;
                        }
                    }
                    string overrideDesc = ReadIniString(pINI, "SWPanel." + id, "Description");
                    if (!string.IsNullOrEmpty(overrideDesc))
                    {
                        desc = overrideDesc;
                    }
                    descriptions[id] = desc;
                }

                _configLoaded = true;
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     调用游戏原生 StringTable::LoadString @ 0x734E60 查询 CSF 标签对应的文本。
        //     查不到（返回空或以 MISSING: 开头）时返回 string.Empty。
        private static unsafe string LoadCSFString(string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                return string.Empty;
            }
            try
            {
                // const wchar_t* __fastcall StringTable::LoadString(
                //     const char* pLabel,  // ECX
                //     char* pExtraData,   // EDX (nullptr)
                //     const char* pFile,  // stack
                //     int nLine           // stack
                // );
                // 通过 FastCallTransferStation：首参 = 地址，后续依次填 ECX / EDX / stack...
                IntPtr ansiLabel = AnsiStringToHGlobal(label);
                try
                {
                    var func = (delegate* unmanaged[Thiscall]<int, IntPtr, IntPtr, IntPtr, int, IntPtr>)ASM.FastCallTransferStation;
                    IntPtr result = func(0x734E60, ansiLabel, IntPtr.Zero, IntPtr.Zero, 0);
                    if (result == IntPtr.Zero)
                    {
                        return string.Empty;
                    }
                    string text = Marshal.PtrToStringUni(result);
                    // 原生对未命中 label 返回 "MISSING:<label>" 字符串
                    if (!string.IsNullOrEmpty(text) && text.StartsWith("MISSING:", StringComparison.Ordinal))
                    {
                        return string.Empty;
                    }
                    return text ?? string.Empty;
                }
                finally
                {
                    Marshal.FreeHGlobal(ansiLabel);
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private static IntPtr AnsiStringToHGlobal(string s)
        {
            if (s == null) return IntPtr.Zero;
            byte[] bytes = Encoding.Default.GetBytes(s);
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr, bytes.Length, 0);
            return ptr;
        }

        // 实例字段 buffer，参考 INIFileReader 的做法
        private byte[] readBuffer = new byte[2048];

        //
        // 摘要:
        //     从 CCINIClass 读取一个字符串值。参考 INIFileReader.ReadString 的实现。
        //     返回值 > 0 表示成功（写入字节数），= 0 表示 key 不存在。
        private string ReadIniString(Pointer<CCINIClass> pINI, string section, string key)
        {
            int ret = pINI.Ref.ReadString(section, key, "", readBuffer, readBuffer.Length);
            if (ret <= 0)
            {
                return string.Empty;
            }
            string str = Encoding.Default.GetString(readBuffer);
            str = str.Substring(0, str.IndexOf('\0'));
            return str.Trim();
        }

        //
        // 摘要:
        //     重写 Arrange：忽略父级 finalRect，根据 Surface.ViewBound 计算绝对位置——
        //     横向排列在屏幕左侧、底部：
        //       X = ViewBound 左边缘
        //       Y = ViewBound 底部 - 按钮高度
        //     面板宽度 = 按钮数 * ButtonWidth + (按钮数 - 1) * Interval。
        //     设置 ClientRectangle 后由 Panel.ArrangeChildren 横向排列按钮。
        public override RectangleStruct Arrange(RectangleStruct finalRect)
        {
            RectangleStruct view = Surface.ViewBound;

            int count = _children.Count;
            int width = count > 0 ? count * ButtonWidth + (count - 1) * Interval : 0;

            int x = view.X;
            int y = view.Y + view.Height - ButtonHeight;

            RectangleStruct absolute = new RectangleStruct(x, y, width, ButtonHeight);
            ClientRectangle = absolute;
            ArrangeChildren(ClientRectangle);
            return ClientRectangle;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            try
            {
                // 懒加载：首次更新时从 Rules INI 读取配置
                if (!_configLoaded)
                {
                    LoadFromINI(CCINIClass.INI_Rules);
                }

                // 检测玩家超武列表变化，增删按钮
                RebuildButtons();

                // 检测 ViewBound 变化，触发重新布局以更新位置
                RectangleStruct view = Surface.ViewBound;
                if (view != _lastViewBound)
                {
                    _lastViewBound = view;
                    _isDirty = true;
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     遍历配置的超武 ID（swIds），按顺序匹配当前玩家的超武：
        //       1. SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(id) 按字符串 ID 查找超武类型；
        //       2. HouseClass.Player.FindSuperWeapon(pType) 获取玩家的超武实例。
        //     当匹配结果（数量或指针）变化时，清空并重建按钮列表。
        private void RebuildButtons()
        {
            if (!_configLoaded || swIds.Count == 0)
            {
                if (_lastSupers.Count > 0)
                {
                    _lastSupers.Clear();
                    ClearChildren();
                }
                return;
            }

            Pointer<HouseClass> pPlayer = HouseClass.Player;
            if (pPlayer.IsNull)
            {
                return;
            }

            List<Pointer<SuperClass>> currentSupers = new List<Pointer<SuperClass>>();
            List<string> currentDescs = new List<string>();
            List<string> currentPCXs = new List<string>();

            foreach (string id in swIds)
            {
                Pointer<SuperWeaponTypeClass> pType = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(id);
                if (pType.IsNull)
                {
                    continue;
                }

                Pointer<SuperClass> pSuper = pPlayer.Ref.FindSuperWeapon(pType);
                if (pSuper.IsNull || !pSuper.Ref.Granted)
                {
                    continue;
                }

                currentSupers.Add(pSuper);
                string desc = descriptions.TryGetValue(id, out string d) ? d : string.Empty;
                currentDescs.Add(desc);
                string pcx = sidebarPCXs.TryGetValue(id, out string p) ? p : string.Empty;
                currentPCXs.Add(pcx);
            }

            // 检测变化：数量不同或任一指针不同则重建
            bool changed = currentSupers.Count != _lastSupers.Count;
            if (!changed)
            {
                for (int i = 0; i < currentSupers.Count; i++)
                {
                    if (currentSupers[i] != _lastSupers[i])
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (changed)
            {
                _lastSupers = currentSupers;
                ClearChildren();
                for (int i = 0; i < currentSupers.Count; i++)
                {
                    AddChild(new SuperWeaponButton(currentSupers[i], currentDescs[i], currentPCXs[i]));
                }
            }
        }
    }
}
