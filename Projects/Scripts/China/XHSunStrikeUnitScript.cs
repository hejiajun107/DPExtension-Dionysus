using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(XHSunStrikeUnitScript))]
    public class XHSunStrikeUnitScript : TechnoScriptable
    {
        //移动的直线距离
        private const int MoveDistance = 500;

        private Random random = new Random(120524);

        public XHSunStrikeUnitScript(TechnoExt owner) : base(owner) { }

        public bool isActived = false;

        public int currentDuration = 0;

        static ColorStruct innerColor = new ColorStruct(255, 128, 0);
        static ColorStruct outerColor = new ColorStruct(255, 128, 0);
        static ColorStruct outerSpread = new ColorStruct(255, 128, 0);


        static Pointer<WarheadTypeClass> fireWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHFireWH");


        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<BulletTypeClass> pball1 => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("XHSunStrikeBall1");

        private int height = 0;
        private CoordStruct start;

        private int radius = 0;

        private int sAngle = 0;

        private int rof = 0;


        public override void OnUpdate()
        {
            if (!isActived)
            {
                return;
            }

            if (radius <= 0)
            {

                currentDuration = 0;
                isActived = false;
                return;
            }

            if (rof++ < 2)
            {
                return;
            }
            rof = 0;


            for (var angle = sAngle; angle <= sAngle + 360; angle += 30)
            {
                //火焰圈
                var pos = new CoordStruct(start.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), start.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), start.Z);

                int damage = 8;
                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, fireWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(pos);
            }

            radius -= 30;
            sAngle += 20;
        }



        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (isActived == false)
            {
                isActived = true;
                int height = Owner.OwnerObject.Ref.Base.GetHeight();
                var location = pTarget.Ref.GetCoords() + new CoordStruct(random.Next(-500, 500), random.Next(-500, 500), 0); //Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0,0,-height);

                radius = 700;

                //起始点
                start = location;
                sAngle = 0;
            }

        }




    }
}
