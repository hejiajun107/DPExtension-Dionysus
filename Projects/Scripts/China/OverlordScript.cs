using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(OverlordScript))]
    public class OverlordScript : TechnoScriptable
    {
        //贴上mk2的buff
        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");

        //static Pointer<WarheadTypeClass> healthWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("YhExigencyRepairWh");

        static Pointer<WarheadTypeClass> buffWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("YhExigencyBuffWh");
        
        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private bool IsMkIIUpdated = false;

        private int delay = 0;

        public OverlordScript(TechnoExt owner) : base(owner) { }

        public override void OnUpdate()
        {
            if(IsMkIIUpdated && delay > 0)
            {
                delay--;
            }
        }


        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
        Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {

            if (IsMkIIUpdated == false)
            {
                //判断是否来自升级弹头
                if (pWH.Ref.Base.ID.ToString() == "MarkIISpWh")
                {
                    IsMkIIUpdated = true;
                    Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                    CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                    Pointer<BulletClass> mk2bullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, mk2Warhead, 100, false);
                    mk2bullet.Ref.DetonateAndUnInit(currentLocation);
                }
            }
            else
            {
                if (!pAttackingHouse.IsNull)
                {
                    
                    var ownerHouse = Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex;
                    if (!pAttackingHouse.Ref.IsAlliedWith(ownerHouse) && pAttackingHouse.Ref.ArrayIndex != ownerHouse)
                    {
                        if(delay<=0)
                        {
                            if (Owner.OwnerObject.Ref.Base.Health < 500)
                            {
                                //紧急维修
                                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                                CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                                Pointer<BulletClass> buffBullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), pTechno, 1, buffWarhead, 100, false);
                                buffBullet.Ref.DetonateAndUnInit(currentLocation);

                                delay = 1500;
                            }
                        }
                    }
                }
            }
        }



    }
}
