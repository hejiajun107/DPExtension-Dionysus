﻿using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(AirportScript))]
    public class AirportScript : TechnoScriptable
    {

        public AirportScript(TechnoExt owner) : base(owner) { }

        static Pointer<WarheadTypeClass> repairWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("AirportRepWh");

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private int rof = 0;
        public override void OnUpdate()
        {
            if (rof-- > 0)
            {
                return;
            }
            rof = 200;

            if (Owner.OwnerObject.Ref.Base.IsAlive && Owner.OwnerObject.Ref.Base.IsOnMap && Owner.OwnerObject.Ref.Base.InLimbo == false)
            {
                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 20, repairWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }
        }
    }
}
