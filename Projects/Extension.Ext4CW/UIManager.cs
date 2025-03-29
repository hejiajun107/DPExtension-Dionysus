using DynamicPatcher;
using Extension.EventSystems;
using PatcherYRpp.FileFormats;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Extension.CW
{
    [Serializable]
    public class UIManager
    {
        public void Init()
        {
            //EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.SidebarRenderEvent, OnSidebarRender);
            //EventSystem.Tactical.AddTemporaryHandler(EventSystem.Tactical.TactcialRenderEvent, OnTacticalRender);
        }

        public void OnSidebarRender(object sender, EventArgs e)
        {
            
        }

        public void OnTacticalRender(object sender, EventArgs e)
        {
            if(e is TacticalEventArgs args)
            {
                if (args.IsBeginRender)
                {
                    TacticalEarlyRender();
                }
                else
                {
                    TacticalLateRender();
                }
            }
        }

        private void TacticalEarlyRender()
        {
            Logger.Log("TacticalEarlyRender");
        }

        private void TacticalLateRender()
        {
            //Logger.Log("画了");
            //if (FileSystem.TyrLoadSHPFile("XHCDBAR.shp", out Pointer<SHPStruct> pCustomSHP))
            //{
            //    Pointer<Surface> pSurface = Surface.Current;

            //    RectangleStruct rect = pSurface.Ref.GetRect();
            //    Logger.Log(Surface.ViewBound.Width.ToString() + " " + Surface.ViewBound.Height.ToString());
            //    Point2D point = new Point2D(20, Surface.ViewBound.Height - 100);
            //    {
            //        var frame = 1;
            //        pSurface.Ref.DrawSHP(FileSystem.UNITx_PAL, pCustomSHP, frame, point, rect.GetThisPointer());
            //    }
            //}
        }

    }
}
