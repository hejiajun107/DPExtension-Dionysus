using DynamicPatcher;
using Extension.UI.Core;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extension.UI
{
    //
    // 摘要:
    //     超武面板容器。从 INI [SWPanel] 读取配置的超武 ID 列表，
    //     从当前玩家（HouseClass.Player）的超武列表中匹配，创建 SuperWeaponButton
    //     并纵向排列在 sidebar 左侧（ViewBound 右边缘）、屏幕底部。
    public class SuperWeaponPanel : Panel
    {
        //
        // 摘要:
        //     配置的超武 ID 列表（来自 INI [SWPanel] SWList，逗号分隔）。
        private List<string> swIds = new List<string>();

        //
        // 摘要:
        //     ID -> Description 映射（来自 [SWPanel.SWxxx] Description）。
        private Dictionary<string, string> descriptions = new Dictionary<string, string>();

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

        // 诊断日志去重标志
        private bool _hasLoggedPlayerNull;
        private HashSet<string> _loggedNotFound = new HashSet<string>();
        private HashSet<string> _loggedNotOwned = new HashSet<string>();

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
                Logger.Log("[SWPanel] 解析到 {0} 个超武 ID: {1}", swIds.Count, string.Join(", ", swIds));

                // 读取 Interval（按钮间距，默认 2）
                string intervalStr = ReadIniString(pINI, "SWPanel", "Interval");
                int interval = 2;
                if (int.TryParse(intervalStr, out int parsed))
                {
                    interval = parsed;
                }
                Interval = interval;

                // 读取每个超武的 Description
                foreach (string id in swIds)
                {
                    descriptions[id] = ReadIniString(pINI, "SWPanel." + id, "Description");
                }

                _configLoaded = true;
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
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
        //     横向排列在 sidebar 左侧、屏幕底部：
        //       X = ViewBound 右边缘 - 面板总宽度
        //       Y = ViewBound 底部 - 按钮高度
        //     面板宽度 = 按钮数 * ButtonWidth + (按钮数 - 1) * Interval。
        //     设置 ClientRectangle 后由 Panel.ArrangeChildren 横向排列按钮。
        public override RectangleStruct Arrange(RectangleStruct finalRect)
        {
            RectangleStruct view = Surface.ViewBound;

            int count = _children.Count;
            int width = count > 0 ? count * ButtonWidth + (count - 1) * Interval : 0;

            int x = view.X + view.Width - width;
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
                if (!_hasLoggedPlayerNull)
                {
                    Logger.Log("[SWPanel] HouseClass.Player 为空，跳过（可能场景尚未完全初始化）");
                    _hasLoggedPlayerNull = true;
                }
                return;
            }
            _hasLoggedPlayerNull = false;

            List<Pointer<SuperClass>> currentSupers = new List<Pointer<SuperClass>>();
            List<string> currentDescs = new List<string>();

            foreach (string id in swIds)
            {
                Pointer<SuperWeaponTypeClass> pType = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(id);
                if (pType.IsNull)
                {
                    if (!_loggedNotFound.Contains(id))
                    {
                        Logger.Log("[SWPanel] 超武类型 '{0}' 未在 [SuperWeaponTypes] 中找到", id);
                        _loggedNotFound.Add(id);
                    }
                    continue;
                }

                Pointer<SuperClass> pSuper = pPlayer.Ref.FindSuperWeapon(pType);
                if (pSuper.IsNull)
                {
                    if (!_loggedNotOwned.Contains(id))
                    {
                        Logger.Log("[SWPanel] 玩家没有超武 '{0}'（类型已找到，但 HouseClass.Player 无此超武实例）", id);
                        _loggedNotOwned.Add(id);
                    }
                    continue;
                }

                // 找到了，从"未找到/未拥有"日志集合中移除（以便下次再丢失时重新记录）
                _loggedNotFound.Remove(id);
                _loggedNotOwned.Remove(id);
                currentSupers.Add(pSuper);
                string desc = descriptions.TryGetValue(id, out string d) ? d : string.Empty;
                currentDescs.Add(desc);
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
                Logger.Log("[SWPanel] 超武按钮列表变化: {0} -> {1}，重建按钮", _lastSupers.Count, currentSupers.Count);
                _lastSupers = currentSupers;
                ClearChildren();
                for (int i = 0; i < currentSupers.Count; i++)
                {
                    AddChild(new SuperWeaponButton(currentSupers[i], currentDescs[i]));
                }
            }
        }
    }
}
