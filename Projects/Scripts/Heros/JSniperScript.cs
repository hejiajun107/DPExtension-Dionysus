using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(JSniperScript))]
    public class JSniperScript : TechnoScriptable
    {
        public JSniperScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter(owner, 12);
        }

        private ManaCounter _manaCounter;


        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if (mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                if (_manaCounter.Cost(80))
                {
                    YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("ArcherSpAnim"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    Owner.OwnerObject.Ref.Ammo = 1;
                }
            }

            if(Owner.OwnerObject.Ref.Ammo == 0)
            {
                _manaCounter.Resume();
            }
            else
            {
                _manaCounter.Pause();
            }
        }



        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (Owner.OwnerObject.Ref.Base.InLimbo)
                return;

            if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
            {
                if (_manaCounter.Cost(80))
                {
                    YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("ArcherSpAnim"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    Owner.OwnerObject.Ref.Ammo = 1;
                }
            }

            //if (weaponIndex == 0)
            //{

            //}
            //if (weaponIndex == 1)
            //{
            //    if(_manaCounter.Cost(100))
            //    {
            //        Owner.OwnerObject.Ref.Ammo = 1;
            //    }
            //    //if (pTarget.CastToTechno(out Pointer<TechnoClass> pTechno))
            //    //{
            //    //    TechnoExt pTargetExt = TechnoExt.ExtMap.Find(pTechno);

            //    //    if (pTargetExt.GameObject.GetComponent(VirusSpreadDecorator.ID) == null)
            //    //    {
            //    //        pTargetExt.GameObject.CreateScriptComponent(nameof(VirusSpreadDecorator), VirusSpreadDecorator.ID, "VirusSpread Decorator", Owner, pTargetExt, 1000);
            //    //    }
            //    //}
            //}
        }
    }


    [ScriptAlias(nameof(ArcherArrowScript))]
    [Serializable]
    public class ArcherArrowScript : BulletScriptable
    {
        public ArcherArrowScript(BulletExt owner) : base(owner)
        {
        }

        private static Pointer<WeaponTypeClass> BowSpitWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("BowSpitWeapon");

        public override void OnDetonate(Pointer<CoordStruct> pCoords)
        {
            if (!Owner.OwnerObject.Ref.Owner.IsNull)
            {
                if (Owner.OwnerObject.Ref.Owner.Ref.Ammo > 0)
                {
                    Owner.OwnerObject.Ref.Owner.Ref.Ammo = 0;
                    var technos = ObjectFinder.FindTechnosNear(pCoords.Ref, (int)(2.5 * Game.CellSize));

                    List<BulletVelocity> list = new List<BulletVelocity>()
                    {
                        new BulletVelocity(0,0,30),
                        new BulletVelocity(60,0,30),
                        new BulletVelocity(0,60,30),
                        new BulletVelocity(-60,0,30),
                        new BulletVelocity(0,-60,30),
                        new BulletVelocity(60,60,30),
                        new BulletVelocity(-60,-60,30),
                    };

                    int vIndex = 0;

                    foreach (var techno in technos)
                    {
                        foreach(var tTarget in technos)
                        {
                            var bullet = BowSpitWeapon.Ref.Projectile.Ref.CreateBullet(tTarget.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner, BowSpitWeapon.Ref.Damage, BowSpitWeapon.Ref.Warhead, BowSpitWeapon.Ref.Speed, BowSpitWeapon.Ref.Warhead.Ref.Bright);
                            if (vIndex > list.Count - 1)
                            {
                                vIndex = 0;
                            }
                            bullet.Ref.MoveTo(techno.Ref.Base.GetCoords() + new CoordStruct(0, 0, 200), list[vIndex]);
                            bullet.Ref.SetTarget(tTarget.Convert<AbstractClass>());
                            vIndex++;
                        }
                    }

                }
            }
        }
    }

    [ScriptAlias(nameof(ArcherSpiltScript))]
    [Serializable]
    public class ArcherSpiltScript : BulletScriptable
    {
        public ArcherSpiltScript(BulletExt owner) : base(owner)
        {
        }

        private bool inited = false;
        private int initHeight = 0;
        private bool over = false;
        private double initX;
        private double initY;

        public override void OnUpdate()
        {
            if (inited == false)
            {
                inited = true;
                initHeight = Owner.OwnerObject.Ref.Base.GetHeight();
                initX = Owner.OwnerObject.Ref.Velocity.X;
                initY = Owner.OwnerObject.Ref.Velocity.Y;
            }

            if(!over)
            {
                if (Owner.OwnerObject.Ref.Base.GetHeight() < (initHeight + 1000))
                {
                    Owner.OwnerObject.Ref.Velocity.Z = 30;
                    Owner.OwnerObject.Ref.Velocity.X = initX == 0 ? 0 : (initX > 0 ? 30 : -30);
                    Owner.OwnerObject.Ref.Velocity.Y = initY == 0 ? 0 : (initY > 0 ? 30 : -30);
                }
                else
                {
                    over = true;
                }
            }
        }
    }


    [ScriptAlias(nameof(VirusSpreadDecorator))]
    [Serializable]
    public class VirusSpreadDecorator : TechnoScriptable
    {
        public static int ID = 514022;
        public VirusSpreadDecorator(TechnoExt owner, TechnoExt self, int lifetime) : base(self)
        {
            Owner = (owner);
            Self = (self);
            _lifetime = lifetime;
            _startTime = lifetime - 100;
        }

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> pWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("JVirusSpreadWH");

        TechnoExt Owner;
        TechnoExt Self;

        int _lifetime = 1000;

        //开始传播的延迟
        private int _startTime = 100;

        int rof = 2;

        public override void OnUpdate()
        {
            if (Owner.IsNullOrExpired() || Self.IsNullOrExpired() || _lifetime <= 0)
            {
                DetachFromParent();
                return;
            }

            if (rof-- > 0 && --_lifetime > 0)
            {
                return;
            }
            rof = 5;

            var owner = Owner;
            


            if (_lifetime < _startTime)
            {
                var current = Self.OwnerObject.Ref.Base.Base.GetCoords();
                Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(owner.OwnerObject.Convert<AbstractClass>(), owner.OwnerObject, 1, pWarhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(current);
            }


        }



    }

}