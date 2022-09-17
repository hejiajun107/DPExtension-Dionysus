using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(IronCoreAttchEffectScript))]
    public class IronCoreAttchEffectScript : AttachEffectScriptable
    {
        public IronCoreAttchEffectScript(TechnoExt owner) : base(owner)
        {
        }

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> ironWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ICTKCoreIronOtherWh");
        static Pointer<WarheadTypeClass> breakMindWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BreakMindControlWh");


        private int immnueCoolDown = 0;

        //private int initHouse = 0;

        public override void OnUpdate()
        {
            //if (initHouse != 0)
            //{
            //    Pointer<TechnoClass> pTechno = Owner.OwnerObject;
            //    if (!pTechno.Ref.Owner.IsNull)
            //    {
            //        initHouse = pTechno.Ref.Owner.Ref.ArrayIndex;
            //    }
            //}

            if (immnueCoolDown > 0)
                immnueCoolDown--;

            base.OnUpdate();
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (Duration < 1850)
                return;

            if (pAttackingHouse.IsNull)
            {
                return;
            }
            if (pAttackingHouse.Ref.ArrayIndex == Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex || Owner.OwnerObject.Ref.Owner.Ref.IsAlliedWith(pAttackingHouse))
            {
                return;
            }
            if ((immnueCoolDown <= 0 && (pDamage.Ref > 20 || pWH.Ref.MindControl)))
            {
                var pTechno = Owner.OwnerObject;
                CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();

                //if (initHouse != 0 && !pTechno.Ref.Owner.IsNull)
                //{
                //    if (pTechno.Ref.Owner.Ref.ArrayIndex != initHouse)
                //    {
                //        Pointer<BulletClass> pBullet2 = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, breakMindWarhead, 100, false);
                //        pBullet2.Ref.DetonateAndUnInit(currentLocation);
                //    }
                //}
                immnueCoolDown = 1500;
                Pointer<BulletClass> pBullet = bulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, ironWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(currentLocation);
            }

        }

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            Duration = duration;
            if (immnueCoolDown > 0)
            {
                immnueCoolDown -= 5;
            }
        }


    }
}
