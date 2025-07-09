using DynamicPatcher;
using Extension.Coroutines;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    [ScriptAlias(nameof(MultipleAttachWeapon))]
    public class MultipleAttachWeapon : TechnoScriptable
    {
        public MultipleAttachWeapon(TechnoExt owner) : base(owner)
        {
        }

        public override void Awake()
        {
            INIComponentWith<MultipleAttachWeaponConfig> INI = this.CreateRulesIniComponentWith<MultipleAttachWeaponConfig>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            if(!string.IsNullOrWhiteSpace(INI.Data.AttachWeaponSection1))
            {
                Owner.GameObject.CreateScriptComponent(nameof(AttachWeaponScript), "AttachWeaponSection1", Owner, INI.Data.AttachWeaponSection1);
            }
            if (!string.IsNullOrWhiteSpace(INI.Data.AttachWeaponSection2))
            {
                Owner.GameObject.CreateScriptComponent(nameof(AttachWeaponScript), "AttachWeaponSection2", Owner, INI.Data.AttachWeaponSection2);
            }
            if (!string.IsNullOrWhiteSpace(INI.Data.AttachWeaponSection3))
            {
                Owner.GameObject.CreateScriptComponent(nameof(AttachWeaponScript), "AttachWeaponSection3", Owner, INI.Data.AttachWeaponSection3);
            }
            if (!string.IsNullOrWhiteSpace(INI.Data.AttachWeaponSection4))
            {
                Owner.GameObject.CreateScriptComponent(nameof(AttachWeaponScript), "AttachWeaponSection4", Owner, INI.Data.AttachWeaponSection4);
            }
            if (!string.IsNullOrWhiteSpace(INI.Data.AttachWeaponSection5))
            {
                Owner.GameObject.CreateScriptComponent(nameof(AttachWeaponScript), "AttachWeaponSection5", Owner, INI.Data.AttachWeaponSection5);
            }
            if (!string.IsNullOrWhiteSpace(INI.Data.AttachWeaponSection6))
            {
                Owner.GameObject.CreateScriptComponent(nameof(AttachWeaponScript), "AttachWeaponSection6", Owner, INI.Data.AttachWeaponSection6);
            }
            if (!string.IsNullOrWhiteSpace(INI.Data.AttachWeaponSection7))
            {
                Owner.GameObject.CreateScriptComponent(nameof(AttachWeaponScript), "AttachWeaponSection7", Owner, INI.Data.AttachWeaponSection7);
            }
            if (!string.IsNullOrWhiteSpace(INI.Data.AttachWeaponSection8))
            {
                Owner.GameObject.CreateScriptComponent(nameof(AttachWeaponScript), "AttachWeaponSection8", Owner, INI.Data.AttachWeaponSection8);
            }
        }


    }

    public class MultipleAttachWeaponConfig : INIAutoConfig
    {
        [INIField(Key = "MultipleAttachWeapon.AttachWeaponSection1")]
        public string AttachWeaponSection1;
        [INIField(Key = "MultipleAttachWeapon.AttachWeaponSection2")]
        public string AttachWeaponSection2;
        [INIField(Key = "MultipleAttachWeapon.AttachWeaponSection3")]
        public string AttachWeaponSection3;
        [INIField(Key = "MultipleAttachWeapon.AttachWeaponSection4")]
        public string AttachWeaponSection4;
        [INIField(Key = "MultipleAttachWeapon.AttachWeaponSection5")]
        public string AttachWeaponSection5;
        [INIField(Key = "MultipleAttachWeapon.AttachWeaponSection6")]
        public string AttachWeaponSection6;
        [INIField(Key = "MultipleAttachWeapon.AttachWeaponSection7")]
        public string AttachWeaponSection7;
        [INIField(Key = "MultipleAttachWeapon.AttachWeaponSection8")]
        public string AttachWeaponSection8;
    }


    [Serializable]
    [ScriptAlias(nameof(AttachWeaponScript))]
    public class AttachWeaponScript : TechnoScriptable
    {
        public AttachWeaponScript(TechnoExt owner) : base(owner)
        {
           
        }

        public AttachWeaponScript(TechnoExt owner,string section) : base(owner)
        {
            Section = section;
        }

        public string Section { get; private set; } 

        private INIComponentWith<AttachWeaponConfig> INI;

        public CoordStruct FLH1;
        public CoordStruct FLH2;

        /// <summary>
        /// 主武器rof
        /// </summary>
        private int rof1 = 0;
        /// <summary>
        /// 副武器rof
        /// </summary>
        private int rof2 = 0;

        private TechnoExt pickedTarget = null;

        private int pickTargetDelay = 0;

        private int workingHeight = -1;

        public override void Awake()
        {
            if(string.IsNullOrWhiteSpace(Section))
            {
                INI = this.CreateRulesIniComponentWith<AttachWeaponConfig>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            }
            else
            {
                INI = this.CreateRulesIniComponentWith<AttachWeaponConfig>(Section);
            }

            if (INI.Data.PrimaryAttachWeaponFLH != null)
            {
                if (INI.Data.PrimaryAttachWeaponFLH.Length >= 3)
                {
                    FLH1 = new CoordStruct(INI.Data.PrimaryAttachWeaponFLH[0], INI.Data.PrimaryAttachWeaponFLH[1], INI.Data.PrimaryAttachWeaponFLH[2]);
                }
                else
                {
                    FLH1 = new CoordStruct(0, 0, 0);
                }
            }

            if (INI.Data.SecondaryAttachWeaponFLH != null)
            {
                if (INI.Data.SecondaryAttachWeaponFLH.Length >= 3)
                {
                    FLH2 = new CoordStruct(INI.Data.SecondaryAttachWeaponFLH[0], INI.Data.SecondaryAttachWeaponFLH[1], INI.Data.SecondaryAttachWeaponFLH[2]);
                }
                else
                {
                    FLH2 = new CoordStruct(0, 0, 0);
                }
            }

            workingHeight = INI.Data.WorkingHeight;


            base.Awake();
        }

        public override void OnUpdate()
        {
            if (rof1 > 0)
                rof1--;

            if (rof2 > 0)
                rof2--;

            if (pickTargetDelay > 0)
                pickTargetDelay--;

            //是否分开开火
            if (INI.Data.SeparateFire)
            {
                if (INI.Data.WorkingHeight <= -1 || Owner.OwnerObject.Ref.Base.GetHeight() > INI.Data.WorkingHeight)
                {
                    SeparateFireUpdate();
                }
            }
        }


        private void SeparateFireUpdate()
        {
            if (!INI.Data.AutoPickTarget)
            {
                if (Owner.OwnerObject.Ref.Target.IsNull)
                {
                    return;
                }

                if (Owner.OwnerObject.Ref.SelectWeapon(Owner.OwnerObject.Ref.Target) == 0)
                {
                    if (rof1 <= 0)
                    {
                        TryFire(Owner.OwnerObject.Ref.Target, 0, false);
                    }
                }
                else if (Owner.OwnerObject.Ref.SelectWeapon(Owner.OwnerObject.Ref.Target) == 1)
                {
                    if (rof2 <= 0)
                    {
                        TryFire(Owner.OwnerObject.Ref.Target, 0, false);
                    }
                }
            }
            else
            {
                //自动获取目标，只考虑主武器
                var picked = false;
                if (rof1 > 0)
                {
                    return;
                }

                var weapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(GetAttachWeaponName(0));

                //如果本体有攻击目标且强制同步目标则直接攻击本体目标
                if (Owner.OwnerObject.Ref.Target.IsNotNull && INI.Data.AlwaysSameTarget)
                {
                    if (CheckRange(Owner.OwnerObject.Ref.Target, weapon))
                    {
                        picked = true;
                        if (Owner.OwnerObject.Ref.Target.CastToTechno(out var ptechno))
                        {
                            if (!GameUtil.CanAffectTarget(weapon, ptechno))
                            {
                                picked = false;
                            }
                            else
                            {
                                pickedTarget = TechnoExt.ExtMap.Find(ptechno);
                            }
                        }

                        if (picked)
                        {
                            TryFire(Owner.OwnerObject.Ref.Target, 0, false);
                            return;
                        }
                    }
                }

                //已经有目标且可攻击
                if (!pickedTarget.IsNullOrExpired())
                {

                    if (CheckRange(pickedTarget.OwnerObject.Convert<AbstractClass>(), weapon))
                    {
                        if (GameUtil.CanAffectTarget(weapon, pickedTarget.OwnerObject))
                        {
                            picked = true;
                        }
                        else
                        {
                            picked = false;
                        }
                    }
               
                    if (picked)
                    {
                        TryFire(pickedTarget.OwnerObject.Convert<AbstractClass>(), 0, false);
                        return;
                    }

                    pickedTarget = null;
                }


                if (Owner.OwnerObject.Ref.Target.IsNotNull)
                {

                    if (CheckRange(Owner.OwnerObject.Ref.Target, weapon))
                    {
                        picked = true;
                        if (Owner.OwnerObject.Ref.Target.CastToTechno(out var ptechno))
                        {
                            if (!GameUtil.CanAffectTarget(weapon, ptechno))
                            {
                                picked = false;
                            }
                        }

                        if (picked)
                        {
                            TryFire(Owner.OwnerObject.Ref.Target, 0, false);
                            return;
                        }
                    }
                }

                //需要寻找个新目标
                if (TryPickTarget(weapon, out var technoTarget))
                {
                    picked = true;
                    pickedTarget = technoTarget;
                    TryFire(technoTarget.OwnerObject.Convert<AbstractClass>(), 0, false);
                    return;
                }
                else
                {
                    technoTarget = null;
                }

            }
        }


        private bool TryPickTarget(Pointer<WeaponTypeClass> weapon,out TechnoExt technoExt)
        {
            technoExt = null;
            if (pickTargetDelay > 0)
                return false;

            pickTargetDelay = 10;

            var zhongli = new[]
            {
                "Special",
                "Neutral"
            };

            var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            if (INI.Data.FindTargetCenterAdjust != null)
            {
                var adjust = INI.Data.FindTargetCenterAdjust;
                coord = ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, new CoordStruct(adjust[0], adjust[1], adjust[2]), INI.Data.FindTargetCenterTurret);
            }

            List<Pointer<ObjectClass>> list = ObjectFinder.FindTechnosNear(coord, (weapon.Ref.Range + Owner.OwnerObject.Ref.Type.Ref.AirRangeBonus))
                .Where(x => !x.Ref.Base.GetOwningHouse().Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner) && !zhongli.Contains(x.Ref.Base.GetOwningHouse().Ref.Type.Ref.Base.ID))
                .Where(x => CheckRange(x.Convert<AbstractClass>(), weapon) && GameUtil.CanAffectTarget(weapon,x.Convert<TechnoClass>()))
                .OrderBy(x => x.Ref.Base.GetCoords().DistanceFrom(coord))
                .ToList();

            if (list.Count > 0)
            {
                var ptech = list.FirstOrDefault().Convert<TechnoClass>();
                technoExt = TechnoExt.ExtMap.Find(ptech);
                return !technoExt.IsNullOrExpired();
            }

            return false;
        }


        private string GetAttachWeaponName(int weaponIndex)
        {
            if(weaponIndex == 0)
            {
                if (Owner.OwnerObject.Ref.Veterancy.IsElite())
                {
                    return INI.Data.ElitePrimaryAttachWeapon ?? INI.Data.RookiePrimaryAttachWeapon ?? INI.Data.PrimaryAttachWeapon;
                }else if(Owner.OwnerObject.Ref.Veterancy.IsRookie())
                {
                    return INI.Data.RookiePrimaryAttachWeapon ?? INI.Data.PrimaryAttachWeapon;
                }
                else
                {
                    return INI.Data.PrimaryAttachWeapon;
                }
            }
            else if(weaponIndex == 1)
            {
                if (Owner.OwnerObject.Ref.Veterancy.IsElite())
                {
                    return INI.Data.EliteSecondaryAttachWeapon ?? INI.Data.RookieSecondaryAttachWeapon ?? INI.Data.SecondaryAttachWeapon;
                }
                else if (Owner.OwnerObject.Ref.Veterancy.IsRookie())
                {
                    return INI.Data.RookieSecondaryAttachWeapon ?? INI.Data.SecondaryAttachWeapon;
                }
                else
                {
                    return INI.Data.SecondaryAttachWeapon;
                }
            }

            return string.Empty;
        }


        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            //如果不分开开火则跟随武器开火
            if(!INI.Data.SeparateFire)
            {
                if(INI.Data.Delay == 0)
                {
                    TryFire(pTarget, weaponIndex, INI.Data.IgnoreRof);
                }
                else
                {
                    Owner.GameObject.StartCoroutine(DelayFire(weaponIndex));
                }
            }
        }

        
        IEnumerator DelayFire(int weaponIndex)
        {
            yield return new WaitForFrames(INI.Data.Delay);

            if (Owner.OwnerObject.Ref.Target.IsNotNull)
            {
                TryFire(Owner.OwnerObject.Ref.Target, weaponIndex, INI.Data.IgnoreRof);
            }
        }

        private void TryFire(Pointer<AbstractClass> pTarget, int weaponIndex, bool ignoreRof)
        {
            if (weaponIndex == 0 && (rof1 <= 0 || ignoreRof))
            {
                if (!string.IsNullOrWhiteSpace(INI.Data.PrimaryAttachWeapon))
                {
                    var weapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(GetAttachWeaponName(0));
                    var myWeapon = Owner.OwnerObject.Ref.GetWeapon(weaponIndex);
                    if (!CheckRange(pTarget, weapon))
                    {
                        return;
                    }
                    var mult = 1;//Owner.OwnerObject.Ref.GetROF(0) / myWeapon.Ref.WeaponType.Ref.ROF;
                    SimulateBurstFireOnce(weapon, pTarget, FLH1, 0);
                    rof1 = weapon.Ref.ROF * mult;

                }
            }
            else if (weaponIndex == 1 && (rof2 <= 0 || ignoreRof))
            {
                if (!string.IsNullOrWhiteSpace(INI.Data.SecondaryAttachWeapon))
                {
                    var weapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(GetAttachWeaponName(1));
                    var myWeapon = Owner.OwnerObject.Ref.GetWeapon(weaponIndex);
                    if(!CheckRange(pTarget,weapon))
                    {
                        return;
                    }
                    var mult = 1;//Owner.OwnerObject.Ref.GetROF(0) / myWeapon.Ref.WeaponType.Ref.ROF;
                    SimulateBurstFireOnce(weapon, pTarget, FLH2, 1);
                    rof2 = weapon.Ref.ROF * mult;
                }
            }
        }

        private bool CheckRange(Pointer<AbstractClass> pTarget,Pointer<WeaponTypeClass> pWeapon)
        {
            if (pTarget.IsNull)
                return false;

            var range = pWeapon.Ref.Range;

            if (pTarget.Ref.IsInAir())
            {
                range += Owner.OwnerObject.Ref.Type.Ref.AirRangeBonus;
            }

            var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            if (INI.Data.FindTargetCenterAdjust != null)
            {
                var adjust = INI.Data.FindTargetCenterAdjust;
                coord = ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, new CoordStruct(adjust[0], adjust[1], adjust[2]), INI.Data.FindTargetCenterTurret);
            }

            if (coord.BigDistanceForm(pTarget.Ref.GetCoords()) > range)
                return false;
            return true;
        }


        private void SimulateBurstFireOnce(Pointer<WeaponTypeClass> pWeapon,Pointer<AbstractClass> pTarget,CoordStruct flh,int idxWeapon)
        {

            Pointer<BulletTypeClass> pBulletType = pWeapon.Ref.Projectile;
            //int damage = (int)(pWeapon.Ref.Damage * fireMult);
            Pointer<WarheadTypeClass> pWH = pWeapon.Ref.Warhead;
            int speed = pWeapon.Ref.Speed;
            bool bright = pWH.Ref.Bright;

            var flip = 1;

            var burst = 1;

            if (pWeapon.Ref.Burst > 0)
            {
                burst = pWeapon.Ref.Burst;
            }

            for(var i = 0; i < burst; i++)
            {
                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pTarget, Owner.OwnerObject, pWeapon.Ref.Damage, pWH, speed, bright);
                pBullet.Ref.WeaponType = pWeapon;
                //// 设置所属
                //pBullet.SetSourceHouse(pAttackingHouse);
                //if (default == bulletVelocity)
                //{
                var sourcePos = ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, new CoordStruct(flh.X, flh.Y * flip, flh.Z), INI.Data.Turret);
                flip = -flip;
                var bulletVelocity = GetBulletVelocity(sourcePos, pTarget.Ref.GetCoords());
                //}

                if (pWeapon.Ref.IsLaser)
                {
                    pBullet.Ref.Base.SetLocation(pTarget.Ref.GetCoords());
                    Owner.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), idxWeapon, pWeapon, sourcePos);

                }

                pBullet.Ref.SetTarget(pTarget);
                pBullet.Ref.MoveTo(sourcePos, bulletVelocity);

                //if (default != targetPos)
                //{
                pBullet.Ref.TargetCoords = pTarget.Ref.GetCoords();
                //}
                if (pBulletType.Ref.Inviso && !pBulletType.Ref.Airburst)
                {
                    pBullet.Ref.Detonate(pTarget.Ref.GetCoords());
                    pBullet.Ref.Base.UnInit();
                }

                if (Owner.OwnerObject.IsNotNull)
                {
                    DrawWeaponAnim(Owner.OwnerObject.Convert<ObjectClass>(), pWeapon, sourcePos, pTarget.Ref.GetCoords());
                }



                if (pWeapon.Ref.Report.Count > 0)
                {
                    int index = MathEx.Random.Next(0, pWeapon.Ref.Report.Count - 1);
                    int soundIndex = pWeapon.Ref.Report.Get(index);
                    if (soundIndex != -1)
                    {
                        VocClass.PlayAt(soundIndex, sourcePos, IntPtr.Zero);
                    }
                }
            }
           
       
        }


        private void DrawWeaponAnim(Pointer<ObjectClass> pShooter, Pointer<WeaponTypeClass> pWeapon, CoordStruct sourcePos, CoordStruct targetPos)
        {
            // Anim
            if (pWeapon.Ref.Anim.Count > 0)
            {
                int facing = pWeapon.Ref.Anim.Count;
                int index = 0;
                if (facing % 8 == 0)
                {
                    CoordStruct tempSourcePos = sourcePos;
                    tempSourcePos.Z = 0;
                    CoordStruct tempTargetPos = targetPos;
                    tempTargetPos.Z = 0;
                    Direction dir = GameUtil.Point2Dir(tempSourcePos, tempTargetPos);
                    index = (int)dir;
                }
                Pointer<AnimTypeClass> pAnimType = pWeapon.Ref.Anim.Get(index);
                // Logger.Log($"{Game.CurrentFrame} 获取第{index}个动画[{(pAnimType.IsNull ? "不存在" : pAnimType.Convert<AbstractTypeClass>().Ref.ID)}]");
                if (!pAnimType.IsNull)
                {
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, sourcePos);
                    pAnim.Ref.SetOwnerObject(pShooter);
                }
            }
        }

        private BulletVelocity GetBulletVelocity(CoordStruct sourcePos, CoordStruct targetPos)
        {
            CoordStruct bulletFLH = new CoordStruct(1, 0, 0);
            DirStruct bulletDir = GameUtil.Point2DirStruct(sourcePos, targetPos);
            SingleVector3D bulletV = GetFLHAbsoluteOffset(bulletFLH, bulletDir, default);
            return new BulletVelocity(bulletV.X, bulletV.Y, bulletV.Z);
        }

        private  SingleVector3D GetFLHAbsoluteOffset(CoordStruct flh, DirStruct dir, CoordStruct turretOffset)
        {
            SingleVector3D offset = default;
            if (null != flh && default != flh)
            {
                Matrix3DStruct matrix3D = new Matrix3DStruct(true);
                matrix3D.Translate(turretOffset.X, turretOffset.Y, turretOffset.Z);
                matrix3D.RotateZ((float)dir.radians());
                offset = GetFLHOffset(ref matrix3D, flh);
            }
            return offset;
        }

        private SingleVector3D GetFLHOffset(ref Matrix3DStruct matrix3D, CoordStruct flh)
        {
            // Step 4: apply FLH offset
            matrix3D.Translate(flh.X, flh.Y, flh.Z);
            SingleVector3D result = Game.MatrixMultiply(matrix3D);
            // Resulting FLH is mirrored along X axis, so we mirror it back - Kerbiter
            result.Y *= -1;
            return result;
        }

    }

    public class AttachWeaponConfig : INIAutoConfig
    {
        [INIField(Key = "AttachWeapon.PrimaryAttachWeapon")]
        public string PrimaryAttachWeapon = null;
        [INIField(Key = "AttachWeapon.SecondaryAttachWeapon")]
        public string SecondaryAttachWeapon = null;
        [INIField(Key = "AttachWeapon.RookiePrimaryAttachWeapon")]
        public string RookiePrimaryAttachWeapon = null;
        [INIField(Key = "AttachWeapon.RookieSecondaryAttachWeapon")]
        public string RookieSecondaryAttachWeapon = null;
        [INIField(Key = "AttachWeapon.ElitePrimaryAttachWeapon")]
        public string ElitePrimaryAttachWeapon = null;
        [INIField(Key = "AttachWeapon.EliteSecondaryAttachWeapon")]
        public string EliteSecondaryAttachWeapon = null;
        [INIField(Key = "AttachWeapon.PrimaryAttachWeaponFLH")]
        public int[] PrimaryAttachWeaponFLH = null;
        [INIField(Key = "AttachWeapon.SecondaryAttachWeaponFLH")]
        public int[] SecondaryAttachWeaponFLH = null;
        [INIField(Key = "Turret")]
        public bool Turret = false;
        /// <summary>
        /// 是否独立开火，即不跟随本体武器一起开火
        /// </summary>
		[INIField(Key = "AttachWeapon.SeparateFire")]
		public bool SeparateFire = false;
        /// <summary>
        /// 当跟随武器发射（SeparateFire为fasle时）是否无视ROF发射
        /// </summary>
        [INIField(Key = "AttachWeapon.IgnoreRof")]
        public bool IgnoreRof = false;
        /// <summary>
        /// 是否自动获取目标而不跟随单位
        /// </summary>
        [INIField(Key = "AttachWeapon.AutoPickTarget")]
        public bool AutoPickTarget = false;
        /// <summary>
        /// 是否强制和主单位攻击同一个目标
        /// </summary>
        [INIField(Key = "AttachWeapon.AlwaysSameTarget")]

        public bool AlwaysSameTarget = false;
        [INIField(Key = "AttachWeapon.Delay")]

        public int Delay = 0;

        /// <summary>
        /// 自动寻敌的时候中心点偏移量
        /// </summary>
        [INIField(Key = "AttachWeapon.FindTargetCenterAdjust")]

        public int[] FindTargetCenterAdjust = null;

        /// <summary>
        /// 自动寻敌的时候中心点偏移量
        /// </summary>
        [INIField(Key = "AttachWeapon.FindTargetCenterTurret")]

        public bool FindTargetCenterTurret = false;

        [INIField(Key = "AttachWeapon.WorkingHeight")]

        public int WorkingHeight = -1;
    }
}
