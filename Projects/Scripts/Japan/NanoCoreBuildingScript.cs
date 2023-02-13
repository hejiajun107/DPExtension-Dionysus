using DynamicPatcher;
using Extension.CW;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(NanoCoreBuildingScript))]
    [Serializable]
    public class NanoCoreBuildingScript : TechnoScriptable
    {
        public NanoCoreBuildingScript(TechnoExt owner) : base(owner)
        {
        }

        private INIComponentWith<NanoCoreBuildData> INI;

        public override void Awake()
        {
            INI = GameObject.CreateRulesIniComponentWith<NanoCoreBuildData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            base.Awake();
        }

        public override void Start()
        {
            var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            var gext = Owner.GameObject.GetComponent<TechnoGlobalExtension>();
            if(gext != null)
            {
                if(gext.IsDeployedFrom == true)
                {
                    gext.IgnoreBaseNormal = true;
                    DetachFromParent();
                    return;
                }

            }

            if (Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
            {
                var technos = ObjectFinder.FindTechnosNear(coord, 7 * Game.CellSize);
                foreach (var pobj in technos)
                {
                    if (pobj.CastToTechno(out var ptechno))
                    {
                        if (ptechno.Ref.Owner != Owner.OwnerObject.Ref.Owner)
                            continue;

                        if (ptechno.Ref.Type.Ref.Base.Base.ID != "JPNMCT")
                            continue;

                        var nanoType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(INI.Data.NanoCore);
                        if (nanoType.IsNull)
                            continue;

                        var pNano = nanoType.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();
                        var centerCoord = ptechno.Ref.Base.Base.GetCoords();
                        var cell = CellClass.Coord2Cell(centerCoord);
                        if (TechnoPlacer.PlaceTechnoNear(pNano, cell, false))
                        {
                            if (pNano.Ref.Base.Base.GetCoords().DistanceFrom(centerCoord) > 3 * Game.CellSize)
                            {
                                pNano.Ref.Base.UnInit();
                                continue;
                            }

                            Owner.OwnerObject.Ref.Base.Remove();

                            if (pNano.CastToFoot(out var pfoot))
                            {
                                if (MapClass.Instance.TryGetCellAt(coord, out var pcell))
                                {
                                    pfoot.Ref.MoveTo(coord);
                                    Owner.OwnerObject.Convert<MissionClass>().Ref.QueueMission(Mission.Move, true);
                                }
                            }

                            Owner.OwnerObject.Ref.Base.UnInit();


                            break ;
                        }
                        else
                        {
                            pNano.Ref.Base.UnInit();
                        }

                    }
                }
            }

            DetachFromParent();
        }

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {

        }
    }

    [Serializable]
    public class NanoCoreBuildData : INIAutoConfig
    {
        [INIField(Key = "Building.NanoCore")]
        public string NanoCore;
    }

    [ScriptAlias(nameof(NanoCoreScript))]
    [Serializable]
    public class NanoCoreScript : TechnoScriptable
    {
        public NanoCoreScript(TechnoExt owner) : base(owner)
        {
        }

        private string cameo;

        private string cameoPCX;

        public override void Awake()
        {

            var deploysInto = Owner.GameObject.CreateRulesIniComponentWith<NanoCoreData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID).Data?.DeploysInto;
            if (!string.IsNullOrEmpty(deploysInto))
            {
                var image = Owner.GameObject.CreateRulesIniComponentWith<NanoCoreBuilding>(deploysInto).Data?.Image;
                if (string.IsNullOrEmpty(image))
                {
                    image = deploysInto;
                }

                var art = Owner.GameObject.CreateArtIniComponentWith<NanoCoreDataArt>(image);

                cameoPCX = art.Data.CameoPCX;
                cameo = !string.IsNullOrEmpty(art.Data.Cameo) ? art.Data.Cameo : (!string.IsNullOrEmpty(art.Data.CameoPCX) ? art.Data.CameoPCX.Replace(".pcx", "") : string.Empty);
            }
        }

        public override void OnRender()
        {
            //Logger.Log(cameoPCX);
            //if (Owner.OwnerObject.Ref.Base.IsSelected && !Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap)
            //{
            //}
            //else
            //{
            //    return;
            //}
            //if (!string.IsNullOrEmpty(cameoPCX))
            //{
            //    Logger.Log("before");
            //    PCX.Instance.Ref.LoadFile(cameoPCX);
            //    Logger.Log("load");
            //    var surface = PCX.Instance.Ref.GetSurface(cameoPCX, Pointer<BytePalette>.Zero);
            //    Logger.Log("surface");

            //    if (surface.IsNull)
            //    {
            //        Logger.Log("GG");
            //        return;
            //    }
            //    ref var srcSurface = ref surface.Ref.BaseSurface;


            //    Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.BaseAbstract.GetCoords());


            //    var rect = new Rectangle(point.X - srcSurface.Width / 2, point.Y - 48 - 50,
            //                srcSurface.Width, srcSurface.Height);

            //    rect = Rectangle.Intersect(rect, new Rectangle(0, 0, Surface.Current.Ref.Width, Surface.Current.Ref.Height));

            //    var drawRect = new RectangleStruct(rect.X, rect.Y, rect.Width, rect.Height);

            //    Surface.Current.Ref.Blit(Surface.ViewBound, drawRect
            //        , surface.Convert<Surface>(), srcSurface.GetRect(), srcSurface.GetRect(), true, true);


            //}


            if (string.IsNullOrEmpty(cameo))
                return;


            if (Owner.OwnerObject.Ref.Owner == HouseClass.Player)
            {
                if (Owner.OwnerObject.Ref.Base.IsSelected && !Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap)
                {

                    var surface = PngIconLoader.GetSurface(cameo);
                    if (surface != null)
                    {
                        ref var srcSurface = ref surface.Ref.BaseSurface;

                        Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.BaseAbstract.GetCoords());

                        //var rect = new Rectangle(point.X - srcSurface.Width / 2, point.Y - srcSurface.Height / 2,
                        //    srcSurface.Width, srcSurface.Height);
                        var rect = new Rectangle(point.X - srcSurface.Width / 2, point.Y - 48 - 50,
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



    [Serializable]
    public class NanoCoreData : INIAutoConfig
    {
        [INIField(Key = "DeploysInto")]
        public string DeploysInto;
    }

    [Serializable]
    public class NanoCoreBuilding : INIAutoConfig
    {
        [INIField(Key = "Image")]
        public string Image;
    }

    [Serializable]
    public class NanoCoreDataArt : INIAutoConfig
    {
        [INIField(Key = "Cameo")]

        public string Cameo;

        [INIField(Key = "CameoPCX")]
        public string CameoPCX;
    }


}
