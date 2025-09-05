using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Scripts.Tavern;

namespace Scripts.Tavern
{
    [ScriptAlias(nameof(CommanderUnitScript))]
    [Serializable]
    public class CommanderUnitScript : TechnoScriptable
    {
        public CommanderUnitScript(TechnoExt owner) : base(owner)
        {
        }


        public override void OnUpdate()
        {
            var node = TavernGameManager.Instance.FindPlayerNodeByHouse(Owner.OwnerObject.Ref.Owner);
            var slot = node.CommanderSlot;

            var prerequisites = TavernGameManager.Instance.CommanderTypes.Select(x => x.Prerequisites).ToList();

            slot.InitComander(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);

            //删除所有提供建造前提的建筑
            var buildings = ObjectFinder.FindTechnosNear(slot.Owner.OwnerObject.Ref.Base.Base.GetCoords(), 10 * Game.CellSize).Select(x=>x.Convert<TechnoClass>())
                .Where(x=>x.Ref.Owner == slot.Owner.OwnerObject.Ref.Owner && prerequisites.Contains(x.Ref.Type.Ref.Base.Base.ID)).ToList();

            foreach (var building in buildings) 
            {
                building.Ref.Base.Remove();
                building.Ref.Base.UnInit();
            }

            Owner.OwnerObject.Ref.Base.Remove();
            Owner.OwnerObject.Ref.Base.UnInit();
        }
    }
}
