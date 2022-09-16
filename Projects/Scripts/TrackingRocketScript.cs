using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;


namespace DpLib.Scripts
{
    [Serializable]
    public class TrackingRocketScript : TechnoScriptable
    {
        public TrackingRocketScript(TechnoExt owner) : base(owner)
        {
        }

        private bool targetSetted = false;

        TechnoExt pTargetRef;


        public override void OnUpdate()
        {
           
            base.OnUpdate();

            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            var mission = Owner.OwnerObject.Convert<MissionClass>();
            Logger.Log($"MISSION:{mission.Ref.CurrentMission}");

            var tar = Owner.OwnerObject.Ref.Target;

            if (!tar.IsNull)
            {
                var targetCoord = tar.Ref.GetCoords();
                var colorBlue = new ColorStruct(0, 0, 255);
                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(location, targetCoord, colorBlue, colorBlue, colorBlue, 8);
                pLaser.Ref.Thickness = 5;
                pLaser.Ref.IsHouseColor = true;

                if (targetSetted)
                {
                    if (!pTargetRef.IsNullOrExpired())
                    {
                        Logger.Log("找到Techno了");
                        Owner.OwnerObject.Ref.Target = pTargetRef.OwnerObject.Convert<AbstractClass>();
                        if (Owner.OwnerObject.CastToFoot(out var pfoot))
                        {
                            Logger.Log("是pfoot");
           
                            var loco = pfoot.Ref.Locomotor.ToLocomotionClass();
                            var targetLocation = pTargetRef.OwnerObject.Ref.Base.Base.GetCoords();

                            if(MapClass.Instance.TryGetCellAt(targetLocation, out var pCell))
                            {
                                Owner.OwnerObject.Ref.SetDestination(pCell.Convert<AbstractClass>(), false);
                            }
                   
                        }
                    }
                    return;
                }

                var cell = tar.Convert<CellClass>();
                
                if(!cell.IsNull)
                {
                    var techno = cell.Ref.FindTechnoNearestTo(new Point2D(60, 60), false, Owner.OwnerObject);
                    pTargetRef=(TechnoExt.ExtMap.Find(techno));
                    if (!pTargetRef.IsNullOrExpired())
                    {
                        Owner.OwnerObject.Ref.Target = pTargetRef.OwnerObject.Convert<AbstractClass>();
                    }
                }
                targetSetted = true;

                //targetSetted = true;
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);
        }
    }
}
