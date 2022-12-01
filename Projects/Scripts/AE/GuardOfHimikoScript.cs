using DpLib.Scripts.AE;
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
    [ScriptAlias(nameof(GuardOfHimikoScript))]
    public class GuardOfHimikoScript : AttachEffectScriptable
    {
        public GuardOfHimikoScript(TechnoExt owner) : base(owner){ }

        static Pointer<BulletTypeClass> shieldBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> ShieldRemoveDamageWarheadType1 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ShieldRemoveDamageWHA1");
        static Pointer<WarheadTypeClass> ShieldRemoveDamageWarheadType2 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ShieldRemoveDamageWHA2");

        static Pointer<AnimTypeClass> HimikoFallAnimType => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("HimikoFallA1");

        private int totalDamage;
        private bool inShield = true;

        //private int shieldTime = 300;

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            Pointer<TechnoClass> ownerTechno = Owner.OwnerObject;
            Pointer<AnimClass> HimikoFallAnim = YRMemory.Create<AnimClass>(HimikoFallAnimType, ownerTechno.Ref.Base.Base.GetCoords());
            HimikoFallAnim.Ref.SetOwnerObject(ownerTechno.Convert<ObjectClass>());

            //Logger.Log("获得了卑弥呼AE");
       
            //Logger.Log("开启护盾");

            //base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {

            Pointer<TechnoClass> ownerTechno = Owner.OwnerObject;

            //Logger.Log(pAttacker.IsNull ? "没拿到攻击者" : "拿到了攻击者");

            int trueDamage = MapClass.GetTotalDamage(pDamage.Ref, pWH, ownerTechno.Ref.Type.Ref.Base.Armor, DistanceFromEpicenter);

            //Logger.Log("真实受到的伤害" + trueDamage + "单位当前血量" + ownerTechno.Ref.Base.Health + "税前伤害" + pDamage.Ref + "弹头衰减距离" + DistanceFromEpicenter + "攻击弹头" + pWH.Ref.Base.ID);

            //Logger.Log(inShield ? "开启护盾了" : "未开启护盾");

          

            if (inShield)
            {
                //Logger.Log("真实伤害" + trueDamage);

                totalDamage += trueDamage;

                //Logger.Log("卑弥呼护盾无敌效果开启...");

                if ("Super" == pWH.Ref.Base.ID)
                {
                    //Logger.Log("受到游戏结算的伤害");
                    return;
                }
                pDamage.Ref = 0;

                //Logger.Log(pDamage.Ref == 0 ? "伤害已清零" : "伤害依旧为："+ pDamage.Ref);
            }

            //base.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
        }

        public override void OnAttachEffectRemove()
        {
            Pointer<TechnoClass> ownerTechno = Owner.OwnerObject;
            
            //Logger.Log("护盾已移除");

            inShield = false;

            totalDamage = totalDamage * 2;

            //Logger.Log("结算伤害" + totalDamage);

            //ownerTechno.Ref.Base.Health -= totalDamage;
            //ownerTechno.Ref.Base.TakeDamage(totalDamage, ShieldRemoveDamageWarheadType, false);
            
            if(totalDamage >= 0)
            {
                Pointer<BulletClass> ShieldRemoveDamageBullet = shieldBulletType.Ref.CreateBullet(ownerTechno.Convert<AbstractClass>(), Pointer<TechnoClass>.Zero, totalDamage, ShieldRemoveDamageWarheadType1, 100, false);
                ShieldRemoveDamageBullet.Ref.DetonateAndUnInit(ownerTechno.Ref.Base.Base.GetCoords());
            }
            else
            {
                Pointer<BulletClass> ShieldRemoveDamageBullet = shieldBulletType.Ref.CreateBullet(ownerTechno.Convert<AbstractClass>(), Pointer<TechnoClass>.Zero, totalDamage, ShieldRemoveDamageWarheadType2, 100, false);
                ShieldRemoveDamageBullet.Ref.DetonateAndUnInit(ownerTechno.Ref.Base.Base.GetCoords());
            }
           
            

            //base.OnAttachEffectRemove();
        }
    }
}
