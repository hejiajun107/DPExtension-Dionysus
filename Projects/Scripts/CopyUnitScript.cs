/*
 Author:DegradedJun
 bilibili:https://space.bilibili.com/38860280
 */

using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(CopyUnitScript))]
    public class CopyUnitScript : TechnoScriptable
    {
        public CopyUnitScript(TechnoExt owner) : base(owner)
        {
        }

        bool converted = false;
        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);
            if (!converted)
            {
                var myType = Owner.OwnerObject.Ref.Base.Base.WhatAmI();
                if (myType != AbstractType.Unit && myType != AbstractType.Infantry)
                    return;

                var targetType = pTarget.Ref.WhatAmI();
                if (targetType == myType)
                {
                    if (pTarget.CastToTechno(out var pTechno))
                    {
                        switch (targetType)
                        {
                            case AbstractType.Unit:
                                {
                                    Owner.OwnerObject.Convert<UnitClass>().Ref.Type = pTechno.Ref.Type.Convert<UnitTypeClass>();
                                    converted = true;
                                    break;
                                }
                            case AbstractType.Infantry:
                                {
                                    Owner.OwnerObject.Convert<InfantryClass>().Ref.Type = pTechno.Ref.Type.Convert<InfantryTypeClass>();
                                    converted = true;
                                    break;
                                }
                            default:
                                return;
                        }
                    }
                }


            }

        }
    }
}
