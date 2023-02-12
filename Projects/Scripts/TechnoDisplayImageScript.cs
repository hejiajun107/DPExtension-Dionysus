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
            art = Owner.GameObject.CreateArtIniComponentWith<DisplayDataArt>(string.IsNullOrEmpty(ini.Data.Image)? Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID : ini.Data.Image);

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
        public INIComponentWith<DisplayDataArt> art;


        private static Dictionary<string, YRClassHandle<BSurface>> surfacesCache = new Dictionary<string, YRClassHandle<BSurface>>();
        private static Dictionary<string, int> offsetYCache = new Dictionary<string, int>();

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
            var key = Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID;
            if (!surfacesCache.ContainsKey(key))
            {
                //if (surface == null)
                {
                    Font font = new Font("Microsoft YaHei", 8, FontStyle.Regular);

                    //ini.Data.DrawingText
                    if (dics.ContainsKey(ini.Data.DrawingText))
                    {
                        var text = dics[ini.Data.DrawingText];

                        var stext = string.Empty;

                        //var sizeF = g1.MeasureString("你好", font, new SizeF(100, 1000), StringFormat.GenericTypographic);

                        var sizeF = EstimateSize(text, out stext);

                        int widthRect = (int)sizeF.Width + 40;
                        int heightRect = (int)sizeF.Height + 2;

                        var bitmap = new Bitmap((int)sizeF.Width + 40, (int)sizeF.Height + 2 + (ini.Data.ShowCameo ? 60 : 0));

                        offsetY = bitmap.Height;

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
                        if (ini.Data.ShowCameo)
                        {
                            var cameoName = ini.Data.CameoBitmap;
                            if(string.IsNullOrEmpty(cameoName))
                            {
                                cameoName = !string.IsNullOrEmpty(art.Data.Cameo) ? art.Data.Cameo + ".png" : (!string.IsNullOrEmpty(art.Data.CameoPCX) ? art.Data.CameoPCX.ToLower().Replace(".pcx", ".png") : string.Empty);
                            }
                            if (!string.IsNullOrEmpty(cameoName))
                            {
                                string cameoPath = $"./DynamicPatcher/Cameos/{cameoName}";
                                if (File.Exists(cameoPath))
                                {
                                    var cameoImage = new Bitmap(cameoPath);
                                    g.DrawImage(cameoImage, new Point((widthRect - 60) / 2, (int)sizeF.Height + 12));
                                }
                            }
                        }

                        g.Save();

                        bitmap = bitmap.Clone(rect, PixelFormat.Format16bppRgb565);
                        var surface = new YRClassHandle<BSurface>(bitmap.Width, bitmap.Height);


                        var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

                        surface.Ref.Allocate(2);
                        Helpers.Copy(data.Scan0, surface.Ref.BaseSurface.Buffer, data.Stride * data.Height);

                        bitmap.UnlockBits(data);

                        offsetYCache.Add(key, offsetY);
                        surfacesCache.Add(key, surface);

                        loaded = true;
                    }
                }

            }
            else
            {
                if (dics.ContainsKey(ini.Data.DrawingText))
                {
                    loaded = true;
                    offsetY = offsetYCache.ContainsKey(key) ? offsetYCache[key] : 0;
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
            var key = Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID;


            if (Owner.OwnerObject.Ref.Owner == HouseClass.Player)
            {
                if (Owner.OwnerObject.Ref.Base.IsSelected && !Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap)
                {
                    if (loaded)
                    {
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

                }

            }
        }
    }

    [Serializable]
    public class DisplayData : INIAutoConfig
    {
        [INIField(Key = "TechnoDisplay.DrawingText")]
        public string DrawingText;

        [INIField(Key = "TechnoDisplay.ShowCameo")]
        public bool ShowCameo = true;

        [INIField(Key = "TechnoDisplay.CameoBitmap")]
        public string CameoBitmap = "";

        [INIField(Key = "Image")]
        public string Image;
    }

    [Serializable]
    public class DisplayDataArt : INIAutoConfig
    {
        [INIField(Key = "Cameo")]

        public string Cameo;

        [INIField(Key = "CameoPCX")]
        public string CameoPCX;
    }
}