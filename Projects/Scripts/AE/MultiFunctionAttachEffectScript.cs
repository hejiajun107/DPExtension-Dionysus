using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using System;

namespace Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(MultiFunctionAttachEffectData))]
    public class MultiFunctionAttachEffectData : INIAutoConfig
    {
        /// <summary>
        /// 是否永续
        /// </summary>
        [INIField(Key = "AttachEffectScript.Forever")]
        public bool Forever = false;

        /// <summary>
        /// 死亡引爆的弹头
        /// </summary>
        [INIField(Key = "AttachEffectScript.DeathWarhead")]
        public string DeathWarhead = string.Empty;

        [INIField(Key = "AttachEffectScript.DeathWarheadDamage")]
        public int DeathWarheadDamage = 0;

        /// <summary>
        /// 当接收到新AE时（refresh 刷新持续时间,sum 叠加持续时间,ignore 持续状态中忽略新ae)
        /// </summary>
        [INIField(Key = "AttachEffectScript.DurationOnRecieveNew")]
        public string DurationOnRecieveNew = "refresh";


    }

    [Serializable]
    public class MultiFunctionAttachEffectScript : AttachEffectScriptable
    {
        public MultiFunctionAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private MultiFunctionAttachEffectData data;

        TechnoExt Attacher;

        private static Pointer<BulletTypeClass> INVISO => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            INIComponentWith<MultiFunctionAttachEffectData> INI;
            INI = this.CreateRulesIniComponentWith<MultiFunctionAttachEffectData>(pWH.Ref.Base.ID);
            data = INI.Data;

            if (pAttacker.CastToTechno(out var techno))
            {
                //记住施加AE的对象
                Attacher = TechnoExt.ExtMap.Find(techno);
            }
        }

        public override void OnUpdate()
        {
            //如果是永续的则无限延长时间
            if (data.Forever)
            {
                Duration = 100;
            }
            base.OnUpdate();
        }

        public override void OnRemove()
        {
            if (Owner.OwnerObject.Ref.Base.Health <= 0)
            {
                if (!string.IsNullOrEmpty(data.DeathWarhead))
                {
                    var warhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find(data.DeathWarhead);
                    var bullet = INVISO.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, data.DeathWarheadDamage, warhead, 100, false);
                    bullet.Ref.Detonate(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    bullet.Ref.Base.UnInit();
                }
            }

            base.OnRemove();
        }

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            switch (data.DurationOnRecieveNew)
            {
                case "refresh":
                    {
                        Duration = duration;
                        break;
                    }
                case "sum":
                    {
                        Duration += duration;
                        break;
                    }
                case "ignore":
                    {
                        break;
                    }
                default:
                    {
                        Duration = duration;
                        break;
                    }
            }

        }

    }
}
