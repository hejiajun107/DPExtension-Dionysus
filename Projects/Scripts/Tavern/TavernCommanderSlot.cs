using Extension.EventSystems;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using Scripts.Tavern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Scripts.Tavern
{
    [ScriptAlias(nameof(TavernCommanderSlot))]
    [Serializable]
    public class TavernCommanderSlot : TechnoScriptable
    {
        public TavernCommanderSlot(TechnoExt owner) : base(owner)
        {
        }

        public ComandComponent Commander { get; private set; }

        private bool _registered = false;

        public override void OnUpdate()
        {
            if (!Register())
                return;
        }

        public bool Register()
        {
            if (_registered)
                return true;

            if (TavernGameManager.Instance is null)
                return false;

            var node = TavernGameManager.Instance.FindPlayerNodeByHouse(Owner.OwnerObject.Ref.Owner);

            if (node is null)
                return false;

            node.RegisterCommanderSlot(this);
            _registered = true;

            return true;
        }
 
        public void InitComander(string key)
        {
            if (Commander is not null)
                return;
            Commander = new ComandComponent() { Key = key,Owner = Owner };
            Commander.OnInit();
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

                if(TavernGameManager.Instance is not null)
                {
                    if (TavernGameManager.Instance.GameStatus == GameStatus.ChooseCommander)
                    {
                        Pointer<Surface> pSurface = Surface.Current;
                        var source = pSurface.Ref.GetRect();
                        Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(-100, 0, 1000));
                        var point2 = new Point2D(point.X, point.Y);
                        pSurface.Ref.DrawText(TavernGameManager.Instance.ComandderSelectTicks.ToString(), source.GetThisPointer(), point2.GetThisPointer(), new ColorStruct(0, 255, 0));
                    }
                }

                if(Commander is null)
                {
                    if (FileSystem.TyrLoadSHPFile("txtcommand.shp", out Pointer<SHPStruct> pCustomSHP))
                    {
                        Pointer<Surface> pSurface = Surface.Current;
                        RectangleStruct rect = pSurface.Ref.GetRect();
                        Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(-450, 0, 850));
                        pSurface.Ref.DrawSHP(FileSystem.UNITx_PAL, pCustomSHP, 0, point, rect.GetThisPointer(), BlitterFlags.None);
                    }
                }
                else
                {
                    GameUtil.RenderCameo(TavernGameManager.Instance.GetTechnoCameo(Commander.Key), Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(-250, 0, 100));
                }
            }
        }



    }
}
