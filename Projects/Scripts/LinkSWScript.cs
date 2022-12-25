using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(LinkSWScript))]
    public class LinkSWScript : TechnoScriptable
    {
        private LinkSWScriptBulletCount linkSWScriptBulletCount;

        public LinkSWScript(TechnoExt owner) : base(owner)
        {
            linkSWScriptBulletCount = new LinkSWScriptBulletCount(Owner);
        }

        private Pointer<TechnoTypeClass> inhibitorTechnoType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("SSTRSPU5");

        private TechnoExt inhibitorTechnoExt;

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            Pointer<TechnoClass> ownerTechno = Owner.OwnerObject;

            //Logger.Log($"当前Put的单位ID是：{ownerTechno.Ref.Type.Ref.Base.Base.ID}");
            if (ownerTechno.Ref.Type.Ref.Base.Base.ID != technoId1 && ownerTechno.Ref.Type.Ref.Base.Base.ID != technoId4)
            {
                //Logger.Log("准备创建0*0抑制建筑");
                TechnoTypeExt inhibitorTechnoTypeExt = TechnoTypeExt.ExtMap.Find(inhibitorTechnoType);
                Pointer<TechnoClass> inhibitorTechno = inhibitorTechnoTypeExt.OwnerObject.Ref.Base.CreateObject(HouseClass.FindCivilianSide()).Convert<TechnoClass>();

                inhibitorTechnoExt = TechnoExt.ExtMap.Find(inhibitorTechno);

                //TechnoPlacer.PlaceTechnoNear(inhibitorTechnoExt.OwnerObject, CellClass.Coord2Cell(coord));
                ++Game.IKnowWhatImDoing;
                if(!inhibitorTechnoExt.OwnerObject.Ref.Base.Put(coord, faceDir))
                {
                    inhibitorTechnoExt.OwnerObject.Ref.Base.UnInit();
                }
                --Game.IKnowWhatImDoing;
            }

            base.OnPut(coord, faceDir);
        }

        private bool inited = false;
        private bool inCurrentSW = false;

        private SuperWeaponExt nextSuperExt;

        static Pointer<SuperWeaponTypeClass> sw1 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeSpeical1");
        static Pointer<SuperWeaponTypeClass> sw2 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeSpeical2");
        static Pointer<SuperWeaponTypeClass> sw3 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeSpeical3");
        static Pointer<SuperWeaponTypeClass> sw4 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeSpeical4");

        private string technoId1 = "SSTRSPU1";
        private string technoId2 = "SSTRSPU2";
        private string technoId3 = "SSTRSPU3";
        private string technoId4 = "SSTRSPU4";

        private List<string> technoIds = new List<string>() {
                "SSTRSPU1",
                "SSTRSPU2",
                "SSTRSPU3",
                "SSTRSPU4"
        };


        private static Pointer<BulletTypeClass> pbullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("SunStrikeSWCannon");
        private static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Special");

        public override void OnUpdate()
        {
            var isAi = !Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman();

            if (!inited)
            {
                inited = true;

                var id = Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID.ToString();
                int num = Int32.Parse(id.Last().ToString());

                //释放前删除还残存的建筑
                if (Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID == technoId1)
                {
                    var li = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, techno => technoIds.Contains(techno.Ref.Type.Ref.Base.Base.ID) && !techno.Equals(Owner.OwnerObject), FindRange.Owner);
                    foreach (var item in li)
                    {
                        if (!item.IsNullOrExpired())
                        {
                            item.OwnerObject.Ref.Base.Remove();
                            item.OwnerObject.Ref.Base.UnInit();
                        }
                    }
                }

                switch (num)
                {
                    case 1:
                        {
                            //Logger.Log("丢1");

                            Pointer<SuperClass> pSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw1);
                            pSuper.Ref.IsCharged = true;

                            Pointer<SuperClass> nextSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw2);
                            nextSuperExt = SuperWeaponExt.ExtMap.Find(nextSuper);
                            nextSuperExt.OwnerObject.Ref.IsCharged = true;
                            if(Owner.OwnerObject.Ref.Owner.Ref.CurrentPlayer && !isAi)
                            {
                                Game.CurrentSWType = sw2.Ref.ArrayIndex;
                            }
                            inCurrentSW = true;

                            if (isAi)
                            {
                                Pointer<SuperClass> psw2 = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw2);
                                Pointer<SuperClass> psw3 = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw3);
                                Pointer<SuperClass> psw4 = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw4);
                                psw2.Ref.IsCharged = true;
                                psw3.Ref.IsCharged = true;
                                psw4.Ref.IsCharged = true;

                                var crlocal = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                                var technos = ObjectFinder.FindTechnosNear(crlocal, 4608);
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
                                    psw2.Ref.Launch(CellClass.Coord2Cell(li[0]), false);
                                }
                                else
                                {
                                    psw2.Ref.Launch(CellClass.Coord2Cell(crlocal), false);
                                }

                                if (li.Count() > 1)
                                {
                                    psw3.Ref.Launch(CellClass.Coord2Cell(li[1]), false);
                                }
                                else
                                {
                                    psw3.Ref.Launch(CellClass.Coord2Cell(crlocal), false);
                                }

                                if (li.Count() > 2)
                                {
                                    psw4.Ref.Launch(CellClass.Coord2Cell(li[2]), false);
                                }
                                else
                                {
                                    psw4.Ref.Launch(CellClass.Coord2Cell(crlocal), false);
                                }
                            }

                            break;
                        }
                    case 2:
                        {
                            //Logger.Log("丢2");
                            if(!isAi)
                            {
                                Pointer<SuperClass> nextSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw3);
                                nextSuperExt = SuperWeaponExt.ExtMap.Find(nextSuper);
                                nextSuperExt.OwnerObject.Ref.IsCharged = true;
                                if (Owner.OwnerObject.Ref.Owner.Ref.CurrentPlayer && !isAi)
                                {
                                    Game.CurrentSWType = sw3.Ref.ArrayIndex;
                                }
                            }
                          
                            inCurrentSW = true;
                            break;
                        }
                    case 3:
                        {
                            //Logger.Log("丢3");
                            if(!isAi)
                            {
                                Pointer<SuperClass> nextSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw4);
                                nextSuperExt = SuperWeaponExt.ExtMap.Find(nextSuper);
                                nextSuperExt.OwnerObject.Ref.IsCharged = true;
                                if (Owner.OwnerObject.Ref.Owner.Ref.CurrentPlayer && !isAi)
                                {
                                    Game.CurrentSWType = sw4.Ref.ArrayIndex;
                                }
                            }
                          
                            inCurrentSW = true;

                            break;
                        }
                    case 4:
                        {
                            //Logger.Log("丢4");

                            //sw1进入冷却
                            Pointer<SuperClass> pSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw1);
                            pSuper.Ref.IsCharged = false;
                            pSuper.Ref.RechargeTimer.Resume();
                            pSuper.Ref.CameoChargeState = 0;

                            var technos = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, techno => technoIds.Contains(techno.Ref.Type.Ref.Base.Base.ID), FindRange.Owner);

                            var techno1 = technos.Where(t => t.OwnerObject.Ref.Type.Ref.Base.Base.ID == technoId1).FirstOrDefault();
                            var techno2 = technos.Where(t => t.OwnerObject.Ref.Type.Ref.Base.Base.ID == technoId2).FirstOrDefault();
                            var techno3 = technos.Where(t => t.OwnerObject.Ref.Type.Ref.Base.Base.ID == technoId3).FirstOrDefault();
                            var techno4 = technos.Where(t => t.OwnerObject.Ref.Type.Ref.Base.Base.ID == technoId4).FirstOrDefault();

                            if (!techno1.IsNullOrExpired())
                            {
                                var t1location = techno1.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 2000);

                                YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("RayBurst1b"), t1location);

                                if (!techno2.IsNullOrExpired())
                                {
                                    var bullet1 = pbullet.Ref.CreateBullet(techno2.OwnerObject.Convert<AbstractClass>(), techno1.OwnerObject, 1, warhead, 40, false);
                                    //bullet1.Ref.MoveTo(t1location, new BulletVelocity(0, 0, 0));
                                    //bullet1.Ref.SetTarget(techno2.OwnerObject.Convert<AbstractClass>());

                                    creatBulletScriptComponent(bullet1, techno2, techno1);
                                }

                                if (!techno3.IsNullOrExpired())
                                {
                                    var bullet2 = pbullet.Ref.CreateBullet(techno3.OwnerObject.Convert<AbstractClass>(), techno1.OwnerObject, 1, warhead, 40, false);
                                    //bullet2.Ref.MoveTo(t1location, new BulletVelocity(0, 0, 0));
                                    //bullet2.Ref.SetTarget(techno3.OwnerObject.Convert<AbstractClass>());

                                    creatBulletScriptComponent(bullet2, techno3, techno1);
                                }

                                if (!techno4.IsNullOrExpired())
                                {
                                    var bullet3 = pbullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), techno1.OwnerObject, 1, warhead, 40, false);
                                    //bullet3.Ref.MoveTo(t1location, new BulletVelocity(0, 0, 0));
                                    //bullet3.Ref.SetTarget(Owner.OwnerObject.Convert<AbstractClass>());

                                    creatBulletScriptComponent(bullet3, techno4, techno1);
                                }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            if (inited && !nextSuperExt.IsNullOrExpired() && !nextSuperExt.OwnerObject.Ref.IsCharged)
            {
                inCurrentSW = false;
            }

            //取消超武释放时也移除所有建筑
            if (inited && inCurrentSW)
            {
                if (Owner.OwnerObject.Ref.Owner.Ref.CurrentPlayer  && !isAi)
                {
                    if (Game.CurrentSWType == -1)
                    {
                        var li = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, techno => technoIds.Contains(techno.Ref.Type.Ref.Base.Base.ID), FindRange.Owner);
                        foreach (var item in li)
                        {
                            if (!item.IsNullOrExpired())
                            {
                                item.OwnerObject.Ref.Base.Remove();
                                item.OwnerObject.Ref.Base.UnInit();
                            }
                        }
                    }
                }
            }

            ////当后三个建筑都死亡则移除第一个建筑
            //if (inited && linkSWScriptBulletCount.getCount() >= 3)
            //{
            //    linkSWScriptBulletCount.zero();

            //    var li = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, techno => techno.Ref.Type.Ref.Base.Base.ID == technoId1, FindRange.Owner);
            //    foreach (var item in li)
            //    {
            //        if (!item.IsNullOrExpired())
            //        {
            //            item.OwnerObject.Ref.Base.Remove();
            //            item.OwnerObject.Ref.Base.UnInit();
            //        }
            //    }
            //}

            base.OnUpdate();
        }


        private void creatBulletScriptComponent(Pointer<BulletClass> bullet, TechnoExt techno, TechnoExt techno1)
        {
            BulletExt bulletExt = BulletExt.ExtMap.Find(bullet);

            if (!bulletExt.IsNullOrExpired() && !techno1.IsNullOrExpired())
            {
                if (techno1.GameObject.GetComponent(LinkSWScriptBullet.ID) == null)
                {
                    techno1.GameObject.CreateScriptComponent(nameof(LinkSWScriptBullet), LinkSWScriptBullet.ID, "LinkSWScriptBullet", techno1, techno, bulletExt, linkSWScriptBulletCount);
                }
            }
        }

        public override void OnRemove()
        {
            if (!inhibitorTechnoExt.IsNullOrExpired())
            {
                inhibitorTechnoExt.OwnerObject.Ref.Base.Remove();
                inhibitorTechnoExt.OwnerObject.Ref.Base.UnInit();
            }

            base.OnRemove();
        }
    }

    [Serializable]
    [ScriptAlias(nameof(LinkSWScriptBullet))]
    public class LinkSWScriptBullet : TechnoScriptable
    {
        public static int ID = 85287;

        private TechnoExt pTechno;
        private BulletExt bulletExt;

        private LinkSWScriptBulletCount linkSWScriptBulletCount;

        public LinkSWScriptBullet(TechnoExt owner, TechnoExt pTechno, BulletExt bulletExt, LinkSWScriptBulletCount linkSWScriptBulletCount) : base(owner)
        {


            this.pTechno = pTechno;
            this.bulletExt = bulletExt;
            this.linkSWScriptBulletCount = linkSWScriptBulletCount;

            //Logger.Log($"进入脚本的单位：{pTechno.OwnerObject.Ref.Type.Ref.Base.Base.ID}");

            var t1location = owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 2000);
            bulletExt.OwnerObject.Ref.MoveTo(t1location, new BulletVelocity(0, 0, 0));
            bulletExt.OwnerObject.Ref.SetTarget(pTechno.OwnerObject.Convert<AbstractClass>());

        }

        public override void OnUpdate()
        {
            if (bulletExt.IsNullOrExpired())
            {
                //Logger.Log($"单位被销毁：{pTechno.OwnerObject.Ref.Type.Ref.Base.Base.ID}");
                pTechno.OwnerObject.Ref.Base.Remove();
                pTechno.OwnerObject.Ref.Base.UnInit();

                linkSWScriptBulletCount.add();
                //Logger.Log($"销毁计数{linkSWScriptBulletCount.getCount()}");
                if (linkSWScriptBulletCount.getCount() >= 3)
                {
                    //Logger.Log($"单位被销毁：{pTechno1.OwnerObject.Ref.Type.Ref.Base.Base.ID}")
                    Owner.OwnerObject.Ref.Base.Remove();
                    Owner.OwnerObject.Ref.Base.UnInit();
                }

                DetachFromParent();
                return;
            }

            base.OnUpdate();
        }
    }

    [Serializable]
    public class LinkSWScriptBulletCount
    {
        private LinkSWScriptCounterScript linkSWScriptCounterScript;

        public LinkSWScriptBulletCount(TechnoExt Owner)
        {
            Owner.GameObject.CreateScriptComponent(nameof(LinkSWScriptCounterScript), LinkSWScriptCounterScript.UniqueId, "LinkSWScriptCounterScript", Owner);
            linkSWScriptCounterScript = Owner.GameObject.GetComponent<LinkSWScriptCounterScript>();
        }

        public void add()
        {
            linkSWScriptCounterScript.Add();
        }

        public int getCount()
        {
            return linkSWScriptCounterScript.GetCount();
        }

        public void zero()
        {
            linkSWScriptCounterScript.Zero();
        }

    }

    [ScriptAlias(nameof(LinkSWScriptCounterScript))]
    [Serializable]
    public class LinkSWScriptCounterScript : TechnoScriptable
    {
        public static int UniqueId = 212231858;

        public LinkSWScriptCounterScript(TechnoExt owner) : base(owner) { }

        private int count = 0;

        public void Add()
        {
            count++;
        }

        public int GetCount()
        {
            return count;
        }

        public void Zero()
        {
            count = 0;
        }

        public override void OnUpdate()
        {
            if (Owner.IsNullOrExpired())
            {
                DetachFromParent();
                return;
            }
        }
    }



    [Serializable]
    [ScriptAlias(nameof(SunStrikeLaserScript))]
    public class SunStrikeLaserScript : BulletScriptable
    {
        public SunStrikeLaserScript(BulletExt owner) : base(owner) { }

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
    }

}


