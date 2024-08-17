using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(NCannonScript))]
    public class NCannonScript : TechnoScriptable
    {
        //贴上mk2的buff
        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");

        static Pointer<WarheadTypeClass> persistArmorWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("NArmorWh");

        static Pointer<WarheadTypeClass> breakArmorWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("NArmorDownWh");

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private bool IsMkIIUpdated = false;

        private int delay = 0;

        public NCannonScript(TechnoExt owner) : base(owner) { }

        public override void OnUpdate()
        {
            if (IsMkIIUpdated && delay > 0)
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

                    Pointer<BulletClass> armorBullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, persistArmorWarhead, 100, false);
                    armorBullet.Ref.DetonateAndUnInit(currentLocation);
                }
            }
            else
            {
                if (!pAttackingHouse.IsNull)
                {

                    var ownerHouse = Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex;
                    //if (!pAttackingHouse.Ref.IsAlliedWith(ownerHouse) && pAttackingHouse.Ref.ArrayIndex != ownerHouse)
                    if (!pDamage.IsNull)
                    {
                        if (MapClass.GetTotalDamage(pDamage.Ref, pWH, Owner.OwnerObject.Ref.Type.Ref.Base.Armor, DistanceFromEpicenter) > 0)
                        {
                            if (delay <= 0)
                            {
                                //if (Owner.OwnerObject.Ref.Base.Health < 300)
                                //{
                                    //紧急维修
                                    Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                                    CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                                    Pointer<BulletClass> buffBullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), pTechno, 1, breakArmorWarhead, 100, false);
                                    buffBullet.Ref.DetonateAndUnInit(currentLocation);

                                    delay = 25;
                                //}
                            }
                        }

                    }
                }
            }
        }



    }
}
