using DynamicPatcher;
using Extension.Decorators;
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

namespace DpLib.Scripts.China
{
    [Serializable]
    public class XHFragmentBulletScirpt : BulletScriptable
    {

        private Random random = new Random(151523);

        public XHFragmentBulletScirpt(BulletExt owner) : base(owner) { }

        public bool isActived = false;

        private int rof = 10;

        TechnoExt pOwnerRef;

        CoordStruct lastLocation;

        bool started = false;


        static Pointer<WarheadTypeClass> fireWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XHFireWH");

        static Pointer<BulletTypeClass> plaserBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("XhCometP2");

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static int colorIndex;

        static List<ColorStruct> Colors = new List<ColorStruct>()
        {
            new ColorStruct(237,28,36),
            new ColorStruct(255,127,39),
            new ColorStruct(255,242,0),
            new ColorStruct(34,177,76),
            new ColorStruct(63,126,129),
            new ColorStruct(0,128,255),
            new ColorStruct(163,73,164)
        };

        static ColorStruct GetColor() 
        {
            colorIndex++;
            if (colorIndex >= Colors.Count())
                colorIndex = 0;
            return Colors[colorIndex];
        }
      


        public bool reverse = false;

        private ColorStruct colorSelect;
   

        private CoordStruct targetLocation;


        public override void OnUpdate()
        {
            CoordStruct nextLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            if (started == false)
            {
                started = true;
                lastLocation = nextLocation;
                colorSelect = GetColor();
                return;
            }

            if (lastLocation.DistanceFrom(nextLocation) > 50)
            {
                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(lastLocation, nextLocation, colorSelect, colorSelect, new ColorStruct(0, 0, 0), 100);
                pLaser.Ref.Thickness = 10;
                pLaser.Ref.IsHouseColor = true;

                lastLocation = nextLocation;
            }

            if (rof-- > 0)
            {
                return;
            }
            rof = 10;

            pOwnerRef = TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
            int height = Owner.OwnerObject.Ref.Base.GetHeight();

            targetLocation = Owner.OwnerObject.Ref.Target.Ref.GetCoords();

            if (!pOwnerRef.IsNullOrExpired())
            {

                var location = targetLocation;

                var currentCell = CellClass.Coord2Cell(location);

                CellSpreadEnumerator enumerator = new CellSpreadEnumerator(7);


                var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords(); //targetLocation; //Owner.OwnerObject.Ref.Base.Base.GetCoords();


                var damage = 20;
                Pointer<BulletClass> pBullet = plaserBullet.Ref.CreateBullet(pOwnerRef.OwnerObject.Convert<AbstractClass>(), pOwnerRef.OwnerObject, damage, fireWarhead, 50, false);

                var rdlocaton = targetLocation + new CoordStruct(random.Next(-1000, 1000), random.Next(-1000, 1000), 0);

                if (MapClass.Instance.TryGetCellAt(rdlocaton, out Pointer<CellClass> cell))
                {
                    pBullet.Ref.SetTarget(cell.Convert<AbstractClass>());
                    pBullet.Ref.MoveTo(currentLocation, new BulletVelocity(0, 0, 0));
                }
            }

        }





    }


}
