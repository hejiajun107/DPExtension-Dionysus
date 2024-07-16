using DynamicPatcher;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [ScriptAlias(nameof(CannotAttackAlliesScript))]
    [Serializable]
    public class CannotAttackAlliesScript : TechnoScriptable
    {
        public CannotAttackAlliesScript(TechnoExt owner) : base(owner)
        {
        }

        public override void Awake()
        {
            var ini = this.CreateRulesIniComponentWith<CannotAttackAlliesConfig>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            delay = ini.Data.delay;
            max = ini.Data.max;
            delivery = ini.Data.limboDelivery;
        }

        private int delay = 0;
        private int max = 1;
        private string delivery = string.Empty;
        private int currentCount = 0;

        public override void OnUpdate()
        {
            if (delay > 0)
            {
                delay--;
            }
            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(currentCount>=max)
                return;

            if (delay <= 0)
            {
                if(pTarget.CastToTechno(out var ptechno))
                {
                    if (ptechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner) && ptechno.Ref.Owner != Owner.OwnerObject.Ref.Owner)
                    {
                        delay = 50;
                        currentCount++;

                        if(!string.IsNullOrWhiteSpace(delivery))
                        {
                            var pSW = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(delivery));
                            pSW.Ref.IsCharged = true;
                            pSW.Ref.Launch(CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords()), true);
                        }
                    }
                }
            }
        }
    }

    public class CannotAttackAlliesConfig : INIAutoConfig
    {
        [INIField(Key = "CannotAttackAllies.Delay")]
        public int delay = 50;

        [INIField(Key = "CannotAttackAllies.Max")]
        public int max = 1;

        [INIField(Key = "CannotAttackAllies.LimboDelivery")]
        public string limboDelivery;
    }
}
