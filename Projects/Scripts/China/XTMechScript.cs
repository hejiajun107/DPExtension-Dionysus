using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(XTMechScript))]
    public class XTMechScript : TechnoScriptable
    {
        public XTMechScript(TechnoExt owner) : base(owner) { }

        TechnoExt pTargetRef;

        static Pointer<WarheadTypeClass> selfWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkipWhSelf");
        static Pointer<WarheadTypeClass> targeWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkipWhTarget");

        //加速buff
        static Pointer<WarheadTypeClass> speedWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkipSpeedWh");

        //盾buff
        static Pointer<WarheadTypeClass> guardWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTGuardWh");

        //砍
        static Pointer<WarheadTypeClass> axeWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTAxeWh2");

        //减速
        static Pointer<WarheadTypeClass> slowWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSlowDownWh");

        


        //贴上mk2的buff
        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");






        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        //检测符合跃迁条件的间隔,每xx帧检测一次
        private int checkRof = 0;
        //跃迁的冷却
        private int transCoolDown = 0;

        private int guardCoolDown = 0;

        private bool IsMkIIUpdated = false;

        //是否充能攻击
        private bool isCharged = false;
        //充能剩余时间
        private int chargedLeftTime = 0;

        public override void OnUpdate()
        {
            if (guardCoolDown > 0)
            {
                guardCoolDown--;
            }

            if (transCoolDown > 0)
            {
                transCoolDown--;
            }

            if (isCharged == true)
            {
                if (chargedLeftTime > 0)
                {
                    chargedLeftTime--;
                }
                else
                {
                    isCharged = false;
                }
            }


            if (checkRof-- <= 0)
            {
                checkRof = 20;

                if (transCoolDown == 0)
                {
                    pTargetRef = (TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Target.Convert<TechnoClass>()));
                    if (!pTargetRef.IsNullOrExpired())
                    {
                        var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                        var targetLocation = pTargetRef.OwnerObject.Ref.Base.Base.GetCoords();
                        var distance = currentLocation.DistanceFrom(targetLocation);
                        if (distance >= 4 * 256 && distance <= 14 * 256)
                        {
                            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, speedWarhead, 100, false);
                            pBullet.Ref.DetonateAndUnInit(currentLocation);
                            
                            //SkipTo(currentLocation, targetLocation);
                            transCoolDown = 400;

                            if (IsMkIIUpdated)
                            {
                                isCharged = true;
                                chargedLeftTime = 180;
                            }

                        }
                    }
                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (isCharged)
            {
                isCharged = false;

                Pointer<BulletClass> pAxe = pBulletType.Ref.CreateBullet(pTarget, Owner.OwnerObject, 120, axeWarhead, 100, false);
                pAxe.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());

                Pointer<BulletClass> pSpd = pBulletType.Ref.CreateBullet(pTarget, Owner.OwnerObject, 1, slowWarhead, 100, false);
                pSpd.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
            }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
        Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (IsMkIIUpdated == false)
            {
                //判断是否来自升级弹头
                if(pWH.Ref.Base.ID.ToString()== "MarkIISpWh")
                {
                    IsMkIIUpdated = true;
                    Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                    CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                    Pointer<BulletClass> mk2bullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, mk2Warhead, 100, false);
                    mk2bullet.Ref.DetonateAndUnInit(currentLocation);
                }
            }


            if (guardCoolDown <= 0 && pDamage.Ref > 20)
            {
                try
                {
                    if (pAttackingHouse.IsNull)
                    {
                        return;
                    }
                    if (pAttackingHouse.Ref.ArrayIndex != Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                    {
                        guardCoolDown = 200;
                        Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                        CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                        Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, guardWarhead, 100, false);
                        pBullet.Ref.DetonateAndUnInit(currentLocation);

                   

                    }
                }
                catch (Exception) {; }
           
            }

        }




        //跃迁至指定位置
        private void SkipTo(CoordStruct location, CoordStruct target)
        {
            //Owner.OwnerObject.Ref.Base.SetLocation(target);

            //if (MapClass.Instance.TryGetCellAt(CellClass.Coord2Cell(target), out Pointer<CellClass> pcell))
            //{
            //    //pcell.Ref.
            //}
            
            //Owner.OwnerObject.Ref.Base.Remove();
            //bool putted = false;
            //CoordStruct lastLocation = target;
            //if (!Owner.OwnerObject.Ref.Base.Put(target, Direction.N))
            //{
            //    Logger.Log("直接设置失败");
            //    CellSpreadEnumerator enumerator = new CellSpreadEnumerator(2);

            //    //放置失败的话在目标附近寻找位置
            //    foreach (CellStruct offset in enumerator)
            //    {
            //        var currentCell = CellClass.Coord2Cell(target);
            //        CoordStruct where = CellClass.Cell2Coord(currentCell + offset, target.Z);

            //        if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
            //        {
            //            if (Owner.OwnerObject.Ref.Base.Put(CellClass.Cell2Coord(pCell.Ref.MapCoords), Direction.N))
            //            {
            //                Logger.Log("周围设置成功");
            //                putted = true;
            //                lastLocation = CellClass.Cell2Coord(pCell.Ref.MapCoords);
            //                break;
            //            }
            //        }
            //    }

            //    //目标附近也失败的话回到原地
            //    if (putted == false)
            //    {
            //        //位移失败保持原位
            //        if (Owner.OwnerObject.Ref.Base.Put(location, Direction.N))
            //        {
            //            Logger.Log("返回原地成功");
            //            //放回原位也失败的话附近找一个位置放置防止吞单位
            //            putted = true;
            //            lastLocation = location;
            //        }
            //        else
            //        {
            //            Logger.Log("返回原地失败");
            //            foreach (CellStruct offset in enumerator)
            //            {
            //                var currentCell = CellClass.Coord2Cell(location);
            //                CoordStruct where = CellClass.Cell2Coord(currentCell + offset, location.Z);

            //                if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
            //                {
            //                    if (Owner.OwnerObject.Ref.Base.Put(CellClass.Cell2Coord(pCell.Ref.MapCoords), Direction.N))
            //                    {
            //                        Logger.Log("返回原地附近成功");
            //                        putted = true;
            //                        lastLocation = CellClass.Cell2Coord(pCell.Ref.MapCoords);
            //                        break;
            //                    }
            //                }
            //            }
            //        }
            //    }


            //}
            //else
            //{
            //    putted = true;
            //    lastLocation = target;
            //}

            //if (putted && lastLocation != null)
            //{
            //Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, selfWarhead, 100, false);
            //pBullet.Ref.DetonateAndUnInit(location);

            //Pointer<BulletClass> pBullet2 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, targeWarhead, 100, false);
            //pBullet2.Ref.DetonateAndUnInit(target);

            //pBullet2.Ref.DetonateAndUnInit(lastLocation);
            //}



        }

    }
}
