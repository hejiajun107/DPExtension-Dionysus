using DpLib.Scripts.Soviet;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.Yuri
{
    [Serializable]
    [ScriptAlias(nameof(MagnetronScript))]
    public class MagnetronScript : TechnoScriptable
    {

        public MagnetronScript(TechnoExt owner) : base(owner) { }


        //贴上mk2的buff
        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");
        static Pointer<WarheadTypeClass> shakeWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("TeleEmpWh");
        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");


        private bool IsMkIIUpdated = false;

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (IsMkIIUpdated)
            {
                Pointer<BulletClass> damageBullet = pBulletType.Ref.CreateBullet(pTarget.Convert<AbstractClass>(), Owner.OwnerObject, 80, shakeWarhead, 100, false);
                damageBullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
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
