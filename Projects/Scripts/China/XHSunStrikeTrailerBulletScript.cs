using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(XHSunStrikeTrailerBulletScript))]

    public class XHSunStrikeTrailerBulletScript : BulletScriptable
    {
        public XHSunStrikeTrailerBulletScript(BulletExt owner) : base(owner)
        {

        }

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

        private ColorStruct colorSelected;


        CoordStruct lastLocation;

        bool started = false;

        public override void OnUpdate()
        {
            CoordStruct nextLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            if (started == false)
            {
                started = true;
                lastLocation = nextLocation;
                colorSelected = GetColor();
                return;
            }

            //nextLocation.Z += 50;
            if (lastLocation.DistanceFrom(nextLocation) > 50)
            {
                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(lastLocation, nextLocation, colorSelected, colorSelected, new ColorStruct(0, 0, 0), 100);
                pLaser.Ref.Thickness = 10;
                pLaser.Ref.IsHouseColor = true;

                lastLocation = nextLocation;
            }
        }

    }
}
