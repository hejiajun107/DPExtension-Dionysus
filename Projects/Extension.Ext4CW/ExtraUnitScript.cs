using Extension.CW;
using Extension.CWUtilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [ScriptAlias("ExtraUnitScript")]
    [Serializable]
    public class ExtraUnitMasterScript:TechnoScriptable
    {
        public ExtraUnitMasterScript(TechnoExt owner):base(owner)
        {

        }

        private bool configInited = false;

        //额外的构造函数，为了通过脚本直接附加的时候使用
        public ExtraUnitMasterScript(TechnoExt owner, ExtraUnitSetting setting) : base(owner)
        {
            configInited = true;
            Setting = setting;

        }

        private List<TechnoExt> salveTechnos;

        public ExtraUnitSetting Setting;

        public override void Awake()
        {
            //如果构造函数已经注入设置的则跳过此处的设置
            if (!configInited)
            {
                //获取本体的INI
                var settingINI = this.CreateRulesIniComponentWith<ExtraUnitSetting>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
                Setting = settingINI.Data;
                configInited = true;
            }

            var salveIni = this.CreateRulesIniComponentWith<ExtraUnitDefination>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);

            //获取附加单位的定义
            foreach (var item in Setting.ExtraUnitDefinations)
            {
                salveIni.Section = item;
                var defination = salveIni.Data;
                var type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(defination.ExtraUnitType);
                if (type == null)
                    continue;
                var techno = type.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();
                if (techno == null)
                    continue;
                var technoExt = TechnoExt.ExtMap.Find(techno);
                if (technoExt.IsNullOrExpired())
                    continue;

                if(techno.Ref.Base.Put(Owner.OwnerObject.Ref.Base.Base.GetCoords(), Direction.N))
                {
                    salveTechnos.Add(technoExt);
                    technoExt.GameObject.CreateScriptComponent(nameof(ExtraUnitSalveScript), ExtraUnitSalveScript.UniqueId, "ExtraUnitSalveScript", salveTechnos, Owner.OwnerObject, defination);
                }
                else
                {
                    techno.Ref.Base.UnInit();
                }
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

      

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            base.OnPut(coord, faceDir);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }

    }

    [Serializable]
    public class ExtraUnitSalveScript:TechnoScriptable
    {
        public static int UniqueId = 1145142;

        private TechnoExt Master;
        ExtraUnitDefination Defination;

        public CoordStruct Position;

        public ExtraUnitSalveScript(TechnoExt owner,TechnoExt master, ExtraUnitDefination defination) : base(owner)
        {
            Master = master;
            Defination = defination;
        }

        public override void Awake()
        {
            if (Defination.ExtraUnitPostion != null)
            {
                if (Defination.ExtraUnitPostion.Length >= 3)
                {
                    Position = new CoordStruct(Defination.ExtraUnitPostion[0], Defination.ExtraUnitPostion[1], Defination.ExtraUnitPostion[2]);
                }
                else
                {
                    Position = new CoordStruct(0, 0, 0);
                }
            }
            else
            {
                Position = new CoordStruct(0, 0, 0);
            }
         
        }

        public override void OnUpdate()
        {
            UpdateState();
        }

        private void UpdateState()
        {
            if(!Master.IsNullOrExpired())
            {
                Disable();
                return;
            }

            if (Master.OwnerObject.Ref.IsSinking)
            {
                Disable();
                return;
            }

            if(Master.OwnerObject.Ref.Base.InLimbo)
            {
                Owner.OwnerObject.Ref.Base.InLimbo = true;
            }
            else
            {
                if(Owner.OwnerObject.Ref.Base.InLimbo == true)
                {
                    Owner.OwnerObject.Ref.Base.InLimbo = false;
                }
            }

            //todo同步坐标


            if(Owner.OwnerObject.Ref.Base.Base.WhatAmI() == AbstractType.Building)
            {
                Owner.OwnerObject.Ref.Base.MarkForRedraw();
            }
            else
            {
                Owner.OwnerObject.Convert<FootClass>().Ref.Locomotor.Lock();
            }

        }

        private void Disable()
        {
            DetachFromParent();
            Owner.OwnerObject.Ref.Base.UnInit();
        }

        private bool CanAttackTarget(Pointer<AbstractClass> pTarget)
        {
            int i = Owner.OwnerObject.Ref.SelectWeapon(pTarget);
            FireError fireError = Owner.OwnerObject.Ref.GetFireError(pTarget, i, true);
            switch (fireError)
            {
                case FireError.ILLEGAL:
                case FireError.CANT:
                case FireError.MOVING:
                case FireError.RANGE:
                    return false;
            }
            return true;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }

    [Serializable]
    public partial class ExtraUnitSetting : INIAutoConfig
    {
        [INIField(Key = "ExtraUnit.Definations")]
        public string[] ExtraUnitDefinations;
    }

    [Serializable]
    public partial class ExtraUnitDefination : INIAutoConfig
    {
        /// <summary>
        /// 附加单位对应的TechnoType
        /// </summary>
        [INIField(Key = "ExtraUnit.Type")]
        public string ExtraUnitType;
        /// <summary>
        /// 附加单位的相对坐标FLH
        /// </summary>
        [INIField(Key = "ExtraUnit.Position")]
        public int[] ExtraUnitPostion = null;
        /// <summary>
        /// 附加单位是否绑定炮塔
        /// </summary>
        [INIField(Key = "ExtraUnit.BindTurret")]
        public bool BindTurret = false;
        /// <summary>
        /// 附加单位是否传递经验
        /// </summary>
        [INIField(Key = "ExtraUnit.ExperienceToMaster")]
        public bool ExperienceToMaster = false;
        /// <summary>
        /// 附件单位是否同步所属方
        /// </summary>
        [INIField(Key = "ExtraUnit.SameOwner")]
        public bool SameOwner = true;
        /// <summary>
        /// 附件单位是否同步攻击目标
        /// </summary>
        [INIField(Key = "ExtraUnit.SameTarget")]
        public bool SameTarget = true;
    }
}
