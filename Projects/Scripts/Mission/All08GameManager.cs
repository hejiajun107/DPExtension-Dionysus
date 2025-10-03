using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using Extension.Coroutines;
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using Scripts;

namespace Scripts
{
    [ScriptAlias(nameof(All08GameManager))]
    [Serializable]
    public class All08GameManager : TechnoScriptable
    {
        public All08GameManager(TechnoExt owner) : base(owner)
        {
        }

        private MissionData missionData = new MissionData();
        public override void Awake()
        {
            missionData = MissionDataHelper.Load();
            //重置08数据
            missionData.DataAll08 = new DataAll08();
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (pWH.IsNull)
                return;

            if(pWH.Ref.Base.ID == "MSSIGWh1")
            {
                if(missionData.DataAll08.FindTeams < 3)
                {
                    missionData.DataAll08.FindTeams++;
                }
            }
            else if(pWH.Ref.Base.ID == "MSSIGWh2")
            {
                Owner.GameObject.StartCoroutine(GiveMoneyAfter(50, 2000));
            }
            else if (pWH.Ref.Base.ID == "MSSIGWh3")
            {
                Owner.GameObject.StartCoroutine(GiveMoneyAfter(50, 2000));
            }
            else if (pWH.Ref.Base.ID == "MSSIGWh4")
            {
                missionData.DataAll08.AirForceActited = true;
            }
            else if(pWH.Ref.Base.ID == "MSSIGWh5")
            {
                missionData.DataAll08.Cash = Owner.OwnerObject.Ref.Owner.Ref.Available_Money();
                MissionDataHelper.Save(missionData);
            }

        }

        IEnumerator GiveMoneyAfter(int delay,int amount)
        {
            yield return new  WaitForFrames(delay);
            Owner.OwnerObject.Ref.Owner.Ref.TransactMoney(2000);
        }
    }
}
