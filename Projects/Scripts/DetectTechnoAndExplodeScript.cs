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

namespace Scripts
{
    [ScriptAlias(nameof(DetectTechnoAndExplodeScript))]
    [Serializable]
    public class DetectTechnoAndExplodeScript : TechnoScriptable
    {
        public DetectTechnoAndExplodeScript(TechnoExt owner) : base(owner)
        {
        }

        public List<string> required = new List<string>();
        private int range = 1;

        
        public override void Awake()
        {
            var ini = this.CreateRulesIniComponentWith<DetectTechnoData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);

            if (!string.IsNullOrWhiteSpace(ini.Data.DetectTechnos))
            {
                required = ini.Data.DetectTechnos.Split(',').ToList();
            }

            range = ini.Data.Range;
            base.Awake();
        }

        private int delay = 5;



        public override void OnUpdate()
        {
            if (delay-- > 0)
            {
                return;
            }

            delay = 5;

            var objs = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), range * Game.CellSize)
                .Select(x => x.Convert<TechnoClass>().Ref.Type.Ref.Base.Base.ID.ToString()).ToList();
                
            if(objs.Intersect(required).Any())
            {
                Owner.OwnerObject.Ref.Base.TakeDamage(1000, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Super"), false);
            }
        }

     
    }

    
    public class DetectTechnoData : INIAutoConfig
    {
        [INIField(Key = "DetectTechno.Types")]
        public string DetectTechnos;
        [INIField(Key = "DetectTechno.Range")]
        public int Range = 1;
    }
}
