﻿using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
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
        public LinkSWScript(TechnoExt owner) : base(owner) { }

        private Pointer<TechnoTypeClass> inhibitorTechnoType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("SWLinkUnit5");

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
                inhibitorTechnoExt.OwnerObject.Ref.Base.Put(coord, faceDir);
                --Game.IKnowWhatImDoing;
            }

            base.OnPut(coord, faceDir);
        }

        private bool inited = false;
        private bool inCurrentSW = false;

        Pointer<SuperClass> nextSuper;

        static Pointer<SuperWeaponTypeClass> sw1 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("MultiTargetSpeical1");
        static Pointer<SuperWeaponTypeClass> sw2 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("MultiTargetSpeical2");
        static Pointer<SuperWeaponTypeClass> sw3 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("MultiTargetSpeical3");
        static Pointer<SuperWeaponTypeClass> sw4 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("MultiTargetSpeical4");

        private string technoId1 = "SWLinkUnit1";
        private string technoId2 = "SWLinkUnit2";
        private string technoId3 = "SWLinkUnit3";
        private string technoId4 = "SWLinkUnit4";

        private List<string> technoIds = new List<string>() {
                "SWLinkUnit1",
                "SWLinkUnit2",
                "SWLinkUnit3",
                "SWLinkUnit4"
        };


        private static Pointer<BulletTypeClass> pbullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("HuimieLaserCannon");
        private static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("AP");


        private LinkSWScriptBulletCount linkSWScriptBulletCount = new LinkSWScriptBulletCount();

        public override void OnUpdate()
        {
            if (!inited)
            {
                inited = true;

                var id = Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID.ToString();
                int num = Int32.Parse(id.Last().ToString());

                //释放前删除还残存的建筑
                if (Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID == technoId1)
                {
                    var li = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, techno => technoIds.Contains(techno.Ref.Type.Ref.Base.Base.ID) && techno.Ref.Type.Ref.Base.Base.ID != technoId1, FindRange.Owner);
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

                            nextSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw2);
                            nextSuper.Ref.IsCharged = true;
                            Game.CurrentSWType = sw2.Ref.ArrayIndex;
                            inCurrentSW = true;
                            break;
                        }
                    case 2:
                        {
                            //Logger.Log("丢2");

                            nextSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw3);
                            nextSuper.Ref.IsCharged = true;
                            Game.CurrentSWType = sw3.Ref.ArrayIndex;
                            inCurrentSW = true;
                            break;
                        }
                    case 3:
                        {
                            //Logger.Log("丢3");

                            nextSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw4);
                            nextSuper.Ref.IsCharged = true;
                            Game.CurrentSWType = sw4.Ref.ArrayIndex;
                            inCurrentSW = true;
                            break;
                        }
                    case 4:
                        {
                            //Logger.Log("丢4");

                            //sw1进入冷却
                            Pointer<SuperClass> pSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw1);
                            pSuper.Ref.IsCharged = false;
                            pSuper.Ref.RechargeTimer.Start(600);
                            pSuper.Ref.CameoChargeState = 0;

                            var technos = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, techno => technoIds.Contains(techno.Ref.Type.Ref.Base.Base.ID), FindRange.Owner);

                            var techno1 = technos.Where(t => t.OwnerObject.Ref.Type.Ref.Base.Base.ID == technoId1).FirstOrDefault();
                            var techno2 = technos.Where(t => t.OwnerObject.Ref.Type.Ref.Base.Base.ID == technoId2).FirstOrDefault();
                            var techno3 = technos.Where(t => t.OwnerObject.Ref.Type.Ref.Base.Base.ID == technoId3).FirstOrDefault();
                            var techno4 = technos.Where(t => t.OwnerObject.Ref.Type.Ref.Base.Base.ID == technoId4).FirstOrDefault();

                            if (!techno1.IsNullOrExpired())
                            {
                                var t1location = techno1.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 2000);

                                if (!techno2.IsNullOrExpired())
                                {
                                    var bullet1 = pbullet.Ref.CreateBullet(techno2.OwnerObject.Convert<AbstractClass>(), techno1.OwnerObject, 1, warhead, 50, false);
                                    //bullet1.Ref.MoveTo(t1location, new BulletVelocity(0, 0, 0));
                                    //bullet1.Ref.SetTarget(techno2.OwnerObject.Convert<AbstractClass>());

                                    creatBulletScriptComponent(bullet1, techno2, techno1);
                                }

                                if (!techno3.IsNullOrExpired())
                                {
                                    var bullet2 = pbullet.Ref.CreateBullet(techno3.OwnerObject.Convert<AbstractClass>(), techno1.OwnerObject, 1, warhead, 50, false);
                                    //bullet2.Ref.MoveTo(t1location, new BulletVelocity(0, 0, 0));
                                    //bullet2.Ref.SetTarget(techno3.OwnerObject.Convert<AbstractClass>());

                                    creatBulletScriptComponent(bullet2, techno3, techno1);
                                }

                                if (!techno4.IsNullOrExpired())
                                {
                                    var bullet3 = pbullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), techno1.OwnerObject, 1, warhead, 50, false);
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

            if (inited && nextSuper.IsNotNull && !nextSuper.Ref.IsCharged)
            {
                inCurrentSW = false;
            }

            //取消超武释放时也移除所有建筑
            if (inited && inCurrentSW)
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

            //当后三个建筑都死亡则移除第一个建筑
            if (inited && linkSWScriptBulletCount.GetCount() >= 3)
            {
                linkSWScriptBulletCount.Zero();
                var li = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, techno => techno.Ref.Type.Ref.Base.Base.ID == technoId1, FindRange.Owner);
                foreach (var item in li)
                {
                    if (!item.IsNullOrExpired())
                    {
                        item.OwnerObject.Ref.Base.Remove();
                        item.OwnerObject.Ref.Base.UnInit();
                    }
                }
            }

            base.OnUpdate();
        }

        private void creatBulletScriptComponent(Pointer<BulletClass> bullet, TechnoExt techno, TechnoExt techno1)
        {
            BulletExt bulletExt = BulletExt.ExtMap.Find(bullet);

            if (!bulletExt.IsNullOrExpired())
            {
                if (bulletExt.GameObject.GetComponent(LinkSWScriptBullet.ID) == null)
                {
                    bulletExt.GameObject.CreateScriptComponent(nameof(LinkSWScriptBullet), LinkSWScriptBullet.ID, "LinkSWScriptBullet", bulletExt, techno, techno1, linkSWScriptBulletCount);
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
    public class LinkSWScriptBullet : BulletScriptable
    {
        public static int ID = 85287;

        private TechnoExt pTechno;
        private TechnoExt pTechno1;

        private LinkSWScriptBulletCount linkSWScriptBulletCount;

        public LinkSWScriptBullet(BulletExt owner, TechnoExt pTechno, TechnoExt pTechno1, LinkSWScriptBulletCount linkSWScriptBulletCount) : base(owner)
        {
            this.pTechno = pTechno;
            this.pTechno1 = pTechno1;
            this.linkSWScriptBulletCount = linkSWScriptBulletCount;

            //Logger.Log($"进入脚本的单位：{pTechno.Ref.Type.Ref.Base.Base.ID}");

            var t1location = pTechno1.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 2000);

            owner.OwnerObject.Ref.MoveTo(t1location, new BulletVelocity(0, 0, 0));
            owner.OwnerObject.Ref.SetTarget(pTechno.OwnerObject.Convert<AbstractClass>());
        }

        public override void OnDetonate(Pointer<CoordStruct> pCoords)
        {
            pTechno.OwnerObject.Ref.Base.Remove();
            pTechno.OwnerObject.Ref.Base.UnInit();

            linkSWScriptBulletCount.Add();

            DetachFromParent();
            return;

            //base.OnDetonate(pCoords);
        }
    }

    [Serializable]

    public class LinkSWScriptBulletCount
    {
        private static int count;


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
    }
}


