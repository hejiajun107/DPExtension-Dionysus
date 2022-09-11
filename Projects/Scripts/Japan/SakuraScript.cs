using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.Japan
{
    [Serializable]
    public class SakuraScript : TechnoScriptable
    {
        public SakuraScript(TechnoExt owner) : base(owner)
        {
        }

        private bool IsDead = false;

        private int delay = 100;
        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            Logger.Log(Owner.OwnerObject.Ref.SpawnOwner.IsNull);

            base.OnFire(pTarget, weaponIndex);


            //Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(Owner.OwnerObject.Ref.Base.Base.GetCoords(), Owner.OwnerObject.Ref.SpawnOwner.Ref.Base.Base.GetCoords(), new ColorStruct(255,0,0), new ColorStruct(255, 0, 0), new ColorStruct(255, 0, 0), 40);
            //pLaser.Ref.Thickness = 10;
            //pLaser.Ref.IsHouseColor = true;

        }
    }
}
