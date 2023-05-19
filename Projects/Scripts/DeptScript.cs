using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(DeptScript))]
    public class DeptScript : TechnoScriptable
    {

        public DeptScript(TechnoExt owner) : base(owner) 
        {
            pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }


        static Pointer<WarheadTypeClass> repairWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DEPTRepDpWh");
        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<AnimTypeClass> deptAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("DeptRing");

        private SwizzleablePointer<AnimClass> pAnim;

        private bool inited = false;

        private int rof = 0;
        public override void OnUpdate()
        {
            if (!inited)
            {
                inited = true;
                var anim = YRMemory.Create<AnimClass>(deptAnim, Owner.OwnerObject.Ref.Base.Base.GetCoords());
                pAnim.Pointer = anim;
                //anim.Ref.SetOwnerObject(Owner.OwnerObject.Convert<ObjectClass>());
            }

            var visible = true;

            if (Owner.OwnerObject.Ref.Owner != HouseClass.Player)
                visible = false;

            if (!Owner.OwnerObject.Ref.Base.IsSelected)
                visible = false;

            //Logger.Log(visible);
            if (!pAnim.IsNull)
            {
                pAnim.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                pAnim.Ref.Invisible = !visible;
            }

            if (rof-- > 0)
            {
                return;
            }
            rof = 120;

            if (Owner.OwnerObject.Ref.Base.IsAlive && Owner.OwnerObject.Ref.Base.IsOnMap && Owner.OwnerObject.Ref.Base.InLimbo == false)
            {
                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 40, repairWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }
        }

        public override void OnRemove()
        {
            if (!pAnim.IsNull)
            {
                pAnim.Ref.TimeToDie = true;
                pAnim.Ref.Base.UnInit();
                pAnim.Pointer = IntPtr.Zero;
            }
        }
    }
}
