using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.CW;
using Extension.Ext4CW;
using Extension.Utilities;
using PatcherYRpp.Utilities;
using Extension.INI;
using DynamicPatcher;

namespace Scripts.Yuri
{
    [ScriptAlias(nameof(SMNKScript))]
    [Serializable]
    public class SMNKScript : TechnoScriptable
    {
        private static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SMNKSSB");

        private static Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");


        public SMNKScript(TechnoExt owner) : base(owner)
        {
            pAnim1 = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
            pAnim2 = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
            pAnim3 = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }

        private SwizzleablePointer<AnimClass> pAnim1;
        private SwizzleablePointer<AnimClass> pAnim2;
        private SwizzleablePointer<AnimClass> pAnim3;

        public bool isShieldOpen = false;

        public bool isAnimShowing = false;

        private int AISwtichCoolDown = 50;

        private int rof = 100;

        private int displayrof = 600;

        private int AIBehavior = 0;

        public override void Awake()
        {
            var ini = this.CreateRulesIniComponentWith<SMNKData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            AIBehavior = ini.Data.AIBehavior;

            base.Awake();
        }

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if(mission.Ref.CurrentMission == Mission.Unload)
            {
                isShieldOpen = !isShieldOpen;

                OnSwitchStatus(isShieldOpen);
            }

            if (AISwtichCoolDown >= 0)
            {
                AISwtichCoolDown--;
            }


            if(AIBehavior != 1)
            {
                //AI相关逻辑
                if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                {
                    if(AIBehavior == 0)
                    {
                        if (AISwtichCoolDown == 0)
                        {
                            if (Owner.OwnerObject.Ref.Target.IsNull)
                            {
                                if (!isShieldOpen)
                                {
                                    isShieldOpen = true;
                                    AISwtichCoolDown = 50;
                                }
                            }
                            else
                            {
                                if (isShieldOpen)
                                {
                                    isShieldOpen = false;
                                    AISwtichCoolDown = 50;
                                }
                            }
                        }
                    }
                    else if (AIBehavior == 2)
                    {
                        isShieldOpen = true;
                    }
                   
                }
            }
           


            if (isShieldOpen)
            {
                if(rof--<=0)
                {
                    rof = 100;

                    var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                    var pBullet1 = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 0, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SMNKCloackWh"), 100, false);
                    var pBullet2 = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Pointer<TechnoClass>.Zero, 0, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SMNKDWWH"), 100, false);
                    pBullet1.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    pBullet2.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
                UpdateAnim();
            }
            else
            {
                isAnimShowing = false;

                if (displayrof >= 0)
                {
                    displayrof--;
                }
                else
                {
                    displayrof = 600;
                    var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                    var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 0, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SMNKWaveWh"), 100, false);
                    pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }


        }

        private void OnSwitchStatus(bool isOpen)
        {
            if(isOpen)
            {
                rof = 0;
                CreateAnim();
            }
            else
            {
                KillAnim();
            }
        }

        private bool CanShowAnim()
        {
            var vehicles = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), 2 * Game.CellSize).Select(x => x.Convert<TechnoClass>()).Where(x => x.Ref.Type.Ref.Base.Base.ID == "SMNK" && x.Ref.Owner == Owner.OwnerObject.Ref.Owner && x != Owner.OwnerObject).ToList();

            foreach(var vehicle in vehicles)
            {
                var ext = TechnoExt.ExtMap.Find(vehicle);
                var component = ext.GameObject.GetComponent<SMNKScript>();
                if (component.isAnimShowing)
                {
                    return false;
                }
            }
            return true;
        }


        private void UpdateAnim()
        {
            CheckAnim();

            pAnim1.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            pAnim2.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            pAnim3.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords());

            var canShowAnim = CanShowAnim();
            pAnim3.Ref.Pause();


            if(!Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap && Owner.OwnerObject.Ref.Owner == HouseClass.Player)
            {
                pAnim3.Ref.Invisible = false;
            }
            else 
            {
                pAnim3.Ref.Invisible = true;
            }

            if (!Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap && canShowAnim)
            {
                pAnim1.Ref.Invisible = false;
                pAnim2.Ref.Invisible = false;
                isAnimShowing = true;
            }
            else
            {
                pAnim1.Ref.Invisible = true;
                pAnim2.Ref.Invisible = true;
                isAnimShowing = false;
            }
        }

        private void CheckAnim()
        {
            if (pAnim1.IsNull || pAnim2.IsNull || pAnim3.IsNull)
            {
                CreateAnim();
            }

        }

        private void CreateAnim()
        {
            if (!pAnim1.IsNull || !pAnim2.IsNull || !pAnim3.IsNull)
            {
                KillAnim();
            }

            var anim1 = YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("CloackShield1"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
            var anim2 = YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("CloackShield2"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
            var anim3 = YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("CloackActived"), Owner.OwnerObject.Ref.Base.Base.GetCoords());


            pAnim1.Pointer = anim1;
            pAnim2.Pointer = anim2;
            pAnim3.Pointer = anim3;

        }

        private void KillAnim()
        {
            if (!pAnim1.IsNull)
            {
                pAnim1.Ref.TimeToDie = true;
                pAnim1.Ref.Base.UnInit();
                pAnim1.Pointer = IntPtr.Zero;
            }

            if (!pAnim2.IsNull)
            {
                pAnim2.Ref.TimeToDie = true;
                pAnim2.Ref.Base.UnInit();
                pAnim2.Pointer = IntPtr.Zero;
            }

            if (!pAnim3.IsNull)
            {        
                pAnim3.Ref.TimeToDie = true;
                pAnim3.Ref.Base.UnInit();
                pAnim3.Pointer = IntPtr.Zero;
            }
        }

        public override void OnRemove()
        {
            KillAnim();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (pTarget.IsNull)
                return;

            if (pTarget.Ref.WhatAmI() == AbstractType.Building)
                return;

            if (pTarget.CastToTechno(out var ptechno))
            {
                var technoExt = TechnoExt.ExtMap.Find(ptechno);

                if (technoExt.IsNullOrExpired())
                    return;

                var gext = technoExt.GameObject.GetTechnoGlobalComponent();

                if ((ptechno.Ref.IsMindControlled() || ptechno.Ref.Type.Ref.ImmuneToPsionics) && !gext.Data.IsEpicUnit && !gext.Data.IsHero)
                {
                    var damage = Owner.OwnerObject.Ref.Veterancy.IsElite() ? 240 : 120;
                    var pbullet = inviso.Ref.CreateBullet(pTarget, Owner.OwnerObject, damage, warhead, 100, true);
                    pbullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
                }
            }
        }
    }

    public class SMNKData : INIAutoConfig
    {
        /// <summary>
        /// 0 默认  1 始终关闭  2 始终开启
        /// </summary>
        [INIField(Key = "SMNK.AIBahavior")]
        public int AIBehavior = 0; 
    
    }

    [ScriptAlias(nameof(RCROSScript))]
    [Serializable]
    public class RCROSScript : TechnoScriptable
    {
        private static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("RCROSAGWhB");

        private static Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");


        public RCROSScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex != 0)
                return;

            if (pTarget.IsNull)
                return;

            if (pTarget.Ref.WhatAmI() == AbstractType.Building)
                return;

            if (pTarget.CastToTechno(out var ptechno))
            {
                var technoExt = TechnoExt.ExtMap.Find(ptechno);

                if (technoExt.IsNullOrExpired())
                    return;

                var gext = technoExt.GameObject.GetTechnoGlobalComponent();

                if ((ptechno.Ref.IsMindControlled() || ptechno.Ref.Type.Ref.ImmuneToPsionics) && !gext.Data.IsEpicUnit && !gext.Data.IsHero)
                {
                    var damage = Owner.OwnerObject.Ref.Veterancy.IsElite() ? 170 : 85;
                    var pbullet = inviso.Ref.CreateBullet(pTarget, Owner.OwnerObject, damage, warhead, 100, true);
                    pbullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
                }
            }
        }
    }
}
