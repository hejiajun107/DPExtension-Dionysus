using Extension.CW;
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

namespace Scripts.China
{
    [ScriptAlias(nameof(XHSunStrikeSWScript))]
    [Serializable]
    public class XHSunStrikeSWScript : SuperWeaponScriptable
    {
        public XHSunStrikeSWScript(SuperWeaponExt owner) : base(owner)
        {
        }

        static Pointer<SuperWeaponTypeClass> sw1 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeSpeical1");
        static Pointer<SuperWeaponTypeClass> sw2 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeSpeical2");
        static Pointer<SuperWeaponTypeClass> sw3 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeSpeical3");
        static Pointer<SuperWeaponTypeClass> sw4 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeSpeical4");

        public Random rd = new Random(114514);


        private static Pointer<BulletTypeClass> pbullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeSWCannon");
        private static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Special");

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            var id = Owner.OwnerObject.Ref.Type.Ref.Base.ID.ToString();
            var house = Owner.OwnerObject.Ref.Owner;
            var housext = HouseExt.ExtMap.Find(house);
            var ghouse = housext.GameObject.GetComponent<HouseGlobalExtension>();
            if (ghouse == null)
                return;
            var index = 1;
            if (id.EndsWith("1"))
            {
                index = 1;
                ghouse.XHSunstrikeTarget1 = CellClass.Cell2Coord(cell);
            }
            else if (id.EndsWith("2"))
            {
                index = 2;
                ghouse.XHSunstrikeTarget2 = CellClass.Cell2Coord(cell);

            }
            else if (id.EndsWith("3"))
            {
                index = 3;
                ghouse.XHSunstrikeTarget3 = CellClass.Cell2Coord(cell);
            }
            else
            {
                index = 4;
                ghouse.XHSunstrikeTarget4 = CellClass.Cell2Coord(cell);
            }

            if (isPlayer && index != 4)
            {
                Pointer<SuperClass> pSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw1);
                pSuper.Ref.IsCharged = true;
                var swType = SelectSWType(index);
                Pointer<SuperClass> nextSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(swType);
                nextSuper.Ref.IsCharged = true;
                Game.CurrentSWType = swType.Ref.ArrayIndex;
            }

            if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
            {

                var crlocal = ghouse.XHSunstrikeTarget1;
                var technos = ObjectFinder.FindTechnosNear(crlocal, 18 * Game.CellSize);
                var li = technos.Where(x =>
                {

                    if (x.CastToTechno(out var pxtech))
                    {
                        if (pxtech.Ref.Owner.IsNull)
                            return false;
                        return !Owner.OwnerObject.Ref.Owner.Ref.IsAlliedWith(pxtech.Ref.Owner);
                    }
                    return false;
                }).OrderByDescending(x => x.Ref.Base.GetCoords().DistanceFrom(crlocal)).Select(x => x.Ref.Base.GetCoords()).Take(3).ToList();


                if (li.Count() > 0)
                {
                    ghouse.XHSunstrikeTarget2 = li[0];
                }
                else
                {
                    ghouse.XHSunstrikeTarget2 = ghouse.XHSunstrikeTarget1;
                }

                if (li.Count() > 1)
                {
                    ghouse.XHSunstrikeTarget3 = li[1];
                }
                else
                {
                    ghouse.XHSunstrikeTarget3 = ghouse.XHSunstrikeTarget1;
                }

                if (li.Count() > 2)
                {
                    ghouse.XHSunstrikeTarget4 = li[2];
                }
                else
                {
                    ghouse.XHSunstrikeTarget4 = ghouse.XHSunstrikeTarget1;
                }
                index = 4;
            }

            if (index == 4)
            {

                var t1location = ghouse.XHSunstrikeTarget1 + new CoordStruct(0, 0, 2000);


                YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("RayBurst1b"), t1location);

                var technoType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("SSTRSPU2");
                var techno = technoType.Ref.Base.CreateObject(Owner.OwnerObject.Ref.owner).Convert<TechnoClass>();

                List<CoordStruct> targets = new List<CoordStruct>();

                if (techno.Ref.Base.Put(ghouse.XHSunstrikeTarget1, Direction.N))
                {
                    var maxRange = 18 * Game.CellSize;
                    var minRange = 2 * Game.CellSize;


                    int lineCount = 0;
                    if (ghouse.XHSunstrikeTarget2.BigDistanceForm(ghouse.XHSunstrikeTarget1) > maxRange)
                    {
                        ghouse.XHSunstrikeTarget2 = GetLineEnd(ghouse.XHSunstrikeTarget1, ghouse.XHSunstrikeTarget2, maxRange);
                    }

                    targets.Add(ghouse.XHSunstrikeTarget2);

                    if (MapClass.Instance.TryGetCellAt(ghouse.XHSunstrikeTarget2, out var pcell1))
                    {
                        var bullet1 = pbullet.Ref.CreateBullet(pcell1.Convert<AbstractClass>(), techno, 1, warhead, 40, false);

                        bullet1.Ref.MoveTo(t1location, new BulletVelocity(0, 0, 0));
                        bullet1.Ref.SetTarget(pcell1.Convert<AbstractClass>());

                        lineCount++;
                    }


                    if (ghouse.XHSunstrikeTarget3.BigDistanceForm(ghouse.XHSunstrikeTarget1) > maxRange)
                    {
                        ghouse.XHSunstrikeTarget3 = GetLineEnd(ghouse.XHSunstrikeTarget1, ghouse.XHSunstrikeTarget3, maxRange);
                    }

                    if (targets.Where(x => ghouse.XHSunstrikeTarget3.BigDistanceForm(x) <= minRange).Any())
                    {
                        ghouse.XHSunstrikeTarget3 += new CoordStruct((rd.Next(100) > 50 ? -1 : 1) * rd.Next(512, 1000), (rd.Next(100) > 50 ? -1 : 1) * rd.Next(512, 1000), 0);
                    }
                    targets.Add(ghouse.XHSunstrikeTarget3);


                    if (MapClass.Instance.TryGetCellAt(ghouse.XHSunstrikeTarget3, out var pcell2))
                    {
                        var bullet2 = pbullet.Ref.CreateBullet(pcell2.Convert<AbstractClass>(), techno, 1, warhead, 40, false);
                        bullet2.Ref.MoveTo(t1location, new BulletVelocity(0, 0, 0));
                        bullet2.Ref.SetTarget(pcell2.Convert<AbstractClass>());
                        lineCount++;
                    }

                    if (ghouse.XHSunstrikeTarget4.BigDistanceForm(ghouse.XHSunstrikeTarget1) > maxRange)
                    {
                        ghouse.XHSunstrikeTarget4 = GetLineEnd(ghouse.XHSunstrikeTarget1, ghouse.XHSunstrikeTarget4, maxRange);
                    }

                    if (targets.Where(x => ghouse.XHSunstrikeTarget4.BigDistanceForm(x) <= minRange).Any())
                    {
                        ghouse.XHSunstrikeTarget4 += new CoordStruct((rd.Next(100) > 50 ? -1 : 1) * rd.Next(512, 1000), (rd.Next(100) > 50 ? -1 : 1) * rd.Next(512, 1000), 0);
                    }

                    if (MapClass.Instance.TryGetCellAt(ghouse.XHSunstrikeTarget4, out var pcell3))
                    {
                        var bullet3 = pbullet.Ref.CreateBullet(pcell3.Convert<AbstractClass>(), techno, 1, warhead, 40, false);
                        bullet3.Ref.MoveTo(t1location, new BulletVelocity(0, 0, 0));
                        bullet3.Ref.SetTarget(pcell3.Convert<AbstractClass>());
                        lineCount++;
                    }

                    var ext = TechnoExt.ExtMap.Find(techno);
                    ext.GameObject.CreateScriptComponent(nameof(XHSunStrikerMaintainerScript), "XHSunStrikerMaintainerScript", ext, lineCount);
                }


                Pointer<SuperClass> pSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw1);
                pSuper.Ref.IsCharged = false;
                pSuper.Ref.RechargeTimer.Resume();
                pSuper.Ref.CameoChargeState = 0;
            }


        }

        private CoordStruct GetLineEnd(CoordStruct start, CoordStruct end, int range)
        {
            var flipX = end.X > start.X ? 1 : -1;
            var flipY = end.Y > start.Y ? 1 : -1;
            var cita = Math.Atan(Math.Abs((end.Y - start.Y) / (end.X - start.X)));
            var cs = range;
            var dest = new CoordStruct((start.X + (int)(cs * Math.Cos(cita) * flipX)), start.Y + (int)(cs * Math.Sin(cita)) * flipY, start.Z);
            return dest;
        }

        private Pointer<SuperWeaponTypeClass> SelectSWType(int index)
        {
            switch (index)
            {
                case 1:
                    return sw2;
                case 2:
                    return sw3;
                case 3:
                    return sw4;
                case 4:
                    throw new InvalidOperationException();
                default:
                    throw new InvalidOperationException();
            }
        }


    }


    [Serializable]
    [ScriptAlias(nameof(XHSunStrikeCDUnitScript))]
    public class XHSunStrikeCDUnitScript : TechnoScriptable
    {
        public XHSunStrikeCDUnitScript(TechnoExt owner) : base(owner)
        {
        }

        static Pointer<SuperWeaponTypeClass> sw1 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeSpeical1");

        private bool inited = false;

        public override void OnUpdate()
        {
            if (!inited)
            {
                inited = true;
                Pointer<SuperClass> pSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw1);
                pSuper.Ref.IsCharged = true;
                Owner.OwnerObject.Ref.Base.Remove();
                Owner.OwnerObject.Ref.Base.UnInit();
            }
        }
    }

    [Serializable]
    [ScriptAlias(nameof(XHSunStrikerMaintainerScript))]
    public class XHSunStrikerMaintainerScript : TechnoScriptable
    {
        public XHSunStrikerMaintainerScript(TechnoExt owner, int count) : base(owner)
        {
            Remaining = count;
        }

        public int Remaining = 0;

        public override void OnUpdate()
        {
            if (Remaining <= 0)
            {
                Owner.OwnerObject.Ref.Base.UnInit();
            }
        }

    }



    [Serializable]
    [ScriptAlias(nameof(SunStrikeNLaserScript))]
    public class SunStrikeNLaserScript : BulletScriptable
    {
        public SunStrikeNLaserScript(BulletExt owner) : base(owner) { }

        private bool IsActive = false;

        TechnoExt pTargetRef;

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private Pointer<WeaponTypeClass> weapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeSWWP");

        private CoordStruct start;


        public override void OnUpdate()
        {
            if (IsActive == false)
            {
                IsActive = true;
                start = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                pTargetRef = TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
                YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeStart"), start);
                return;
            }

            var height = Owner.OwnerObject.Ref.Base.GetHeight();
            var target = Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, -height);

            if (!pTargetRef.IsNullOrExpired())
            {
                var pTechno = pTargetRef.OwnerObject;

                Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), pTechno, weapon.Ref.Damage, weapon.Ref.Warhead, 100, false);

                pBullet.Ref.Base.SetLocation(target);
                pTechno.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, weapon, start);
                pBullet.Ref.DetonateAndUnInit(target);
            }
        }

        public override void OnDestroy()
        {
            if (!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                var techno = Owner.OwnerObject.Ref.Owner;
                var technoExt = TechnoExt.ExtMap.Find(techno);

                if (!technoExt.IsNullOrExpired())
                {
                    var component = technoExt.GameObject.GetComponent<XHSunStrikerMaintainerScript>();
                    if (component != null)
                    {
                        component.Remaining--;
                    }
                }
            }
        }
    }




}