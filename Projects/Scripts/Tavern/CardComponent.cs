using DynamicPatcher;
using Extension.EventSystems;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using Scripts.Tavern;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Tavern
{
    [ScriptAlias(nameof(CardComponent))]
    [Serializable]
    public class CardComponent : TechnoScriptable
    {
        public CardComponent(TechnoExt owner) : base(owner)
        {
        }


        private static Dictionary<string, YRClassHandle<BSurface>> surfacesCache = new Dictionary<string, YRClassHandle<BSurface>>();
        private const int widgetWidth = 300;
        private int offsetY = 500;

        public CardType CardType { get; set; }

        public void ChangeOwner(TechnoExt owner)
        {
            Owner = owner;
        }

        public override void OnUpdate()
        {
            if (CardType is null)
                return;
           if(!surfacesCache.ContainsKey(CardType.Key) && !string.IsNullOrWhiteSpace(CardType.Key))
           {
                CreateTexture();
           }
              
            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            
        }

        public override void Awake()
        {
            EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
            base.Awake();
        }
        public override void OnDestroy()
        {
            EventSystem.GScreen.RemoveTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
            base.OnDestroy();
        }

        public void OnGScreenRender(object sender, EventArgs args)
        {
            if (args is GScreenEventArgs gScreenEvtArgs)
            {
                if (!gScreenEvtArgs.IsLateRender)
                {
                    return;
                }

                if(CardType is not null)
                {
                    RenderPCX(CardType.Cameo, -250, 0, 50);
                }

                if(Owner.OwnerObject.Ref.Owner == HouseClass.Player && Owner.OwnerObject.Ref.Base.IsSelected)
                {
                    RenderDesc();
                }
            }
        }


        public void CreateTexture()
        {
            var key = CardType.Key;
            if (!surfacesCache.ContainsKey(key))
            {
                {
                    Font font = new Font("Microsoft YaHei", 8, FontStyle.Regular);

                    var text = CardType.Description;

                    var stext = string.Empty;

                    var sizeF = EstimateSize(text, out stext);

                    int widthRect = (int)sizeF.Width + 40;
                    int heightRect = (int)sizeF.Height + 2;

                    var bitmap = new Bitmap((int)sizeF.Width + 40, (int)sizeF.Height + 2);

                    Graphics g = Graphics.FromImage(bitmap);
                    StringFormat format = new StringFormat(StringFormatFlags.NoClip);
                    SolidBrush blackBrush = new SolidBrush(Color.FromArgb(255, 30, 30, 30));
                    SolidBrush whiteBrush = new SolidBrush(Color.White);
                    Pen whitePen = new Pen(whiteBrush, 2);

                    var fillrect = new Rectangle(0, 0, widthRect, heightRect);
                    var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                    g.FillRectangle(blackBrush, fillrect);
                    g.DrawRectangle(whitePen, fillrect);
                    g.DrawString(stext, font, whiteBrush, PointF.Empty, format);
                       
                    g.Save();

                    bitmap = bitmap.Clone(rect, PixelFormat.Format16bppRgb565);
                    var surface = new YRClassHandle<BSurface>(bitmap.Width, bitmap.Height);


                    var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

                    surface.Ref.Allocate(2);
                    Helpers.Copy(data.Scan0, surface.Ref.BaseSurface.Buffer, data.Stride * data.Height);

                    bitmap.UnlockBits(data);

                    surfacesCache.Add(key, surface);

                }
            }
            else
            {
                //offsetY = offsetYCache.ContainsKey(key) ? offsetYCache[key] : 0;
            }
        }

        private void RenderPCX(string pcxName, int offsetX, int offsetY, int offsetZ)
        {
            var loaded = PCX.Instance.LoadFile(pcxName);
            if (!loaded)
            {
                Logger.Log($"{pcxName}不存在");
                return;
            }
            var pcx = PCX.Instance.GetSurface(pcxName, Pointer<BytePalette>.Zero);
            RectangleStruct pcxBounds = new RectangleStruct(0, 0, pcx.Ref.Base.Base.Width, pcx.Ref.Base.Base.Height);
            Pointer<Surface> pSurface = Surface.Current;
            RectangleStruct rect = pSurface.Ref.GetRect();
            Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.BaseAbstract.GetCoords() + new CoordStruct(offsetX, offsetY, offsetZ));
            var source = new RectangleStruct(point.X, point.Y, pcx.Ref.Base.Base.Width, pcx.Ref.Base.Base.Height);
            PCX.Instance.BlitToSurface(source.GetThisPointer(), pSurface.Convert<DSurface>(), pcx);
        }

        private void RenderDesc()
        {
            var key = CardType.Key;
            var surface = surfacesCache.ContainsKey(key) ? surfacesCache[key] : null;
            if (surface != null)
            {
                ref var srcSurface = ref surface.Ref.BaseSurface;

                Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.BaseAbstract.GetCoords());
                //point += new Point2D(0, -srcSurface.Height);

                //var rect = new Rectangle(point.X - srcSurface.Width / 2, point.Y - srcSurface.Height / 2,
                //    srcSurface.Width, srcSurface.Height);
                var rect = new Rectangle(point.X - srcSurface.Width / 2, point.Y - offsetY - 50,
                        srcSurface.Width, srcSurface.Height);

                rect = Rectangle.Intersect(rect, new Rectangle(0, 0, Surface.Current.Ref.Width, Surface.Current.Ref.Height));

                var drawRect = new RectangleStruct(rect.X, rect.Y, rect.Width, rect.Height);

                Surface.Current.Ref.Blit(Surface.ViewBound, drawRect
                    , surface.Pointer.Convert<Surface>(), srcSurface.GetRect(), srcSurface.GetRect(), true, true);
            }
        }


        private SizeF EstimateSize(string text, out string lined)
        {

            //一行9个中文18个英文
            var cnL = 100 / 9;
            var enL = 100 / 18;
            var lines = new List<string>();

            var oriLines = text.Split('@').ToList();
            var sb = new StringBuilder();
            var length = 0;
            foreach (var line in oriLines)
            {
                foreach (var chr in line)
                {
                    var chlength = IsCnChar(chr) ? cnL : enL;
                    if (length + chlength > widgetWidth)
                    {
                        lines.Add(sb.ToString());
                        length = 0;
                        sb.Clear();
                    }
                    length += chlength;
                    sb.Append(chr);
                }
                if (length > 0)
                {
                    lines.Add(sb.ToString());
                    length = 0;
                    sb.Clear();
                }
            }

            lined = string.Join("\n", lines);
            //return new SizeF(widgetWidth, lines.Count * 15 > 1000 ? 1000 : lines.Count * 15);
            return new SizeF(widgetWidth, 500);
        }

        private bool IsCnChar(char ch)
        {
            return ch >= 0x4e00 && ch <= 0x9fbb;
        }

        public void RelaseCompnent()
        {
            EventSystem.GScreen.RemoveTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
        }

    }

    [Serializable]
    public class CardType
    {
        /// <summary>
        /// 卡牌注册名，需要唯一，可与单位注册名一致
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 对应单位的注册名
        /// </summary>
        public string TechnoType { get; set; }

        /// <summary>
        /// 图标,仅PCX
        /// </summary>
        public string Cameo { get; set; }

        /// <summary>
        /// 对应脚本
        /// </summary>
        public string Scripts { get; set; }
        /// <summary>
        /// 等级（多少级后才会出现，从1开始）
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 在卡池中的数量
        /// </summary>
        public int Amount { get; set; }
        /// <summary>
        /// 卡牌带有的标签
        /// </summary>
        public List<string> Tags { get; set; }
    }

}
