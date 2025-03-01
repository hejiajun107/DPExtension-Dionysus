using DynamicPatcher;
using Extension.EventSystems;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using System;
using System.Numerics;

namespace Scripts.China
{
    [ScriptAlias(nameof(XHWarShipScript))]
    [Serializable]
    public class XHWarShipScript : TechnoScriptable
    {
        public XHWarShipScript(TechnoExt owner) : base(owner)
        {
           
        }

        private bool InIonStorm;
        private int Delay = 0;

        private int charge = 0;

        private int chargeMax = 1000;

        public override void Awake()
        {
            EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
        }

        public override void OnDestroy()
        {
            EventSystem.GScreen.RemoveTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
        }

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            if(Owner.TryGetHouseGlobalExtension(out var houseComponent))
            {
                houseComponent.RegisterEpicUnit(Owner);
            }

            
        }

        public override void OnUpdate()
        {
            if (charge < chargeMax)
            {
                charge++;
            }

            //UpdateAnim();
        }

        public override void OnRemove()
        {
            if (Owner.TryGetHouseGlobalExtension(out var houseComponent))
            {
                houseComponent.RemoveEpicUnit(Owner);
            }
        }


        
        public void OnGScreenRender(object sender, EventArgs args)
        {
            if(args is GScreenEventArgs gScreenEvtArgs)
            {
                if (!gScreenEvtArgs.IsLateRender)
                {
                    return;
                }

                if (!Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap && Owner.OwnerObject.Ref.Owner == HouseClass.Player && Owner.OwnerObject.Ref.Base.IsSelected)
                {
                    if (FileSystem.TyrLoadSHPFile("XHCDBAR.shp", out Pointer<SHPStruct> pCustomSHP))
                    {
                        Pointer<Surface> pSurface = Surface.Current;
                        RectangleStruct rect = pSurface.Ref.GetRect();
                        Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.BaseAbstract.GetCoords() + new CoordStruct(0, 0, 300));
                        {
                            var frame = (int)((charge / (double)chargeMax) * 100);

                            pSurface.Ref.DrawSHP(FileSystem.UNITx_PAL, pCustomSHP, frame, point, rect.GetThisPointer());
                        }
                    }
                }
            }
        }

    }
}
