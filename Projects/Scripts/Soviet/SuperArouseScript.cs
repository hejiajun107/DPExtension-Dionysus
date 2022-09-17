using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.Soviet
{
    [Serializable]
    [ScriptAlias(nameof(SuperArouseScript))]
    public class SuperArouseScript : TechnoScriptable
    {
        public SuperArouseScript(TechnoExt owner) : base(owner)
        {
        }

        private bool inited = false;

        static Pointer<SuperWeaponTypeClass> sw1 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("RushnukeSpecial");
        static Pointer<SuperWeaponTypeClass> sw2 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("TeslaRushSpecial");
        static Pointer<SuperWeaponTypeClass> sw3 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SovietParaTankSpecial");


        public override void OnUpdate()
        {
            if(!inited)
            {
                Pointer<HouseClass> pOwner = Owner.OwnerObject.Ref.Owner;
                Pointer<SuperClass> pSuper1 = pOwner.Ref.FindSuperWeapon(sw1);
                Pointer<SuperClass> pSuper2 = pOwner.Ref.FindSuperWeapon(sw2);
                Pointer<SuperClass> pSuper3 = pOwner.Ref.FindSuperWeapon(sw3);

                pSuper1.Ref.IsCharged = true;
                pSuper2.Ref.IsCharged = true;
                pSuper3.Ref.IsCharged = true;

                Owner.OwnerObject.Ref.Base.UnInit();
            }
            base.OnUpdate();
        }
    }
}
