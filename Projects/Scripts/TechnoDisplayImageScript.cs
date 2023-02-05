using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using Extension.Coroutines;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Newtonsoft.Json;
using PatcherYRpp;
using PatcherYRpp.FileFormats;

namespace Scripts
{
    //[GlobalScriptable(typeof(TechnoExt))]
    [ScriptAlias(nameof(TechnoDisplayImageScript))]
    [Serializable]
    public class TechnoDisplayImageScript : TechnoScriptable
    {
        public TechnoDisplayImageScript(TechnoExt owner) : base(owner) { }

        public override void Awake()
        {
            ini = Owner.GameObject.CreateRulesIniComponentWith<DisplayData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            if (!string.IsNullOrEmpty(ini.Data.DrawingText))
            {
                CreateTexture();
            }
        }


        static TechnoDisplayImageScript()
        {
            var path = "./DynamicPatcher/TechnoDisplay.json";
            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    var json = sr.ReadToEnd();
                    if (!string.IsNullOrEmpty(json))
                    {
                        dics = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        if (dics == null)
                        {
                            dics = new Dictionary<string, string>();
                        }
                    }
                }
            }
        }

        private static Dictionary<string, string> dics = new Dictionary<string, string>();

        //private static Dictionary<string, YRClassHandle<BSurface>> surfaceCache = new Dictionary<string, YRClassHandle<BSurface>>();


        public INIComponentWith<DisplayData> ini;


        private YRClassHandle<BSurface> surface;
        private bool loaded = false;

        private const int widgetWidth = 300;

        private int offsetY = 0;

        public override void Start()
        {
            if (string.IsNullOrEmpty(ini.Data.DrawingText))
            {
                DetachFromParent();
            }
        }

        public override void OnUpdate()
        {

        }

        public void CreateTexture()
        {
            if (surface == null)
            {
                Font font = new Font("Microsoft YaHei", 8, FontStyle.Regular);

                //ini.Data.DrawingText
                if (dics.ContainsKey(ini.Data.DrawingText))
                {
                    var text = dics[ini.Data.DrawingText];

                    var stext = string.Empty;

                    //var sizeF = g1.MeasureString("你好", font, new SizeF(100, 1000), StringFormat.GenericTypographic);

                    var sizeF = EstimateSize(text, out stext);
                    offsetY = (int)sizeF.Height;

                    var bitmap = new Bitmap((int)sizeF.Width + 40, (int)sizeF.Height + 2);
                    Graphics g = Graphics.FromImage(bitmap);
                    StringFormat format = new StringFormat(StringFormatFlags.NoClip);
                    SolidBrush blackBrush = new SolidBrush(Color.FromArgb(255, 30, 30, 30));
                    SolidBrush whiteBrush = new SolidBrush(Color.White);
                    Pen whitePen = new Pen(whiteBrush, 2);
                    var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                    g.FillRectangle(blackBrush, rect);
                    g.DrawRectangle(whitePen, rect);
                    g.DrawString(stext, font, whiteBrush, PointF.Empty, format);
                    g.Save();

                    bitmap = bitmap.Clone(rect, PixelFormat.Format16bppRgb565);
                    surface = new YRClassHandle<BSurface>(bitmap.Width, bitmap.Height);


                    var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

                    surface.Ref.Allocate(2);
                    Helpers.Copy(data.Scan0, surface.Ref.BaseSurface.Buffer, data.Stride * data.Height);

                    bitmap.UnlockBits(data);

                    loaded = true;
                }
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
            return new SizeF(widgetWidth, lines.Count * 15 > 1000 ? 1000 : lines.Count * 15);

        }

        private bool IsCnChar(char ch)
        {
            return ch >= 0x4e00 && ch <= 0x9fbb;
        }

        public override void OnRender()
        {
            if (loaded)
            {
                if (Owner.OwnerObject.Ref.Base.IsSelected)
                {
                    if (Owner.OwnerObject.Ref.Base.InLimbo || !Owner.OwnerObject.Ref.Base.IsOnMap)
                        return;
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
        }
    }

    [Serializable]
    public class DisplayData : INIAutoConfig
    {
        [INIField(Key = "TechnoDisplay.DrawingText")]
        public string DrawingText;
    }
}