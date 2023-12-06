using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Soviet
{
    [ScriptAlias(nameof(STankBunkerScript))]
    [Serializable]
    public class STankBunkerScript : TechnoScriptable
    {
        public STankBunkerScript(TechnoExt owner) : base(owner)
        {
            pBunkerAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }

        private int rof = 100;
        private int erof = 30;

        private static List<CoordStruct> coordOffets = new List<CoordStruct>()
        {
            new CoordStruct(256,256,30),
            new CoordStruct(256,-256,30),
            new CoordStruct(-256,256,30),
            new CoordStruct(-256,-256,30),
        };

        private int currentChargeIndex = 0;
        private int currentEIndex = 0;

        private static Pointer<WeaponTypeClass> chargeWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("AssaultBolt");

        private SwizzleablePointer<AnimClass> pBunkerAnim;

        public override void OnUpdate()
        {
            CreateAnim();

            if (!CanWork())
            {
                pBunkerAnim.Ref.Invisible = true;
                return;
            }

            if (Owner.OwnerObject.Ref.BunkerLinkedItem.IsNull)
            {
                pBunkerAnim.Ref.Invisible = true;
                return;
            }

            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if(mission.Ref.CurrentMission == Mission.Construction || mission.Ref.CurrentMission == Mission.Selling)
            {
                pBunkerAnim.Ref.Invisible = true;
            }
            else
            {
                pBunkerAnim.Ref.Invisible = false;

            }


            if (erof <= 0)
            {
                erof = 30;
                Owner.OwnerObject.Ref.Electric_Zap(Owner.OwnerObject.Convert<AbstractClass>(), chargeWeapon, Owner.OwnerObject.Ref.Base.Base.GetCoords() + coordOffets[currentChargeIndex]);
                Pointer<EBolt> pBolt = YRMemory.Create<EBolt>();
                if (!pBolt.IsNull)
                {
                    var eSource = Owner.OwnerObject.Ref.Base.Base.GetCoords() + coordOffets[currentEIndex];
                    pBolt.Ref.Fire(eSource, eSource + new CoordStruct(MathEx.Random.Next(-50, 50), MathEx.Random.Next(-50, 50), 100), 0);
                    pBolt.Ref.AlternateColor = false;
                    currentEIndex++;
                    if (currentEIndex > 3)
                        currentEIndex = 0;
                }
            }
            else
            {
                erof--;
            }

            if (rof-- > 0)
                return;

            rof = 100;
            var strength = Owner.OwnerObject.Ref.BunkerLinkedItem.Ref.Type.Ref.Base.Strength;
            if(Owner.OwnerObject.Ref.BunkerLinkedItem.Ref.Base.Health < strength)
            {
                Owner.OwnerObject.Ref.BunkerLinkedItem.Ref.Base.Health = Owner.OwnerObject.Ref.BunkerLinkedItem.Ref.Base.Health + 5 > strength ? strength : Owner.OwnerObject.Ref.BunkerLinkedItem.Ref.Base.Health + 5;
            }

            Owner.OwnerObject.Ref.Electric_Zap(Owner.OwnerObject.Ref.BunkerLinkedItem.Convert<AbstractClass>(), chargeWeapon, Owner.OwnerObject.Ref.Base.Base.GetCoords() + coordOffets[currentChargeIndex]);

            currentChargeIndex++;
            if (currentChargeIndex > 3)
                currentChargeIndex = 0;

            var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
            var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Ref.BunkerLinkedItem.Convert<AbstractClass>(), Owner.OwnerObject, 0, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("TKBEffectWh"), 100, true);
            pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
        }

        public bool CanWork()
        {
            double powerP = Owner.OwnerObject.Ref.Owner.Ref.GetPowerPercentage();
            return Owner.OwnerObject.Ref.IsPowerOnline() & (powerP >= 1);
        }

        public override void OnRemove()
        {
            KillAnim();
        }

        private void CreateAnim()
        {
            if (pBunkerAnim.IsNull)
            {
                var anim = YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("NASTBNK_C"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                pBunkerAnim.Pointer = anim;
            }
        }

        private void KillAnim()
        {
            if (!pBunkerAnim.IsNull)
            {
                pBunkerAnim.Ref.TimeToDie = true;
                pBunkerAnim.Ref.Base.UnInit();
                pBunkerAnim.Pointer = IntPtr.Zero;
            }
        }


        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            int trueDamage = MapClass.GetTotalDamage(pDamage.Ref, pWH, Owner.OwnerObject.Ref.Type.Ref.Base.Armor, DistanceFromEpicenter);

            if (trueDamage > 1)
            {
                if (DistanceFromEpicenter > Game.CellSize)
                {
                    pDamage.Ref =(int)(pDamage.Ref * 0.2);
                }
            }
        }
    }
}
