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

        private INIComponentWith<BunkerWallData> settingINI;

        public override void Awake()
        {
            settingINI = this.CreateRulesIniComponentWith<BunkerWallData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
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
            var Data = settingINI.Data;


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
                        if (IsSameGroup(west.Item2, east.Item2))
                        {
                            if (!IsBunkerWallPost(west.Item2))
                            {
                                return;
                            }

                            //if (IsFence(west.Item2) && IsNearBy(Owner.OwnerObject, west.Item3))
                            //{
                            //    if (Data.WestEastWall != west.Item2)
                            //    {
                            //        type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.BunkerWallPost);
                            //    }
                            //}
                            //if (IsFence(east.Item2) && IsNearBy(Owner.OwnerObject, east.Item3))
                            //{
                            //    if (Data.WestEastWall != east.Item2)
                            //    {
                            //        type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.BunkerWallPost);
                            //    }
                            //}
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
                        if (IsSameGroup(north.Item2, south.Item2))
                        {
                           
                            if (!IsBunkerWallPost(north.Item2))
                            {
                                return;
                            }

                            //if (IsFence(north.Item2) && IsNearBy(Owner.OwnerObject, north.Item3))
                            //{
                            //    if (Data.NorthSouthWall != north.Item2)
                            //    {
                            //        type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.BunkerWallPost);
                            //    }
                            //}
                            //if (IsFence(south.Item2) && IsNearBy(Owner.OwnerObject, south.Item3))
                            //{
                            //    if (Data.NorthSouthWall != south.Item2)
                            //    {
                            //        type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.BunkerWallPost);
                            //    }
                            //}
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

        private Tuple<bool, string,CoordStruct> FindWallPost(Pointer<CellClass> pCell, Direction direction)
        {
            var Data = settingINI.Data;

            List<string> LaserPosts = new List<string>()
            {
                "RAPPST","RAPPST2",Data.BunkerWallPost//,Data.WestEastWall,Data.NorthSouthWall
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
                    if (LaserPosts.Contains(id))
                    {
                        finded = true;
                        wallName = id;
                        return Tuple.Create(finded, wallName,pLast.Ref.Base.GetCoords());
                    }
                }
            }
            return Tuple.Create(finded, wallName, new CoordStruct(0, 0, 0));
        }

        private bool IsSameGroup(string wall1, string wall2)
        {
            var Data = settingINI.Data;

            List<string> LaserPosts = new List<string>()
            {
                Data.BunkerWallPost//,Data.WestEastWall,Data.NorthSouthWall
            };

            if (wall1 == wall2)
            {
                return true;
            }
            else
            {
                return LaserPosts.Contains(wall1) && LaserPosts.Contains(wall2);
            }
        }

        private bool IsBunkerWallPost(string wall)
        {
            var Data = settingINI.Data;

            List<string> LaserPosts = new List<string>()
            {
                Data.BunkerWallPost//,Data.WestEastWall,Data.NorthSouthWall
            };

            return LaserPosts.Contains(wall);
        }

        private bool IsBothFence(string wall1,string wall2)
        {
            return IsFence(wall1) && IsFence(wall2);
        }

        private bool IsFence(string wall)
        {
            var Data = settingINI.Data;

            List<string> LaserPosts = new List<string>()
            {
               Data.WestEastWall,Data.NorthSouthWall
            };

            return LaserPosts.Contains(wall);
        }

        public bool IsNearBy(Pointer<TechnoClass> one, CoordStruct coord)
        {
            var distance = one.Ref.Base.Base.GetCoords().DistanceFrom(coord);
            return distance != double.NaN ? distance < 300 : false;
        }
    }


    [ScriptAlias(nameof(BunkerWallFenceScript))]
    [Serializable]
    public class BunkerWallFenceScript : TechnoScriptable
    {
        public BunkerWallFenceScript(TechnoExt owner) : base(owner)
        {
        }


        private Tuple<bool, string, CoordStruct> FindWallPost(Pointer<CellClass> pCell, Direction direction)
        {

            List<string> LaserPosts = new List<string>()
            {
                "WAWALLCR"
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
                    if (LaserPosts.Contains(id))
                    {
                        finded = true;
                        wallName = id;
                        return Tuple.Create(finded, wallName, pLast.Ref.Base.GetCoords());
                    }
                }
            }
            return Tuple.Create(finded, wallName, new CoordStruct(0, 0, 0));
        }


        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            int trueDamage = MapClass.GetTotalDamage(pDamage.Ref, pWH, Owner.OwnerObject.Ref.Type.Ref.Base.Armor, DistanceFromEpicenter);

            if (trueDamage < 0)
                return;
            //if(trueDamage < Owner.OwnerObject.Ref.Base.Health)
            //    return;

            var id = Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID;
            bool postExist = false;

            if (MapClass.Instance.TryGetCellAt(Owner.OwnerObject.Ref.Base.Base.GetCoords(), out var pCell))
            {
                if (id == "WAWALLSN")
                {
                    if (FindWallPost(pCell, Direction.W).Item1 && FindWallPost(pCell, Direction.E).Item1)
                    {
                        postExist = true;
                    }
                }
                else
                {
                    if (FindWallPost(pCell, Direction.N).Item1 && FindWallPost(pCell, Direction.S).Item1)
                    {
                        postExist = true;
                    }
                }
            }

            if (postExist)
            {
                pDamage.Ref = (int)(pDamage.Ref / 5);
                //Owner.OwnerObject.Ref.Base.Health = 1;
            }
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
