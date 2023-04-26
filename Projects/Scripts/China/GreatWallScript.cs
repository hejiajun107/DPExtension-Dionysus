using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;

namespace Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(GreatWallScript))]
    public class GreatWallScript : TechnoScriptable
    {
        public GreatWallScript(TechnoExt owner) : base(owner)
        {

        }

        private int checkDelay = 10;

        private string dummyType = "LIFTDUMMY";

        private CoordStruct adjust = new CoordStruct(0, 0, 500);

        TechnoExt techno;

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if(mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                if (techno.IsNullOrExpired())
                    return;

                var tPassenger = Owner.OwnerObject.Ref.Passengers.FirstPassenger.Convert<TechnoClass>();
                if (tPassenger.IsNull)
                    return;

                if(tPassenger.Ref.Type.Ref.Base.Base.ID == dummyType)
                {
                    Owner.OwnerObject.Ref.Passengers.RemoveFirstPassenger();
                    tPassenger.Ref.Base.Remove();
                    tPassenger.Ref.Base.UnInit();
                }

                var component = techno.GameObject.GetComponent<GreatWallEffectScript>();

                if (component != null)
                    techno.GameObject.RemoveComponent(component);

                var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                techno.OwnerObject.Ref.Base.Remove();
                TechnoPlacer.PlaceTechnoNear(techno.OwnerObject,CellClass.Coord2Cell(currentLocation));
            }

            if (checkDelay-- >= 0)
                return;

            checkDelay = 10;

            if (Owner.OwnerObject.Ref.Passengers.FirstPassenger.IsNull)
                return;

            var pPassenger = Owner.OwnerObject.Ref.Passengers.FirstPassenger.Convert<TechnoClass>();
            if (pPassenger.IsNull)
                return;

            if (pPassenger.Ref.Type.Ref.Base.Base.ID == dummyType)
            {
                if(techno.IsNullOrExpired())
                {
                    Owner.OwnerObject.Ref.Passengers.RemoveFirstPassenger();
                    pPassenger.Ref.Base.Remove();
                    pPassenger.Ref.Base.UnInit();
                }
                return;
            }

            var ext = TechnoExt.ExtMap.Find(pPassenger);
            if (ext.IsNullOrExpired())
                return;

            var type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(dummyType);
            var dummy = type.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();

            Owner.OwnerObject.Ref.Passengers.RemoveFirstPassenger();

            if(dummy.CastToFoot(out var pfoot))
            {
                Owner.OwnerObject.Ref.AddPassenger(pfoot);
            }

            techno = ext;

            var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords() + adjust;

            if (techno.GameObject.GetComponent<GreatWallEffectScript>() == null)
            {
                techno.GameObject.CreateScriptComponent(nameof(GreatWallEffectScript), GreatWallEffectScript.UniqueId, nameof(GreatWallEffectScript), techno, coord);
            }

            ++Game.IKnowWhatImDoing;
            techno.OwnerObject.Ref.Base.Put(coord + new CoordStruct(0, 0, -100), Direction.N);
            --Game.IKnowWhatImDoing;


        }

        public override void OnRemove()
        {
            if(techno.IsNullOrExpired()) { return; }
            var component = techno.GameObject.GetComponent<GreatWallEffectScript>();

            if (component != null)
                techno.GameObject.RemoveComponent(component);

            var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            techno.OwnerObject.Ref.Base.Remove();
            TechnoPlacer.PlaceTechnoNear(techno.OwnerObject, CellClass.Coord2Cell(currentLocation));
        }
    }


    [Serializable]
    [ScriptAlias(nameof(GreatWallEffectScript))]
    public class GreatWallEffectScript : TechnoScriptable
    {
        public static int UniqueId = 20230424;

        public GreatWallEffectScript(TechnoExt owner,CoordStruct targetCoord) : base(owner)
        {
            target = targetCoord;
        }

        private CoordStruct target;

        private int healthDelay = 10;

        public override void OnUpdate()
        {
            if(Owner.IsNullOrExpired())
            { 
                DetachFromParent();
                return; 
            }

            if(healthDelay--<=0)
            {
                healthDelay = 10;
                var strenth = Owner.OwnerObject.Ref.Type.Ref.Base.Strength;
                var health = Owner.OwnerObject.Ref.Base.Health;
                Owner.OwnerObject.Ref.Base.Health = health + 5 > strenth ? strenth : health + 5;
            }
        }

        public override void OnLateUpdate()
        {
            if (Owner.IsNullOrExpired())
            {
                DetachFromParent();
                return;
            }

            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
                return;

            var pfoot = Owner.OwnerObject.Convert<FootClass>();
            pfoot.Ref.StopMoving();
            pfoot.Convert<FootClass>().Ref.StopDrive();
            pfoot.Convert<FootClass>().Ref.AbortMotion();
            pfoot.Convert<FootClass>().Ref.Locomotor.Lock();
            pfoot.Convert<FootClass>().Ref.StopDrive();

            Owner.OwnerObject.Ref.SetDestination(default, false);

            if (Owner.OwnerObject.Ref.Target.IsNotNull)
            {
                var target = Owner.OwnerObject.Ref.Target;
                if (!Owner.OwnerObject.Ref.IsCloseEnoughToAttack(target))
                    mission.Ref.ForceMission(Mission.Stop);
            }
            else if (mission.Ref.CurrentMission == Mission.Attack && !Owner.OwnerObject.Ref.IsCloseEnoughToAttackCoords(Owner.OwnerObject.Ref.GetTargetCoords()))
                mission.Ref.ForceMission(Mission.Stop);
            else if (mission.Ref.CurrentMission == Mission.AttackMove)
                mission.Ref.ForceMission(Mission.Stop);

            if (Owner.OwnerObject.Ref.Base.Base.GetCoords().Z < target.Z)
                Owner.OwnerObject.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 10));
            else
                Owner.OwnerObject.Ref.Base.SetLocation(target);
        }

        private bool CanAttackTarget(Pointer<AbstractClass> pTarget)
        {
            int i = Owner.OwnerObject.Ref.SelectWeapon(pTarget);
            FireError fireError = Owner.OwnerObject.Ref.GetFireError(pTarget, i, true);
            switch (fireError)
            {
                case FireError.ILLEGAL:
                case FireError.CANT:
                case FireError.MOVING:
                case FireError.RANGE:
                    return false;
            }
            return true;
        }

        private FireError GetFireError(Pointer<AbstractClass> pTarget)
        {
            int i = Owner.OwnerObject.Ref.SelectWeapon(pTarget);
            FireError fireError = Owner.OwnerObject.Ref.GetFireError(pTarget, i, false);
            return fireError;
        }
    }
}
