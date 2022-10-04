using Extension.CW;
using Extension.CWUtilities;
using Extension.Ext;
using Extension.Ext4CW.Untilities;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using PatcherYRpp.Utilities;
using System.Reflection;

namespace Scripts
{
    [ScriptAlias("ExtraUnitScript")]
    [Serializable]
    public class ExtraUnitMasterScript : TechnoScriptable
    {
        public ExtraUnitMasterScript(TechnoExt owner) : base(owner)
        {

        }

        private bool configInited = false;

        //额外的构造函数，为了通过脚本直接附加的时候使用
        public ExtraUnitMasterScript(TechnoExt owner, ExtraUnitSetting setting) : base(owner)
        {
            configInited = true;
            Setting = setting;

        }

        private List<TechnoExt> salveTechnos = new List<TechnoExt>();

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
        }

        public override void Start()
        {
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

                //var dir = defination.BindTurret && Owner.OwnerObject.Ref.HasTurret() ? GameUtil.Facing2Dir(Owner.OwnerObject.Ref.TurretFacing) : GameUtil.Facing2Dir(Owner.OwnerObject.Ref.Facing);
                salveTechnos.Add(technoExt);
                technoExt.GameObject.CreateScriptComponent(nameof(ExtraUnitSalveScript), ExtraUnitSalveScript.UniqueId, "ExtraUnitSalveScript", technoExt, Owner, defination);

                //if (techno.Ref.Base.Put(Owner.OwnerObject.Ref.Base.Base.GetCoords(), dir)) 
                //{
                //    if(techno.Ref.HasTurret())
                //    {
                //        techno.Ref.TurretFacing.set(dir.ToDirStruct());
                //    }
                //  }
                //else
                //{
                //    techno.Ref.Base.UnInit();
                //}
            }

            PutSalves();
        }

        public override void OnUpdate()
        {
            if (NeedPut)
            {
                NeedPut = false;
                PutSalves();
            }


            var masterMission = Owner.OwnerObject.Convert<MissionClass>();
            if (masterMission.Ref.CurrentMission == Mission.Stop)
            {
                foreach (var slave in salveTechnos)
                {
                    if (!slave.IsNullOrExpired())
                    {
                        var mission = slave.OwnerObject.Convert<MissionClass>();
                        mission.Ref.ForceMission(Mission.Stop);
                    }
                }
            }
        }
       
        private bool NeedPut = false;

        private void PutSalves()
        {
            foreach (var salve in salveTechnos)
            {
                if (!salve.IsNullOrExpired())
                {
                    if (!salve.OwnerObject.Ref.Base.IsOnMap)
                    {
                        ++Game.IKnowWhatImDoing;
                        salve.OwnerObject.Ref.Base.Put(Owner.OwnerObject.Ref.Base.Base.GetCoords(), GameUtil.Facing2Dir(Owner.OwnerObject.Ref.Facing));
                        --Game.IKnowWhatImDoing;
                        salve.OwnerObject.Ref.Facing.set(Owner.OwnerObject.Ref.Facing.current());
                        if (salve.OwnerObject.Ref.HasTurret())
                        {
                            salve.OwnerObject.Ref.TurretFacing.set(Owner.OwnerObject.Ref.Facing.current());
                        }
                    }
                }
            }

        }

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {

            NeedPut = true;

        }

        public override void OnDestroy()
        {
            foreach (var salve in salveTechnos)
            {
                if (!salve.IsNullOrExpired())
                {
                    salve.OwnerObject.Ref.Base.UnInit();
                }
            }
        }

        public override void OnRemove()
        {
            foreach (var salve in salveTechnos)
            {
                if (!salve.IsNullOrExpired())
                {
                    salve.OwnerObject.Ref.Base.Remove();
                }
            }
            base.OnRemove();
        }

    }

    [ScriptAlias(nameof(ExtraUnitSalveScript))]
    [Serializable]
    public class ExtraUnitSalveScript : TechnoScriptable
    {
        public static int UniqueId = 1145142;

        public TechnoExt Master;
        ExtraUnitDefination Defination;

        public CoordStruct Position;

        public float VirtualVeterancy = 0f;

        public ExtraUnitSalveScript(TechnoExt owner, TechnoExt master, ExtraUnitDefination defination) : base(owner)
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

        public override void Start()
        {
            VirtualVeterancy = Owner.OwnerObject.Ref.Veterancy.Veterancy;
            base.Start();
        }

        public override void OnUpdate()
        {
            UpdateState();
        }

        private void UpdateState()
        {
            if (Master.IsNullOrExpired())
            {
                Disable();
                return;
            }

            if (Master.OwnerObject.Ref.IsSinking)
            {
                Disable();
                return;
            }

            ////同步put remove
            //if(Master.OwnerObject.Ref.Base.InLimbo)
            //{
            //    Owner.OwnerObject.Ref.Base.InLimbo = true;
            //}
            //else
            //{
            //    if(Owner.OwnerObject.Ref.Base.InLimbo == true)
            //    {
            //        Owner.OwnerObject.Ref.Base.InLimbo = false;
            //    }
            //}

            //Owner.OwnerObject.Ref.Base.Mark(MarkType.UP); 

            //同步所属
            if (Defination.SameOwner)
            {
                if (Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex != Master.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                {
                    Owner.OwnerObject.Ref.SetOwningHouse(Master.OwnerObject.Ref.Owner);
                }
            }

            Owner.OwnerObject.Ref.Base.Mark(MarkType.UP);

            //同步位置
            var location = ExHelper.GetFLHAbsoluteCoords(Master.OwnerObject, Position, Defination.BindTurret);
            Owner.OwnerObject.Ref.Base.SetLocation(location);


            if (Owner.OwnerObject.Ref.Base.Base.WhatAmI() == AbstractType.Building)
            {
                Owner.OwnerObject.Ref.Base.UpdatePlacement(PlacementType.Redraw);
            }
            else
            {
                Owner.OwnerObject.Convert<FootClass>().Ref.Locomotor.Lock();
            }



            //传递select
            if (Owner.OwnerObject.Ref.Base.IsSelected == true)
            {
                Owner.OwnerObject.Ref.Base.Deselect();
                if (!Master.OwnerObject.Ref.Base.IsSelected)
                {
                    Master.OwnerObject.Ref.Base.Select();
                }
            }

            var target = Master.OwnerObject.Ref.Target;

            //同步目标
            if (Defination.SameTarget)
            {
                if (target.IsNotNull)
                {
                    if (CanAttackTarget(target))
                    {
                        var mission = Owner.OwnerObject.Convert<MissionClass>();
                        Owner.OwnerObject.Ref.SetTarget(target);
                        mission.Ref.ForceMission(Mission.Attack);
                    }
                }
                else
                {
                    if (Defination.SameLoseTarget)
                    {
                        var mission = Owner.OwnerObject.Convert<MissionClass>();
                        mission.Ref.ForceMission(Mission.Stop);
                    }
                    else
                    {
                        if(Owner.OwnerObject.Ref.Target.IsNotNull)
                        {
                            if (!Owner.OwnerObject.Ref.IsCloseEnoughToAttack(Owner.OwnerObject.Ref.Target))
                            {
                                var mission = Owner.OwnerObject.Convert<MissionClass>();
                                mission.Ref.ForceMission(Mission.Stop);
                                Owner.OwnerObject.Ref.SetTarget(default);
                            }
                        }
                    }
                }
            }
            else
            {
                if (Owner.OwnerObject.Ref.Target.IsNull && target.IsNotNull)
                {
                    if (CanAttackTarget(target))
                    {
                        var mission = Owner.OwnerObject.Convert<MissionClass>();
                        Owner.OwnerObject.Ref.SetTarget(target);
                        mission.Ref.ForceMission(Mission.Attack);
                    }
                }
            }


            ////同步炮塔，方向
            if (Owner.OwnerObject.Ref.Target.IsNull)
            {
                if(!Defination.BindTurret)
                {
                    Owner.OwnerObject.Ref.Facing.set(Master.OwnerObject.Ref.Facing.current());
                }
                else
                {
                    Owner.OwnerObject.Ref.Facing.set(Master.OwnerObject.Ref.TurretFacing.current());
                }
                

                if (Owner.OwnerObject.Ref.HasTurret())
                {
                    if (Master.OwnerObject.Ref.HasTurret() && Defination.BindTurret)
                    {
                        Owner.OwnerObject.Ref.TurretFacing.turn(Master.OwnerObject.Ref.TurretFacing.current());
                    }
                    else
                    {
                        Owner.OwnerObject.Ref.TurretFacing.turn(Master.OwnerObject.Ref.Facing.current());
                    }
                }
            }


            ////同步经验
            //if(Defination.ExperienceToMaster)
            //{
            //    if (Owner.OwnerObject.Ref.Veterancy.Veterancy > 0f)
            //    {
            //        var veterancy = Owner.OwnerObject.Ref.Veterancy.Veterancy;

            //        //转移到主体
            //        if (!Master.OwnerObject.Ref.Veterancy.IsElite())
            //        {
            //            float trans = (2f - Master.OwnerObject.Ref.Veterancy.Veterancy) >= (veterancy - VirtualVeterancy) ? (veterancy - VirtualVeterancy) : (2f - Master.OwnerObject.Ref.Veterancy.Veterancy);
            //            Master.OwnerObject.Ref.Veterancy.Add(trans);
            //            Owner.OwnerObject.Ref.Veterancy.Add(-trans);
            //        }
            //    }
            //}

            //同步等级
            if (Defination.ExperienceToMaster)
            {
                Owner.OwnerObject.Ref.Veterancy.Veterancy = Master.OwnerObject.Ref.Veterancy.Veterancy;
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

        private FireError GetFireError(Pointer<AbstractClass> pTarget)
        {
            int i = Owner.OwnerObject.Ref.SelectWeapon(pTarget);
            FireError fireError = Owner.OwnerObject.Ref.GetFireError(pTarget, i, false);
            return fireError;
        }


        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

    
    }


    /// <summary>
    /// 给附加单位使用，把Bullet所属转移给本体
    /// </summary>
    [ScriptAlias(nameof(ExtraUnitBulletScript))]
    [Serializable]
    public class ExtraUnitBulletScript : BulletScriptable
    {
        public ExtraUnitBulletScript(BulletExt owner) : base(owner)
        {
        }

        private bool inited = false;

        public override void OnUpdate()
        {
            if(!inited)
            {
                inited = true;
                if (Owner.OwnerObject.Ref.Owner != null)
                {
                    var techno = TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
                    if(techno!=null)
                    {
                        var salveScript = techno.GameObject.GetComponent<ExtraUnitSalveScript>();
                        if (salveScript!=null)
                        {
                            if(!salveScript.Master.IsNullOrExpired())
                            {
                                Owner.OwnerObject.Ref.Owner = salveScript.Master.OwnerObject;
                            }
                        }
                    }
                }
            }
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
        /// 附加单位与本体保持相同等级
        /// </summary>
        [INIField(Key = "ExtraUnit.SameVeterancy")]
        public bool SameVeterancy = true;
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

        /// </summary>
        [INIField(Key = "ExtraUnit.SameLoseTarget")]
        public bool SameLoseTarget = true;
    }













}
