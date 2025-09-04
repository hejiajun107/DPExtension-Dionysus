using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using Scripts.Tavern;

namespace Scripts.Tavern
{
    [ScriptAlias(nameof(RefreshSWScript))]
    [Serializable]
    public class RefreshSWScript : SuperWeaponScriptable
    {
        public RefreshSWScript(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            if(TavernGameManager.Instance is not null)
            {
                var node = TavernGameManager.Instance.FindPlayerNodeByHouse(Owner.OwnerObject.Ref.Owner);
                if(node is not null)
                {
                    node.OnRefreshShop();
                }
            }
        }
    }

    [ScriptAlias(nameof(UpgradeBaseSWScript))]
    [Serializable]
    public class UpgradeBaseSWScript : SuperWeaponScriptable
    {
        public UpgradeBaseSWScript(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            if (TavernGameManager.Instance is not null)
            {
                var node = TavernGameManager.Instance.FindPlayerNodeByHouse(Owner.OwnerObject.Ref.Owner);
                if (node is not null)
                {

                    var cost = TavernGameManager.Instance.GetUpgradeBaseCost(Owner.OwnerObject.Ref.Owner);
                    if(cost > 0)
                    {
                        if(Owner.OwnerObject.Ref.Owner.Ref.Available_Money() < cost)
                        {
                            TavernGameManager.Instance.SoundNoMoney();
                            return;
                        }
                        else
                        {
                            Owner.OwnerObject.Ref.Owner.Ref.TransactMoney(-cost);
                            TavernGameManager.Instance.ShowFlyingTextAt($"-{cost}", node.Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 500), 1);
                        }
                    }

                    node.OnUpgrade();
                }
            }
        }
    }


    [ScriptAlias(nameof(ReadyToStartSWScript))]
    [Serializable]
    public class ReadyToStartSWScript : SuperWeaponScriptable
    {
        public ReadyToStartSWScript(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            if (TavernGameManager.Instance is not null)
            {
                var node = TavernGameManager.Instance.FindPlayerNodeByHouse(Owner.OwnerObject.Ref.Owner);
                if (node is not null)
                {
                    node.IsReady = !node.IsReady;
                }
            }
        }
    }

    [ScriptAlias(nameof(VoteToSkipSWScript))]
    [Serializable]
    public class VoteToSkipSWScript : SuperWeaponScriptable
    {
        public VoteToSkipSWScript(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            if (TavernGameManager.Instance is not null)
            {
                var node = TavernGameManager.Instance.FindPlayerNodeByHouse(Owner.OwnerObject.Ref.Owner);
                if (node is not null)
                {
                    node.VoteSkiped = !node.VoteSkiped;

                    var psw = node.Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(Owner.OwnerObject.Ref.Type);
                    psw.Ref.IsCharged = false;
                    psw.Ref.RechargeTimer.Start(1);
                }
            }
        }
    }





}
