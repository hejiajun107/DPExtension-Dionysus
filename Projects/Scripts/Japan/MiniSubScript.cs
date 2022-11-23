using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(MiniSubScript))]
    [Serializable]
    public class MiniSubScript : TechnoScriptable
    {
        public MiniSubScript(TechnoExt owner) : base(owner)
        {
        }

        private int delay = 600;
        private bool started = false;

        static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<WarheadTypeClass> pWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MiniSubExpWh");

        static Pointer<WarheadTypeClass> pFlash => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SubBuffWh");


        static Pointer<WarheadTypeClass> pChaos => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChaosDamageWh");


        public override void OnUpdate()
        {
            if (started)
            {
                if (delay-- <= 0)
                {
                    Owner.OwnerObject.Ref.Base.TakeDamage(1000, pChaos, false);
                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(weaponIndex==1)
            {
                var mission = Owner.OwnerObject.Convert<MissionClass>();
                mission.Ref.ForceMission(Mission.Guard);
                if(!started)
                {
                    started = true;
                    var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Pointer<TechnoClass>.Zero, 1, pFlash, 100, false);
                    bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    return;
                }
             
            }
        }

        public override void OnRemove()
        {
            if (started)
            {
                if (Owner.OwnerObject.Ref.Base.Health <= 0)
                {
                    var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, Owner.OwnerObject.Ref.Veterancy.IsElite() ? 500 : 250, pWarhead, 100, true);
                    bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }
        }

       
    }
}
