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

        private const int Padding = 4;
        private const int LineHeight = 12;
        private const int CharWidth = 6;
        private const int Gap = 4;

        //
        // 摘要:
        //     绘制 Tooltip。计算位置（控件右侧，空间不足时左侧），
        //     用 FillRect 绘制深色背景，DrawRect 绘制边框，DrawText 逐行绘制文本。
        //     用 try-catch 包裹绘制 API 保证异常安全。
        public void Draw()
        {
            if (!IsVisible || Target == null || Lines == null || Lines.Count == 0)
            {
                return;
            }

            try
            {
                RectangleStruct targetRect = Target.ClientRectangle;
                RectangleStruct screen = Surface.ViewBound;

                // 估算 Tooltip 尺寸
                int maxLineWidth = 0;
                for (int i = 0; i < Lines.Count; i++)
                {
                    int w = (Lines[i] ?? string.Empty).Length * CharWidth;
                    if (w > maxLineWidth)
                    {
                        maxLineWidth = w;
                    }
                }
                int tooltipWidth = maxLineWidth + Padding * 2;
                int tooltipHeight = Lines.Count * LineHeight + Padding * 2;

                // 默认放在控件右侧
                int x = targetRect.X + targetRect.Width + Gap;
                int y = targetRect.Y;

                // 右侧空间不足则放左侧
                if (x + tooltipWidth > screen.X + screen.Width)
                {
                    x = targetRect.X - tooltipWidth - Gap;
                }

                // 水平越界则夹紧到屏幕内
                if (x + tooltipWidth > screen.X + screen.Width)
                {
                    x = screen.X + screen.Width - tooltipWidth;
                }
                if (x < screen.X)
                {
                    x = screen.X;
                }

                // 垂直越界则夹紧到屏幕内
                if (y + tooltipHeight > screen.Y + screen.Height)
                {
                    y = screen.Y + screen.Height - tooltipHeight;
                }
                if (y < screen.Y)
                {
                    y = screen.Y;
                }

                RectangleStruct bgRect = new RectangleStruct(x, y, tooltipWidth, tooltipHeight);

                // 绘制深色背景
                Surface.Current.Ref.FillRect(bgRect, Drawing.RGB2DWORD(BackColor));
                // 绘制边框
                Surface.Current.Ref.DrawRect(bgRect, Drawing.RGB2DWORD(BorderColor));

                // 逐行绘制文本
                for (int i = 0; i < Lines.Count; i++)
                {
                    string line = Lines[i] ?? string.Empty;
                    int tx = x + Padding;
                    int ty = y + Padding + i * LineHeight;
                    Surface.Current.Ref.DrawText(line, tx, ty, TextColor, TextPrintType.NoShadow);
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }
    }
}
