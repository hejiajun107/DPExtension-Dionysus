using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Shared;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Reflection;

namespace DpLib.Scripts
{
    [Serializable]
    [ScriptAlias(nameof(CComandScript))]
    public class CComandScript : TechnoScriptable
    {
        public CComandScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner,12);
        }


        private ManaCounter _manaCounter;

        static Pointer<WeaponTypeClass> Weapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("FakeWeaponTimeStop");
        //static Pointer<WeaponTypeClass> Weapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("RedEye2");
        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        //static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("TimeStopPro");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ChronoTimeStopWH");

        private int delay = 0;

        static Pointer<WarheadTypeClass> animWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("CCShieldWh");

        private int rof = 0;

        private int checkb = 0;
        

        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                StartTimeStop();
            }
            //if (delay <= 100)
            //{
            //    delay++;
            //}
            if (delay > 0)
            {
                var center = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                delay--;
                if (rof-- <= 0)
                {
                    rof = 30;
                    var pStop = bulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, warhead, 100, false);
                    pStop.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }

                if (checkb-- <= 0)
                {
                    checkb = 2;

                    foreach (var bullet in BulletClass.Array)
                    {
                        if(!bullet.Ref.Type.Ref.Inviso)
                        {
                            if(bullet.Ref.Base.Base.GetCoords().BigDistanceForm(center) <= (5 * Game.CellSize))
                            {
                                var bulletExt = BulletExt.ExtMap.Find(bullet);
                                var component = bulletExt.GameObject.GetComponent<CComandStopBullet>();

                                if (component != null)
                                {
                                    component.Duration = 20;
                                }
                                else
                                {
                                    bulletExt.GameObject.CreateScriptComponent(nameof(CComandStopBullet), CComandStopBullet.UniqueId, "CComandStopBullet", bulletExt);
                                }
                            }

                        }
                    }
                }
            }
        }

        private void StartTimeStop()
        {
            if (_manaCounter.Cost(100))
            {
                var pbullet = bulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, animWarhead, 100, false);
                pbullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                delay = 300;
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if(Owner.OwnerObject.Ref.Base.InLimbo)
                return;
            if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
            {
                StartTimeStop();
            }
            //Pointer<TechnoClass> pTechno = Owner.OwnerObject;

            //if (delay >= 100)
            //{
            //    if (_manaCounter.Cost(50))
            //    {
            //        Pointer<BulletClass> pBullet = Weapon.Ref.Projectile.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 10, Weapon.Ref.Warhead, Weapon.Ref.Speed, false);
            //        CoordStruct where = pTechno.Ref.Base.Base.GetCoords();
            //        BulletVelocity velocity = new BulletVelocity(0, 0, 0);

            //        pBullet.Ref.MoveTo(where + new CoordStruct(0, 0, 1000), velocity);
            //        pBullet.Ref.SetTarget(pTarget);

            //        delay = 0;
            //        //pBullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
            //    }
            //}

        }



    }


    [Serializable]
    [ScriptAlias(nameof(CComandStopBullet))]
    public class CComandStopBullet : BulletScriptable
    {
        public CComandStopBullet(BulletExt owner) : base(owner)
        {
        }

        public static int UniqueId = 20230304; 

        public int Duration = 20;

        private bool inited = false;

        BulletVelocity initVelocity;

        public override void OnLateUpdate()
        {
            if (!inited)
            {
                inited = true;
                initVelocity = Owner.OwnerRef.Velocity;
            }

            if (Duration-- > 0)
            {
                var velocity = Owner.OwnerRef.Velocity;
                Owner.OwnerRef.Velocity = new BulletVelocity(velocity.X * 0.2, velocity.Y * 0.2, Owner.OwnerRef.Type.Ref.Arcing ? velocity.Z * 0.2 + 6 : velocity.Z * 0.2);
            }
            else
            {
                Owner.OwnerRef.Velocity = initVelocity;
                DetachFromParent();
            }
        }
    }

}
