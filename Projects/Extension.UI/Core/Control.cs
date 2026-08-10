using Extension.EventSystems;
using Extension.UI.Core;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Extension.UI
{
    public class Control : UIComponent
    {
        private const double DOUBLE_CLICK_TIME = 1.0;

        private Control parent;

        //
        // 摘要:
        //     A list of the control's children. Don't add children to this list directly; call
        //     the AddChild method instead.
        protected List<Control> _children = new List<Control>();

        private List<Control> updateList = new List<Control>();

        private List<Control> drawList = new List<Control>();

        private List<Control> childAddQueue = new List<Control>();

        private List<Control> childRemoveQueue = new List<Control>();





        private int _x;

        private int _y;

        private int _width;

        private int _height;

        private int _scaling = 1;

        private int _initScaling;

        private bool CursorOnControl;

        private float alpha = 1f;

        public int CursorTextureIndex;

        private bool isActive;

        private bool _ignoreInputOnFrame;

        //private ControlDrawMode drawMode;

        private bool _isChangingSize;

        private TimeSpan timeSinceLastLeftClick = TimeSpan.Zero;

        private bool isLeftPressedOn;

        private bool isRightPressedOn;

        private bool isIteratingChildren;

        //
        // 摘要:
        //     布局是否需要重新计算。AddChild/RemoveChild/尺寸改变时置 true，
        //     Update 中检测到后重新 Arrange 并置 false。
        protected bool _isDirty = true;

        //
        // 摘要:
        //     上一次 Arrange 传入的 finalRect，用于 _isDirty 触发的重新布局时复用，
        //     避免使用自身 ClientRectangle 导致 Right/Bottom 等对齐方式累积偏移。
        private RectangleStruct _lastArrangeRect;

        private bool _hasBeenArranged = false;


        #region Attributes
        /// <summary>
        /// 控件是否可见且可交互。隐藏 UIComponent.Visible（默认 false），
        /// 此处默认 true 以使 HitTest 正常工作。
        /// </summary>
        public new bool Visible { get; set; } = true;

        /// <summary>
        /// 当前是否被鼠标悬停（供 Tooltip 等使用）。
        /// </summary>
        public bool IsHovered { get; protected internal set; }

        /// <summary>
        /// 悬停计时帧数，由 UIRoot 在 OnUpdate 中递增，超过阈值触发 OnHover。
        /// </summary>
        protected internal int _hoverFrameCount;

        /// <summary>
        /// 点击穿透此元素
        /// </summary>
        public bool ClickThrough { get; set; }
        /// <summary>
        /// 水平对齐
        /// </summary>
        public HorizontalAlignment? HorizontalAlignment { get; set; }
        /// <summary>
        /// 垂直对齐
        /// </summary>
        public VerticalAlignment? VerticalAlignment { get; set; }
        /// <summary>
        /// 方向
        /// </summary>
        public FlowDirection FlowDirection { get; set; }

        /// <summary>
        /// 外边距，Arrange 时根据对齐方式扣除
        /// </summary>
        public MarginStruct Margin { get; set; }

        public int? Top { get; set; }

        public int? Bottom { get; set; }

        public int? Left { get; set; } 

        public int? Right { get; set; }

        #endregion


        public Control Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
                //this.ParentChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        //
        // 摘要:
        //     The non-scaled display rectangle of the control inside its parent.
        public RectangleStruct ClientRectangle
        {
            get
            {
                return new RectangleStruct(_x, _y, _width, _height);
            }
            set
            {
                _x = value.X;
                _y = value.Y;
                if (value.Width != _width || value.Height != _height)
                {
                    _width = value.Width;
                    _height = value.Height;
                    _isDirty = true;
                    //OnSizeChanged();
                }
                //OnClientRectangleUpdated();
            }
        }





        public virtual void ParseFromXml(XmlNode xml)
        {
            if (xml.Attributes["Top"] is not null)
            {

            }

            if (xml.Attributes["Left"] is not null)
            {

            }

            if (xml.Attributes["HorizontalAlignment"] is not null)
            {

            }

            if (xml.Attributes["VerticalAlignment"] is not null)
            {

            }

            if (xml.Attributes["FlowDirection"] is not null)
            {
                FlowDirection = (FlowDirection)Enum.Parse(typeof(FlowDirection), xml.Attributes["FlowDirection"].Value) ;
            }

            if (xml.Attributes["ClickThrough"] is not null)
            {
                var val = true;
                _ = bool.TryParse(xml.Attributes["ClickThrough"].Value, out val);
                ClickThrough = val;
            }
            else
            {
                ClickThrough = false;
            }

        }

        //
        // 摘要:
        //     将子控件加入添加队列，延迟到下一次 Update 时真正移入 _children。
        //     不要在遍历 _children 时直接调用此方法修改集合。
        public void AddChild(Control child)
        {
            if (child == null)
            {
                return;
            }

            childAddQueue.Add(child);
            child.Parent = this;
            _isDirty = true;
        }

        //
        // 摘要:
        //     将子控件加入移除队列，延迟到下一次 Update 时真正移出 _children。
        //     不要在遍历 _children 时直接调用此方法修改集合。
        public void RemoveChild(Control child)
        {
            if (child == null)
            {
                return;
            }

            childRemoveQueue.Add(child);
            _isDirty = true;
        }

        //
        // 摘要:
        //     立即清空所有子控件及增删队列，把每个子控件的 Parent 置 null。
        //     供 UIRoot 等容器在场景结束时清理使用。
        public void ClearChildren()
        {
            foreach (Control child in _children)
            {
                child.Parent = null;
            }
            _children.Clear();
            childAddQueue.Clear();
            childRemoveQueue.Clear();
            _isDirty = true;
        }

        //
        // 摘要:
        //     返回当前子控件数量（不包含增删队列中的）。
        public int ChildrenCount => _children.Count;

        //
        // 摘要:
        //     返回子控件只读副本（不包含增删队列中的）。供诊断和外部遍历使用。
        public List<Control> GetChildren()
        {
            return new List<Control>(_children);
        }

        //
        // 摘要:
        //     处理增删队列，将队列中的控件移入/移出 _children。处理完清空队列。
        //     移除时把 child.Parent 置 null。
        private void ProcessQueues()
        {
            if (childAddQueue.Count > 0)
            {
                foreach (Control child in childAddQueue)
                {
                    if (!_children.Contains(child))
                    {
                        _children.Add(child);
                    }
                }
                childAddQueue.Clear();
            }

            if (childRemoveQueue.Count > 0)
            {
                foreach (Control child in childRemoveQueue)
                {
                    if (_children.Remove(child))
                    {
                        child.Parent = null;
                    }
                }
                childRemoveQueue.Clear();
            }
        }

        //
        // 摘要:
        //     先处理增删队列，再调用 OnUpdate，最后遍历子控件 Update。
        //     遍历前做快照避免遍历过程中集合被修改。
        public override void Update()
        {
            ProcessQueues();

            if (_isDirty)
            {
                _isDirty = false;
                Arrange(_hasBeenArranged ? _lastArrangeRect : ClientRectangle);
            }

            base.Update();

            OnUpdate();

            foreach (Control child in _children.ToList())
            {
                child.Update();
            }
        }

        //
        // 摘要:
        //     先调用 OnDraw，再遍历子控件 Draw。
        //     绘制目标为当前 Surface（PatcherYRpp.Surface.Current），子类在 OnDraw 中通过 Surface.Current 进行绘制。
        //     遍历前做快照避免遍历过程中集合被修改。
        public override void Draw()
        {
            base.Draw();

            OnDraw();

            foreach (Control child in _children.ToList())
            {
                child.Draw();
            }
        }

        //
        // 摘要:
        //     供子类重写的更新逻辑，由 Update 调用。
        protected virtual void OnUpdate()
        {
        }

        //
        // 摘要:
        //     供子类重写的绘制逻辑，由 Draw 调用。
        protected virtual void OnDraw()
        {
        }

        //
        // 摘要:
        //     计算自身期望尺寸。默认实现：遍历子控件 Measure，取最大需求；
        //     若自身已指定尺寸则取较大者，再加上 Margin。返回 Point2D(期望宽, 期望高)。
        public virtual Point2D Measure(Point2D availableSize)
        {
            int desiredWidth = 0;
            int desiredHeight = 0;

            foreach (Control child in _children)
            {
                Point2D childDesired = child.Measure(availableSize);
                if (childDesired.X > desiredWidth)
                {
                    desiredWidth = childDesired.X;
                }
                if (childDesired.Y > desiredHeight)
                {
                    desiredHeight = childDesired.Y;
                }
            }

            if (_width > desiredWidth)
            {
                desiredWidth = _width;
            }
            if (_height > desiredHeight)
            {
                desiredHeight = _height;
            }

            desiredWidth += Margin.Left + Margin.Right;
            desiredHeight += Margin.Top + Margin.Bottom;

            return new Point2D(desiredWidth, desiredHeight);
        }

        //
        // 摘要:
        //     根据 HorizontalAlignment/VerticalAlignment 和 Margin，从父级提供的 finalRect
        //     计算自身的对齐后矩形。Stretch 会拉伸到 finalRect 宽/高扣除 Margin。
        protected RectangleStruct ComputeAlignedRect(RectangleStruct finalRect)
        {
            int x = finalRect.X;
            int y = finalRect.Y;
            int width = _width;
            int height = _height;

            if (HorizontalAlignment.HasValue)
            {
                switch (HorizontalAlignment.Value)
                {
                    case Core.HorizontalAlignment.Left:
                        x = finalRect.X + Margin.Left;
                        break;
                    case Core.HorizontalAlignment.Center:
                        x = finalRect.X + (finalRect.Width - width) / 2;
                        break;
                    case Core.HorizontalAlignment.Right:
                        x = finalRect.X + finalRect.Width - width - Margin.Right;
                        break;
                    case Core.HorizontalAlignment.Stretch:
                        x = finalRect.X + Margin.Left;
                        width = finalRect.Width - Margin.Left - Margin.Right;
                        break;
                }
            }

            if (VerticalAlignment.HasValue)
            {
                switch (VerticalAlignment.Value)
                {
                    case Core.VerticalAlignment.Top:
                        y = finalRect.Y + Margin.Top;
                        break;
                    case Core.VerticalAlignment.Center:
                        y = finalRect.Y + (finalRect.Height - height) / 2;
                        break;
                    case Core.VerticalAlignment.Bottom:
                        y = finalRect.Y + finalRect.Height - height - Margin.Bottom;
                        break;
                    case Core.VerticalAlignment.Stretch:
                        y = finalRect.Y + Margin.Top;
                        height = finalRect.Height - Margin.Top - Margin.Bottom;
                        break;
                }
            }

            return new RectangleStruct(x, y, width, height);
        }

        //
        // 摘要:
        //     根据 finalRect 计算自身最终 ClientRectangle，然后递归排列子控件。
        //     记录 finalRect 供 _isDirty 重新布局时复用。
        public virtual RectangleStruct Arrange(RectangleStruct finalRect)
        {
            _lastArrangeRect = finalRect;
            _hasBeenArranged = true;

            ClientRectangle = ComputeAlignedRect(finalRect);

            ArrangeChildren(ClientRectangle);

            return ClientRectangle;
        }

        //
        // 摘要:
        //     排列子控件。默认实现：所有子控件使用相同的 ClientRectangle 作为 finalRect，
        //     子控件各自根据自身对齐方式定位。Panel 等布局容器可重写此方法实现堆叠等布局。
        protected virtual void ArrangeChildren(RectangleStruct clientRect)
        {
            Point2D available = new Point2D(clientRect.Width, clientRect.Height);
            foreach (Control child in _children)
            {
                child.Measure(available);
                child.Arrange(clientRect);
            }
        }

        //
        // 摘要:
        //     命中测试：返回位于指定屏幕坐标的最上层可交互控件。
        //     若控件不可见或设置了 ClickThrough，返回 null（不拦截鼠标）。
        //     若坐标在 ClientRectangle 内，倒序遍历子控件递归命中测试；
        //     子控件命中则返回子控件，否则返回 this。
        public virtual Control HitTest(Point2D pos)
        {
            if (!Visible || ClickThrough)
            {
                return null;
            }

            RectangleStruct rect = ClientRectangle;
            if (pos.X < rect.X || pos.X >= rect.X + rect.Width ||
                pos.Y < rect.Y || pos.Y >= rect.Y + rect.Height)
            {
                return null;
            }

            for (int i = _children.Count - 1; i >= 0; i--)
            {
                Control hit = _children[i].HitTest(pos);
                if (hit != null)
                {
                    return hit;
                }
            }

            return this;
        }

        #region Mouse Virtual Methods

        //
        // 摘要:
        //     鼠标进入控件时触发。供子类重写。
        //     使用 protected internal 以便 UIRoot 在同程序集内对其他 Control 实例调用。
        protected internal virtual void OnMouseEnter()
        {
        }

        //
        // 摘要:
        //     鼠标离开控件时触发。供子类重写。
        protected internal virtual void OnMouseLeave()
        {
        }

        //
        // 摘要:
        //     鼠标按下时触发。供子类重写。
        protected internal virtual void OnMouseDown(MouseEventArgs e)
        {
        }

        //
        // 摘要:
        //     鼠标释放时触发。供子类重写。
        protected internal virtual void OnMouseUp(MouseEventArgs e)
        {
        }

        //
        // 摘要:
        //     点击（同一控件 Down+Up）时触发。供子类重写。
        protected internal virtual void OnClick()
        {
        }

        //
        // 摘要:
        //     鼠标悬停超过阈值帧数时触发。供子类重写。
        protected internal virtual void OnHover()
        {
        }

        //
        // 摘要:
        //     返回 Tooltip 显示的文本行。默认返回 null（无 Tooltip）。
        //     子类可重写此方法返回多行文本，由 UIRoot 在 Hover 触发时调用。
        public virtual List<string> GetTooltipLines()
        {
            return null;
        }

        #endregion

    }

    //
    // 摘要:
    //     外边距结构，Arrange 时按对齐方式扣除。
    public struct MarginStruct
    {
        public MarginStruct(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
