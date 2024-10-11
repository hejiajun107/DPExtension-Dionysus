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
        private List<Control> _children = new List<Control>();

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



        #region Attributes
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



    }
}
