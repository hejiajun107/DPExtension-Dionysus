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

namespace DPMisc.DynamicPatcher.Scripts.MyScripts.Test
{
    [Serializable]
    [ScriptAlias(nameof(FutureStrafeTScript))]
    public class FutureStrafeTScript : TechnoScriptable
    {
        public FutureStrafeTScript(TechnoExt owner) : base(owner) { }

        private int Range = 2;//扫射目标左右两边的距离

        private int range = 0;

        private Pointer<WeaponTypeClass> indicatorWeaponType => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("GOLTGuidWeapon");//指引扫射的武器

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(weaponIndex==0)
            {
                Pointer<TechnoClass> ownerTechno = Owner.OwnerObject;
                CoordStruct ownerCoord = ownerTechno.Ref.Base.Base.GetCoords();
                //DebugUtilities.MarkLocation(ownerCoord, new ColorStruct(0, 0, 255), beamHeight: 832, thickness: 4, duration: 150);

                CoordStruct targetCoord = pTarget.Ref.GetCoords();
                //DebugUtilities.MarkLocation(targetCoord, new ColorStruct(0, 0, 255), beamHeight: 832, thickness: 4, duration: 150);

                //获取开火的坐标
                CoordStruct flh = Owner.OwnerObject.Ref.GetWeapon(weaponIndex).Ref.FLH;
                CoordStruct rightFireCoord = ExHelper.GetFLHAbsoluteCoords(ownerTechno, flh, isOnTurret: true, flipY: 1);
                //DebugUtilities.MarkLocation(rightFireCoord, new ColorStruct(255, 0, 0), beamHeight: 832, thickness: 4, duration: 150);
                CoordStruct leftFireCoord = ExHelper.GetFLHAbsoluteCoords(ownerTechno, flh, isOnTurret: true, flipY: -1);
                //DebugUtilities.MarkLocation(leftFireCoord, new ColorStruct(0, 255, 0), beamHeight: 832, thickness: 4, duration: 150);

                //计算目标点左右的坐标
                CoordStruct distanceCoord = targetCoord - ownerCoord;
                double distance = targetCoord.DistanceFrom(ownerCoord);

                if (Range == 0)
                {
                    //Logger.Log($"Range为{range}，range设置为射程的一半{Owner.OwnerObject.Ref.GetWeapon(weaponIndex).Ref.WeaponType.Ref.Range / 2}");
                    range = Owner.OwnerObject.Ref.GetWeapon(weaponIndex).Ref.WeaponType.Ref.Range / 2;
                }
                else
                {
                    //Logger.Log($"Range为{range}，range设置为{Range * Game.CellSize}");
                    range = Range * Game.CellSize;
                }

                double angle = Math.Atan2(distanceCoord.Y, distanceCoord.X);
                double rightLeftDistance = Math.Sqrt(Math.Pow(distance, 2) + Math.Pow(range, 2));
                double rightLeftAngle = Math.Atan2(range, distance);

                //计算左边目标点的坐标
                double leftAngle = angle + rightLeftAngle;
                double leftX = Math.Cos(leftAngle) * rightLeftDistance;
                double leftY = Math.Sin(leftAngle) * rightLeftDistance;
                CoordStruct zeroLeftTargetCoord = new CoordStruct((int)leftX + ownerCoord.X, (int)leftY + ownerCoord.Y, 0);
                //DebugUtilities.MarkLocation(zeroLeftTargetCoord, new ColorStruct(255, 0, 0), beamHeight: 832, thickness: 4, duration: 120);

                //计算右边目标点的坐标
                double rightAngle = angle - rightLeftAngle;
                double rightX = Math.Cos(rightAngle) * rightLeftDistance;
                double rightY = Math.Sin(rightAngle) * rightLeftDistance;
                CoordStruct zeroRightTargetCoord = new CoordStruct((int)rightX + ownerCoord.X, (int)rightY + ownerCoord.Y, 0);
                //DebugUtilities.MarkLocation(zeroRightTargetCoord, new ColorStruct(0, 255, 0), beamHeight: 832, thickness: 4, duration: 120);

                //如果目标是单位 交叉的指向目标的左右两点，然后往中间夹
                if (pTarget.CastToTechno(out Pointer<TechnoClass> pTechno))
                {
                    Pointer<BulletTypeClass> indicatorBulletType = indicatorWeaponType.Ref.projectile.Convert<BulletTypeClass>();
                    Pointer<BulletClass> rightIndicatorBullet = indicatorBulletType.Ref.CreateBullet(pTarget, ownerTechno, indicatorWeaponType.Ref.Damage, indicatorWeaponType.Ref.Warhead, indicatorWeaponType.Ref.Speed, indicatorWeaponType.Ref.Bright);
                    creatBulletScriptComponent(rightIndicatorBullet, zeroRightTargetCoord, zeroLeftTargetCoord, weaponIndex, false, targetCoord);

                    Pointer<BulletClass> leftIndicatorBullet = indicatorBulletType.Ref.CreateBullet(pTarget, ownerTechno, indicatorWeaponType.Ref.Damage, indicatorWeaponType.Ref.Warhead, indicatorWeaponType.Ref.Speed, indicatorWeaponType.Ref.Bright);
                    creatBulletScriptComponent(leftIndicatorBullet, zeroLeftTargetCoord, zeroRightTargetCoord, weaponIndex, true, targetCoord);
                }

                //如果目标是地面 从开火点交叉扫向目标点
                if (pTarget.CastToCell(out Pointer<CellClass> pCell))
                {
                    Pointer<BulletTypeClass> indicatorBulletType = indicatorWeaponType.Ref.projectile.Convert<BulletTypeClass>();
                    Pointer<BulletClass> rightIndicatorBullet = indicatorBulletType.Ref.CreateBullet(pTarget, ownerTechno, indicatorWeaponType.Ref.Damage, indicatorWeaponType.Ref.Warhead, indicatorWeaponType.Ref.Speed, indicatorWeaponType.Ref.Bright);
                    creatBulletScriptComponent(rightIndicatorBullet, rightFireCoord, zeroLeftTargetCoord, weaponIndex, true, targetCoord);

                    Pointer<BulletClass> leftIndicatorBullet = indicatorBulletType.Ref.CreateBullet(pTarget, ownerTechno, indicatorWeaponType.Ref.Damage, indicatorWeaponType.Ref.Warhead, indicatorWeaponType.Ref.Speed, indicatorWeaponType.Ref.Bright);
                    creatBulletScriptComponent(leftIndicatorBullet, leftFireCoord, zeroRightTargetCoord, weaponIndex, false, targetCoord);
                }

            }
        }

        private void creatBulletScriptComponent(Pointer<BulletClass> bullet, CoordStruct startCoord, CoordStruct endCoord ,int weaponIndex ,bool isRightFire, CoordStruct targetCoord)
        {
            BulletExt bulletExt = BulletExt.ExtMap.Find(bullet);

            if (!bulletExt.IsNullOrExpired() && !Owner.IsNullOrExpired())
            {
                if (bulletExt.GameObject.GetComponent(FutureStrafeTScriptBullet.ID) == null)
                {
                    bulletExt.GameObject.CreateScriptComponent(nameof(FutureStrafeTScriptBullet), FutureStrafeTScriptBullet.ID, "FutureStrafeTScriptBullet", bulletExt, startCoord, endCoord, Owner, range, weaponIndex, isRightFire, targetCoord);
                }
            }
        }
    }

    [Serializable]
    [ScriptAlias(nameof(FutureStrafeTScriptBullet))]
    public class FutureStrafeTScriptBullet : BulletScriptable
    {
        public static int ID = 301051032;

        private CoordStruct startCoord;
        private CoordStruct endCoord;

        private CoordStruct targetCoord;

        private TechnoExt ownerTechnoExt;

        private int range;
        private int weaponIndex;

        private bool isRightFire;

        public FutureStrafeTScriptBullet(BulletExt owner, CoordStruct startCoord, CoordStruct endCoord, TechnoExt ownerTechnoExt, int range, int weaponIndex, bool isRightFire ,CoordStruct targetCoord) : base(owner)
        {
            this.startCoord = startCoord;
            this.endCoord = endCoord;
            this.ownerTechnoExt = ownerTechnoExt;
            this.range = range;
            this.weaponIndex = weaponIndex;
            this.isRightFire = isRightFire;
            this.targetCoord = targetCoord;

            Owner.OwnerObject.Ref.MoveTo(startCoord + new CoordStruct(0,0,3000), Owner.OwnerObject.Ref.Velocity);
        }

        private Pointer<WeaponTypeClass> weaponType => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("GOLTEffectWeapon");//扫射的武器

        private ColorStruct InnerColor = new ColorStruct(117, 251, 76);

        private ColorStruct OuterColor = new ColorStruct(255, 0, 0);

        private ColorStruct OuterSpread = new ColorStruct(188, 236, 237);

        private int LaserDuration = 1;

        private int Thickness = 5;

        public override void OnUpdate()
        {
            Pointer<BulletClass> indicatorBullet = Owner.OwnerObject;
            CoordStruct nowCoord = indicatorBullet.Ref.Base.Base.GetCoords();

            CoordStruct coord = endCoord - startCoord;

            double distance = endCoord.DistanceFrom(startCoord);

            if (ownerTechnoExt.IsNullOrExpired())
            {
                DetachFromParent();
                indicatorBullet.Ref.Base.UnInit();
            }

            if (!double.IsNaN(distance))
            {
                //double time = distance / indicatorBullet.Ref.Speed;
                double time = distance / 64;
                indicatorBullet.Ref.Velocity.X = coord.X / time;
                indicatorBullet.Ref.Velocity.Y = coord.Y / time;
                indicatorBullet.Ref.Velocity.Z = 0;
            }

            CoordStruct target = new CoordStruct(nowCoord.X, nowCoord.Y, 0);
            if (MapClass.Instance.TryGetCellAt(target, out Pointer<CellClass> pCell))
            {
                target.Z += pCell.Ref.GetCenterCoords().Z;

                if (pCell.Ref.ContainsBridge())
                {
                    target.Z += Game.BridgeHeight;
                }
            }

            Pointer<TechnoClass> ownerTechno = ownerTechnoExt.OwnerObject;
            Pointer<BulletTypeClass> bulletType = weaponType.Ref.projectile.Convert<BulletTypeClass>();
            Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(ownerTechno.Convert<AbstractClass>(), ownerTechno, (int)(weaponType.Ref.Damage * ownerTechno.Ref.FirepowerMultiplier), weaponType.Ref.Warhead, weaponType.Ref.Speed, weaponType.Ref.Bright);
            pBullet.Ref.Base.SetLocation(target);




            //绘制激光部分
            CoordStruct flh = ownerTechnoExt.OwnerObject.Ref.GetWeapon(weaponIndex).Ref.FLH;
            CoordStruct rightFireCoord = ExHelper.GetFLHAbsoluteCoords(ownerTechnoExt.OwnerObject, flh, isOnTurret: true, flipY: 1);
            CoordStruct leftFireCoord = ExHelper.GetFLHAbsoluteCoords(ownerTechnoExt.OwnerObject, flh, isOnTurret: true, flipY: -1);
            CoordStruct laserStartCoord = new CoordStruct(0,0,0);
            if (isRightFire)
            {
                laserStartCoord = rightFireCoord;
            }
            else
            {
                laserStartCoord = leftFireCoord;
            }
            //Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(laserStartCoord, target, InnerColor, OuterColor, OuterSpread, LaserDuration);
            //if (Thickness != 0)
            //{
            //    pLaser.Ref.IsHouseColor = true;
            //    pLaser.Ref.Thickness = Thickness;
            //}
            ownerTechnoExt.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, weaponType, laserStartCoord);


            pBullet.Ref.DetonateAndUnInit(target);


            //判断中断扫射的条件
            CoordStruct zeroStartCoord = new CoordStruct(startCoord.X, startCoord.Y, 0);
            CoordStruct zeroNowCoord = new CoordStruct(nowCoord.X, nowCoord.Y, 0);
            //Logger.Log($"当前距离：{zeroStartCoord.DistanceFrom(zeroNowCoord)} --- 半径：{ownerTechno.Ref.GetWeapon(weaponIndex).Ref.WeaponType.Ref.Range}");
            CoordStruct zeroEndCoord = new CoordStruct(endCoord.X, endCoord.Y, 0);
            if (zeroStartCoord.DistanceFrom(zeroNowCoord) >= zeroStartCoord.DistanceFrom(zeroEndCoord))
            {
                indicatorBullet.Ref.Base.UnInit();
            }

         

            //if (targetCoord.DistanceFrom(ownerTechnoExt.OwnerObject.Ref.Base.Base.GetCoords()) > ownerTechnoExt.OwnerObject.Ref.GetWeapon(weaponIndex).Ref.WeaponType.Ref.Range)
            //{
            //    indicatorBullet.Ref.Base.UnInit();
            //}

            //base.OnUpdate();
        }
    }
}
