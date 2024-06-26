﻿using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using System;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(ScSniperScript))]

    public class ScSniperScript : TechnoScriptable
    {
        public ScSniperScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner, 15);
        }

        private ManaCounter _manaCounter;

        static Pointer<BulletTypeClass> pBullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> pHealWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ScSniperHealthWH");
        static Pointer<WarheadTypeClass> pCrazyWH => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ScCrazyWh");



        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("ScSniprDeploy"), location);
                var bullet = pBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 100, pCrazyWH, 100, true);
                bullet.Ref.DetonateAndUnInit(location);
            }



            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);
            //if (weaponIndex == 1)
            //{
            //    var bullet = pBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, pCrazyWH, 100, true);
            //    bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            //}
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            base.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);

            if (Owner.OwnerObject.Ref.Base.Health <= 75)
            {
                if (!pAttacker.IsNull)
                    if (pAttacker.CastToTechno(out var pAttTechno))
                    {
                        if (_manaCounter.Cost(100))
                        {
                            var bullet = pBullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pAttTechno, 700, pHealWH, 100, true);
                            bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                        }
                    }
            }

        }
    }
}
