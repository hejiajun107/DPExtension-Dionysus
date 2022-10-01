using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [Serializable]
    //[GlobalScriptable(typeof(TechnoExt))]
    public class DebugScriptable : TechnoScriptable
    {
        public DebugScriptable(TechnoExt owner) : base(owner)
        {
        }

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            Logger.Log($"{Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID}被放置在({coord.X},{coord.Y},{coord.Z}),朝向{faceDir}");
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            string name = string.Empty;
            if (pTarget.CastToTechno(out var ptechno))
            {
                name = ptechno.Ref.Type.Ref.Base.Base.ID;
            }
            Logger.Log($"{Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID}使用武器{weaponIndex}向{pTarget.Ref.WhatAmI()}:{pTarget.Ref}开火");
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            string attacker = "不知道是谁";
            
            if (!pAttacker.IsNull)
            {
                if (pAttacker.CastToTechno(out var ptechno))
                {
                    attacker = ptechno.Ref.Type.Ref.Base.Base.ID;
                }
            }

            Logger.Log($"{Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID}受到来自{attacker}的伤害,伤害值{pDamage.Ref},弹头{pWH.Ref.Base.ID},当前生命值{Owner.OwnerObject.Ref.Base.Health}");
        }
       
    }
}
