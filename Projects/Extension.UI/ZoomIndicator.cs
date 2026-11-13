using Extension.UI.Core;
using PatcherYRpp;
using System;

namespace Extension.UI
{
    /// <summary>
    /// 缩放刻度盘：战场顶部居中的弧形刻度，显示当前缩放倍率。
    /// 纯装饰控件，不参与命中测试，也不消费鼠标。
    /// </summary>
    public class ZoomIndicator : Control
    {
        private const int Ticks = 17;
        private const double HalfAngleDeg = 78.0;
        private const double WidthRatio = 0.14; // 椭圆半宽占屏幕宽度的比例
        private const int TopMargin = 32;
        private const int TickLength = 16;
        private const int MajorTickLength = 22;
        private const int HoldMs = 900;
        private const int FadeMs = 400;

        // 16-bit RGB565
        private const ushort ColorArc = 0xFFFF;     // 浅灰（外弧）
        private const ushort ColorTick = 0xFFFF;    // 白色（刻度）
        private const ushort ColorActive = 0x07FF;  // 青色（当前倍率）
        private const ushort ColorFadeFrom = 0x4208; // 淡入起点（深灰）

        public override Control HitTest(Point2D pos)
        {
            return null;
        }

        protected override void OnDraw()
        {
            int elapsed = Environment.TickCount - ZoomController.LastChangeTick;
            if (ZoomController.LastChangeTick == 0 || elapsed < 0 || elapsed > HoldMs + FadeMs)
            {
                return;
            }

            // 停留 HoldMs 后淡出，不支持 alpha，用亮度插值模拟
            double fade = elapsed <= HoldMs ? 1.0 : 1.0 - (elapsed - HoldMs) / (double)FadeMs;

            ref Surface surface = ref Surface.Current.Ref;
            RectangleStruct screen = surface.GetRect();
            int centerX = screen.Width / 2;
            int top = TopMargin;

            // 扁椭圆：半宽随屏幕自适应，垂直半径 = 半宽 / 4（改分母可调扁度，越大越扁）
            int rx = (int)(screen.Width * WidthRatio);
            int ry = Math.Max(40, rx / 3);
            double halfAngle = HalfAngleDeg * Math.PI / 180.0;

            int activeIndex = ZoomToIndex(ZoomController.CurrentZoom);
            Point2D[] outer = new Point2D[Ticks];
            Point2D[] inner = new Point2D[Ticks];

            for (int i = 0; i < Ticks; i++)
            {
                double t = i / (double)(Ticks - 1);
                // 0 度 = 正下方，向左右各展开 HalfAngleDeg
                double angle = -halfAngle + 2 * halfAngle * t;
                double sin = Math.Sin(angle);
                double cos = Math.Cos(angle);

                outer[i] = new Point2D(
                    centerX + (int)Math.Round(rx * sin),
                    top + (int)Math.Round(ry * cos));

                // 沿椭圆法线向内收固定长度，保证扁椭圆上每根刻度一样长
                double nx = sin / rx;
                double ny = cos / ry;
                double len = Math.Sqrt(nx * nx + ny * ny);
                if (len < 1e-6)
                {
                    len = 1e-6;
                }
                nx /= len;
                ny /= len;

                int tickLength = (i == Ticks / 2) ? MajorTickLength : TickLength;
                inner[i] = new Point2D(
                    outer[i].X - (int)Math.Round(nx * tickLength),
                    outer[i].Y - (int)Math.Round(ny * tickLength));
            }

            // 外弧：相邻刻度外端连线
            ushort arcColor = Blend(ColorFadeFrom, ColorArc, fade);
            for (int i = 0; i < Ticks - 1; i++)
            {
                DrawThickLine(ref surface, outer[i], outer[i + 1], arcColor, 1);
            }

            // 中间倍率文字（淡出后半段不再画，避免明暗不一致）
            if (fade > 0.5)
            {
                string label = ZoomController.CurrentZoom.ToString("0.##") + "x";
                surface.DrawText(
                    label,
                    centerX,
                    top + (int)(ry * 0.30),
                    new ColorStruct(255, 255, 255),
                    TextPrintType.Center | TextPrintType.Score | TextPrintType.NoShadow);
            }

            // 刻度：白色为主，当前倍率用青色并加粗
            for (int i = 0; i < Ticks; i++)
            {
                bool active = i == activeIndex;
                ushort color = active
                    ? Blend(ColorFadeFrom, ColorActive, fade)
                    : Blend(ColorFadeFrom, ColorTick, fade);
                int thickness = active ? 2 : 1;

                DrawThickLine(ref surface, inner[i], outer[i], color, thickness);
            }
        }

        /// <summary>
        /// DrawLine 只有 1px，用近似圆形的偏移重绘实现加粗。
        /// </summary>
        private static void DrawThickLine(ref Surface surface, Point2D a, Point2D b, ushort color, int thickness)
        {
            if (thickness <= 1)
            {
                surface.DrawLine(a, b, color);
                return;
            }

            int half = thickness / 2;
            int limit = half * half + 1;

            for (int dx = -half; dx <= half; dx++)
            {
                for (int dy = -half; dy <= half; dy++)
                {
                    if (dx * dx + dy * dy > limit)
                    {
                        continue;
                    }

                    surface.DrawLine(
                        new Point2D(a.X + dx, a.Y + dy),
                        new Point2D(b.X + dx, b.Y + dy),
                        color);
                }
            }
        }

        /// <summary>
        /// 倍率映射到刻度索引：以 1.0 为中点向两侧分段铺满。
        /// 0.67 → 0，1.0 → 8（正中），2.0 → 16。
        /// </summary>
        private static int ZoomToIndex(double zoom)
        {
            if (zoom <= 0)
            {
                return Ticks / 2;
            }

            int half = Ticks / 2;
            double index;

            if (zoom <= 1.0)
            {
                // 左半段：MinZoom..1.0 摊到 0..half
                index = half * (1.0 - Math.Log(zoom) / Math.Log(ZoomController.MinZoom));
            }
            else
            {
                // 右半段：1.0..MaxZoom 摊到 half..Ticks-1
                index = half + half * (Math.Log(zoom) / Math.Log(ZoomController.MaxZoom));
            }

            int result = (int)Math.Round(index);
            if (result < 0)
            {
                result = 0;
            }
            if (result > Ticks - 1)
            {
                result = Ticks - 1;
            }
            return result;
        }

        /// <summary>16-bit RGB565 通道插值。t = 0 取 a，t = 1 取 b。</summary>
        private static ushort Blend(ushort a, ushort b, double t)
        {
            if (t <= 0)
            {
                return a;
            }
            if (t >= 1)
            {
                return b;
            }

            int ar = (a >> 11) & 0x1F, ag = (a >> 5) & 0x3F, ab = a & 0x1F;
            int br = (b >> 11) & 0x1F, bg = (b >> 5) & 0x3F, bb = b & 0x1F;

            int r = (int)(ar + (br - ar) * t);
            int g = (int)(ag + (bg - ag) * t);
            int bl = (int)(ab + (bb - ab) * t);

            return (ushort)((r << 11) | (g << 5) | bl);
        }
    }
}
