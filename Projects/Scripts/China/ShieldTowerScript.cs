using DynamicPatcher;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.China
{
    [ScriptAlias(nameof(ShieldTowerScript))]
    [Serializable]
    public class ShieldTowerScript : TechnoScriptable
    {
        public ShieldTowerScript(TechnoExt owner) : base(owner)
        {
            pShieldAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
            pShowAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }


        private static Pointer<AnimTypeClass> pBreak => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("CNSHTRC");
        private static Pointer<AnimTypeClass> pCreate => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("CNSHTRA");
        private static Pointer<AnimTypeClass> pEffect => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("CNSHTRB");
        private static Pointer<AnimTypeClass> pShow => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("CNSHTRBALL");

        private int _max = 1000;
        private int _recover = 1000;

        public bool isBreakDown = false;

        private int current = 1000;

        private int recoverTimer = 0;

        private int displayTimer = 0;

        private int healthRof = 3;


        private SwizzleablePointer<AnimClass> pShieldAnim;

        private SwizzleablePointer<AnimClass> pShowAnim;


        private int checkRof = 100;

        public override void Awake()
        {
            var ini = GameObject.CreateRulesIniComponentWith<ShieldTowerData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            _max = ini.Data.MaxShield;
            _recover = ini.Data.RecoverDelay;
            current = _max;
            base.Awake();
        }

        public bool CanWork()
        {
            double powerP = Owner.OwnerObject.Ref.Owner.Ref.GetPowerPercentage();
            return (!isBreakDown) && Owner.OwnerObject.Ref.IsPowerOnline() & (powerP >= 1);
        }

        public override void OnUpdate()
        {
            if(!isBreakDown)
            {
                if(healthRof--<=0)
                {
                    healthRof = 3;
                    if (current < _max)
                        current++;
                }
            }
            else
            {
                recoverTimer++;
                if(recoverTimer == _recover - 30)
                {
                    YRMemory.Create<AnimClass>(pCreate, Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
                if (recoverTimer >= _recover)
                {
                    isBreakDown = false;
                    current = _max;
                }
            }

            CreateAnim();

            var canwork = CanWork();

            pShowAnim.Ref.Invisible = !canwork;
            if (canwork && displayTimer > 0)
            {
                displayTimer--;
                pShieldAnim.Ref.Invisible = false;
            }
            else
            {
                pShieldAnim.Ref.Invisible = true;
            }


            var mission = Owner.OwnerObject.Convert<MissionClass>();

            if (mission.Ref.CurrentMission == Mission.Construction || mission.Ref.CurrentMission == Mission.Selling)
                return;

            if (checkRof > 0)
            {
                checkRof--;
            }
            else
            {
                checkRof = 100;
                var objs = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), 3 * Game.CellSize);
                foreach(var obj in objs)
                {
                    var techno = obj.Convert<TechnoClass>();
                    if (techno.Ref.Type.Ref.Base.Base.ID == Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID)
                        continue;
                    if (!techno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                        continue;

                    var ext = TechnoExt.ExtMap.Find(techno);
                    var script = ext.GameObject.GetComponent<ShieldTRProtectedScript>();
                    if(script is null)
                    {
                        ext.GameObject.CreateScriptComponent(nameof(ShieldTRProtectedScript), ShieldTRProtectedScript.UniqueId, nameof(ShieldTRProtectedScript), ext, Owner);
                    }
                    else
                    {
                        script.Tower = Owner;
                        script.Duration = 120;
                    }

                }
            }
        }

        private int experience = 0;

        public void GiveExp(int exp)
        {
            if (exp <= 10000)
            {
                experience += exp;
            }

            if (exp >= 5000 && exp < 10000)
            {
                if (!Owner.OwnerObject.Ref.Veterancy.IsRookie())
                {
                    Owner.OwnerObject.Ref.Veterancy.SetRookie();
                }
            }
            else if (exp >= 10000)
            {
                if (!Owner.OwnerObject.Ref.Veterancy.IsElite())
                {
                    Owner.OwnerObject.Ref.Veterancy.SetElite();
                }
            }
        }

        public int CosumeShield(int damage,bool giveExp)
        {
            if (!CanWork())
                return damage;

            double k = 1;
            int exp = 0;

            if (Owner.OwnerObject.Ref.Veterancy.IsRookie())
            {
                k = 0.9;
            }else if (Owner.OwnerObject.Ref.Veterancy.IsElite())
            {
                k = 0.7;
            }

            if (current > damage * k)
            {
                exp = damage;
                current -= (int)(damage * k);
                displayTimer = 50;
                damage = 0;
            }
            else
            {
                exp = (int)(current / k);
                damage = damage - (int)(current/k);
                if (damage < 0)
                {
                    damage = 0;
                }
                current = 0;
            }

            if (current <= 0)
            {
                displayTimer = 0;
                recoverTimer = 0;
                isBreakDown = true;
                YRMemory.Create<AnimClass>(pBreak, Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }

            return damage;
        }

        public override void OnRemove()
        {
            KillAnim();
        }

        private void CreateAnim()
        {
            if (pShieldAnim.IsNull)
            {
                var anim = YRMemory.Create<AnimClass>(pEffect, Owner.OwnerObject.Ref.Base.Base.GetCoords());
                pShieldAnim.Pointer = anim;
            }
            if (pShowAnim.IsNull)
            {
                var anim = YRMemory.Create<AnimClass>(pShow, Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 900));
                pShowAnim.Pointer = anim;
            }

        }

        private void KillAnim()
        {
            if (!pShieldAnim.IsNull)
            {
                pShieldAnim.Ref.TimeToDie = true;
                pShieldAnim.Ref.Base.UnInit();
                pShieldAnim.Pointer = IntPtr.Zero;
            }

            if (!pShowAnim.IsNull)
            {
                pShowAnim.Ref.TimeToDie = true;
                pShowAnim.Ref.Base.UnInit();
                pShowAnim.Pointer = IntPtr.Zero;
            }
        }


    }


    public class ShieldTowerData : INIAutoConfig
    {
        [INIField(Key = "ShieldTower.Max")]
        public int MaxShield = 1000;
        [INIField(Key = "ShieldTower.Recover")]
        public int RecoverDelay = 1200;
    }

    [ScriptAlias(nameof(ShieldTRProtectedScript))]
    [Serializable]
    public class ShieldTRProtectedScript : TechnoScriptable
    {
        public int Duration = 120;

        public static int UniqueId = 20231122;

        public TechnoExt Tower;

        public ShieldTRProtectedScript(TechnoExt owner,TechnoExt tower) : base(owner)
        {
            Tower = tower;
        }

        public override void OnUpdate()
        {
            if (Tower.IsNullOrExpired())
                DetachFromParent();

            if (Duration-- <= 0)
            {
                DetachFromParent();
            }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if(Tower.IsNullOrExpired())
                return;

            var component = Tower.GameObject.GetComponent<ShieldTowerScript>();
            if (component is null)
                return;

            if (!component.CanWork())
                return;
           

            Pointer<TechnoClass> ownerTechno = Owner.OwnerObject;
            var absDamage = pDamage.Ref;
            int trueDamage = MapClass.GetTotalDamage(pDamage.Ref, pWH, ownerTechno.Ref.Type.Ref.Base.Armor, DistanceFromEpicenter);
            if (trueDamage < 1)
                return;

            var rate = Math.Round((double)absDamage / (double)trueDamage, 2);

            var giveExp = false;

            if (pAttackingHouse.IsNotNull)
            {
                if(!Owner.OwnerObject.Ref.Owner.Ref.IsAlliedWith(pAttackingHouse))
                {
                    giveExp = true;
                }
            }
            var damageLeft = component.CosumeShield(trueDamage,true);

            if(damageLeft==0)
            {
                pDamage.Ref = 0;
            }
            else
            {
                pDamage.Ref = (int)(damageLeft * rate);
            }

        }
    }
}
