using DynamicPatcher;
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
        private bool removeAfterTriggered = true;
        private string sw = string.Empty;
        private int range = 1;
        private bool actived = false;

        public override void Awake()
        {
            var ini = this.CreateRulesIniComponentWith<SpecialCrateData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            warhead = ini.Data.Warhead;
            removeAfterTriggered = ini.Data.RemoveAfterTriggered;
            sw = ini.Data.SW;
            range = ini.Data.Range;

            if(!string.IsNullOrWhiteSpace(ini.Data.Required))
            {
                required = ini.Data.Required.Split(',').ToList();
            }
        }

        public override void OnUpdate()
        {
            if (actived)
                return;

            if (rof-- > 0)
                return;

            rof = 5;

            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            var technos = ObjectFinder.FindTechnosNear(location, range * Game.CellSize).Where(x => !x.Ref.InLimbo).OrderByDescending(x => x.Ref.Base.GetCoords().DistanceFrom(location)).ToList();

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

            if (!string.IsNullOrEmpty(sw))
            {
                var swType = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(sw);
                if (swType.IsNotNull)
                {
                    var psw = target.Convert<TechnoClass>().Ref.Owner.Ref.FindSuperWeapon(swType);
                    psw.Ref.IsCharged = true;
                    psw.Ref.Launch(CellClass.Coord2Cell(target.Ref.Base.GetCoords()), true);
                    psw.Ref.IsCharged = false;
                }
            }

            if (removeAfterTriggered)
            {
                Owner.OwnerObject.Ref.Base.Remove();
                Owner.OwnerObject.Ref.Base.UnInit();
            }
            else
            {
                actived = true;
            }
        }
    }

    public class SpecialCrateData : INIAutoConfig
    {
        [INIField(Key = "SpecialCrate.Required")]
        public string Required = "";

        [INIField(Key = "SpecialCrate.Warhead")]
        public string Warhead = "";


        [INIField(Key = "SpecialCrate.SuperWeapon")]
        public string SW = "";

        [INIField(Key = "SpecialCrate.RemoveAfterTriggered")]
        public bool RemoveAfterTriggered = true;

        [INIField(Key = "SpecialCrate.Range")]
        public int Range = 1;

    }

}
