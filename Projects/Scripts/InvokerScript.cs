using DynamicPatcher;
using Extension.Coroutines;
using Extension.Decorators;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using Microsoft.AspNetCore.SignalR.Client;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    [ScriptAlias(nameof(InvokerScript))]
    public class InvokerScript : TechnoScriptable
    {
        public static HubConnection connection = null;

        public static Queue<SkillType> SkillCmd = new Queue<SkillType>();

        private static readonly object qLocker = new object();

        private static Pointer<BulletTypeClass> pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private static Pointer<BulletTypeClass> bTornado = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("TornadoSeeker");

        private static Pointer<BulletTypeClass> bMeotor = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("MeteorSeeker");


        private bool InGhostWalk = false;

        private Random rd = new Random(114514);

        private static List<string> Ices = new List<string>()
        {
            "FICECLOUD1",
            "FICECLOUD2",
            "FICECLOUD3"
        };

        public InvokerScript(TechnoExt owner) : base(owner)
        {
            if (connection == null)
            {

                var querystringData = new Dictionary<string, string>
                {
                    { "UserId", "75159C4B-5AD6-4F28-85F9-33E7C917C995" }
                };

                connection = new HubConnectionBuilder()
                 .WithUrl("http://localhost:5000/GameHub?UserId=75159C4B-5AD6-4F28-85F9-33E7C917C995")
                 .Build();


                connection.On<SkillType>("CastSkill", sk =>
                {
                    lock (qLocker)
                    {
                        SkillCmd.Enqueue(sk);
                    }
                });


                Task.Run(async () =>
                {
                    await connection.StartAsync();
                });
            }
        }

        private bool started = false;

        public override void OnUpdate()
        {
            if(!started)
            {
                if (connection.State == HubConnectionState.Connected)
                {
                    Task.Run(async () =>
                    {
                        await connection.SendAsync("Start");
                    });
                }
            }
    

            SkillType? skill = null;
            if (SkillCmd.Count > 0)
            {
                lock (SkillCmd)
                {
                    if (SkillCmd.Count > 0)
                    {
                        skill = SkillCmd.Dequeue();
                    }
                }
            }

            if (skill != null)
            {
                switch (skill.Value)
                {
                    case SkillType.ColdSnap:
                        {
                            if (MapClass.Instance.TryGetCellAt(DisplayClass.Display_ZoneCell + DisplayClass.Display_ZoneOffset, out var pcell))
                            {
                                var target = pcell.Ref.Base.GetCoords();
                                if (target.BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords()) > Game.CellSize * 10)
                                {
                                    SkillCanced(skill.Value);
                                }
                                else
                                {
                                    FaceToCoord(target);
                                    var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 50, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ColdSnapWH"), 100, false);
                                    bullet.Ref.DetonateAndUnInit(target);
                                    YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("IColdSnap"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                                    SkillCompleted(skill.Value);
                                }
                            }
                            else
                            {
                                SkillCanced(skill.Value);
                            }
                            break;
                        }
                    case SkillType.IcwWall:
                        {

                            if (MapClass.Instance.TryGetCellAt(DisplayClass.Display_ZoneCell + DisplayClass.Display_ZoneOffset, out var pcell))
                            {
                                SkillCompleted(skill.Value);

                                var mycoord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                                var facing = Owner.OwnerObject.Ref.Facing.current().GetValue();

                                var angle = facing / ((short.MaxValue - short.MinValue) / 360);
                                var langle = (double)facing / 32768 * Math.PI - Math.PI / 2;

                                //var center = new CoordStruct(mycoord.X + (int)(600 * (Math.Cos(langle))), mycoord.Y + (int)(600 * Math.Sin(langle)), mycoord.Z);
                                var center = pcell.Ref.Base.GetCoords();

                                GameObject.StartCoroutine(StartIceWall(center, langle));
                            }
                            else
                            {
                                SkillCanced(skill.Value);
                            }
                         
                            break;
                        }
                    case SkillType.SunStrike:
                        {
                            SkillCompleted(skill.Value);

                            if (MapClass.Instance.TryGetCellAt(DisplayClass.Display_ZoneCell + DisplayClass.Display_ZoneOffset, out var pcell))
                            {

                                var target = pcell.Ref.Base.GetCoords();
                                FaceToCoord(target);
                                YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("ISunstrikeCharge"), target);
                                GameObject.StartCoroutine(SunStrike(target));
                            }

                            break;
                        }
                    case SkillType.GhostWalk:
                        {
                            SkillCompleted(skill.Value);
                            YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("IGhostWalk"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                            InGhostWalk = true;
                            YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("RING"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                            GameObject.StartCoroutine(StartGhostWalk());
                            break;
                        }
                    case SkillType.ForgeSpirit:
                        {
                            SkillCompleted(skill.Value);
                            var old = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, x => x.Ref.Type.Ref.Base.Base.ID == "FORGESP", FindRange.Owner);
                            foreach (var o in old)
                            {
                                o.OwnerObject.Ref.Base.Remove();
                                o.OwnerObject.Ref.Base.UnInit();
                            }

                            for (var i = 0; i <= 1; i++)
                            {
                                TechnoPlacer.PlaceTechnoNear(TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("FORGESP"), Owner.OwnerObject.Ref.Owner, CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords()));
                            }

                            break;
                        }
                    case SkillType.Alacrity:
                        {
                            SkillCompleted(skill.Value);
                            YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("IAlacrity"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                            var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("AlacrityWh"), 100, false);
                            bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                            break;
                        }
                    case SkillType.DeafeningBlast:
                        {

                            if (MapClass.Instance.TryGetCellAt(DisplayClass.Display_ZoneCell + DisplayClass.Display_ZoneOffset, out var pcell))
                            {
                                var target = pcell.Ref.Base.GetCoords();
                                if (target.BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords()) > Game.CellSize * 8)
                                {
                                    SkillCanced(skill.Value);
                                }
                                else
                                {
                                    FaceToCoord(target);
                                    YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("IWave"), Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0,0,0));
                                    SkillCompleted(skill.Value);

                                    var objs = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), Game.CellSize * 10);
                                    foreach(var obj in objs)
                                    {
                                        if(obj.CastToTechno(out var ptech))
                                        {
                                            if (!ptech.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                                            {
                                                var ext = TechnoExt.ExtMap.Find(ptech);
                                                if(ext != null)
                                                {
                                                    if(ext.GameObject.GetComponent<WaveEffectScript>() == null)
                                                    {
                                                        ext.GameObject.CreateScriptComponent(nameof(WaveEffectScript), WaveEffectScript.UniqueId, nameof(WaveEffectScript), ext, Owner.OwnerObject.Ref.Base.Base.GetCoords(),Owner);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                SkillCanced(skill.Value);
                            }
                            break;
                        }
                    case SkillType.EMP:
                        {
                            if (MapClass.Instance.TryGetCellAt(DisplayClass.Display_ZoneCell + DisplayClass.Display_ZoneOffset, out var pcell))
                            {
                                var target = pcell.Ref.Base.GetCoords();
                                if (target.BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords()) > Game.CellSize * 10)
                                {
                                    SkillCanced(skill.Value);
                                }
                                else
                                {
                                    FaceToCoord(target);
                                    YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("EmpMakeSound"), target);
                                    GameObject.StartCoroutine(EmpBlast(target));
                                    SkillCompleted(skill.Value);
                                }
                            }
                            else
                            {
                                SkillCanced(skill.Value);
                            }
                            break;
                        }
                    case SkillType.Tornado:
                        {

                            if (MapClass.Instance.TryGetCellAt(DisplayClass.Display_ZoneCell + DisplayClass.Display_ZoneOffset, out var pcell))
                            {
                                var target = pcell.Ref.Base.GetCoords();
                                if (target.BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords()) > Game.CellSize * 25)
                                {
                                    SkillCanced(skill.Value);
                                }
                                else
                                {
                                    FaceToCoord(target);
                                    YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("ITornado"), Owner.OwnerObject.Ref.Base.Base.GetCoords());

                                    var start = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                                    var flipX =1;//target.X > start.X ? 1 : -1;
                                    var flipY = 1;//target.Y > start.Y ? 1 : -1;

                                    var cita = Math.Atan2((target.Y - start.Y), (target.X - start.X));


                                    var cs = 25 * Game.CellSize;
                                    var dest = new CoordStruct((start.X + (int)(cs * Math.Cos(cita) * flipX)), start.Y + (int)(cs * Math.Sin(cita)) * flipY, start.Z);

                                    var bullet = bTornado.Ref.CreateBullet(pcell.Convert<AbstractClass>(), Owner.OwnerObject, 20, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("TornadoWarhead"), 60, false);
                                    bullet.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 200), new BulletVelocity(0, 0, 0));
                                    if(MapClass.Instance.TryGetCellAt(dest, out var pdest))
                                    {
                                        bullet.Ref.SetTarget(pdest.Convert<AbstractClass>());
                                    }
                                    else
                                    {
                                        bullet.Ref.SetTarget(pcell.Convert<AbstractClass>());
                                    }
                                    SkillCompleted(skill.Value);
                                }
                            }
                            else
                            {
                                SkillCanced(skill.Value);
                            }
                            break;
                        }
                    case SkillType.Meteor:
                        {
                            if (MapClass.Instance.TryGetCellAt(DisplayClass.Display_ZoneCell + DisplayClass.Display_ZoneOffset, out var pcell))
                            {
                                var target = pcell.Ref.Base.GetCoords();
                                if (target.BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords()) > Game.CellSize * 10)
                                {
                                    SkillCanced(skill.Value);
                                }
                                else
                                {
                                    var start = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                                    var flipX = 1;//;target.X > start.X ? 1 : -1;
                                    var flipY = 1;//; target.Y > start.Y ? 1 : -1;

                                    var cita = Math.Atan2((target.Y - start.Y) , (target.X - start.X));
                                    var cs = -10 * Game.CellSize;
                                    //var sdest = new CoordStruct((target.X + (int)(cs * Math.Cos(cita) * flipX)), target.Y + (int)(cs * Math.Sin(cita)) * flipY, target.Z);

                                    FaceToCoord(target);
                                    YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("IMeotor"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                                    for(var i = 0; i <= 2; i++)
                                    {
                                        var angle = (cita + (i - 1) * 3.14 / 180 * 30);
                                        var dest = new CoordStruct((target.X + (int)(cs * Math.Cos(angle) * flipX)), target.Y + (int)(cs * Math.Sin(angle)) * flipY, target.Z);
                                        var bullet = bMeotor.Ref.CreateBullet(pcell.Convert<AbstractClass>(), Owner.OwnerObject, 500, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MeotorHitWh"), 50, false);
                                        bullet.Ref.MoveTo(dest + new CoordStruct(0, 0, 3000), new BulletVelocity(0, 0, 0));
                                        bullet.Ref.SetTarget(pcell.Convert<AbstractClass>());
                                    }
                            
                                    SkillCompleted(skill.Value);
                                }
                            }
                            else
                            {
                                SkillCanced(skill.Value);
                            }
                            break;
                        }
                    case SkillType.UltraSunStrike:
                        {
                            SkillCompleted(skill.Value);

                            var targets = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, x => !x.Ref.Base.InLimbo, FindRange.Enermy).OrderBy(x=> MathEx.Random.Next()).Take(30);

                            foreach (var targetTechno in targets)
                            {
                              
                                var target = targetTechno.OwnerObject.Ref.Base.Base.GetCoords();
                                YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("ISunstrikeCharge"), target);
                                GameObject.StartCoroutine(SunStrike(target));

                            }
                            break;
                        }
                    case SkillType.Blink:
                        {
                            if (MapClass.Instance.TryGetCellAt(DisplayClass.Display_ZoneCell + DisplayClass.Display_ZoneOffset, out var pcell))
                            {
                                var target = pcell.Ref.Base.GetCoords();
                                if (target.BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords()) > Game.CellSize * 20)
                                {
                                    SkillCanced(skill.Value);
                                }
                                else
                                {
                                    GameObject.StartCoroutine(BlinkTo(target));
                                    SkillCompleted(skill.Value);
                                }
                            }
                            else
                            {
                                SkillCanced(skill.Value);
                            }
                            break;
                        }
                    case SkillType.BKB:
                        {
                            SkillCompleted(skill.Value);
                            var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(),Pointer<TechnoClass>.Zero, 10, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BKBWh"), 100, false);
                            bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }


        }

        private void FaceToCoord(CoordStruct coord)
        {
            if (Owner.OwnerObject.CastToFoot(out Pointer<FootClass> pfoot))
            {
                var dir = GameUtil.Point2Dir(Owner.OwnerObject.Ref.Base.Base.GetCoords(), coord);
                var tdir = new DirStruct(16, (short)DirStruct.TranslateFixedPoint(3, 16, (uint)dir));
                Owner.OwnerObject.Ref.Facing.set(tdir);
            }
        }


        IEnumerator StartGhostWalk()
        {
            // wait super weapon to reset
            var timer = 0;

            while (InGhostWalk && timer <= 1000)
            {
                timer += 20;

                var bullet1 = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("GhostWalkWh1"), 100, false);
                var bullet2 = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 5, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("GhostWalkWh2"), 100, false);
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                bullet1.Ref.DetonateAndUnInit(location);
                bullet2.Ref.DetonateAndUnInit(location);

                yield return new WaitForFrames(20);
            }

            InGhostWalk = false;
        }

        IEnumerator EmpBlast(CoordStruct target)
        {
            for (var i = 0; i <= 20; i++)
            {
                var bullet2 = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 50 + i * 100, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("InvEmpChWh"), 100, true);
                bullet2.Ref.DetonateAndUnInit(target);
                yield return new WaitForFrames(10);
            }

            YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("Invempbst"), target);
            yield return new WaitForFrames(55);

            if (target != null)
            {
                var bullet1 = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 400, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("InvEmpWh"), 100, false);
                bullet1.Ref.DetonateAndUnInit(target);
            }
        }

        IEnumerator StartIceWall(CoordStruct center, double langle)
        {
            int xPerCell = (int)(256 * Math.Sin(langle));
            int yPerCell = -(int)(256 * Math.Cos(langle));

            var locations = new List<CoordStruct>();
            for (int i = -10; i < 11; i++)
            {
                CoordStruct pos = center + new CoordStruct(xPerCell * i, yPerCell * i, 0);

                if (MapClass.Instance.TryGetCellAt(pos, out var pCell1))
                {
                    pos.Z = pCell1.Ref.Base.GetCoords().Z; // 手动修正地形高度
                    if (pCell1.Ref.ContainsBridge())
                    {
                        pos.Z += Game.BridgeHeight; // 手动修正桥面高度
                    }
                } // 最终得到位于地面、水面、桥面的Coords

                locations.Add(pos);
            }

            locations = locations.OrderBy(x => x.BigDistanceForm(center)).ToList();

            yield return new WaitForFrames(30);

            //生成初始冰墙
            foreach(var loc in locations)
            {
                YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(Ices[rd.Next(0, 3)]), loc);
                yield return new WaitForFrames(3);
            }

            for (var j = 0; j <= 15; j++)
            {
                for (int i = -10; i < 11; i++)
                {
                    CoordStruct pos = center + new CoordStruct(xPerCell * i, yPerCell * i, 0);

                    if (MapClass.Instance.TryGetCellAt(pos, out var pCell1))
                    {
                        pos.Z = pCell1.Ref.Base.GetCoords().Z; // 手动修正地形高度
                        if (pCell1.Ref.ContainsBridge())
                        {
                            pos.Z += Game.BridgeHeight; // 手动修正桥面高度
                        }
                    } // 最终得到位于地面、水面、桥面的Coords

                    YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(Ices[rd.Next(0, 3)]), pos);

                    var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 5, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("IceWallWh"), 100, false);
                    bullet.Ref.DetonateAndUnInit(pos);
                }
                YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("IIceWall"), center);

                yield return new WaitForFrames(20);
            }
        }

        IEnumerator SunStrike(CoordStruct center)
        {
            var startAngle = 0;
            var radius = 4 * Game.CellSize;

            var color = new ColorStruct(250, 80, 44);
            var thickness = 50;

            for (var i = 0; i <= 200; i++)
            {
                //CoordStruct lastpos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(startAngle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(startAngle * Math.PI / 180), 5)), center.Z);

                //for (var angle = startAngle + 5; angle < startAngle + 340; angle += 5)
                //{
                //    var currentPos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z);
                //    Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(lastpos, currentPos, color, color, color, 5);
                //    pLaser.Ref.Thickness = 5;
                //    pLaser.Ref.IsHouseColor = true;
                //    lastpos = currentPos;
                //}

                //radius -= 1;
                //startAngle += 5;

                //Pointer<LaserDrawClass> pMiddle = YRMemory.Create<LaserDrawClass>(center, center + new CoordStruct(0, 0, 3000), color, color, color, 5);
                //pMiddle.Ref.Thickness = 50 - i / 5;
                //pMiddle.Ref.IsHouseColor = true;

                if (i % 10 == 0)
                {
                    var pBullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 0, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Special"), 100, false);
                    pBullet.Ref.Base.SetLocation(center);
                    Owner.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeLaser"), center + new CoordStruct(0, 0, 9000));
                    pBullet.Ref.DetonateAndUnInit(center);
                }

                yield return new WaitForFrames(1);
            }
            yield return new WaitForFrames(20);

            Pointer<LaserDrawClass> pMiddle2 = YRMemory.Create<LaserDrawClass>(center, center + new CoordStruct(0, 0, 3000), color, color, color, 5);
            pMiddle2.Ref.Thickness = 60;
            pMiddle2.Ref.IsHouseColor = true;

            var technos = ObjectFinder.FindTechnosNear(center, 3 * Game.CellSize);
            var count = technos.Count;

            if (count > 0)
            {
                for (var j = 0; j < count; j++)
                {
                    TechnoPlacer.PlaceTechnoNear(TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("FORGESP2"), Owner.OwnerObject.Ref.Owner, CellClass.Coord2Cell(center));
                }
            }

            if (count == 0)
                count = 1;

            var damage = 1200 / count;
            var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ISunStrWh"), 100, false);
            bullet.Ref.DetonateAndUnInit(center);
        }


        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            InGhostWalk = false;
        }

        private void SkillCompleted(SkillType skill)
        {
            InGhostWalk = false;
            Task.Run(async () =>
            {
                await connection?.InvokeAsync("CastSkillComplete", skill);
            });
        }

        private void SkillCanced(SkillType skill)
        {
            Task.Run(async () =>
            {
                await connection?.InvokeAsync("CancelSkill", skill);
            });
        }

        IEnumerator BlinkTo(CoordStruct target)
        {
            var pTechno = Owner.OwnerObject;
            var animType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("CHRONOEXPMINI");
            var pfoot = pTechno.Convert<FootClass>();
            //pfoot.Ref.Locomotor.Stop_Moving();

            var mission = pTechno.Convert<MissionClass>();

            if (MapClass.Instance.TryGetCellAt(pTechno.Ref.Base.Base.GetCoords(), out Pointer<CellClass> pCell))
            {
                pfoot.Ref.Base.SetDestination(Pointer<AbstractClass>.Zero, false);
                mission.Ref.QueueMission(Mission.Move, false);
                mission.Ref.NextMission();
                mission.Ref.ForceMission(Mission.Sleep);
            }
            //mission.Ref.ForceMission(Mission.Sleep);
            //pfoot.Ref.Locomotor.Stop_Moving();
            //mission.Ref.ForceMission(Mission.Stop);
            yield return new WaitForFrames(10);
            YRMemory.Create<AnimClass>(animType, Owner.OwnerObject.Ref.Base.Base.GetCoords());
            TrySetLocation(target);
            YRMemory.Create<AnimClass>(animType, target);
            //yield return new WaitForFrames(1);
            //if (MapClass.Instance.TryGetCellAt(target, out Pointer<CellClass> pCell2))
            //{
            //    pfoot.Ref.Base.SetDestination(pCell2.Convert<AbstractClass>(), false);
            //    mission.Ref.QueueMission(Mission.Move, false);
            //    mission.Ref.NextMission();
            //}
            //mission.Ref.ForceMission(Mission.Stop);
            //yield return new WaitForFrames(1);


        }

        private bool TrySetLocation(CoordStruct location)
        {
            if (!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                var pTechno = Owner.OwnerObject;
                var mission = pTechno.Convert<MissionClass>();
                mission.Ref.ForceMission(Mission.Area_Guard);

                var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                //位置
                var pfoot = pTechno.Convert<FootClass>();
                //pTechno.Ref.SetDestination(default);
                if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
                {
                    var source = pTechno.Ref.Base.Base.GetCoords();
                    pfoot.Ref.Locomotor.Mark_All_Occupation_Bits(0);
                    pfoot.Ref.Locomotor.Force_Track(-1, source);
                    pTechno.Ref.Base.UnmarkAllOccupationBits(source);
                    var cLocal = pCell.Ref.Base.GetCoords();
                    var pLocal = new CoordStruct(cLocal.X, cLocal.Y, location.Z);
                    pTechno.Ref.Base.SetLocation(pLocal);
                    pTechno.Ref.Base.UnmarkAllOccupationBits(pLocal);
                    pTechno.Ref.Base.Scatter(location, true, true);
                }

                return true;
            }
            return false;
        }


    }


    public enum SkillType
    {
        None,
        //QQQ
        ColdSnap,
        //QQW
        GhostWalk,
        //QQE
        IcwWall,
        //QWW
        Tornado,
        //QWE
        DeafeningBlast,
        //QEE
        ForgeSpirit,
        //WWW
        EMP,
        //WWE
        Alacrity,
        //WEE
        Meteor,
        //EEE
        SunStrike,
        //R
        Invoke,
        //alt
        UltraSunStrike,

        //space,
        FlyBoot,
        //3
        Blink,
        //4
        Refresh,
        //z
        BKB

    }


    [ScriptAlias(nameof(WaveEffectScript))]
    [Serializable]
    public class WaveEffectScript : TechnoScriptable
    {
        public WaveEffectScript(TechnoExt owner,CoordStruct start,TechnoExt invoker) : base(owner)
        {
            Start = start;
            Invoker = invoker;
        }

        public static int UniqueId = 20230610;

        private static Pointer<BulletTypeClass> pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        TechnoExt Invoker;
        CoordStruct Start;
        CoordStruct End;
        bool inited = false;

        public int duration = 0;

        public override void OnUpdate()
        {
            if (!inited)
            {
                End = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                inited = true;

                Pointer<BulletClass> pBullet;
                if(!Invoker.IsNullOrExpired())
                {
                    pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Invoker.OwnerObject, 50, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("QWEWh"), 100, false);
                }
                else
                {
                    pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 50, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("QWEWh"), 100, false);
                }

                pBullet.Ref.DetonateAndUnInit(End);
            }


            if (duration++ <= 150)
            {
                var flipX = 1;//-(End.X > End.X ? 1 : -1);
                var flipY = 1;// -(End.Y > End.Y ? 1 : -1);

                var cita = Math.Atan2((End.Y - Start.Y) , (End.X - Start.X)) ; //Math.Atan(Math.Abs((End.Y - Start.Y) / (End.X - Start.X)));

                var radius = End.BigDistanceForm(Start);

                var cs = (duration * (500d /150d)) + radius;

                var dest = new CoordStruct((Start.X + (int)(cs * Math.Cos(cita) * flipX)), Start.Y + (int)(cs * Math.Sin(cita)) * flipY, Start.Z);

                if (MapClass.Instance.TryGetCellAt(dest,out var pCell))
                {
                    dest.Z = pCell.Ref.Base.GetCoords().Z; 
                    if (pCell.Ref.ContainsBridge())
                    {
                        dest.Z += Game.BridgeHeight; 
                    }

                    var pTechno = Owner.OwnerObject;
                    
                    //位置
                    if (pTechno.CastToFoot(out Pointer<FootClass> pfoot))
                    {
                        var source = pTechno.Ref.Base.Base.GetCoords();
                        pfoot.Ref.Locomotor.Mark_All_Occupation_Bits(0);
                        pfoot.Ref.Locomotor.Force_Track(-1, source);
                        pTechno.Ref.Base.UnmarkAllOccupationBits(source);
                        var cLocal = pCell.Ref.Base.GetCoords();
                        var pLocal = dest;
                        pTechno.Ref.Base.SetLocation(pLocal);
                        pTechno.Ref.Base.UnmarkAllOccupationBits(pLocal);
                    }
                }
            }
            else
            {
                DetachFromParent();
            }
        }
    }



    [ScriptAlias(nameof(TornadoAttachEffect))]
    [Serializable]
    public class TornadoAttachEffect : AttachEffectScriptable
    {
        public TornadoAttachEffect(TechnoExt owner) : base(owner) { }

        private CoordStruct startLocation;

        private bool inited = false;

        private int angle = 0;

        public override void OnUpdate()
        {
            if (Owner.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Building)
            {
                if (!inited)
                {
                    inited = true;
                    startLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                    angle = new Random(startLocation.X + startLocation.Y + startLocation.Z).Next(0, 360);
                }

                var radius = 512;

                var center = startLocation + new CoordStruct(0, 0, 1000);

                List<CoordStruct> coordList = new List<CoordStruct>();

                for (var vangle = 0; vangle <= 360; vangle += 15)
                {
                    coordList.Add(new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z));
                }


                var blocation = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z);
                SetLocation(blocation);

                angle += 10;
            }

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {

        }

        public override void OnRemove()
        {

        }

        /// <summary>
        /// 当AE效果将要被移除前触发的效果
        /// </summary>
        public override void OnAttachEffectRemove()
        {
            Owner.OwnerObject.Ref.Base.TakeDamage(100, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Super"), false);
            SetLocation(startLocation);
        }

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH,
         Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            return;
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (pWH.IsNull)
                return;

            if(pWH.Ref.Base.ID != "Super" || pWH.Ref.Base.ID!= "TornadoWarhead")
            {
                if (pDamage.Ref > 0)
                {
                    pDamage.Ref = 0;
                }
                return;
            }
            base.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
        }

        private void SetLocation(CoordStruct location)
        {
            if (Owner.OwnerObject.Ref.Base.Base.WhatAmI() == AbstractType.Building)
                return;

            if (!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                var pTechno = Owner.OwnerObject;

                var mission = pTechno.Convert<MissionClass>();
                mission.Ref.ForceMission(Mission.Stop);

                pTechno.Ref.SetTarget(default);
                pTechno.Ref.SetDestination(default, false);

                pTechno.Ref.Base.SetLocation(location);
            }
        }

    }

    [ScriptAlias(nameof(TornadoBulletScript))]
    [Serializable]
    public class TornadoBulletScript : BulletScriptable
    {
        public TornadoBulletScript(BulletExt owner) : base(owner)
        {
            pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }

        private static Pointer<BulletTypeClass> inviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        private static Pointer<WarheadTypeClass> warhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("TornadoWarhead");

        private SwizzleablePointer<AnimClass> pAnim;

        static Pointer<AnimTypeClass> TornadoAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("Tornado");


        private bool inited = false;

        private CoordStruct lastCoord;

        private int delay = 20;


        public override void OnUpdate()
        {
            if (Owner.OwnerObject.Ref.Owner.IsNull)
            { return; }

            if (pAnim.IsNull)
            {
                CreateAnim();
            }

            if (delay-- == 0)
            {
                delay = 20;
                YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("XTornado"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }

            pAnim.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, -Owner.OwnerObject.Ref.Base.GetHeight() + 2000));

            if (inited)
            {
                lastCoord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            }
            else
            {
                if (Owner.OwnerObject.Ref.Base.Base.GetCoords().DistanceFrom(lastCoord) < 1500)
                {
                    return;
                }
                lastCoord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            }

            var pBullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Ref.Owner.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner, 100, warhead, 100, false);
            pBullet.Ref.Detonate(lastCoord + new CoordStruct(0, 0, -Owner.OwnerObject.Ref.Base.GetHeight()));
            pBullet.Ref.Base.UnInit();

        }


        private void CreateAnim()
        {
            if (!pAnim.IsNull)
            {
                KillAnim();
            }

            var anim = YRMemory.Create<AnimClass>(TornadoAnim, Owner.OwnerObject.Ref.Base.Base.GetCoords());
            pAnim.Pointer = anim;
        }

        private void KillAnim()
        {
            if (!pAnim.IsNull)
            {
                pAnim.Ref.TimeToDie = true;
                pAnim.Ref.Base.UnInit();
                pAnim.Pointer = IntPtr.Zero;
            }
        }

        public override void OnDestroy()
        {
            if (!pAnim.IsNull)
            {
                KillAnim();
            }
        }

    }




    [ScriptAlias(nameof(MeteorBulletScript))]
    [Serializable]
    public class MeteorBulletScript : BulletScriptable
    {
        public MeteorBulletScript(BulletExt owner) : base(owner)
        {
            pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }


        private SwizzleablePointer<AnimClass> pAnim;

        static Pointer<AnimTypeClass> meteorAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("HDMeteor");

        static Pointer<BulletTypeClass> bmeteor => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("MeteorSeeker2");

        private bool inited = false;

        CoordStruct start; 
        CoordStruct target;

        public override void OnUpdate()
        {
            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;

            if (!Owner.OwnerObject.Ref.Base.IsOnMap)
                return;

            if (pAnim.IsNull)
            {
                CreateAnim();
            }

            if (inited == false)
            {
                inited = true;
                start = Owner.OwnerObject.Ref.SourceCoords;
                target = Owner.OwnerRef.TargetCoords;
            }



            pAnim.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords());
        }

        public override void OnDetonate(Pointer<CoordStruct> pCoords)
        {
            var flipX =1;//target.X > start.X ? 1 : -1;
            var flipY =1;//target.Y > start.Y ? 1 : -1;

            var cita = Math.Atan2((target.Y - start.Y) , (target.X - start.X));


            var cs = 15 * Game.CellSize;
            var dest = new CoordStruct((target.X + (int)(cs * Math.Cos(cita) * flipX)), target.Y + (int)(cs * Math.Sin(cita)) * flipY, target.Z);

            if (MapClass.Instance.TryGetCellAt(dest, out var pdest))
            {
                var bullet = bmeteor.Ref.CreateBullet(pdest.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner, 150, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MeotorHitWh2"), 20, false);
                bullet.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 200), new BulletVelocity(0, 0, 0));
                bullet.Ref.SetTarget(pdest.Convert<AbstractClass>());
            }
           
        }


        private void CreateAnim()
        {
            if (!pAnim.IsNull)
            {
                KillAnim();
            }

            var anim = YRMemory.Create<AnimClass>(meteorAnim, Owner.OwnerObject.Ref.Base.Base.GetCoords());
            pAnim.Pointer = anim;
        }

        private void KillAnim()
        {
            if (!pAnim.IsNull)
            {
                pAnim.Ref.TimeToDie = true;
                pAnim.Ref.Base.UnInit();
                pAnim.Pointer = IntPtr.Zero;
            }
        }

        public override void OnDestroy()
        {
            if (!pAnim.IsNull)
            {
                KillAnim();
            }
        }

    }

    [ScriptAlias(nameof(Meteor2BulletScript))]
    [Serializable]
    public class Meteor2BulletScript : BulletScriptable
    {
        public Meteor2BulletScript(BulletExt owner) : base(owner)
        {
            pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }


        private SwizzleablePointer<AnimClass> pAnim;

        static Pointer<AnimTypeClass> meteorAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("HDMeteor");

        private static Pointer<BulletTypeClass> pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private bool inited = false;

        int rof = 10;

        public override void OnUpdate()
        {
            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;

            if (!Owner.OwnerObject.Ref.Base.IsOnMap)
                return;

            if (pAnim.IsNull)
            {
                CreateAnim();
            }

            pAnim.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, -Owner.OwnerObject.Ref.Base.GetHeight() + 256));

            if (rof-- <= 0)
            {
                rof = 10;
                var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner, 150, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MeotorHitWh2"), 100, false);
                bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, -Owner.OwnerObject.Ref.Base.GetHeight()));
            }

        }


        private void CreateAnim()
        {
            if (!pAnim.IsNull)
            {
                KillAnim();
            }

            var anim = YRMemory.Create<AnimClass>(meteorAnim, Owner.OwnerObject.Ref.Base.Base.GetCoords());
            pAnim.Pointer = anim;
        }

        private void KillAnim()
        {
            if (!pAnim.IsNull)
            {
                pAnim.Ref.TimeToDie = true;
                pAnim.Ref.Base.UnInit();
                pAnim.Pointer = IntPtr.Zero;
            }
        }

        public override void OnDestroy()
        {
            if (!pAnim.IsNull)
            {
                KillAnim();
            }
        }

    }
}