using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Linq;

namespace Scripts.Yuri
{
    [Serializable]
    [ScriptAlias(nameof(BlackHandScript))]
    public class BlackHandScript : TechnoScriptable
    {
        public BlackHandScript(TechnoExt owner) : base(owner)
        {
        }

        //Oil 100 20

        private int rof = 150;

        public override void OnUpdate()
        {
            if(rof-->0)
            {
                return;
            }

            rof = 150;

            var technos = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), 5 * Game.CellSize).Where(x=>x.Ref.Base.WhatAmI() == AbstractType.Building).Select(x=> x.Convert<TechnoClass>()).ToList();

            var houses = technos.Where(x => !x.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner)).Select(x => x.Ref.Owner).Distinct().ToList();

            foreach (var house in houses)
            {
                if(house.IsNull)
                    continue;

                var ownerName = house.Ref.Type.Ref.Base.ID.ToString();
                if (ownerName == "Special" || ownerName == "Neutral")
                    continue;

                house.Ref.TransactMoney(-30);
                var pWarhead = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("TEMPLEMONEYWH");
                var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 0, pWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                
            }
        }

    }
}
