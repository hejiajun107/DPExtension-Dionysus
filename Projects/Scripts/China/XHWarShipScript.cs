using DynamicPatcher;
using Extension.EventSystems;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using PatcherYRpp.Utilities;
using System;
using System.Linq;
using System.Numerics;
using System.Reflection;

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

        private int charge = 500;

        private int chargeMax = 1000;

        public bool CanFire { get
            {
                return charge >= chargeMax;
            } 
        }

        public void Refresh()
        {
            charge = 0;
        }

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
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                if (Owner.OwnerObject.Ref.Owner.Ref.IsControlledByCurrentPlayer())
                {
                    var swType = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("XHCallFireSW");
                    Pointer<SuperClass> pSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(swType);
                    pSuper.Ref.IsCharged = true;
                    MapClass.UnselectAll();
                    Game.CurrentSWType = swType.Ref.ArrayIndex;
                }
            }

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


    [ScriptAlias(nameof(XHCallFireSWScript))]
    [Serializable]
    public class XHCallFireSWScript : SuperWeaponScriptable
    {
        public XHCallFireSWScript(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            if (Owner.OwnerObject.Ref.Owner.TryGetHouseGlobalExtension(out var houseGlobalExtension))
            {
                var epics = houseGlobalExtension.FindEpicUnitByType("CNXHWSHIP");
                var coord = CellClass.Cell2Coord(cell);
                foreach(var ep in epics)
                {
                    if(coord.BigDistanceForm(ep.OwnerObject.Ref.Base.Base.GetCoords()) > Game.CellSize * 45)
                    {
                        continue;
                    }

                    var component = ep.GameObject.GetComponents<XHWarShipScript>().FirstOrDefault();
                    if(component!= null)
                    {
                        if (component.CanFire)
                        {
                            if(MapClass.Instance.TryGetCellAt(cell,out var pcell))
                            {
                                ep.OwnerObject.Ref.SetTarget(default);
                                component.Refresh();
                                ep.OwnerObject.Ref.Ammo = 1;
                                ep.OwnerObject.Ref.SetTarget(pcell.Convert<AbstractClass>());
                            }
                        }
                    }
                }
            }
            base.OnLaunch(cell, isPlayer);
        }
    }
}
