using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(ExileBulletScript))]
    public class ExileBulletScript : BulletScriptable
    {
        public ExileBulletScript(BulletExt owner) : base(owner)
        {
        }

        private bool actived = false;
        internal static List<string> immnueTypes => new List<string>() { "ZSTNK", "TU160", "GNTNK", "CNXHWSHIP", "CNXHWSHIP", "EPICTNK" };

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<WarheadTypeClass> damageWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ExileDamageWh");
        static Pointer<WarheadTypeClass> transWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ExileTransWh");

        private int delay = 80;


        public override void OnUpdate()
        {
            if (delay-- >= 0)
            {
                Owner.OwnerObject.Ref.Speed = 10;
                DrawCircleLaser();
                return;
            }

            if (actived == false)
            {
                actived = true;

                if (Owner.OwnerObject.Ref.Owner.IsNull)
                    return;


                //发射建筑
                var pBuilding = Owner.OwnerObject.Ref.Owner;

                var location = Owner.OwnerObject.Ref.TargetCoords;

                var damageBullet = pBulletType.Ref.CreateBullet(pBuilding.Convert<AbstractClass>(), pBuilding, 300, damageWarhead, 100, true);
                damageBullet.Ref.DetonateAndUnInit(location);


                var passengerCount = pBuilding.Ref.Passengers.NumPassengers;


                if (passengerCount >= 15)
                {
                    return;
                }

                var maxCount = 15 - passengerCount;


                var cell = CellClass.Coord2Cell(location);

                CellSpreadEnumerator enumerator = new CellSpreadEnumerator(2);


                foreach (CellStruct offset in enumerator)
                {
                    if (maxCount <= 0)
                        break;

                    CoordStruct where = CellClass.Cell2Coord(cell + offset, location.Z);
                    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                    {
                        var techno = pCell.Ref.FindTechnoNearestTo(new Point2D(60, 60), false, pBuilding);

                        var technoExt = TechnoExt.ExtMap.Find(techno);
                        if (technoExt == null)
                            continue;

                        if (technoExt.OwnerObject.Ref.Owner.IsNull)
                            continue;

                        if (technoExt.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Unit)
                            continue;

                        if (immnueTypes.Contains(technoExt.OwnerObject.Ref.Type.Ref.Base.Base.ID))
                            continue;


                        var transBullet = pBulletType.Ref.CreateBullet(pBuilding.Convert<AbstractClass>(), pBuilding, 1, transWarhead, 100, true);
                        transBullet.Ref.DetonateAndUnInit(technoExt.OwnerObject.Ref.Base.Base.GetCoords());

                        if (technoExt.OwnerObject.Ref.Base.Remove())
                        {
                            pBuilding.Ref.Passengers.AddPassenger(technoExt.OwnerObject.Convert<FootClass>());
                            maxCount--;
                        }

                    }
                }



            }
            base.OnUpdate();
        }

        private int startAngle = 0;

        private ColorStruct color = new ColorStruct(64,0,128);

        private void DrawCircleLaser()
        {
            var radius = 512;

            var center = Owner.OwnerObject.Ref.TargetCoords;

            CoordStruct lastpos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(startAngle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(startAngle * Math.PI / 180), 5)), center.Z);

            var lastAngle = startAngle + (360 / 80) * (80 - delay);

            for (var angle = startAngle + 5; angle < lastAngle; angle += 5)
            {
                var currentPos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z);
                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(lastpos, currentPos, color, color, color, 5);
                pLaser.Ref.Thickness = 10;
                pLaser.Ref.IsHouseColor = true;
                lastpos = currentPos;
            }

            var line = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(lastAngle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(lastAngle * Math.PI / 180), 5)), center.Z);
            Pointer<LaserDrawClass> pLine = YRMemory.Create<LaserDrawClass>(lastpos, center, color, color, color, 5);
            pLine.Ref.Thickness = 10;
            pLine.Ref.IsHouseColor = true;

            //startAngle += 2;
        }


    }
}
