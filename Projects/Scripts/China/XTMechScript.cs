using DynamicPatcher;
using Extension.Coroutines;
using Extension.Ext;
using Extension.Ext4CW.Untilities;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using Microsoft.AspNet.SignalR.Client.Infrastructure;
using PatcherYRpp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(XTMechScript))]
    public class XTMechScript : TechnoScriptable
    {
        public XTMechScript(TechnoExt owner) : base(owner) { }

        TechnoExt pTargetRef;

        static Pointer<WarheadTypeClass> selfWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkipWhSelf");
        static Pointer<WarheadTypeClass> targeWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkipWhTarget");

        //加速buff
        static Pointer<WarheadTypeClass> speedWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkipSpeedWh");

        //盾buff
        static Pointer<WarheadTypeClass> guardWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTGuardWh");

        //砍
        static Pointer<WarheadTypeClass> axeWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTAxeWh2");

        //减速
        static Pointer<WarheadTypeClass> slowWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSlowDownWh");



        static Pointer<WarheadTypeClass> rofWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTRofWH");


        //贴上mk2的buff
        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");






        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        //检测符合跃迁条件的间隔,每xx帧检测一次
        private int checkRof = 0;
        //跃迁的冷却
        private int transCoolDown = 0;

        private int guardCoolDown = 0;

        private bool IsMkIIUpdated = false;

        //是否充能攻击
        private bool isCharged = false;
        //充能剩余时间
        private int chargedLeftTime = 0;

        public override void OnUpdate()
        {
            if (guardCoolDown > 0)
            {
                guardCoolDown--;
            }

            if (transCoolDown > 0)
            {
                transCoolDown--;
            }

            if (isCharged == true)
            {
                if (chargedLeftTime > 0)
                {
                    chargedLeftTime--;
                }
                else
                {
                    isCharged = false;
                }
            }


            if (checkRof-- <= 0)
            {
                checkRof = 20;

                if (transCoolDown == 0)
                {
                    pTargetRef = (TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Target.Convert<TechnoClass>()));
                    if (!pTargetRef.IsNullOrExpired())
                    {
                        var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                        var targetLocation = pTargetRef.OwnerObject.Ref.Base.Base.GetCoords();
                        var distance = currentLocation.DistanceFrom(targetLocation);
                        if (distance >= 4 * 256 && distance <= 14 * 256)
                        {
                            Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, speedWarhead, 100, false);
                            pBullet.Ref.DetonateAndUnInit(currentLocation);

                            //SkipTo(currentLocation, targetLocation);
                            transCoolDown = 400;

                            if (IsMkIIUpdated)
                            {
                                isCharged = true;
                                chargedLeftTime = 180;
                            }

                        }
                    }
                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            Pointer<BulletClass> pRof = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Pointer<TechnoClass>.Zero, 0, rofWarhead, 100, false);
            pRof.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

            if (isCharged)
            {
                isCharged = false;

                Pointer<BulletClass> pAxe = pBulletType.Ref.CreateBullet(pTarget, Owner.OwnerObject, 120, axeWarhead, 100, false);
                pAxe.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());

                Pointer<BulletClass> pSpd = pBulletType.Ref.CreateBullet(pTarget, Owner.OwnerObject, 1, slowWarhead, 100, false);
                pSpd.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
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


            if (guardCoolDown <= 0 && pDamage.Ref > 20)
            {
                try
                {
                    if (pAttacker.IsNull)
                        return;

                    if (pAttackingHouse.IsNull)
                        return;

                    if (pAttackingHouse.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                        return;

                    var currentHeight = Owner.OwnerObject.Ref.Base.GetHeight();
                    var sourceHeight = pAttacker.Ref.GetHeight();

                    if (sourceHeight - currentHeight > 500)
                        return;

                    var sourceCoord = pAttacker.Ref.Base.GetCoords();
                    var targetCoord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                    var dir = GameUtil.Point2Dir(targetCoord, sourceCoord);
                    var face = GameUtil.Facing2Dir(Owner.OwnerObject.Ref.TurretFacing);

                    var diff = Math.Abs((int)face - (int)dir);
                    if (diff > 4)
                        diff = 8 - diff;

                    if (Math.Abs(diff) >= 2)
                    {
                        return;
                    }

                    guardCoolDown = 180;
                    Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                    CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, guardWarhead, 100, false);
                    pBullet.Ref.DetonateAndUnInit(currentLocation);


                }
                catch (Exception) {; }

            }

        }




        //跃迁至指定位置
        private void SkipTo(CoordStruct location, CoordStruct target)
        {
            //Owner.OwnerObject.Ref.Base.SetLocation(target);

            //if (MapClass.Instance.TryGetCellAt(CellClass.Coord2Cell(target), out Pointer<CellClass> pcell))
            //{
            //    //pcell.Ref.
            //}

            //Owner.OwnerObject.Ref.Base.Remove();
            //bool putted = false;
            //CoordStruct lastLocation = target;
            //if (!Owner.OwnerObject.Ref.Base.Put(target, Direction.N))
            //{
            //    Logger.Log("直接设置失败");
            //    CellSpreadEnumerator enumerator = new CellSpreadEnumerator(2);

            //    //放置失败的话在目标附近寻找位置
            //    foreach (CellStruct offset in enumerator)
            //    {
            //        var currentCell = CellClass.Coord2Cell(target);
            //        CoordStruct where = CellClass.Cell2Coord(currentCell + offset, target.Z);

            //        if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
            //        {
            //            if (Owner.OwnerObject.Ref.Base.Put(CellClass.Cell2Coord(pCell.Ref.MapCoords), Direction.N))
            //            {
            //                Logger.Log("周围设置成功");
            //                putted = true;
            //                lastLocation = CellClass.Cell2Coord(pCell.Ref.MapCoords);
            //                break;
            //            }
            //        }
            //    }

            //    //目标附近也失败的话回到原地
            //    if (putted == false)
            //    {
            //        //位移失败保持原位
            //        if (Owner.OwnerObject.Ref.Base.Put(location, Direction.N))
            //        {
            //            Logger.Log("返回原地成功");
            //            //放回原位也失败的话附近找一个位置放置防止吞单位
            //            putted = true;
            //            lastLocation = location;
            //        }
            //        else
            //        {
            //            Logger.Log("返回原地失败");
            //            foreach (CellStruct offset in enumerator)
            //            {
            //                var currentCell = CellClass.Coord2Cell(location);
            //                CoordStruct where = CellClass.Cell2Coord(currentCell + offset, location.Z);

            //                if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
            //                {
            //                    if (Owner.OwnerObject.Ref.Base.Put(CellClass.Cell2Coord(pCell.Ref.MapCoords), Direction.N))
            //                    {
            //                        Logger.Log("返回原地附近成功");
            //                        putted = true;
            //                        lastLocation = CellClass.Cell2Coord(pCell.Ref.MapCoords);
            //                        break;
            //                    }
            //                }
            //            }
            //        }
            //    }


            //}
            //else
            //{
            //    putted = true;
            //    lastLocation = target;
            //}

            //if (putted && lastLocation != null)
            //{
            //Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, selfWarhead, 100, false);
            //pBullet.Ref.DetonateAndUnInit(location);

            //Pointer<BulletClass> pBullet2 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, targeWarhead, 100, false);
            //pBullet2.Ref.DetonateAndUnInit(target);

            //pBullet2.Ref.DetonateAndUnInit(lastLocation);
            //}



        }

    }


    [Serializable]
    [ScriptAlias(nameof(XTMechHeroScript))]
    public class XTMechHeroScript : TechnoScriptable
    {
        public XTMechHeroScript(TechnoExt owner) : base(owner)
        {
            pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }


        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> axeWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTAxeWh");

        static Pointer<AnimTypeClass> lvAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("LV100");

        private SwizzleablePointer<AnimClass> pAnim;

        public static TechnoExt RegisteredXTMECH;


        private int CurrentLevel = 1;

        private bool isShieldOnline = false;
        private bool isRingOnline = false;
        private bool isBoltOnline = false;
        private bool isShakeOnline = false;

        private bool isFireOnline = false;

        public bool FireOn = false;

        private int ringRof = 50;

        private bool isInit = false;

        private int fireAttachRof = 20;
        private int fireHealRof = 2;
        private int fireBlastRof = 150;


        public override void OnUpdate()
        {
            if (RegisteredXTMECH.IsNullOrExpired())
            {
                RegisteredXTMECH = Owner;
            }

            if (!isInit)
            {
                isInit = true;
                Owner.OwnerObject.Ref.Owner.GiveSuperWeapon(SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkillLV1ASW"));
                Owner.OwnerObject.Ref.Owner.GiveSuperWeapon(SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkillLV1BSW"));
            }

            if (CurrentLevel >= 10 && !isShieldOnline)
            {
                Owner.OwnerObject.Ref.Owner.GiveSuperWeapon(SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkillLV10SW"));
                isShieldOnline = true;
                Pointer<BulletClass> pShield = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTMechShieldWh"), 100, true);
                pShield.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }

            if (CurrentLevel >= 20 && !isRingOnline)
            {
                Owner.OwnerObject.Ref.Owner.GiveSuperWeapon(SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkillLV20SW"));
                isRingOnline = true;
                Pointer<BulletClass> pRing = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTMechRingWh"), 100, true);
                pRing.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }

            if (CurrentLevel >= 30 && !isBoltOnline)
            {
                isBoltOnline = true;
                Owner.OwnerObject.Ref.Owner.GiveSuperWeapon(SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkillLV30SW"));
            }

            if (CurrentLevel >= 50 && !isShakeOnline)
            {
                isShakeOnline = true;
                Owner.OwnerObject.Ref.Owner.GiveSuperWeapon(SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkillLV50SW"));
            }


            if (CurrentLevel >= 70 && !isFireOnline)
            {
                isFireOnline = true;
                Owner.OwnerObject.Ref.Owner.GiveSuperWeapon(SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkillLV70SW"));
            }

            if (FireOn)
            {
                if (fireHealRof-- <= 0)
                {
                    fireHealRof = 2;
                    if (Owner.OwnerObject.Ref.Base.Health > 5)
                    {
                        Owner.OwnerObject.Ref.Base.Health = Owner.OwnerObject.Ref.Base.Health - 3;
                    }
                }

                if (fireAttachRof-- <= 0)
                {
                    fireAttachRof = 20;
                    Pointer<BulletClass> pInviso = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTPoweredWh"), 100, true);
                    pInviso.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

                }
            }

            if (isRingOnline)
            {
                if (ringRof-- <= 0)
                {
                    ringRof = 50;
                    isShieldOnline = true;
                    Pointer<BulletClass> pInviso = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTMechRingEffWh"), 100, true);
                    pInviso.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }

            if (fireBlastRof > 0)
            {
                fireBlastRof--;
            }

            if (Owner.OwnerObject.Ref.Veterancy.Veterancy > 1.0)
            {
                Owner.OwnerObject.Ref.Veterancy.Reset();
                if (CurrentLevel < 100)
                {
                    CurrentLevel++;
                }
            }

            if (pAnim.IsNull)
            {
                CreateAnim();
            }

            pAnim.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(-120, -120, 0));
            pAnim.Ref.Animation.Value = CurrentLevel;
            pAnim.Ref.Pause();
            if (!Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap)
            {
                pAnim.Ref.Invisible = false;
            }
            else
            {
                pAnim.Ref.Invisible = true;
            }

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            Pointer<BulletClass> pAxe = pBulletType.Ref.CreateBullet(pTarget, Owner.OwnerObject, 20 + CurrentLevel * 3, axeWarhead, 100, true);
            pAxe.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());

            if (pTarget.CastToTechno(out var ptechno))
            {
                if (!ptechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                {
                    var strength = Owner.OwnerObject.Ref.Type.Ref.Base.Strength;
                    var health = Owner.OwnerObject.Ref.Base.Health + (int)(200 * CurrentLevel * 0.01) + 30;
                    Owner.OwnerObject.Ref.Base.Health = health > strength ? strength : health;
                }
            }

            if (FireOn)
            {
                Owner.GameObject.StartCoroutine(FireBlast(pTarget.Ref.GetCoords()));
            }

        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {

            if (CurrentLevel > 1)
            {
                pDamage.Ref = (int)(pDamage.Ref * (1 - 0.007 * CurrentLevel));
            }
        }

        public override void OnRemove()
        {
            KillAnim();
        }

        private void CreateAnim()
        {
            if (!pAnim.IsNull)
            {
                KillAnim();
            }

            var anim = YRMemory.Create<AnimClass>(lvAnim, Owner.OwnerObject.Ref.Base.Base.GetCoords());
            //anim.Ref.SetOwnerObject(Owner.OwnerObject.Convert<ObjectClass>());
            pAnim.Pointer = anim;
        }

        private void KillAnim()
        {
            if (!pAnim.IsNull)
            {
                pAnim.Ref.TimeToDie = true;
                pAnim.Ref.Base.UnInit();
                pAnim.Pointer = IntPtr.Zero;
            }
        }


        IEnumerator FireBlast(CoordStruct center)
        {
            var startAngle = 0;
            fireBlastRof = 150;
            for (var radius = 0; radius <= 5 * 256; radius += 100)
            {
                for (var angle = startAngle; angle < startAngle + 360; angle += 30)
                {
                    var pos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z);
                    int damage = 20;
                    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTFireWH"), 100, false);
                    pBullet.Ref.DetonateAndUnInit(pos + new CoordStruct(0, 0, 0));
                }

                startAngle -= 3;
                yield return new WaitForFrames(1);
            }
        }

        public void ShakeSkill(CoordStruct targetCoord)
        {
            var dir = GameUtil.Point2Dir(Owner.OwnerObject.Ref.Base.Base.GetCoords(), targetCoord);
            Owner.OwnerObject.Ref.TurretFacing.turn(dir.ToDirStruct());
            YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SwordWaveWithSound"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
            Owner.GameObject.StartCoroutine(SpellShakeSkill(targetCoord));
        }

        IEnumerator SpellShakeSkill(CoordStruct targetCoord)
        {
            var start = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var range = 9;


            var flipX = targetCoord.X > start.X ? 1 : -1;
            var flipY = targetCoord.Y > start.Y ? 1 : -1;

            var cita = Math.Atan(Math.Abs((targetCoord.Y - start.Y) / (targetCoord.X - start.X)));

            List<CoordStruct> targetCoords = new List<CoordStruct>();

            for (var i = 1; i <= range; i += 2)
            {
                var cs = i * Game.CellSize;
                var dest = new CoordStruct((start.X + (int)(cs * Math.Cos(cita) * flipX)), start.Y + (int)(cs * Math.Sin(cita)) * flipY, start.Z);
                targetCoords.Add(dest);
                YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("XTGapA"), dest);
                yield return new WaitForFrames(5);
            }

            foreach(var target in targetCoords)
            {
                var bullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 150, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ShockerWH"), 100, true);
                bullet.Ref.DetonateAndUnInit(target);
            }
        }
        

    }


    [ScriptAlias(nameof(XTLV30SkillSWScript))]
    [Serializable]
    public class XTLV30SkillSWScript : SuperWeaponScriptable
    {
        public XTLV30SkillSWScript(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            if (XTMechHeroScript.RegisteredXTMECH.IsNullOrExpired())
                return;
            var techno = XTMechHeroScript.RegisteredXTMECH;
            if (isPlayer)
            {
                techno.OwnerObject.Ref.Base.Select();
            }
            techno.GameObject.CreateScriptComponent(nameof(XTMechLV30EffectScript), "XTMechLV30EffectScript", techno);
        }
    }



    [ScriptAlias(nameof(XTLV50SkillSWScript))]
    [Serializable]
    public class XTLV50SkillSWScript : SuperWeaponScriptable
    {
        public XTLV50SkillSWScript(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            if (XTMechHeroScript.RegisteredXTMECH.IsNullOrExpired())
                return;
            var techno = XTMechHeroScript.RegisteredXTMECH;
            if (isPlayer)
            {
                techno.OwnerObject.Ref.Base.Select();
            }
            var script = techno.GameObject.GetComponent<XTMechHeroScript>();
            if (script != null)
            {
                script.ShakeSkill(CellClass.Cell2Coord(cell));
            }
        }
    }



    [ScriptAlias(nameof(XTLV70SkillSWScript))]
    [Serializable]
    public class XTLV70SkillSWScript : SuperWeaponScriptable
    {
        public XTLV70SkillSWScript(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            if (XTMechHeroScript.RegisteredXTMECH.IsNullOrExpired())
                return;
            var techno = XTMechHeroScript.RegisteredXTMECH;
            if (isPlayer)
            {
                techno.OwnerObject.Ref.Base.Select();
            }

            var script = techno.GameObject.GetComponent<XTMechHeroScript>();
            if (script != null)
            {
                script.FireOn = !script.FireOn;
            }
        }
    }



    [ScriptAlias(nameof(XTMechLV30EffectScript))]
    [Serializable]
    public class XTMechLV30EffectScript : TechnoScriptable
    {
        public XTMechLV30EffectScript(TechnoExt owner) : base(owner)
        {
        }

        private int duration = 250;



        public override void OnUpdate()
        {
            if (duration-- <= 0)
            {
                DetachFromParent();
                return;
            }

            if (duration == 240 || duration == 120)
            {
                Pointer<BulletClass> pBullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTSkipSpeedWh"), 100, false);
                pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }

            if (duration == 240 || duration == 210)
            {
                Pointer<BulletClass> pBullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTGuardWh"), 100, false);
                pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }



            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            Pointer<BulletClass> pAxe = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(pTarget, Owner.OwnerObject, 100, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("XTBoltWh"), 100, true);
            pAxe.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
            DetachFromParent();
        }
    }

}
