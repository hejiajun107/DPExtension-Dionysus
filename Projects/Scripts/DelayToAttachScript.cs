﻿using Extension.CW;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{
    [Serializable]
    public class DelayToAttachScript : TechnoScriptable
    {
        public DelayToAttachScript(TechnoExt owner) : base(owner)
        {
            var scriptArgs = "";
            var gext = owner.GameObject.GetComponent<TechnoGlobalExtension>();
            if (gext != null)
            {
                scriptArgs = gext.Data.ScriptArgs;
            }

            if (!string.IsNullOrEmpty(scriptArgs))
            {
                var args = scriptArgs.Split(',');
                if (int.TryParse(args[0], out var num))
                {
                    delay = num;
                }


                warhead = args[1];
            }
        }

        static Pointer<BulletTypeClass> bullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private int delay;

        private string warhead;

        public bool inited = false;

        public override void OnUpdate()
        {
            if(inited==false)
            {
                if (delay-- <= 0)
                {
                    inited = true;
                    var pWarhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find(warhead);

                    var pbull = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, pWarhead, 100, false);
                    pbull.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }
            base.OnUpdate();
        }
    }
}
