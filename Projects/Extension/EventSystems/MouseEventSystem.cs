using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.EventSystems
{
    public enum MouseButtons
    {
        None = 0,
        Left = 1,
        Right = 2,
    }

    public class MouseMoveEvent : EventBase
    {
        public override string Name => "MouseMoveEvent";
        public override string Description => "Raised when mouse update";
    }
    public class MouseDownEvent : EventBase
    {
        public override string Name => "MouseDownEvent";
        public override string Description => "Raised when mouse down";
    }

    public class MouseUpEvent : EventBase
    {
        public override string Name => "MouseUpEvent";
        public override string Description => "Raised when mouse up";
    }

    public class MouseWheelEvent : EventBase
    {
        public override string Name => "MouseWheelEvent";
        public override string Description => "Raised when mouse wheel rolled";
    }

    public class MouseEventArgs : EventArgs
    {
        public Point2D Position;
        public MouseButtons Button;

        public MouseEventArgs(Point2D position, MouseButtons button)
        {
            Position = position;
            Button = button;
        }
    }

    public class MouseWheelEventArgs : EventArgs
    {
        /// <summary>滚轮增量，正数向前（放大），负数向后（缩小）。</summary>
        public int Delta;

        /// <summary>鼠标位置（客户区坐标）。</summary>
        public Point2D Position;

        public MouseWheelEventArgs(int delta, Point2D position)
        {
            Delta = delta;
            Position = position;
        }
    }


    public class MouseEventSystem : EventSystem
    {
        public MouseEventSystem()
        {
            MouseMoveEvent = new MouseMoveEvent();
            MouseDownEvent = new MouseDownEvent();
            MouseUpEvent = new MouseUpEvent();
            MouseWheelEvent = new MouseWheelEvent();
        }

        public MouseMoveEvent MouseMoveEvent { get; }
        public MouseDownEvent MouseDownEvent { get; }
        public MouseUpEvent MouseUpEvent { get; }
        public MouseWheelEvent MouseWheelEvent { get; }
    }
}
