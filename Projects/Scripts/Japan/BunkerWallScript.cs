using DynamicPatcher;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(BunkerWallScript))]
    [Serializable]
    public class BunkerWallScript : TechnoScriptable
    {
        public BunkerWallScript(TechnoExt owner) : base(owner)
        {
        }

        private BunkerWallData Data;

        public override void Awake()
        {
            var settingINI = this.CreateRulesIniComponentWith<BunkerWallData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            Data = settingINI.Data;
        }

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            placeCoord = coord;
            putted = true;
        }

        private bool putted = false;
        private bool toreplace = false;
        private CoordStruct placeCoord;
        private bool inited = false;


        public override void OnUpdate()
        {
            if (putted == true && toreplace == false)
            {
                toreplace = true;
                return;
            }

            if (inited)
            {
                return;
            }

            inited = true;



            if (MapClass.Instance.TryGetCellAt(placeCoord, out var pCell))
            {
                var building = Owner.OwnerObject.Convert<BuildingClass>();

                Pointer<TechnoTypeClass> type = Owner.OwnerObject.Ref.Type;

                if (building.Ref.GetCurrentFrame() == 12)
                {
                    type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.WestEastWall);
                    var west = FindWallPost(pCell, Direction.N);
                    var east = FindWallPost(pCell, Direction.S);
                    if (west.Item1 == true && east.Item1 == true)
                    {
                        if (west.Item2 == east.Item2)
                        {
                            if (west.Item2 != Data.BunkerWallPost)
                            {
                                return;
                            }
                        }
                        else
                        {
                            //不同种类不允许链接
                            Owner.OwnerObject.Ref.Base.Remove();
                            Owner.OwnerObject.Ref.Base.UnInit();
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }

                }
                else if (building.Ref.GetCurrentFrame() == 8)
                {
                    type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.NorthSouthWall);
                    var north = FindWallPost(pCell, Direction.W);
                    var south = FindWallPost(pCell, Direction.E);
                    if (north.Item1 == true && south.Item1 == true)
                    {
                        if (north.Item2 == south.Item2)
                        {
                            if (north.Item2 != Data.BunkerWallPost)
                            {
                                return;
                            }
                        }
                        else
                        {
                            //不同种类不允许链接
                            Owner.OwnerObject.Ref.Base.Remove();
                            Owner.OwnerObject.Ref.Base.UnInit();
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                var currentCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                Owner.OwnerObject.Ref.Base.Remove();
                TechnoPlacer.PlaceTechnoNear(type, Owner.OwnerObject.Ref.Owner, currentCell, true);
                Owner.OwnerObject.Ref.Base.UnInit();
            }


        }

        private Tuple<bool, string> FindWallPost(Pointer<CellClass> pCell, Direction direction)
        {
            List<string> LaserPosts = new List<string>()
            {
                "RAPPST","RAPPST2","WAWALLCR"
            };

            bool finded = false;
            string wallName = "";


            Pointer<CellClass> pLast = pCell;

            for (var i = 0; i < 8; i++)
            {
                pLast = pLast.Ref.GetNeighbourCell(direction);
                if (pLast.IsNull)
                    break;
                var build = pLast.Ref.GetBuilding();
                if (!build.IsNull)
                {
                    var id = build.Ref.Base.Type.Ref.Base.Base.ID;
                    Logger.Log("buildingId:" + id);
                    if (LaserPosts.Contains(id))
                    {
                        finded = true;
                        wallName = id;
                        return Tuple.Create(finded, wallName);
                    }
                }
            }
            return Tuple.Create(finded, wallName);
        }
    }

    [Serializable]
    public class BunkerWallData : INIAutoConfig
    {
        [INIField(Key = "BunkerWall.Post")]
        public string BunkerWallPost;

        [INIField(Key = "BunkerWall.WE")]
        public string WestEastWall;

        [INIField(Key = "BunkerWall.NS")]
        public string NorthSouthWall;
    }
}
