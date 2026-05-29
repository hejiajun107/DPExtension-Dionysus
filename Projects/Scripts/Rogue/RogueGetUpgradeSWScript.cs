using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Rogue
{
    [ScriptAlias(nameof(RogueGetUpgradeSWScript))]
    [Serializable]
    public class RogueGetUpgradeSWScript : SuperWeaponScriptable
    {
        public RogueGetUpgradeSWScript(SuperWeaponExt owner) : base(owner)
        {

        }

        private static List<int> UpgradeLevelPool = null;

        public List<int> CurrentLevelPool = null;

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            base.OnLaunch(cell, isPlayer);
        }

        private void CheckAndInitLevelPool()
        {
            if (CurrentLevelPool == null)
            {
                if (UpgradeLevelPool != null)
                {
                    CurrentLevelPool = new List<int>();
                    CurrentLevelPool.AddRange(UpgradeLevelPool);
                }
                else
                {
                    UpgradeLevelPool = new List<int>();
                    UpgradeLevelPool.Add(0);
                    for (var i = 0; i < 9; i++)
                    {
                        //var rd =
                        //UpgradeLevelPool.Add();
                    }
                    CurrentLevelPool.AddRange(UpgradeLevelPool);
                }
            }
        }

    }
}
