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
    [ScriptAlias(nameof(AirStrikeSetTargetScript))]
    public class AirStrikeSetTargetScript : TechnoScriptable
    {
        public AirStrikeSetTargetScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnUpdate()
        {
            if(!Owner.OwnerObject.Ref.Target.IsNull)
            {
                if(Owner.OwnerObject.Ref.Target.Ref.WhatAmI() == PatcherYRpp.AbstractType.Cell)
                {
                    var pCell = Owner.OwnerObject.Ref.Target.Convert<CellClass>();
                    var techno = pCell.Ref.FindTechnoNearestTo(new Point2D(128, 128), false, Owner.OwnerObject);
                    if (!techno.IsNull)
                    {
                        Owner.OwnerObject.Ref.SetTarget(techno.Convert<AbstractClass>());
                    }
                }
            }
        }
    }
}
