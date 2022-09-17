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
    [ScriptAlias(nameof(J20Script))]
    public class J20Script : TechnoScriptable
    {
        public J20Script(TechnoExt owner) : base(owner) { }


        static Pointer<BulletTypeClass> pAGMissle => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("AirJamerMissile");

        static Pointer<BulletTypeClass> pAAMissle => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("AirJamerAirMissileX");



        static Pointer<WarheadTypeClass> empWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("J20EMPWH");
        static Pointer<WarheadTypeClass> aaWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("J20AirWH2");



        //贴上mk2的buff
        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private bool IsMkIIUpdated = false;

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (IsMkIIUpdated)
            {
                if (weaponIndex == 0)
                {
                    var damage = 60;
                    Pointer<BulletClass> pag = pAGMissle.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, empWarhead, 100, false);
                    pag.Ref.SetTarget(pTarget);
                    pag.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords(), new BulletVelocity(0, 0, 0));
                }
                else
                {
                    for(var i = 0; i < 3; i++)
                    {
                        var damage = 70;
                        Pointer<BulletClass> paa = pAAMissle.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, aaWarhead, 90 - i * 10, false);
                        paa.Ref.SetTarget(pTarget);
                        paa.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords(), new BulletVelocity(0, 0, 0));
                    }
                }
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



        }


    }


}
