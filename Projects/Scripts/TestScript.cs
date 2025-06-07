using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(TestScript))]
    public class TestScript : TechnoScriptable
    {
        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");


        public TestScript(TechnoExt owner) : base(owner)
        {

        }

 

        public override void Awake()
        {
            
        }

        public override void OnUpdate()
        {
            Logger.Log("Bullet" + BulletClass.Array.Count());
            //var mission = Owner.OwnerObject.Convert<MissionClass>();
            //if(mission.Ref.CurrentMission != Mission.Guard)
            //{
            //    var technos = ObjectFinder.FindTechnosNear(Owner.OwnerObject.Ref.Base.Base.GetCoords(), 3 * Game.CellSize).Select(x=>x.Convert<TechnoClass>()).ToList();
            //    var car = technos.Where(x => x.Ref.Owner == Owner.OwnerObject.Ref.Owner && x.Ref.Type.Ref.Base.Base.ID == "APYH" && x.Ref.Passengers.GetTotalSize() < 3).FirstOrDefault();
            //    if (car.IsNotNull)
            //    {

            //        mission.Ref.ForceMission(Mission.Enter);
            //        mission.Ref.Mission_Enter();
            //        Owner.OwnerObject.Ref.SetFocus(car.Convert<AbstractClass>());

            //        //Owner.OwnerObject.Ref.SetDestination()
            //    }
            //}

        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
         
        }
    }
}
