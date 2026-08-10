using DynamicPatcher;
using PatcherYRpp;
using System;
using System.Collections.Generic;

namespace Extension.UI
{
    //
    // 摘要:
    //     辅助类，由 UIRoot 管理。当鼠标悬停在控件上超过阈值帧数后，
    //     在控件旁显示说明文本。在 UIRoot.Draw() 之后绘制以保证位于最上层。
    public class Tooltip
    {
        //
        // 摘要:
        //     目标控件引用。ClientRectangle 用于定位 Tooltip。
        public Control Target;

        //
        // 摘要:
        //     显示的文本行。每行单独绘制。
        public List<string> Lines = new List<string>();

        //
        // 摘要:
        //     是否可见。由 UIRoot 在 Hover 触发时置 true，移出控件时置 false。
        public bool IsVisible;

        // 深色背景（FillRect 不支持半透明，使用深色实心背景保证文本可读）
        private static readonly ColorStruct BackColor = new ColorStruct(10, 10, 30);
        // 浅灰色边框
        private static readonly ColorStruct BorderColor = new ColorStruct(200, 200, 200);
        // 白色文本
        private static readonly ColorStruct TextColor = new ColorStruct(255, 255, 255);

        private const int Padding = 8;
        private const int LineHeight = 18;
        private const int AsciiCharWidth = 8;      // fallback：字母/数字按 8px
        private const int NonAsciiCharWidth = 16;  // fallback：中文按 16px
        private const int Gap = 4;
        private const int WidthReservePerLine = 28; // 每行预留（用户要求加大量）：字间距 / 抗锯齿 / advance 取整
        private const int DefaultSidebarWidth = 200; // Sidebar 宽度兜底值（用不到 Parent.Right 时用）

        //
        // 摘要:
        //     绘制 Tooltip。先调用 Fancy_Text_Print_Wide 用超大裁剪矩形在屏外坐标测量每行真实像素宽高
        //     （不污染画面、也不会被视口矩形裁掉返回 0），再计算 Tooltip 背景矩形：
        //     默认放在按钮正上方（按钮头顶的大片空白），空间不足时回退到按钮下方，
        //     最后显式传全屏裁剪矩形 DrawText，保证文字不被图标截断。
        public void Draw()
        {
            if (!IsVisible || Target == null || Lines == null || Lines.Count == 0)
            {
                return;
            }

            try
            {
                Pointer<Surface> pSurf = Surface.Current;
                if (pSurf.IsNull) return;

                RectangleStruct targetRect = Target.ClientRectangle;
                RectangleStruct screen = Surface.ViewBound;
                // 真正 DrawText 用的裁剪矩形 = 整个视口，避免文字被图标 / Sidebar 意外截断
                RectangleStruct drawBound = screen;
                // 测量用的超大裁剪矩形（包含负坐标超大范围，保证测量起点在矩形内）
                RectangleStruct measureBound = new RectangleStruct(-10000000, -10000000, 20000000, 20000000);
                // 测量起点（远小于 Surface 可视区坐标，写字时被 Surface 边界检查拦下 → 不留字迹）
                const int MeasureOrigin = -5000000;
                Point2D result = default;

                // ---- 1. 真实测量每行尺寸（Fancy_Text_Print_Wide + UniString + Drawing.RGB2DWORD，与 DrawText 调用链一致） ----
                int maxLineWidth = 0;
                int measuredLineHeight = LineHeight;
                int foreColorDWORD = Drawing.RGB2DWORD(TextColor);

                for (int i = 0; i < Lines.Count; i++)
                {
                    string line = Lines[i] ?? string.Empty;
                    if (line.Length == 0) continue;

                    Point2D lineStart = new Point2D(MeasureOrigin, MeasureOrigin + i * measuredLineHeight);
                    using (UniString ustr = new UniString(line))
                    {
                        Surface.Fancy_Text_Print_Wide(
                            Pointer<Point2D>.AsPointer(ref result),
                            ustr,
                            pSurf,
                            Pointer<RectangleStruct>.AsPointer(ref measureBound),
                            Pointer<Point2D>.AsPointer(ref lineStart),
                            foreColorDWORD,
                            0,
                            TextPrintType.NoShadow);
                    }
                    int lineW = result.X - lineStart.X;
                    int lineH = result.Y - lineStart.Y;
                    // 每行加大量预留，避免字间距 / 字形 advance / 抗锯齿溢散 造成末尾被裁
                    int lineTotal = lineW + WidthReservePerLine;
                    if (lineTotal > maxLineWidth) maxLineWidth = lineTotal;
                    if (lineH > measuredLineHeight) measuredLineHeight = lineH;
                }

                // 兜底：如果测量返回 0（异常情况），退回到字符数估算 + 预留
                if (maxLineWidth <= 0)
                {
                    for (int i = 0; i < Lines.Count; i++)
                    {
                        string line = Lines[i] ?? string.Empty;
                        int w = 0;
                        for (int j = 0; j < line.Length; j++)
                        {
                            w += (line[j] < 128) ? AsciiCharWidth : NonAsciiCharWidth;
                        }
                        int lineTotal = w + WidthReservePerLine;
                        if (lineTotal > maxLineWidth) maxLineWidth = lineTotal;
                    }
                }
                if (maxLineWidth < 20) maxLineWidth = 20;

                // 行高给点余量（实测可能没算行间距）
                if (measuredLineHeight < LineHeight) measuredLineHeight = LineHeight;
                int lineStep = measuredLineHeight;

                int tooltipWidth = maxLineWidth + Padding * 2;
                int tooltipHeight = Lines.Count * lineStep + Padding * 2;

                // ---- 2.1 计算右侧安全边界（Tooltip 右端对齐 Sidebar 左侧）
                // 优先用 Target.Parent.ClientRectangle.Right（SuperWeaponPanel 右缘紧贴 Sidebar 左缘，最准确），
                // 无 Parent 或值异常时才用 Screen.Right - DefaultSidebarWidth 兜底。
                int screenRight = screen.X + screen.Width;
                int safeRightLimit = screenRight - DefaultSidebarWidth;
                if (Target.Parent != null)
                {
                    int parentRight = Target.Parent.ClientRectangle.X + Target.Parent.ClientRectangle.Width;
                    if (parentRight > 200 && parentRight < screenRight - 20)
                    {
                        // 父容器右缘只要合理就直接用
                        safeRightLimit = parentRight;
                    }
                }
                // 最后夹紧
                if (safeRightLimit > screenRight - 20) safeRightLimit = screenRight - DefaultSidebarWidth;
                if (safeRightLimit < 200) safeRightLimit = 200;

                // ---- 2.2 定位：默认放在按钮正上方，Tooltip 右端顶到 Sidebar 左侧（safeRightLimit） ----
                // 先按左对齐按钮定个初值
                int x = targetRect.X;
                int y = targetRect.Y - tooltipHeight - Gap;

                // 右端必须 ≤ safeRightLimit（否则往左挪，保证右端对齐 Sidebar 左侧）
                if (x + tooltipWidth > safeRightLimit)
                {
                    x = safeRightLimit - tooltipWidth;
                }
                // 水平：右端不越屏；左边界屏内
                if (x + tooltipWidth > screen.X + screen.Width)
                {
                    x = screen.X + screen.Width - tooltipWidth;
                }
                if (x < screen.X) x = screen.X;

                // 上方不够高 → 回退到按钮下方；最后兜底夹紧到屏幕内
                if (y < screen.Y)
                {
                    int yBelow = targetRect.Y + targetRect.Height + Gap;
                    if (yBelow + tooltipHeight <= screen.Y + screen.Height)
                    {
                        y = yBelow;
                    }
                    else
                    {
                        y = screen.Y;
                    }
                }
                if (y + tooltipHeight > screen.Y + screen.Height)
                {
                    y = screen.Y + screen.Height - tooltipHeight;
                }
                if (y < screen.Y) y = screen.Y;

                RectangleStruct bgRect = new RectangleStruct(x, y, tooltipWidth, tooltipHeight);

                // ---- 3. 画背景 + 边框 ----
                pSurf.Ref.FillRect(bgRect, Drawing.RGB2DWORD(BackColor));
                pSurf.Ref.DrawRect(bgRect, Drawing.RGB2DWORD(BorderColor));

                // ---- 4. 逐行绘制文字（显式传全屏 drawBound，避免被图/侧边栏裁掉） ----
                for (int i = 0; i < Lines.Count; i++)
                {
                    string line = Lines[i] ?? string.Empty;
                    Point2D loc = new Point2D(x + Padding, y + Padding + i * lineStep);
                    pSurf.Ref.DrawText(
                        line,
                        Pointer<RectangleStruct>.AsPointer(ref drawBound),
                        Pointer<Point2D>.AsPointer(ref loc),
                        TextColor,
                        TextPrintType.NoShadow);
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }
    }
}
