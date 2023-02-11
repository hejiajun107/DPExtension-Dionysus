using Extension.INI;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Ext4CW
{
    public class PngIconLoader
    {
        private static Dictionary<string, YRClassHandle<BSurface>> cache = new Dictionary<string, YRClassHandle<BSurface>>();

        public static YRClassHandle<BSurface> GetSurface(string key)
        {
            if (!cache.ContainsKey(key))
            {
                if (!string.IsNullOrEmpty(key))
                {
                    string cameoPath = $"./Resources/Cameos/{key}.jpeg";
                    if (File.Exists(cameoPath))
                    {
                        var bitmap = new Bitmap(cameoPath);
                        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                        bitmap = bitmap.Clone(rect, PixelFormat.Format16bppRgb565);
                        var surface = new YRClassHandle<BSurface>(bitmap.Width, bitmap.Height);

                        var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

                        surface.Ref.Allocate(2);
                        Helpers.Copy(data.Scan0, surface.Ref.BaseSurface.Buffer, data.Stride * data.Height);

                        bitmap.UnlockBits(data);

                        cache.Add(key, surface);
                        return surface;
                    }
                }

                cache.Add(key, null);
                return null;
            }
            else
            {
                return cache[key];
            }
        }

    }
}
