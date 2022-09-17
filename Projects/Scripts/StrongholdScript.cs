using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(StrongholdScript))]
    public class StrongholdScript : TechnoScriptable
    {
        public StrongholdScript(TechnoExt owner) : base(owner)
        {
          
        }

        private static int MaxScore = 0;

        static Dictionary<Pointer<HouseClass>, int> HouseScores = new Dictionary<Pointer<HouseClass>, int>();

        Dictionary<Pointer<HouseClass>, int> CapturingMap = new Dictionary<Pointer<HouseClass>, int>();

        static Pointer<WarheadTypeClass> peaceWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("PeaceKillWh");

        static string AnnoncerId;

        private int checkRof = 20;

        private int scoreUpdateRof = 500;

        private int annonceRof = 1000;

        private bool Registered = false;

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (Registered == false)
            {
                Registered = true;
                if(MaxScore==0)
                {
                    MaxScore = 80;
                }
                else
                {
                    MaxScore += 70;
                }
            }

            if (scoreUpdateRof-- <= 0)
            {
                scoreUpdateRof = 500;

                if (!Owner.OwnerObject.Ref.Owner.IsNull)
                {
                    var ownerName = Owner.OwnerObject.Ref.Owner.Ref.Type.Ref.Base.ID.ToString();
                    if (ownerName != "Special" && ownerName != "Neutral")
                    {
                        if(!HouseScores.ContainsKey(Owner.OwnerObject.Ref.Owner))
                        {
                            HouseScores.Add(Owner.OwnerObject.Ref.Owner, 0);
                        }
                        HouseScores[Owner.OwnerObject.Ref.Owner] = HouseScores[Owner.OwnerObject.Ref.Owner] + 1;

                        if (HouseScores[Owner.OwnerObject.Ref.Owner] >= MaxScore)
                        {
                            //结算胜利

                            List<int> killHouse = new List<int>();

                            for(var i=0;i< HouseClass.Array.Count;i++)
                            {
                                var house = HouseClass.Array.Get(i);
                                if(!house.IsNull)
                                {
                                    var houseName = house.Ref.Type.Ref.Base.ID.ToString();
                                    if (houseName != "Special" && houseName != "Neutral")
                                    {
                                        if (!house.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner) && house.Ref.ArrayIndex != Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                                        {
                                           killHouse.Add(house.Ref.ArrayIndex);
                                        }
                                    }
                                       
                                }
                            }

                            if(killHouse.Count > 0)
                            {
                                var technos = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, t =>
                                 !t.Ref.Owner.IsNull && killHouse.Contains(t.Ref.Owner.Ref.ArrayIndex) 
                                 &&t.Ref.Base.IsOnMap
                                    , FindRange.All
                                );
                                foreach(var techno in technos)
                                {
                                    if(!techno.IsNullOrExpired())
                                    {
                                        var ptechno = techno.OwnerObject;
                                        ptechno.Ref.Base.TakeDamage(8000, peaceWarhead, true);
                                    }
                                }
                            }

                        }
                    }
                }
            }

            if (annonceRof-- <= 0)
            {
                //全局只有一个据点来宣布占领情况
                annonceRof = 1000;
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var myId = $"{location.X}{location.Y}{location.Z}";
                if (string.IsNullOrEmpty(AnnoncerId))
                {
                    AnnoncerId = myId;
                }

                if(AnnoncerId == myId)
                {
                    if(HouseScores.Count()>0)
                    {
                        foreach(var houseScore in HouseScores)
                        {
                            //MessageListClass.Instance.PrintMessage($"玩家：{houseScore.Key.Ref.ArrayIndex + 1},据点分数：{houseScore.Value} / 500", ColorSchemeIndex.White, 300, false);
                            string label ="玩家:" + (houseScore.Key.Ref.ArrayIndex + 1).ToString();
                            string message = "据点分数：" + houseScore.Value.ToString() + "/" + MaxScore.ToString();
                            MessageListClass.Instance.PrintMessage(label, message, ColorSchemeIndex.White, 600, true);

                        }
                    }
                }
            }

            if (checkRof-- <= 0)
            {
                checkRof = 20;

                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var currentCell = CellClass.Coord2Cell(location);
                CellSpreadEnumerator enumeratorSource = new CellSpreadEnumerator(10);

                Dictionary<Pointer<HouseClass>, int> houseCounts = new Dictionary<Pointer<HouseClass>, int>();

                foreach (CellStruct offset in enumeratorSource)
                {
                    CoordStruct where = CellClass.Cell2Coord(currentCell + offset, location.Z);

                    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                    {
                        if (pCell.IsNull)
                            continue;

                        Point2D p2d = new Point2D(60, 60);
                        Pointer<ObjectClass> ptargetTechno = pCell.Ref.FirstObject;

                        if (ptargetTechno.IsNull)
                            continue;

                        if (ptargetTechno.Ref.Base.WhatAmI() != AbstractType.Unit && ptargetTechno.Ref.Base.WhatAmI() != AbstractType.Infantry)
                            continue;

                        Pointer<TechnoClass> ptechno = ptargetTechno.Convert<TechnoClass>();
                        if (ptechno.IsNull)
                            continue;

                        if (ptechno.Ref.Owner.IsNull)
                            continue;

                        var ownerName = ptechno.Ref.Owner.Ref.Type.Ref.Base.ID.ToString();
                        if (ownerName == "Special" || ownerName == "Neutral")
                            continue;

                        if (!houseCounts.ContainsKey(ptechno.Ref.Owner))
                        {
                            houseCounts.Add(ptechno.Ref.Owner, 0);
                        }

                        houseCounts[ptechno.Ref.Owner] = houseCounts[ptechno.Ref.Owner] + 1;
                    }
                }


                //选取优势方
                if (houseCounts.Count > 0)
                {
                    var houses = houseCounts.OrderByDescending(h => h.Value).Select(h =>
                      new
                      {
                          House = h.Key,
                          Count = h.Value
                      }).ToList();
                    var winner = houses[0];

                    for (int i = 0; i < houses.Count; i++)
                    {
                        var house = houses[i];
                        if (!CapturingMap.ContainsKey(house.House))
                        {
                            CapturingMap.Add(house.House, 0);
                        }

                        var count = CapturingMap[house.House];
                        if (house.House != winner.House)
                        {
                            if (count > 0)
                            {
                                var winPoint = winner.Count;

                                if (houses.Count > 1)
                                {
                                    var loser = houses[i];
                                    winPoint = winPoint - loser.Count;
                                }

                                var winrate = (int)Math.Ceiling(winPoint / 3d);
                                count -= winrate;
                                if (count < 0)
                                {
                                    count = 0;
                                }
                                CapturingMap[house.House] = count;
                            }
                        }
                        else
                        {
                            if (count < 100)
                            {
                                var winPoint = winner.Count;

                                if (houses.Count > 1)
                                {
                                    var loser = houses[1];
                                    winPoint = winPoint - loser.Count;
                                }

                                var winrate = (int)Math.Ceiling(winPoint / 3d);
                                count += winrate;
                                if (count >= 100)
                                {
                                    count = 100;
                                    Owner.OwnerObject.Ref.Owner = house.House;
                                    Owner.OwnerObject.Ref.Base.UpdatePlacement(PlacementType.Redraw);
                                }
                                CapturingMap[house.House] = count;
                            }
                        }
                    }

                    var keyList = CapturingMap.Keys.ToList();
                    for (var i = 0; i < CapturingMap.Keys.Count(); i++)
                    {
                        var key = keyList[i];
                        var map = CapturingMap[key];
                        if (!houses.Where(h => h.House == key).Any())
                        {
                            var winPoint = winner.Count;
                            var winrate = (int)Math.Ceiling(winPoint / 3d);
                            var val = map - winrate;
                            if (val < 0)
                            {
                                val = 0;
                            }
                            CapturingMap[key] = val;
                        }

                        var color = key.Ref.LaserColor;
                        DrawRing(color, location, map * 15);
                    }
                }

                houseCounts.Clear();
            }
                
           
        }


        private void DrawRing(ColorStruct color, CoordStruct center, int radius)
        {
            CoordStruct lastpos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(0 * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(0 * Math.PI / 180), 5)), center.Z);

            for (var angle = 0 + 5; angle < 360; angle += 5)
            {
                var currentPos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z);
                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(lastpos, currentPos, color, color, color, 40);
                pLaser.Ref.Thickness = 10;
                pLaser.Ref.IsHouseColor = true;
                lastpos = currentPos;
            }
        }
    }


}
