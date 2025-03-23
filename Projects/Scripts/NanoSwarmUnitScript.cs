using DynamicPatcher;
using Extension.EventSystems;
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
    [ScriptAlias(nameof(NanoSwarmUnitScript))]
    public class NanoSwarmUnitScript : TechnoScriptable
    {
        public NanoSwarmUnitScript(TechnoExt owner) : base(owner) { }

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisibleAll");
        //用来禁止单位移动的弹头
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("NanoStopMoveWH");

        static Pointer<WarheadTypeClass> warheadExp => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SA");

        //注册在这里的单位会在撞到屏障时直接摧毁
        private static HashSet<string> TechnoExlodeOnSwarm = new HashSet<string>()
        {
            "V4ROCKET","V3ROCKET","HORNET","V5ROCKET","DMISL","CMISL","Sakura","ScrinBee"
        };

        private int delay = 1000;

        private int checkDelay = 10;

        public override void OnUpdate()
        {
            //此处会使屏障持续delay时间后自动消失
            if (delay-- <= 0)
            {
                Owner.OwnerObject.Ref.Base.Remove();
                Owner.OwnerObject.Ref.Base.UnInit();
                return;
            }

            var center = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            //半径，可以自行更改
            var radius = 1792; //3584; 




            ////这段注释掉了，可以绘制激光来看实际半径是否与动画大小一致
            //foreach (var pos in coordList)
            //{
            //    Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(center, pos + new CoordStruct(0, 0, 0), new ColorStruct(255, 0, 0), new ColorStruct(255, 0, 0), new ColorStruct(255, 0, 0), 5);
            //    pLaser.Ref.Thickness = 10;
            //    pLaser.Ref.IsHouseColor = false;
            //}

            NanoSwarmLateUpdate();

            var nanoBullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisibleSwarm").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Special"), 100, false);
            nanoBullet.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 500),new BulletVelocity(0,0,0));

            if (checkDelay-- > 0)
            {
                return;
            }

            checkDelay = 10;

            //以下为阻止单位进出屏障（以下需要DP2.0，或者Kratos，或者自己在YRPP中补全Locomotion相关代码）
            //遍历单位列表
            //ref DynamicVectorClass<Pointer<TechnoClass>> technos = ref TechnoClass.Array;

            var technos = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), Game.CellSize * 12).Select(x=>x.Convert<TechnoClass>());
            //for (int j = technos.Count() - 1; j >= 0; j--)
            foreach(var pTechno in technos)
            {
                //Pointer<TechnoClass> pTechno = technos.Get(j);

                var location = pTechno.Ref.Base.Base.GetCoords();



                //判断是否在圈的边缘 这里取了圈内外10%的区域作为判定区域(0.7~1.3) //因为增加了检测间隔，因此增加了屏障的厚度使其更不容易通过
                if (location.DistanceFrom(new CoordStruct(center.X, center.Y, location.Z)) >= radius * 0.7 && location.DistanceFrom(new CoordStruct(center.X, center.Y, location.Z)) <= radius * 1.3)
                {
                    if (pTechno.CastToFoot(out Pointer<FootClass> pfoot))
                    {
                        CoordStruct dest = pfoot.Ref.Locomotor.Destination();

                        //Logger.Log($"起点：{location.DistanceFrom(center)}");
                        //Logger.Log($"目标：{dest.DistanceFrom(center)}");

                        if (dest != default)
                        {
                            //判断是试图穿越圈子
                            if ((radius - dest.DistanceFrom(new CoordStruct(center.X, center.Y, location.Z))) * (radius - location.DistanceFrom(new CoordStruct(center.X, center.Y, location.Z))) <= 0)
                            {
                                if (TechnoExlodeOnSwarm.Contains(pTechno.Ref.Type.Ref.Base.Base.ID))
                                {
                                    //直接摧毁单位
                                    var sbullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 500, warheadExp, 100, false);
                                    sbullet.Ref.DetonateAndUnInit(location);
                                }
                                else
                                {
                                    //会对尝试穿越屏障的施加一个零速AE，如果要对飞行单位有效的话要等Phobos的修复空中单位受AE影响的修复发布
                                    var sbullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, warhead, 100, false);
                                    sbullet.Ref.DetonateAndUnInit(location);
                                }
                            }
                        }
                        else
                        {
                            //停了就别动了!
                            var sbullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, warhead, 100, false);
                            sbullet.Ref.DetonateAndUnInit(location);
                        }
                    }
                }
            }



        }


        //public override void Awake()
        //{
        //    EventSystem.General.AddTemporaryHandler(EventSystem.General.LogicClassUpdateEvent, NanoSwarmLateUpdate);
        //}

        //public override void OnDestroy()
        //{
        //    EventSystem.General.RemoveTemporaryHandler(EventSystem.General.LogicClassUpdateEvent, NanoSwarmLateUpdate);
        //}

        private void NanoSwarmLateUpdate()
        {
            //if (args is LogicClassUpdateEventArgs largs)
            //{
            //    //if (!largs.IsLateUpdate)
            //    //    return;
            //}
            //else
            //{
            //    return;
            //}

            var center = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            //半径，可以自行更改
            var radius = 1792; //3584; 

            List<CoordStruct> coordList = new List<CoordStruct>();

            //这里偷懒了，省去了计算直线和圆的交点过程，直接每15°一个区域，取最近的一个区域作为交点
            for (var angle = 0; angle <= 360; angle += 15)
            {
                coordList.Add(new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z));
            }

            //遍历抛射体
            ref DynamicVectorClass<Pointer<BulletClass>> bullets = ref BulletClass.Array;

            //Logger.Log($"抛射体数量:{bullets.Count}");

            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                Pointer<BulletClass> pBullet = bullets.Get(i);

                var target = pBullet.Ref.TargetCoords;
                //if(pBullet.Ref.Owner.IsNull)
                //    continue;
                var source = pBullet.Ref.SourceCoords;//.Owner.Ref.Base.Base.GetCoords();

                var bulletLocation = pBullet.Ref.Base.Base.GetCoords();

                //Logger.Log($"目标：{radius - target.DistanceFrom(center)}");
                //Logger.Log($"起点：{radius - source.DistanceFrom(center)}");
                //判断是否是从圈内到圈外或者圈外到圈内
                if ((radius - target.BigDistanceForm(new CoordStruct(center.X, center.Y, target.Z))) * (radius - source.BigDistanceForm(new CoordStruct(center.X, center.Y, source.Z))) <= 0)
                {
                    //Inviso的抛射体在于圆相交处爆炸
                    if (pBullet.Ref.Type.Ref.Inviso == true)
                    {
                        var ownerLocation = source;//pBullet.Ref.Owner.Ref.Base.Base.GetCoords();
                        //找最近的判定区爆炸（此处如果用公式计算交点更好，但是太麻烦了所以没写）
                        var expTarget = coordList.OrderBy(coord => coord.DistanceFrom(ownerLocation)).FirstOrDefault();

                        if (MapClass.Instance.TryGetCellAt(expTarget, out var pCell))
                        {
                            pBullet.Ref.SetTarget(pCell.Convert<AbstractClass>());
                        }
                        pBullet.Ref.Base.SetLocation(expTarget);
                        pBullet.Ref.Detonate(expTarget);
                        
                        pBullet.Ref.Base.Health = 0;
                        pBullet.Ref.Base.Remove();
                        pBullet.Ref.Base.UnInit();
                    }
                    else
                    {
                        //如果抛射体在圈附近，这里取了圈内外10%的区域作为判定区域(0.8~1.2)
                        if (bulletLocation.DistanceFrom(new CoordStruct(center.X, center.Y, bulletLocation.Z)) >= radius * 0.8 && bulletLocation.DistanceFrom(new CoordStruct(center.X, center.Y, bulletLocation.Z)) <= radius * 1.2)
                        {
                            pBullet.Ref.Detonate(bulletLocation);
                            pBullet.Ref.Base.Health = 0;
                            pBullet.Ref.Base.Remove();
                            pBullet.Ref.Base.UnInit();
                        }
                    }
                }

            }



        }
    }


    [Serializable]
    [ScriptAlias(nameof(NanoSwarnHealScript))]
    public class NanoSwarnHealScript : TechnoScriptable
    {
        public NanoSwarnHealScript(TechnoExt owner) : base(owner)
        {
        }

        private int delay = 20;
        public override void OnUpdate()
        {
            if(delay -- <= 0)
            {
                delay = 50;

                var pwh = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("NanoSwarmRWH");

                //修复受伤友军
                var technos = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), Game.CellSize * 8);

                var healthTechnos = technos.Where(x =>
                {
                    var ptechno = x.Convert<TechnoClass>();
                    if (!ptechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                        return false;

                    if (ptechno.Ref.Base.InLimbo)
                        return false;

                    if(MapClass.GetTotalDamage(10000, pwh, ptechno.Ref.Type.Ref.Base.Armor, 0) == 0)
                        return false;

                    if (ptechno.Ref.Base.Health >= ptechno.Ref.Type.Ref.Base.Strength)
                        return false;

                    return true;

                }).OrderBy( x => ((double)x.Ref.Health / (double)x.Convert<TechnoClass>().Ref.Type.Ref.Base.Strength)).ThenBy(x=>x.Ref.Base.GetCoords().BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords())).Take(5).ToList();

                foreach(var techno in healthTechnos)
                {
                    var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(techno.Convert<AbstractClass>(), Owner.OwnerObject, 100, pwh, 0, true);
                    pInviso.Ref.DetonateAndUnInit(techno.Ref.Base.GetCoords());
                }
            }
        }
    }




    [Serializable]
    [ScriptAlias(nameof(NanoSwarmBulletScript))]
    public class NanoSwarmBulletScript : BulletScriptable
    {

        public NanoSwarmBulletScript(BulletExt owner) : base(owner)
        {
        }

        public override void OnUpdate()
        {
            NanoSwarmLateUpdate();
            Owner.OwnerObject.Ref.Base.Remove();
            Owner.OwnerObject.Ref.Base.UnInit();
        }





        private void NanoSwarmLateUpdate()
        {
            //if (args is LogicClassUpdateEventArgs largs)
            //{
            //    //if (!largs.IsLateUpdate)
            //    //    return;
            //}
            //else
            //{
            //    return;
            //}

            var center = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            //半径，可以自行更改
            var radius = 1792; //3584; 

            List<CoordStruct> coordList = new List<CoordStruct>();

            //这里偷懒了，省去了计算直线和圆的交点过程，直接每15°一个区域，取最近的一个区域作为交点
            for (var angle = 0; angle <= 360; angle += 15)
            {
                coordList.Add(new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z));
            }

            //遍历抛射体
            ref DynamicVectorClass<Pointer<BulletClass>> bullets = ref BulletClass.Array;

            //Logger.Log($"抛射体数量:{bullets.Count}");

            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                Pointer<BulletClass> pBullet = bullets.Get(i);

                if (!pBullet.Ref.Type.Ref.Inviso)
                    continue;

                if (pBullet == Owner.OwnerObject)
                    continue;

                var target = pBullet.Ref.TargetCoords;
                //if(pBullet.Ref.Owner.IsNull)
                //    continue;
                var source = pBullet.Ref.SourceCoords;//.Owner.Ref.Base.Base.GetCoords();

                var bulletLocation = pBullet.Ref.Base.Base.GetCoords();

                //Logger.Log($"目标：{radius - target.DistanceFrom(center)}");
                //Logger.Log($"起点：{radius - source.DistanceFrom(center)}");
                //判断是否是从圈内到圈外或者圈外到圈内
                if ((radius - target.BigDistanceForm(new CoordStruct(center.X, center.Y, target.Z))) * (radius - source.BigDistanceForm(new CoordStruct(center.X, center.Y, source.Z))) <= 0)
                {
                    //Inviso的抛射体在于圆相交处爆炸
                    if (pBullet.Ref.Type.Ref.Inviso == true)
                    {
                        Logger.Log("逮捕Invisible" + Game.CurrentFrame);
                        var ownerLocation = source;//pBullet.Ref.Owner.Ref.Base.Base.GetCoords();
                        //找最近的判定区爆炸（此处如果用公式计算交点更好，但是太麻烦了所以没写）
                        var expTarget = coordList.OrderBy(coord => coord.DistanceFrom(ownerLocation)).FirstOrDefault();

                        if (MapClass.Instance.TryGetCellAt(expTarget, out var pCell))
                        {
                            pBullet.Ref.SetTarget(pCell.Convert<AbstractClass>());
                        }
                        pBullet.Ref.Base.SetLocation(expTarget);
                        pBullet.Ref.Detonate(expTarget);

                        pBullet.Ref.Base.Health = 0;
                        pBullet.Ref.Base.Remove();
                        pBullet.Ref.Base.UnInit();
                    }
                }

            }



        }

    }

}
