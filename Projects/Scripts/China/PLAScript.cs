using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.China
{
    [Serializable]
    public class PLAScript : TechnoScriptable
    {
        public PLAScript(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<TechnoTypeClass> originType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("E9");
        private static Pointer<TechnoTypeClass> targetType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("GATP2");

        public bool inited = false;

        public override void OnUpdate()
        {
            if(!inited)
            {
                inited = true;

                if (Owner.OwnerObject.Ref.Owner.IsNull)
                    return;

                if (Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                {
                    if (Owner.OwnerObject.Ref.Type == originType)
                    {
                        ref DynamicVectorClass<Pointer<BuildingClass>> buildings = ref Owner.OwnerObject.Ref.Owner.Ref.Buildings;

                        bool hasBuilding = false;

                        for (int i = buildings.Count - 1; i >= 0; i--)
                        {
                            Pointer<BuildingClass> pBuilding = buildings.Get(i);

                            if(pBuilding.Ref.Type.Ref.Base.Base.Base.ID == "ZGBGJD" && pBuilding.Ref.Base.Base.IsOnMap == true && !pBuilding.Ref.Base.Base.InLimbo)
                            {
                                hasBuilding = true;
                                break;
                            }
                        }

                        if (hasBuilding)
                        {
                            Owner.OwnerObject.Convert<InfantryClass>().Ref.Type = targetType.Convert<InfantryTypeClass>();
                        }
                    }
                }
            }
        }


    }
}
