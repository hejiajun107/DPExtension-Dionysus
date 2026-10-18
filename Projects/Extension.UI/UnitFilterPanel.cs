using DynamicPatcher;
using Extension.UI.Core;
using PatcherYRpp;
using System;
using System.Collections.Generic;

namespace Extension.UI
{
    //
    // 摘要:
    //     单位过滤面板。3×3 网格，扫描当前选中单位（CurrentObjects）提取不同类型，
    //     每类一格显示 cameo，不足 9 格时保留布局用黑色空格占位。
    //     点击某格过滤当前选中只保留该类型。
    //     外边框用 uiext.pcx 九宫格切片绘制：4 角原尺寸贴角点、4 边单方向平铺、中心不画（被按钮占）。
    //     切片边宽固定 8px（BorderSlice）。
    //     位置：屏幕左侧（ViewBound 左边缘），底部对齐超武栏底部，超武栏右移腾位。
    //     联机安全：仅操作本地 CurrentObjects 选中状态。
    public class UnitFilterPanel : Panel
    {
        // 每格尺寸（与原版 cameo 一致）
        private const int CellWidth = 60;
        private const int CellHeight = 48;
        // 格间距
        private const int CellInterval = 2;
        // 贴底显示（面板底部对齐 ViewBound 底边）
        private const int BottomMargin = 0;

        // 3×3 按钮区域尺寸（不含边框）
        public const int GridWidth = 3 * CellWidth + 2 * CellInterval;   // 184
        public const int GridHeight = 3 * CellHeight + 2 * CellInterval;  // 148

        // 九宫格切片边宽（固定 8px），引自共享边框 PanelFrame。
        public const int BorderSlice = PanelFrame.Slice;

        // 面板总尺寸（含边框），供超武栏定位时引用，避免重叠
        public const int TotalWidth = GridWidth + 2 * BorderSlice;    // 200
        public const int TotalHeight = GridHeight + 2 * BorderSlice;  // 164

        // 面板底色（纯黑）
        private static readonly ColorStruct BackgroundColor = new ColorStruct(0, 0, 0);

        // 上次扫描到的分组键列表（GroupAs 字符串），用于检测变化、避免每帧重建按钮
        private List<string> _lastGroupKeys = new List<string>();

        // 上一次 ViewBound，用于检测屏幕区域变化以重新定位
        private RectangleStruct _lastViewBound;

        static UnitFilterPanel()
        {
            // 默认纵向堆叠，但我们重写 ArrangeChildren 做 3×3 布局
        }

        public UnitFilterPanel()
        {
            Orientation = PanelOrientation.Vertical;
            ClientRectangle = new RectangleStruct(0, 0, GridWidth, GridHeight);
        }

        //
        // 摘要:
        //     重写 Arrange：根据 Surface.ViewBound 计算绝对位置——
        //     左对齐 ViewBound 左边缘，底部对齐超武栏底部（距底 BottomMargin）。
        //     面板总尺寸 = 3×3 网格 + 边框厚度。
        public override RectangleStruct Arrange(RectangleStruct finalRect)
        {
            RectangleStruct view = Surface.ViewBound;

            int x = view.X;
            int y = view.Y + view.Height - TotalHeight - BottomMargin;

            RectangleStruct absolute = new RectangleStruct(x, y, TotalWidth, TotalHeight);
            ClientRectangle = absolute;
            ArrangeChildren(ClientRectangle);
            return ClientRectangle;
        }

        //
        // 摘要:
        //     重写 ArrangeChildren：3×3 网格布局，从边框内沿开始排列。
        protected override void ArrangeChildren(RectangleStruct clientRect)
        {
            int startX = clientRect.X + BorderSlice;
            int startY = clientRect.Y + BorderSlice;

            int count = Math.Min(_children.Count, 9);
            for (int i = 0; i < count; i++)
            {
                int row = i / 3;
                int col = i % 3;
                int x = startX + col * (CellWidth + CellInterval);
                int y = startY + row * (CellHeight + CellInterval);
                _children[i].Arrange(new RectangleStruct(x, y, CellWidth, CellHeight));
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            try
            {
                // 懒加载边框素材（共享 PanelFrame）
                PanelFrame.EnsureInit();

                // 扫描 CurrentObjects 提取不同类型
                RebuildButtons();

                // 检测 ViewBound 变化
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

        // ==== 临时诊断（已注释，需要排查时再启用）====
        // private static string _lastDiag;
        // ==== 临时诊断结束 ====

        //
        // 摘要:
        //     扫描 CurrentObjects，按分组键（GroupAs 字符串）去重提取类型（最多 9 个）。
        //     分组键来自 UnitGroup：读 [单位ID] GroupAs=，未配置时默认为单位自身 ID。
        //     类型列表变化时重建按钮；不足 9 格补空格按钮保留布局。
        private void RebuildButtons()
        {
            var typeMap = new Dictionary<string, Pointer<TechnoTypeClass>>();
            var currentObjs = ObjectClass.CurrentObjects;

            // ==== 临时诊断（已注释，需要排查时再启用）====
            // string diag = "count=" + currentObjs.Count;

            for (int i = 0; i < currentObjs.Count && typeMap.Count < 9; i++)
            {
                var pObj = currentObjs[i];
                if (pObj.IsNull)
                {
                    // diag += $" obj[{i}]=null";
                    continue;
                }
                var pTechno = pObj.Convert<TechnoClass>();
                if (pTechno.IsNull)
                {
                    // diag += $" obj[{i}]=nonTechno";
                    continue;
                }
                // 只统计本地玩家自己的单位：CurrentObjects 里可能混入其他玩家/中立的对象
                if (pTechno.Ref.Owner != HouseClass.Player)
                {
                    continue;
                }
                var pType = pTechno.Ref.Type;
                if (pType.IsNull)
                {
                    // diag += $" obj[{i}]=typeNull";
                    continue;
                }

                // 逐对象隔离：单个类型读 INI 抛异常时不应打断整次扫描
                bool hidden;
                string groupKey;
                try
                {
                    hidden = UnitGroup.IsHidden(pType);
                    groupKey = UnitGroup.GetGroupKey(pType);
                }
                catch (Exception ex)
                {
                    Logger.PrintException(ex);
                    // diag += $" obj[{i}]=EXCEPTION";
                    continue;
                }

                // diag += $" obj[{i}]={id},hidden={hidden},key={groupKey}";

                // Insignificant / DontScore 的单位不进入过滤列表
                if (hidden)
                {
                    continue;
                }
                if (!typeMap.ContainsKey(groupKey))
                {
                    typeMap[groupKey] = pType;
                }
            }

            // ==== 临时诊断（已注释，需要排查时再启用）====
            // diag += " => buttons=" + typeMap.Count;
            // if (diag != _lastDiag)
            // {
            //     _lastDiag = diag;
            //     Logger.Log("[UnitFilterPanel] " + diag);
            // }
            // ==== 临时诊断结束 ====

            // 比较分组键列表是否变化
            var newKeys = new List<string>(typeMap.Keys);
            newKeys.Sort(StringComparer.Ordinal);
            if (IsSameList(_lastGroupKeys, newKeys))
            {
                return;
            }
            // 重建按钮
            ClearChildren();
            // 注意：AddChild 只进队列，_children 下一帧 ProcessQueues 才更新，
            // 必须用局部计数器判断补齐数量，不能用 _children.Count（会死循环）
            int added = 0;
            foreach (var kv in typeMap)
            {
                try
                {
                    AddChild(new UnitFilterButton(kv.Value, kv.Key));
                    added++;
                }
                catch (Exception ex)
                {
                    // 单个按钮构造失败不应拖垮整次重建；added 不自增，随后由空格补齐
                    Logger.PrintException(ex);
                }
            }
            // 不足 9 格补空格
            for (; added < 9; added++)
            {
                AddChild(new UnitFilterButton());
            }

            // 分组键列表在重建成功之后再提交：重建中途抛异常则不提交，
            // 下一帧会重新触发重建，避免面板永久停留在「全黑、无按钮」状态
            _lastGroupKeys = newKeys;

            _isDirty = true;
        }

        private static bool IsSameList(List<string> a, List<string> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        //
        // 摘要:
        //     先铺黑底，再画外边框（共享 PanelFrame 九宫格），最后由 base.Draw() 画子按钮。
        protected override void OnDraw()
        {
            base.OnDraw();

            try
            {
                Pointer<Surface> pSurface = Surface.Current;
                if (pSurface.IsNull)
                {
                    return;
                }

                // 整个面板区域铺黑底（边框透明部分也显黑，不透游戏画面）
                pSurface.Ref.FillRect(ClientRectangle, Drawing.RGB2DWORD(BackgroundColor));

                // 四周完整边框
                PanelFrame.DrawFrame(pSurface, ClientRectangle);
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }
    }
}
