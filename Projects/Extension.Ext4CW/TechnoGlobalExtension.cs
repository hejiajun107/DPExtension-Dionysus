using Extension.CWUtilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.CW
{

    [GlobalScriptable(typeof(TechnoExt))]
    public partial class TechnoGlobalExtension : TechnoScriptable
    {
        public TechnoGlobalExtension(TechnoExt owner) : base(owner)
        {
        }

        INIComponentWith<TechnoGlobalTypeExt> INI;

        public TechnoGlobalTypeExt Data;

        public override void Awake()
        {
            base.Awake();
            INI = this.CreateRulesIniComponentWith<TechnoGlobalTypeExt>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            Data = INI.Data;
            PartialHelper.TechnoAwakeAction(Owner);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            PartialHelper.TechnoUpdateAction(Owner);
        }

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            base.OnPut(coord, faceDir);
            PartialHelper.TechnoPutAction(Owner, coord, faceDir);
        }

        public override void OnRemove()
        {
            base.OnRemove();
            PartialHelper.TechnoRemoveAction(Owner);
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);
            PartialHelper.TechnoFireAction(Owner, pTarget, weaponIndex);
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            base.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
            PartialHelper.TechnoReceiveDamageAction(Owner, pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
        }
    }


    public partial class TechnoGlobalTypeExt : INIAutoConfig
    {
      
    }
}
