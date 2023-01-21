using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(AttackPlaneScript))]
    public class AttackPlaneScript : TechnoScriptable
    {
        public AttackPlaneScript(TechnoExt owner) : base(owner)
        {
        }

        private int currentIndex = 0;

        private int Duration = 0;

        private int damage = 10;

        static Pointer<BulletTypeClass> bullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("JH7Missile");
        static Pointer<WarheadTypeClass> scanWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("A5ScanWh");

        static Pointer<AnimTypeClass> FireAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("A5Fire");

        private Random rd = new Random(114514);

        private CoordStruct targetCoord;

        private List<CoordStruct> FLHS = new List<CoordStruct>()
        {
            new CoordStruct(-30,110,-45),
            new CoordStruct(-30,-110,-45),
            new CoordStruct(-50,167,-50),
            new CoordStruct(-50,-167,-50),
            new CoordStruct(-100,220,-55),
            new CoordStruct(-100,-220,-55),
            new CoordStruct(80,0,-80),
        };

        public CoordStruct GetNextFLH()
        {
            if (currentIndex > FLHS.Count - 1)
            {
                currentIndex = 0;
            }
            var flh = FLHS[currentIndex];
            currentIndex++;
            return ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, flh, false, 1);
        }

        public override void OnUpdate()
        {
            if (Duration > 0)
            {
                if (targetCoord != null)
                {
                    var fireStart = GetNextFLH();
                    var fireEnd = targetCoord + new CoordStruct(rd.Next(-600, 600), rd.Next(-600, 600), 0);

                    if (MapClass.Instance.TryGetCellAt(fireEnd, out var pcell))
                    {
                        Pointer<BulletClass> pBullet = bullet.Ref.CreateBullet(pcell.Convert<AbstractClass>(), Owner.OwnerObject, damage, scanWarhead, 100, true);
                        pBullet.Ref.MoveTo(fireStart, new BulletVelocity(rd.Next(-30, 30), rd.Next(-30, 30), 0));
                        pBullet.Ref.SetTarget(pcell.Convert<AbstractClass>());
                        YRMemory.Create<AnimClass>(FireAnim, fireStart);
                    }
                }

                if (damage < 25)
                {
                    damage++;
                }
                Duration--;
            }
        }


        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(Duration<=0)
            {
                Duration = 30;
                targetCoord = pTarget.Ref.GetCoords() ;
            }
            //base.OnFire(pTarget, weaponIndex);
        }

    }

    [Serializable]
    [ScriptAlias(nameof(AttackPlaneBulletScript))]
    public class AttackPlaneBulletScript : BulletScriptable
    {
        public AttackPlaneBulletScript(BulletExt owner) : base(owner) { }

        public bool isActived = false;

        TechnoExt pOwnerExt;

        private static ColorStruct innerColor = new ColorStruct(200, 200, 160);
        private static ColorStruct outerColor = new ColorStruct(0, 0, 0);
        private static ColorStruct outerSpread = new ColorStruct(0, 0, 0);

        private int damage = 8;

        static Pointer<BulletTypeClass> bullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("JH7Missile");
        static Pointer<WarheadTypeClass> scanWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("A5ScanWh");

        static Pointer<AnimTypeClass> FireAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("A5Fire");

        public override void OnUpdate()
        {
            if (isActived == false)
            {
                pOwnerExt = TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);

                if (!pOwnerExt.IsNullOrExpired())
                {
                    isActived = true;
                }
            }

            if (isActived)
            {
                if (!pOwnerExt.IsNullOrExpired())
                {
                    var script = pOwnerExt.GameObject.GetComponent<AttackPlaneScript>();
                    if (script != null)
                    {
                        var fireStart = script.GetNextFLH();
                        var height = Owner.OwnerObject.Ref.Base.GetHeight();
                        var fireEnd = Owner.OwnerObject.Ref.Base.Base.GetCoords() - new CoordStruct(0, 0, height);

                        if(MapClass.Instance.TryGetCellAt(fireEnd,out var pcell))
                        {
                            Pointer<BulletClass> pBullet = bullet.Ref.CreateBullet(pcell.Convert<AbstractClass>(), pOwnerExt.OwnerObject, damage, scanWarhead, 100, true);
                            pBullet.Ref.MoveTo(fireStart, new BulletVelocity());
                            pBullet.Ref.SetTarget(pcell.Convert<AbstractClass>());
                            YRMemory.Create<AnimClass>(FireAnim, fireStart);
                        }

                        if (damage < 18)
                        {
                            damage++;
                        }

                    }


                }
            }

        }

    }
}
