﻿using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(StormTowerScript))]
    public class StormTowerScript : TechnoScriptable
    {
        public StormTowerScript(TechnoExt owner) : base(owner)
        {
        }

        private bool inited = false;

        private int cAngle = 0;

        static Pointer<TechnoTypeClass> pType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("STROMBALL");

        List<TechnoExt> technos = new List<TechnoExt>();

        private int radius = 420;

        private int height = 550;

        public override void OnUpdate()
        {
            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            if (!inited)
            {
                if (Owner.OwnerObject.Ref.Base.IsOnMap)
                {
                    inited = true;
                }


                for (var angle = cAngle; angle < cAngle + 360; angle += 120)
                {
                    var techno = pType.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();


                    var position = new CoordStruct(location.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), location.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), location.Z + height);

                    var putResult = techno.Ref.Base.Put(position, Direction.N);

                    var ext = TechnoExt.ExtMap.Find(techno);
                    technos.Add(ext);
                }


            }



            if (inited)
            {
                var mission = Owner.OwnerObject.Convert<MissionClass>();
                var angle = cAngle;

                foreach (var item in technos)
                {
                    if (!item.IsNullOrExpired())
                    {
                        var position = new CoordStruct(location.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), location.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), location.Z + height);
                        item.OwnerObject.Ref.Base.SetLocation(position);
                        angle += 120;
                    }
                }

                if (cAngle <= 360)
                    cAngle += 1;
                else
                    cAngle = 0;



            }




            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);
            foreach (var item in technos)
            {
                if (!item.IsNullOrExpired())
                {
                    item.OwnerObject.Ref.SetTarget(pTarget);
                }
            }
        }

        public override void OnRemove()
        {
            foreach (var item in technos)
            {
                if (!item.IsNullOrExpired())
                {
                    item.OwnerObject.Ref.Base.UnInit();
                }
            }
            base.OnRemove();
        }

    }
}
