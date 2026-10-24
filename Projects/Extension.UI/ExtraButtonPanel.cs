using DynamicPatcher;
using Extension.UI.Core;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using System;
using System.Collections.Generic;

namespace Extension.UI
{
    //
    // 摘要:
    //     侧栏附加按钮面板（helpbtn）。位于单位过滤面板右侧、超武栏上方，层级在两者之下
    //     （先加入 UIRoot，绘制时先画，因此被上面两块面板压住）。共用 PanelFrame 九宫格边框。
    //     几何：面板 64×64（48 图标 + 四周 8 边框），并整体向左、向下各探出 8px——
    //       左边框（8px）压到单位过滤面板右边框之下；
    //       下边框（8px）压到超武栏上边框之下；无超武时超武栏整体隐藏，
    //       此时下边框露出来兜底，视觉上仍是闭合的框。
    //     显隐：仅当当前选中的**第一个**己方单位类型配置了 TutorialPCX 时显示，
    //     点击后打开该类型的教程面板。无教程 / 未选中己方单位时整体隐藏。
    public class ExtraButtonPanel : Panel
    {
        // 图标尺寸
        public const int IconSize = 48;

        // 九宫格切片边宽（共享 PanelFrame，固定 8px）
        private const int FrameInset = PanelFrame.Slice;

        // 面板总尺寸 = 图标 + 上下/左右各 8 边框
        public const int TotalWidth = IconSize + 2 * FrameInset;   // 64
        public const int TotalHeight = IconSize + 2 * FrameInset;  // 64

        // 超武栏总高：48 内容 + 上下各 8 边框 = 64。
        // 本面板下沿要再向下探 8px 盖进超武栏的上边框区域，故用它计算 Y。
        private const int SuperWeaponPanelHeight = 48 + 2 * PanelFrame.Slice;

        // 贴底显示（与另外两块面板一致）
        private const int BottomMargin = 0;

        // 面板底色（纯黑）
        private static readonly ColorStruct BackgroundColor = new ColorStruct(0, 0, 0);

        // 图标素材（PCX，从 MIX 中按文件名加载）
        private const string IconPCX = "helpbtn.pcx";

        // 图标加载失败时的占位色（暗灰）：至少能看出 48×48 图标位的位置与大小
        private static readonly ColorStruct IconPlaceholderColor = new ColorStruct(40, 40, 40);

        // 上一次 ViewBound，用于检测屏幕区域变化以重新定位
        private RectangleStruct _lastViewBound;

        public ExtraButtonPanel()
        {
            Orientation = PanelOrientation.Vertical;
            ClientRectangle = new RectangleStruct(0, 0, TotalWidth, TotalHeight);
        }

        //
        // 摘要:
        //     重写 Arrange：忽略父级 finalRect，按 ViewBound 计算绝对位置。
        //       X = ViewBound 左边缘 + 单位过滤面板总宽(200) - 8
        //           → 左边框正好落在单位面板右边框（x=192..200）之下，内容左沿从 x=200 开始
        //       Y = 底 - 超武栏高(64) + 8 - 本面板高(64)
        //           → 下边框正好落在超武栏上边框（底-64..底-56）区域，内容下沿贴住超武栏上沿
        public override RectangleStruct Arrange(RectangleStruct finalRect)
        {
            // 隐藏时不占位、不参与命中测试
            if (!Visible)
            {
                ClientRectangle = new RectangleStruct(0, 0, 0, 0);
                return ClientRectangle;
            }

            RectangleStruct view = Surface.ViewBound;

            int x = view.X + UnitFilterPanel.TotalWidth - FrameInset;
            int y = view.Y + view.Height - SuperWeaponPanelHeight + FrameInset - TotalHeight - BottomMargin;

            RectangleStruct absolute = new RectangleStruct(x, y, TotalWidth, TotalHeight);
            ClientRectangle = absolute;
            return ClientRectangle;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // 依据“第一个己方单位类型”是否有教程决定显隐
            UpdateVisibility();

            // 检测 ViewBound 变化（窗口尺寸改变）以触发重新布局
            RectangleStruct view = Surface.ViewBound;
            if (view != _lastViewBound)
            {
                _lastViewBound = view;
                _isDirty = true;
            }
        }

        //
        // 摘要:
        //     只看当前选中集合里的第一个己方单位类型：它没有教程就不显示按钮，
        //     即使后面还有别的类型配了教程。显隐变化时触发重新布局。
        private void UpdateVisibility()
        {
            bool show = false;
            try
            {
                if (UnitGroup.TryGetFirstLocalSelectedType(out Pointer<TechnoTypeClass> pType))
                {
                    List<string> pages = UnitGroup.GetTutorialPCX(pType);
                    show = pages != null && pages.Count > 0;
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
                show = false;
            }

            if (Visible != show)
            {
                Visible = show;
                _isDirty = true;
            }
        }

        //
        // 摘要:
        //     铺黑底 + 绘制完整四边边框 + 画 48×48 图标占位。
        //     左边框/下边框照常绘制，由上层面板覆盖（无超武时下边框露出来兜底）。
        protected override void OnDraw()
        {
            base.OnDraw();

            if (!Visible)
            {
                return;
            }

            try
            {
                Pointer<Surface> pSurface = Surface.Current;
                if (pSurface.IsNull || ClientRectangle.Width <= 0 || ClientRectangle.Height <= 0)
                {
                    return;
                }

                // 黑底（边框透明处也不透出游戏画面）
                pSurface.Ref.FillRect(ClientRectangle, Drawing.RGB2DWORD(BackgroundColor));

                // 完整九宫格边框：左边被单位过滤面板压住，
                // 下边有超武时被超武栏上边框压住、无超武时露出来
                PanelFrame.DrawFrame(pSurface, ClientRectangle);

                // 48×48 图标区：硬编码使用 helpbtn.pcx（素材已放入 MIX）
                RectangleStruct iconRect = new RectangleStruct(
                    ClientRectangle.X + FrameInset,
                    ClientRectangle.Y + FrameInset,
                    IconSize,
                    IconSize);
                if (!DrawIcon(pSurface, iconRect))
                {
                    // 素材缺失时画占位色，至少能看出图标位
                    pSurface.Ref.FillRect(iconRect, Drawing.RGB2DWORD(IconPlaceholderColor));
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     在图标区内绘制 helpbtn.pcx。素材小于图标区时居中；大于图标区时左上对齐并
        //     裁剪到图标区内（不压到边框）。加载失败返回 false，由调用方画占位色。
        private bool DrawIcon(Pointer<Surface> pSurface, RectangleStruct area)
        {
            PCX.Instance.LoadFile(IconPCX);
            Pointer<BSurface> pPCX = PCX.Instance.GetSurface(IconPCX, Pointer<BytePalette>.Zero);
            if (pPCX.IsNull)
            {
                return false;
            }

            int w = pPCX.Ref.Base.Base.Width;
            int h = pPCX.Ref.Base.Base.Height;
            if (w <= 0 || h <= 0)
            {
                return false;
            }

            int x = area.X + (w < area.Width ? (area.Width - w) / 2 : 0);
            int y = area.Y + (h < area.Height ? (area.Height - h) / 2 : 0);
            RectangleStruct blit = new RectangleStruct(x, y, w, h);

            // 裁剪到 Surface 视口 + 图标区，避免越界或压到边框
            RectangleStruct view = pSurface.Ref.GetRect();
            int left = Math.Max(Math.Max(blit.X, view.X), area.X);
            int top = Math.Max(Math.Max(blit.Y, view.Y), area.Y);
            int right = Math.Min(Math.Min(blit.X + blit.Width, view.X + view.Width), area.X + area.Width);
            int bottom = Math.Min(Math.Min(blit.Y + blit.Height, view.Y + view.Height), area.Y + area.Height);
            if (right <= left || bottom <= top)
            {
                return false;
            }

            RectangleStruct clip = new RectangleStruct(left, top, right - left, bottom - top);
            PCX.Instance.BlitToSurface(clip.GetThisPointer(), pSurface.Convert<DSurface>(), pPCX);
            return true;
        }

        //
        // 摘要:
        //     点击 helpbtn：取当前第一个己方单位类型的教程页并打开模态教程面板。
        //     无教程时直接返回；异常只记日志。
        protected internal override void OnClick()
        {
            if (!Visible)
            {
                return;
            }

            try
            {
                if (!UnitGroup.TryGetFirstLocalSelectedType(out Pointer<TechnoTypeClass> pType))
                {
                    return;
                }
                List<string> pages = UnitGroup.GetTutorialPCX(pType);
                if (pages == null || pages.Count == 0)
                {
                    return;
                }
                UIRoot.Instance.OpenTutorial(UnitGroup.GetTypeId(pType), pages);
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }
    }
}
