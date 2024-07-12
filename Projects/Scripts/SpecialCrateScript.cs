using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Newtonsoft.Json;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [ScriptAlias(nameof(SpecialCrateScript))]
    [Serializable]
    public class SpecialCrateScript : TechnoScriptable
    {
        public SpecialCrateScript(TechnoExt owner) : base(owner)
        {
        }

        private int rof = 5;
        private string warhead = string.Empty;
        private List<string> required = new List<string>();

        public override void Awake()
        {
            var ini = this.CreateRulesIniComponentWith<SpecialCrateData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            warhead = ini.Data.Warhead;

            if(!string.IsNullOrWhiteSpace(ini.Data.Required))
            {
                required = ini.Data.Required.Split(',').ToList();
            }
        }

        public override void OnUpdate()
        {
            if (rof-- > 0)
                return;

            rof = 5;

            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            var technos = ObjectFinder.FindTechnosNear(location, 1 * Game.CellSize).Where(x => !x.Ref.InLimbo).OrderByDescending(x => x.Ref.Base.GetCoords().DistanceFrom(location)).ToList();

            if (!technos.Any())
                return;

            Pointer<ObjectClass> target;

            if(!required.Any())
                target = technos.First();
            else
                target = technos.Where(x => required.Contains(x.Convert<TechnoClass>().Ref.Type.Ref.Base.Base.ID)).FirstOrDefault();

            if (target == null || target.IsNull)
                return;

            if (!string.IsNullOrWhiteSpace(warhead))
            {
                var pbullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(target.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find(warhead), 100, false);
                pbullet.Ref.DetonateAndUnInit(target.Ref.Base.GetCoords());
            }

            Owner.OwnerObject.Ref.Base.Remove();
            Owner.OwnerObject.Ref.Base.UnInit();

        }
    }

    public class SpecialCrateData : INIAutoConfig
    {
        [INIField(Key = "SpecialCrate.Required")]
        public string Required = "";

        [INIField(Key = "SpecialCrate.Warhead")]
        public string Warhead = "";

    }

}
