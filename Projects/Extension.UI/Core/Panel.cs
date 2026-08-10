using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.UI.Core
{
    public enum PanelOrientation
    {
        Vertical,
        Horizontal
    }

    public class Panel : Control
    {
        private int _interval = 2;

        /// <summary>
        /// 子控件之间的间距，默认 2
        /// </summary>
        public int Interval
        {
            get
            {
                return _interval;
            }
            set
            {
                if (_interval != value)
                {
                    _interval = value;
                    _isDirty = true;
                }
            }
        }

        private PanelOrientation _orientation = PanelOrientation.Vertical;

        /// <summary>
        /// 堆叠方向：纵向或横向。默认纵向。
        /// </summary>
        public PanelOrientation Orientation
        {
            get
            {
                return _orientation;
            }
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    _isDirty = true;
                }
            }
        }

        //
        // 摘要:
        //     Measure 尺寸：
        //       纵向：宽度 = 最大子控件宽度，高度 = 所有子控件高度之和 + Interval*(n-1)
        //       横向：宽度 = 所有子控件宽度之和 + Interval*(n-1)，高度 = 最大子控件高度
        //     最后加上自身显式尺寸较大者和 Margin。
        public override Point2D Measure(Point2D availableSize)
        {
            int totalWidth = 0;
            int totalHeight = 0;
            int maxChildWidth = 0;
            int maxChildHeight = 0;
            bool first = true;

            foreach (Control child in _children)
            {
                Point2D childDesired = child.Measure(availableSize);
                if (childDesired.X > maxChildWidth) maxChildWidth = childDesired.X;
                if (childDesired.Y > maxChildHeight) maxChildHeight = childDesired.Y;

                if (_orientation == PanelOrientation.Vertical)
                {
                    if (!first) totalHeight += _interval;
                    totalHeight += childDesired.Y;
                }
                else
                {
                    if (!first) totalWidth += _interval;
                    totalWidth += childDesired.X;
                }
                first = false;
            }

            // 对纵/横向分别填充另一维度
            if (_orientation == PanelOrientation.Vertical)
            {
                totalWidth = maxChildWidth;
            }
            else
            {
                totalHeight = maxChildHeight;
            }

            int selfWidth = (ClientRectangle.Width > totalWidth ? ClientRectangle.Width : totalWidth) + Margin.Left + Margin.Right;
            int selfHeight = (ClientRectangle.Height > totalHeight ? ClientRectangle.Height : totalHeight) + Margin.Top + Margin.Bottom;

            return new Point2D(selfWidth, selfHeight);
        }

        //
        // 摘要:
        //     排列子控件：
        //       纵向：Y 依次堆叠，子控件宽度拉伸到 clientRect.Width
        //       横向：X 依次堆叠，子控件高度拉伸到 clientRect.Height
        protected override void ArrangeChildren(RectangleStruct clientRect)
        {
            Point2D available = new Point2D(clientRect.Width, clientRect.Height);

            if (_orientation == PanelOrientation.Vertical)
            {
                int yOffset = 0;
                bool first = true;
                foreach (Control child in _children)
                {
                    Point2D childDesired = child.Measure(available);
                    int childHeight = childDesired.Y;
                    if (!first) yOffset += _interval;

                    RectangleStruct childRect = new RectangleStruct(
                        clientRect.X,
                        clientRect.Y + yOffset,
                        clientRect.Width,
                        childHeight
                    );
                    child.Arrange(childRect);
                    yOffset += childHeight;
                    first = false;
                }
            }
            else
            {
                int xOffset = 0;
                bool first = true;
                foreach (Control child in _children)
                {
                    Point2D childDesired = child.Measure(available);
                    int childWidth = childDesired.X;
                    if (!first) xOffset += _interval;

                    RectangleStruct childRect = new RectangleStruct(
                        clientRect.X + xOffset,
                        clientRect.Y,
                        childWidth,
                        clientRect.Height
                    );
                    child.Arrange(childRect);
                    xOffset += childWidth;
                    first = false;
                }
            }
        }
    }
}
