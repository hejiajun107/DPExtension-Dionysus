using Extension.Coroutines;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(ScrinMotherShipScript))]
    public class ScrinMotherShipScript : TechnoScriptable
    {
        public ScrinMotherShipScript(TechnoExt owner) : base(owner)
        {
        }

        private const int MAX_STRENGTH = 4000;

        private int trailStartAngle = 0;
        private int trailRadius = 1280;

        private static ColorStruct trailOuterColor = new ColorStruct(64, 0, 128);

        private static ColorStruct laserColor2 = new ColorStruct(0, 0, 128);

        private int zAdjust = 350;

        private CoordStruct targetLocation = default;

        private static Pointer<BulletTypeClass> pBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private static Pointer<BulletTypeClass> misissle => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisoMissile");

        private static Pointer<WarheadTypeClass> exp2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MoShipBlastWh2");

        private static Pointer<WarheadTypeClass> expWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MoShipBlastWh");

        private static Pointer<WarheadTypeClass> shootWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MoShipShootWh");

        private static Pointer<WarheadTypeClass> exp2Warheads => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MoShipBlastWh2s");

        private static Pointer<WarheadTypeClass> expWarheads => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MoShipBlastWhs");

        private static Pointer<SuperWeaponTypeClass> moshipLight => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("MockLightMoShipSpecial");



        //当前阶段的持续时间
        private double stage = 0;

        //计数器，当长时间没有瞄准时退出开火
        private int exitFireDelay = 40;

        //是否正在开火
        private bool isFiring = false;

        public override void OnUpdate()
        {
            //环形激光
            DrawCircleLaser();

            if (exitFireDelay-- <= 0)
            {
                isFiring = false;
                stage = 0;
            }

            if (!isFiring)
                return;


            this.Firing();

            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (isFiring == false)
            {
                isFiring = true;
                stage = 0;
                targetLocation = pTarget.Ref.GetCoords();
            }
            exitFireDelay = 40;
        }

        private void Firing()
        {
            //血量低于30%时，充能速度减半
            if (Owner.OwnerObject.Ref.Base.Health > MAX_STRENGTH * 0.8)
            {
                stage += 2.5;
            }
            else if (Owner.OwnerObject.Ref.Base.Health > MAX_STRENGTH * 0.3)
            {
                stage += 2;
            }
            else
            {
                stage += 1;
            }

            var myLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 0);

            stage += 2;

            //前1200帧点亮4个激光

            var stage2 = (int)(stage >= 1200 ? stage - 1200 : 0);
            stage2 = stage2 >= 500 ? 500 : stage2;
            var stage3 = (int)(stage > 1200 ? stage - 1200 + 500 : 500);

            var color = new ColorStruct(laserColor2.R + (stage2 >= 127 ? (stage2 - 127) / 5 : 0), laserColor2.G, laserColor2.B + (stage2 >= 127 ? 127 : stage2));

            var angle = 125;

            for (var i = 0; i < 6; i++)
            {
                angle = 125 + 60 * i;
                var laserPosition = new CoordStruct(myLocation.X + (int)(trailRadius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), myLocation.Y + (int)(trailRadius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), myLocation.Z + zAdjust);
                if (stage >= i * 200)
                {
                    Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(laserPosition, myLocation, color, color, color, 5);
                    pLaser.Ref.Thickness = (int)((stage3 - 500) / 45) + 5;
                    pLaser.Ref.IsHouseColor = true;
                }
            }

            if (stage >= 2500)
            {
                //开火完成
                isFiring = false;
                stage = 0;
                LaunchCannon();
            }

        }


        private void LaunchCannon()
        {
            var start = Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, -100);

            if (targetLocation != null && targetLocation != default)
            {
                var bigLaserColor = new ColorStruct(192, 192, 1920);
                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(start, targetLocation, bigLaserColor, bigLaserColor, bigLaserColor, 30);
                pLaser.Ref.Thickness = 30;
                pLaser.Ref.IsHouseColor = true;

                //开火动画
                var bulletFire = pBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, shootWarhead, 100, true);
                bulletFire.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

                //伤害和剩余血量成正比
                var damage = (Owner.OwnerObject.Ref.Base.Health / (MAX_STRENGTH / 5)) * 150 + 200;

                //爆炸动画
                var bullet = pBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, (Owner.OwnerObject.Ref.Base.Health > (MAX_STRENGTH / 4)) ? expWarhead : expWarheads, 100, true);
                bullet.Ref.DetonateAndUnInit(targetLocation);

                var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var exp2damge = (Owner.OwnerObject.Ref.Base.Health / (MAX_STRENGTH / 5)) * 100;

                ShowLight(Owner);
                GameObject.StartCoroutine(ToBlast(coord, exp2damge));

            }
        }

        IEnumerator ToBlast(CoordStruct coord, int damage)
        {
            yield return new WaitForFrames(40);
            BlastCircleAt(coord, damage);
        }

        private void BlastCircleAt(CoordStruct coord,int damage)
        {
            //在周围造成伤害
            for (var angle = 0; angle < 360; angle += 60)
            {
                var height = Owner.OwnerObject.Ref.Base.GetHeight();
                var center = coord;

                var radius = 8 * 256;
                var targetPos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z - height);

                var cell = CellClass.Coord2Cell(targetPos);

                if (MapClass.Instance.TryGetCellAt(cell, out var pCell))
                {
                    var exp2damge = damage;
                    var inviso = pBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, exp2damge, (Owner.OwnerObject.Ref.Base.Health > (MAX_STRENGTH / 4)) ? exp2Warhead : exp2Warheads, 50, false);
                    //inviso.Ref.MoveTo(targetPos + new CoordStruct(0, 0, 2000), new BulletVelocity(0, 0, 0));
                    //inviso.Ref.SetTarget(pCell.Convert<AbstractClass>());
                    inviso.Ref.DetonateAndUnInit(targetPos);
                }
            }
        }

        private void DrawCircleLaser()
        {
            var center = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            CoordStruct lastpos = new CoordStruct(center.X + (int)(trailRadius * Math.Round(Math.Cos(trailStartAngle * Math.PI / 180), 5)), center.Y + (int)(trailRadius * Math.Round(Math.Sin(trailStartAngle * Math.PI / 180), 5)), center.Z + zAdjust);

            for (var angle = trailStartAngle + 5; angle < trailStartAngle + 360; angle += 5)
            {
                var currentPos = new CoordStruct(center.X + (int)(trailRadius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(trailRadius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z + zAdjust);
                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(lastpos, currentPos, trailOuterColor, trailOuterColor, trailOuterColor, 5);
                pLaser.Ref.Thickness = 10;
                pLaser.Ref.IsHouseColor = true;
                lastpos = currentPos;
            }

            trailStartAngle += 2;
        }

        private void ShowLight(TechnoExt pOwner)
        {
            Pointer<SuperClass> pSuper = pOwner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(moshipLight);
            CellStruct targetCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            pSuper.Ref.IsCharged = true;
            pSuper.Ref.Launch(targetCell, true);
            pSuper.Ref.IsCharged = false;
        }

    }
}
