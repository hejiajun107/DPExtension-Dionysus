using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Linq;

namespace Scripts.Yuri
{
    [Serializable]
    [ScriptAlias(nameof(BlackHandScript))]
    public class BlackHandScript : TechnoScriptable
    {
        public BlackHandScript(TechnoExt owner) : base(owner)
        {
            pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }

        static Pointer<AnimTypeClass> sleepAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SleepAnim");

        private SwizzleablePointer<AnimClass> pAnim;
        //Oil 100 20

        private int rof = 150;

        private bool isSleeping = false;

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (isSleeping)
            {
                if (mission.Ref.CurrentMission == Mission.Unload)
                {
                    mission.Ref.ForceMission(Mission.Stop);
                    isSleeping = false;
                    KillAnim();
                }
                else if (mission.Ref.CurrentMission == Mission.Move)
                {
                    isSleeping = false;
                    KillAnim();
                }
            }
            else
            {
                if (mission.Ref.CurrentMission == Mission.Unload)
                {
                    isSleeping = true;
                }
            }

            if (isSleeping)
            {
                mission.Ref.ForceMission(Mission.Stop);
                if (pAnim.IsNull)
                {
                    CreateAnim();
                }
                pAnim.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(-90, 60, 0));
                pAnim.Ref.Pause();
                if (!Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap && Owner.OwnerObject.Ref.Owner == HouseClass.Player)
                {
                    pAnim.Ref.Invisible = false;
                }
                else
                {
                    pAnim.Ref.Invisible = true;
                }
            }
        
            if (rof-->0)
            {
                return;
            }

            rof = 150;

            var technos = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), 5 * Game.CellSize).Where(x=>x.Ref.Base.WhatAmI() == AbstractType.Building).Select(x=> x.Convert<TechnoClass>()).ToList();

            var houses = technos.Where(
                x => !x.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner) && !x.Ref.Type.Ref.Insignificant
            ).Select(x => x.Ref.Owner).Distinct().ToList();

            foreach (var house in houses)
            {
                if(house.IsNull)
                    continue;

                var ownerName = house.Ref.Type.Ref.Base.ID.ToString();
                if (ownerName == "Special" || ownerName == "Neutral")
                    continue;

                house.Ref.TransactMoney(-30);
                var pWarhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("TEMPLEMONEYWH");
                var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 0, pWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                
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

            var anim = YRMemory.Create<AnimClass>(sleepAnim, Owner.OwnerObject.Ref.Base.Base.GetCoords());
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


    }
}
