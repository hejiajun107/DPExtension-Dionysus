using DynamicPatcher;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(BunkerWallScript))]
    [Serializable]
    public class BunkerWallScript : TechnoScriptable
    {
        public BunkerWallScript(TechnoExt owner) : base(owner)
        {
        }

        private BunkerWallData Data;

        public override void Awake()
        {
            var settingINI = this.CreateRulesIniComponentWith<BunkerWallData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            Data = settingINI.Data;
        }

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            placeCoord = coord;
        }

        private bool putted = false;
        private bool toreplace = false;
        private CoordStruct placeCoord;

        public override void OnUpdate()
        {
            if(putted == true && toreplace == false)
            {
                toreplace = true;
                return;
            }

            var building = Owner.OwnerObject.Convert<BuildingClass>();

            Pointer<TechnoTypeClass> type = Owner.OwnerObject.Ref.Type;

            if (building.Ref.GetCurrentFrame() == 12)
            {
                type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.WestEastWall);
            }
            else if (building.Ref.GetCurrentFrame() == 8)
            {
                type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(Data.NorthSouthWall);
            }

            var currentCell = CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            Owner.OwnerObject.Ref.Base.Remove();
            TechnoPlacer.PlaceTechnoNear(type, Owner.OwnerObject.Ref.Owner, currentCell, true);
            Owner.OwnerObject.Ref.Base.UnInit();

        }
    }

    [Serializable]
    public class BunkerWallData : INIAutoConfig
    {
        [INIField(Key = "BunkerWall.WE")]
        public string WestEastWall;

        [INIField(Key = "BunkerWall.NS")]
        public string NorthSouthWall;
    }
}
