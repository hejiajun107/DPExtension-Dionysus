using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(TimeWarpEffectScript))]
    [Serializable]
    public class TimeWarpEffectScript : TechnoScriptable
    {
        public TimeWarpEffectScript(TechnoExt owner) : base(owner)
        {
        }

        int rof = 0;

        // static uint CellSpread = 10;
        static int RadiusMax = Game.CellSize * 10;

        static int WidthFirst = Game.CellSize * 3; // 撕裂宽度。攻击点是撕裂带的中点

        private int radius = 2000;
        private int sangle = 0;

        // 此武器仅用于提供贴图激光的参数，主要是指定法线贴图以及纯黑的激光贴图
        static Pointer<WeaponTypeClass> pWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("TimeWarpLaser");
        // 创建抛射体时需要指定一个弹头，抛射体用于绘制贴图激光
        static Pointer<WarheadTypeClass> pWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Special");
        static Pointer<BulletTypeClass> pInvisible => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisble");

        private int delay = 50;

        public override void OnUpdate()
        {
            if (delay >= 0)
            {
                delay--;
                return;
            }

            if (--rof <= 0)
            {
                rof = 1;
                if (radius >= 50)
                {
                    DrawCircleLaser();
                    radius -= 2;
                }
            }
     
        }

        private void CreateSplit(CoordStruct posStart, CoordStruct posEnd, int width)
        {
            Pointer<TechnoClass> pTechno = Owner.OwnerObject;

            double distance = posStart.DistanceFrom(posEnd);
            double ratio = (double)width / 2 / distance;
            // 激光2个端点位于两点连线，靠近目标点，与目标点的距离为width/2
            CoordStruct posFrom = posEnd - new CoordStruct((int)(ratio * (posEnd.X - posStart.X)),
                (int)(ratio * (posEnd.Y - posStart.Y)),
                0);
            CoordStruct posTo = posEnd + new CoordStruct((int)(ratio * (posEnd.X - posStart.X)),
                (int)(ratio * (posEnd.Y - posStart.Y)),
                0);


            int damage = 0;
            Pointer<BulletClass> pBullet = pInvisible.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), pTechno, damage, pWH, 100, false);
            pBullet.Ref.Base.SetLocation(posTo);

            CoordStruct posBullet = pBullet.Ref.Base.Base.GetCoords();

            Pointer<LaserDrawClass> pLaser = pTechno.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, pWeapon, posFrom);
            pLaser.Ref.Thickness = 3;
            pLaser.Ref.IsHouseColor = true;

            pBullet.Ref.Base.UnInit();
        }


        private void DrawCircleLaser()
        {
            var center = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            CoordStruct lastpos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(sangle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(sangle * Math.PI / 180), 5)), center.Z + 0);

            for (var angle = sangle + 5; angle < sangle + 360; angle += 60)
            {
                var inRadius = radius - 50;
                var currentPos = new CoordStruct(center.X + (int)(inRadius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(inRadius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z + 0);
                CreateSplit(lastpos, currentPos, WidthFirst);
                // currentPos;
            }

            sangle += 10;
        }

    }
}
