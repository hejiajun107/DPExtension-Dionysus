using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.AE
{

    [Serializable]
    [ScriptAlias(nameof(AngryEmperorScript))]
    public class AngryEmperorScript : AttachEffectScriptable
    {
        public AngryEmperorScript(TechnoExt owner) : base(owner) { }


        static Pointer<BulletTypeClass> shieldBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisibleAll");
        static Pointer<WarheadTypeClass> shieldWarheadType => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ShieldWHA2");
        static Pointer<WarheadTypeClass> shieldRemoveWarheadType => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ShieldRemoveWHA2");

        private int angryTime = 50;//天皇之怒无敌时间

        private Boolean inShield = false;
        private Boolean inAngry = false;


        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            //Logger.Log("获得了天皇之怒AE");

            //base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
        }

        public override void OnUpdate()
        {
            if (inAngry)
            {
                if(angryTime-- <= 0)
                {
                    inAngry = false;

                    //Logger.Log("关闭天皇之怒锁血状态");

                    Pointer<TechnoClass> ownerTechno = Owner.OwnerObject;

                    Pointer<BulletClass> shieldBullet = shieldBulletType.Ref.CreateBullet(ownerTechno.Convert<AbstractClass>(), ownerTechno, 1, shieldWarheadType, 100, false);
                    shieldBullet.Ref.DetonateAndUnInit(ownerTechno.Ref.Base.Base.GetCoords());
                    inShield = true;

                    //Logger.Log("开启愤怒护盾");

                }
            }

            //base.OnUpdate();
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            Pointer<TechnoClass> ownerTechno = Owner.OwnerObject;

            //Logger.Log(pAttacker.IsNull ? "没拿到攻击者" : "拿到了攻击者");

            int trueDamage = MapClass.GetTotalDamage(pDamage.Ref, pWH, ownerTechno.Ref.Type.Ref.Base.Armor, DistanceFromEpicenter);

            //Logger.Log("真实受到的伤害" + trueDamage + "单位当前血量" + ownerTechno.Ref.Base.Health + "税前伤害" + pDamage.Ref + "弹头衰减距离" + DistanceFromEpicenter + "攻击弹头" + pWH.Ref.Base.ID);
            //Logger.Log(inShield ? "开启天皇之怒护盾了" : "未开启天皇之怒护盾");

            if (inAngry == false && inShield == false && trueDamage >= ownerTechno.Ref.Base.Health)
            {
                if ("Super" == pWH.Ref.Base.ID)
                {
                    //Logger.Log("受到游戏结算的伤害");
                    return;
                }

                //Logger.Log("受到致命伤害...");

                pDamage.Ref = 0;

                //Logger.Log(pDamage.Ref == 0 ? "伤害已清零" : "伤害依旧为：" + pDamage.Ref);

                inAngry = true;

                //Logger.Log("开启天皇之怒");

                if (ownerTechno.Ref.Base.Health > 5)
                {
                    ownerTechno.Ref.Base.Health = 5;
                }
                else
                {
                    ownerTechno.Ref.Base.Health = 1;
                }
            }

            if (inAngry && angryTime > 0)
            {
               //Logger.Log("天皇之怒开启...无敌时间剩余" + angryTime);

                pDamage.Ref = 0;

                //Logger.Log(pDamage.Ref == 0 ? "致死伤害已清零" : "伤害伤害依旧为：" + pDamage.Ref);
            }

            //base.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
        }

        public override void OnAttachEffectRemove()
        {
            Pointer<TechnoClass> ownerTechno = Owner.OwnerObject;
            Pointer<BulletClass> shieldRemoveBullet = shieldBulletType.Ref.CreateBullet(ownerTechno.Convert<AbstractClass>(), ownerTechno, 1, shieldRemoveWarheadType, 100, false);
            shieldRemoveBullet.Ref.DetonateAndUnInit(ownerTechno.Ref.Base.Base.GetCoords());
            inShield = false;

            //Logger.Log("护盾关闭");

            //base.OnAttachEffectRemove();
        }

    }
}
