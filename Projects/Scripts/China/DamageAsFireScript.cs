using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(DamageAsFireScript))]
    public class DamageAsFireScript : TechnoScriptable
    {

        private int delay = 200;
        public DamageAsFireScript(TechnoExt owner) : base(owner)
        {

        }

        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<AnimTypeClass> pAnimType => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("AFIREANIM");

        static Pointer<WarheadTypeClass> buffWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DamageAsFireWh");

        static Pointer<WarheadTypeClass> breakMindWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DAFBreakMindWh");


        private bool inited = false;

        public override void OnUpdate()
        {
            if (!inited)
            {
                inited = true;
                var anim = YRMemory.Create<AnimClass>(pAnimType, Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }

            if (delay % 10 == 0)
            {
                Pointer<BulletClass> pbuff = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, buffWarhead, 100, false);
                pbuff.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

                Pointer<BulletClass> pbreak = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, breakMindWh, 100, false);
                pbreak.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }

            if (delay-- <= 0)
            {
                Owner.OwnerObject.Ref.Base.Remove();
                Owner.OwnerObject.Ref.Base.UnInit();
                return;
            }
            base.OnUpdate();
        }
    }
}
