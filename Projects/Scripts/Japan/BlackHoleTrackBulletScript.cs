using Extension.CW;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.Japan
{
    [Serializable]
    [ScriptAlias(nameof(BlackHoleTrackBulletScript))]
    public class BlackHoleTrackBulletScript : BulletScriptable
    {
        public BlackHoleTrackBulletScript(BulletExt owner) : base(owner)
        {
        }

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisibleAll");
        //黑洞用于强制秒杀的弹头
        static Pointer<WarheadTypeClass> killWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BhForceKillWh");

        static Pointer<WarheadTypeClass> peaceWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("PeaceKillWh");

        static Pointer<WarheadTypeClass> stopWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SBHStopMoveWH");


        private bool inited = false;

        private int radius = 500;

        private CoordStruct center;

        //初始角度
        private int angle = 0;

        private int attachDelay = 200;

        public override void OnUpdate()
        {
            //以下注释掉的段落采用滚筒洗衣机的轨迹
            //if (inited == false)
            //{
            //    inited = true;
            //    //初始半径
            //    radius = (int)Owner.OwnerObject.Ref.TargetCoords.DistanceFrom(Owner.OwnerObject.Ref.Base.Base.GetCoords());

            //    center = Owner.OwnerObject.Ref.TargetCoords;

            //    List<CoordStruct> coordList = new List<CoordStruct>();

            //    for (var vangle = 0; vangle <= 360; vangle += 15)
            //    {
            //        coordList.Add(new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z));
            //    }
            //    var coord = coordList.OrderBy(coord => coord.DistanceFrom(center)).FirstOrDefault();
            //    //获得初始角度
            //    angle = coordList.IndexOf(coord) * 15;
            //}

            //angle += 5;
            //radius -= 10;
            //int targetZ = Owner.OwnerObject.Ref.Base.Base.GetCoords().Z + 5;
            //if (targetZ > center.Z)
            //{
            //    targetZ = center.Z;
            //}

            //if (radius > 0)
            //{
            //    var blocation = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), targetZ);
            //    Owner.OwnerObject.Ref.Base.SetLocation(blocation);
            //}
            //---------------到此处结束----------------------------------




            if (!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                var pTechno = Owner.OwnerObject.Ref.Owner;

                if (attachDelay-- <= 0)
                {
                    attachDelay = 200;
                    var bullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), pTechno, 1, stopWarhead, 50, false);
                    bullet.Ref.DetonateAndUnInit(pTechno.Ref.Base.Base.GetCoords());
                }

                if (Owner.OwnerObject.Ref.TargetCoords.DistanceFrom(Owner.OwnerObject.Ref.Base.Base.GetCoords()) <= 256)
                {
                    if (!pTechno.Ref.Base.IsOnMap || pTechno.Ref.Base.InLimbo)
                    {
                        pTechno.Ref.Base.UnInit();
                    }
                    else
                    {
                        var bullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), pTechno, 100, killWarhead, 50, false);
                        bullet.Ref.DetonateAndUnInit(pTechno.Ref.Base.Base.GetCoords());

                        var ext = TechnoExt.ExtMap.Find(pTechno);
                        bool destroyed = false;
                        if (ext != null)
                        {
                            var gext = ext.GameObject.GetComponent<TechnoGlobalExtension>();
                            if (gext != null)
                            {
                                if (gext.Data.IsHarvester)
                                {
                                    pTechno.Ref.Base.TakeDamage(5000, peaceWarhead, true);
                                    destroyed = true;
                                }
                            }
                        }

                        if (!destroyed)
                        {
                            pTechno.Ref.Base.UnInit();
                        }
                    }
                    return;
                }

                var mission = pTechno.Convert<MissionClass>();
                mission.Ref.ForceMission(Mission.Stop);

                //位置
                if (pTechno.CastToFoot(out Pointer<FootClass> pfoot))
                {
                    pTechno.Ref.SetDestination(default);
                    //pfoot.Ref.Destination = default;
                    var source = pTechno.Ref.Base.Base.GetCoords();
                    pfoot.Ref.Locomotor.Mark_All_Occupation_Bits(0);
                    pfoot.Ref.Locomotor.Force_Track(-1, source);
                    pTechno.Ref.Base.UnmarkAllOccupationBits(source);
                    pfoot.Ref.Locomotor.Lock();
                    var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                    pTechno.Ref.Base.SetLocation(location);
                    pTechno.Ref.Base.UnmarkAllOccupationBits(location);
                }
            }


        }


    }
}
