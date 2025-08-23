using DynamicPatcher;
using Extension.EventSystems;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Experimental
{
    [Serializable]
    [ScriptAlias(nameof(PCXDrawingTestScript))]
    public class PCXDrawingTestScript : TechnoScriptable
    {
        public PCXDrawingTestScript(TechnoExt owner) : base(owner)
        {
        }

        public override void Awake()
        {
            EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
            base.Awake();
        }

        public override void OnDestroy()
        {
            EventSystem.GScreen.RemovePermanentHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
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

                var loaded = PCX.Instance.LoadFile("srock2icon.pcx");
           
                var pcx = PCX.Instance.GetSurface("srock2icon.pcx",Pointer<BytePalette>.Zero);
                RectangleStruct pcxBounds = new RectangleStruct(0, 0, pcx.Ref.Base.Base.Width, pcx.Ref.Base.Base.Height);
                Pointer<Surface> pSurface = Surface.Current;
                RectangleStruct rect = pSurface.Ref.GetRect();
                       
                Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.BaseAbstract.GetCoords() + new CoordStruct(0, 0, 300));
                var source = new RectangleStruct(point.X, point.Y, pcx.Ref.Base.Base.Width, pcx.Ref.Base.Base.Height);
                PCX.Instance.BlitToSurface(source.GetThisPointer(), pSurface.Convert<DSurface>(), pcx);
            }
        }


    }
}
