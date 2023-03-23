using DynamicPatcher;
using Extension.Coroutines;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.China
{
    [ScriptAlias(nameof(DF41Bullet1Script))]
    [Serializable]
    public class DF41Bullet1Script : BulletScriptable
    {
        public DF41Bullet1Script(BulletExt owner) : base(owner)
        {
        }

        private bool inited = false;
        private int initHeight = 0;
        private bool over = false;
        private bool step1 = false;
        private bool step2 = false;
        private bool step3 = false;


        private Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisble");
        private Pointer<WarheadTypeClass> pLaunch => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DFLaunchWave");

        private Pointer<WarheadTypeClass> pPush => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DFPushWave");

        private Pointer<BulletTypeClass> pDrop => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("DF41Seeker6");

        private Pointer<BulletTypeClass> pNext => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("DF41Seeker5");

        private Pointer<WarheadTypeClass> pFull => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DFFallWave");


        public override void OnLateUpdate()
        {
            if (inited == false)
            {
                inited = true;
                initHeight = Owner.OwnerObject.Ref.Base.GetHeight();
            }

            var velocity = Owner.OwnerObject.Ref.Velocity;


            if (!step1)
            {
                if (startingSpeed <= 5)
                {
                    startingSpeed++;
                }
                velocity.X = 0;
                velocity.Y = 0;
                velocity.Z = startingSpeed;
                Owner.OwnerObject.Ref.Velocity = velocity;

                if (Owner.OwnerObject.Ref.Base.GetHeight() > initHeight + 300)
                {
                    var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<AbstractClass>.Zero : Owner.OwnerObject.Ref.Owner.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<TechnoClass>.Zero : Owner.OwnerObject.Ref.Owner, 1, pLaunch, 100, false);
                    bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, -300)); //was 500
                    step1 = true;
                    startingSpeed = 50;
                }
            }
            else if (!step2)
            {
                if (startingSpeed <= 100)
                {
                    startingSpeed++;
                }
                velocity.X = 0;
                velocity.Y = 0;
                velocity.Z = startingSpeed; //Owner.OwnerObject.Ref.Speed;
                Owner.OwnerObject.Ref.Velocity = velocity;



                if (Owner.OwnerObject.Ref.Base.GetHeight() < initHeight + 2000)
                {
                    velocity.X = 0;
                    velocity.Y = 0;
                }
                else
                {
                    var speedMultipler = ((Owner.OwnerObject.Ref.Base.GetHeight() - (initHeight + 2000)) / 2000d);
                    velocity.X = velocity.X * speedMultipler;
                    velocity.Y = velocity.Y * speedMultipler;
                }

                Owner.OwnerObject.Ref.Velocity = velocity;


                if (Owner.OwnerObject.Ref.Base.GetHeight() > initHeight + 4000)
                {
                    step2 = true;
                }
            }
            else if (!step3)
            {
                //换抛射体
                step3 = true;

                var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<AbstractClass>.Zero : Owner.OwnerObject.Ref.Owner.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<TechnoClass>.Zero : Owner.OwnerObject.Ref.Owner, 1, pPush, 100, false);
                bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());


                var bulletNext = pNext.Ref.CreateBullet(Owner.OwnerObject.Ref.Target, Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<TechnoClass>.Zero : Owner.OwnerObject.Ref.Owner, 100, pPush, 40, false);
                bulletNext.Ref.MoveTo(coord, velocity);
                bulletNext.Ref.SetTarget(Owner.OwnerObject.Ref.Target);
                var bulletDrop = pDrop.Ref.CreateBullet(Owner.OwnerObject.Ref.Target, Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<TechnoClass>.Zero : Owner.OwnerObject.Ref.Owner, 20, pFull, 60, false);
                if (MapClass.Instance.TryGetCellAt(coord - new CoordStruct(0, 0, Owner.OwnerObject.Ref.Base.GetHeight()), out var pcell))
                {
                    bulletDrop.Ref.MoveTo(coord, -velocity);
                    bulletDrop.Ref.SetTarget(pcell.Convert<AbstractClass>());
                }
                else
                {
                    bulletDrop.Ref.DetonateAndUnInit(coord);
                }

                Owner.OwnerObject.Ref.Base.UnInit();

            }



        }

        private int startingSpeed = 1;


    }



    [ScriptAlias(nameof(DF41Bullet2Script))]
    [Serializable]
    public class DF41Bullet2Script : BulletScriptable
    {
        public DF41Bullet2Script(BulletExt owner) : base(owner)
        {
        }

        private bool inited = false;

        private int minFly = 10;
        private int maxFly = 100;

        private int minRange = 15;

        public int flyTime = 0;

        private bool up = true;
        private int initHeight = 0;

        private Random rd;

        private Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisble");


        private Pointer<WarheadTypeClass> pPush => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DFPushWave");

        private Pointer<BulletTypeClass> pDrop => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("DF41Seeker4");

        private Pointer<BulletTypeClass> pNext => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("DF41Seeker3");

        private Pointer<WarheadTypeClass> pFull => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DFFallWave");

        private int vZ = 0;

        private int maxDistance = 50;


        public override void OnLateUpdate()
        {
            if (inited == false)
            {
                inited = true;
                initHeight = Owner.OwnerObject.Ref.Base.GetHeight();
                var myCoord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                rd = new Random(myCoord.X + myCoord.Y - myCoord.Z);
                maxDistance = maxDistance + rd.Next(-5, 5);

            }

            flyTime++;

            bool goNext = false;

            //if (flyTime > maxFly)
            //{
            //    goNext = true;
            //}

            var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var distance = new CoordStruct(coord.X, coord.Y, Owner.OwnerObject.Ref.TargetCoords.Z).DistanceFrom(Owner.OwnerObject.Ref.TargetCoords);
            if (!double.IsNaN(distance))
            {
                if (distance < maxDistance * Game.CellSize)
                {
                    if (flyTime > minFly)
                    {
                        goNext = true;
                    }
                }
            }

            var velocity = Owner.OwnerObject.Ref.Velocity;

            if (!goNext)
            {
                //if (Owner.OwnerObject.Ref.Base.GetHeight() > initHeight + 500)
                //{
                //    up = false;
                //}
                //if (Owner.OwnerObject.Ref.Base.GetHeight() < initHeight - 500)
                //{
                //    up = true;
                //}
                //if (up)
                //{
                //    vZ += 2;
                //    if (vZ >= 100)
                //    {
                //        vZ = -20;
                //        up = false;
                //    }
                //}
                //else
                //{
                //    vZ -= 2;
                //    if (vZ <= -100)
                //    {
                //        vZ = 20;
                //        up = true;
                //    }
                //}

                //velocity.Z = vZ;//up ? 60 : -60;
                //velocity.Z = 0;

                var total = Owner.OwnerObject.Ref.TargetCoords.BigDistanceForm(Owner.OwnerObject.Ref.SourceCoords);
                if (!double.IsNaN(total))
                {
                    var t = total / Owner.OwnerObject.Ref.Speed;
                    Owner.OwnerObject.Ref.Velocity.X = (Owner.OwnerObject.Ref.TargetCoords.X - Owner.OwnerObject.Ref.SourceCoords.X) / t * 3;
                    Owner.OwnerObject.Ref.Velocity.Y = (Owner.OwnerObject.Ref.TargetCoords.Y - Owner.OwnerObject.Ref.SourceCoords.Y) / t * 3;

                    Owner.OwnerObject.Ref.Velocity.Z = 0;
                }

                //Owner.OwnerObject.Ref.Velocity = velocity;
            }
            else
            {

                var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<AbstractClass>.Zero : Owner.OwnerObject.Ref.Owner.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<TechnoClass>.Zero : Owner.OwnerObject.Ref.Owner, 1, pPush, 100, false);
                bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());


                var bulletNext = pNext.Ref.CreateBullet(Owner.OwnerObject.Ref.Target, Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<TechnoClass>.Zero : Owner.OwnerObject.Ref.Owner, 100, pPush, 25, false);
                //bulletNext.Ref.MoveTo(coord, Owner.OwnerObject.Ref.Velocity);
                bulletNext.Ref.MoveTo(coord, new BulletVelocity(rd.Next(-100,100), rd.Next(-100, 100), rd.Next(-100, 100)));
                bulletNext.Ref.SetTarget(Owner.OwnerObject.Ref.Target);
                var bulletDrop = pDrop.Ref.CreateBullet(Owner.OwnerObject.Ref.Target, Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<TechnoClass>.Zero : Owner.OwnerObject.Ref.Owner, 20, pFull, 60, false);
                if (MapClass.Instance.TryGetCellAt(coord - new CoordStruct(0, 0, Owner.OwnerObject.Ref.Base.GetHeight()), out var pcell))
                {
                    bulletDrop.Ref.MoveTo(coord, -velocity);
                    bulletDrop.Ref.SetTarget(pcell.Convert<AbstractClass>());
                }
                else
                {
                    bulletDrop.Ref.DetonateAndUnInit(coord);
                }

                Owner.OwnerObject.Ref.Base.UnInit();
            }
        }
    }



    [ScriptAlias(nameof(DF41Bullet3Script))]
    [Serializable]
    public class DF41Bullet3Script : BulletScriptable
    {
        public DF41Bullet3Script(BulletExt owner) : base(owner)
        {
        }

        private bool inited = false;

        private int minFly = 10;
        private int maxFly = 100;

        private int minRange = 15;

        public int flyTime = 0;

        private bool up = true;
        private int initHeight = 0;


        private Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisble");


        private Pointer<WarheadTypeClass> pPush => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DFPushWave");


        private Pointer<BulletTypeClass> pDrop => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("DF41Seeker2");

        private Pointer<BulletTypeClass> pNext => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("DF41Seeker1");

        private Pointer<WarheadTypeClass> pFull => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DFFallWave");

        private Random rd;

        private int maxDistance = 20;


        public override void OnLateUpdate()
        {
            if (inited == false)
            {
                inited = true;
                initHeight = Owner.OwnerObject.Ref.Base.GetHeight();
                var myCoord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                rd = new Random(myCoord.X + myCoord.Y - myCoord.Z);
                maxDistance = maxDistance + rd.Next(-2, 2);
            }

            flyTime++;

            bool goNext = false;

            //if (flyTime > maxFly)
            //{
            //    goNext = true;
            //}

            var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var distance = new CoordStruct(coord.X, coord.Y, Owner.OwnerObject.Ref.TargetCoords.Z).BigDistanceForm(Owner.OwnerObject.Ref.TargetCoords);
            if (!double.IsNaN(distance))
            {
                if (distance < maxDistance * Game.CellSize)
                {
                    if (flyTime > minFly)
                    {
                        goNext = true;
                    }
                }
            }

            var velocity = Owner.OwnerObject.Ref.Velocity;

            if (!goNext)
            {
                if (Owner.OwnerObject.Ref.Base.GetHeight() < initHeight + 2000)
                {
                    velocity.Z = 60;
                }

                //var total = Owner.OwnerObject.Ref.TargetCoords.BigDistanceForm(Owner.OwnerObject.Ref.SourceCoords);
                //if (!double.IsNaN(total))
                //{
                //    var t = total / Owner.OwnerObject.Ref.Speed;
                //    Owner.OwnerObject.Ref.Velocity.X = (Owner.OwnerObject.Ref.TargetCoords.X - Owner.OwnerObject.Ref.SourceCoords.X) / t;
                //    Owner.OwnerObject.Ref.Velocity.Y = (Owner.OwnerObject.Ref.TargetCoords.Y - Owner.OwnerObject.Ref.SourceCoords.Y) / t;
                //}

                Owner.OwnerObject.Ref.Velocity = velocity;
            }
            else
            {

                var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<AbstractClass>.Zero : Owner.OwnerObject.Ref.Owner.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<TechnoClass>.Zero : Owner.OwnerObject.Ref.Owner, 1, pPush, 100, false);
                bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());


                var bulletNext = pNext.Ref.CreateBullet(Owner.OwnerObject.Ref.Target, Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<TechnoClass>.Zero : Owner.OwnerObject.Ref.Owner, 100, pPush, 100, false);
                //bulletNext.Ref.MoveTo(coord, velocity);
                bulletNext.Ref.MoveTo(coord, new BulletVelocity(rd.Next(-200,200), rd.Next(-200, 200), rd.Next(-200, 200)));
                bulletNext.Ref.SetTarget(Owner.OwnerObject.Ref.Target);
                var bulletDrop = pDrop.Ref.CreateBullet(Owner.OwnerObject.Ref.Target, Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<TechnoClass>.Zero : Owner.OwnerObject.Ref.Owner, 20, pFull, 60, false);
                if (MapClass.Instance.TryGetCellAt(coord - new CoordStruct(0, 0, Owner.OwnerObject.Ref.Base.GetHeight()), out var pcell))
                {
                    bulletDrop.Ref.MoveTo(coord, -Owner.OwnerObject.Ref.Velocity);
                    bulletDrop.Ref.SetTarget(pcell.Convert<AbstractClass>());
                }
                else
                {
                    bulletDrop.Ref.DetonateAndUnInit(coord);
                }

                Owner.OwnerObject.Ref.Base.UnInit();
            }
        }
    }



    [ScriptAlias(nameof(DF41Bullet4Script))]
    [Serializable]
    public class DF41Bullet4Script : BulletScriptable
    {
        public DF41Bullet4Script(BulletExt owner) : base(owner)
        {
        }


        private bool inited = false;

        private int minFly = 10;
        private int maxFly = 100;

        private int minRange = 15;

        public int flyTime = 0;

        private bool up = true;
        private int initHeight = 0;


        private Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisble");


        private Pointer<WarheadTypeClass> pPush => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DFPushWave");

        private Pointer<WarheadTypeClass> pFull => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("DFFallWave");

        private Pointer<WeaponTypeClass> pWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("DF41SpitWeapon");

        private Random rd;

        private int maxDistance = 15;


        public override void OnLateUpdate()
        {
            if (inited == false)
            {
                inited = true;
                initHeight = Owner.OwnerObject.Ref.Base.GetHeight();
                var myCoord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                rd = new Random(myCoord.X + myCoord.Y - myCoord.Z);
                maxDistance = maxDistance + rd.Next(-2, 2);
            }

            flyTime++;

            bool goNext = false;



            var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var distance = new CoordStruct(coord.X, coord.Y, Owner.OwnerObject.Ref.TargetCoords.Z).BigDistanceForm(Owner.OwnerObject.Ref.TargetCoords);
            if (!double.IsNaN(distance))
            {
                if (distance < maxDistance * Game.CellSize)
                {
                    if (flyTime > minFly)
                    {
                        goNext = true;
                    }
                }
            }

            var velocity = Owner.OwnerObject.Ref.Velocity;

            if (!goNext)
            {
                if (Owner.OwnerObject.Ref.Base.GetHeight() > initHeight - 1500)
                {
                    velocity.Z = -60;
                }

                Owner.OwnerObject.Ref.Velocity = velocity;
            }
            else
            {

                var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<AbstractClass>.Zero : Owner.OwnerObject.Ref.Owner.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<TechnoClass>.Zero : Owner.OwnerObject.Ref.Owner, 1, pPush, 100, false);
                bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());

                CoordStruct targetCoord;
                if (!Owner.OwnerObject.Ref.Target.IsNull)
                {
                    targetCoord = Owner.OwnerObject.Ref.Target.Ref.GetCoords();
                }
                else
                {
                    targetCoord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                }

                var technos = ObjectFinder.FindTechnosNear(targetCoord, 8 * Game.CellSize);

                var techno10 = technos.Where(x =>
                {
                    if (x.CastToTechno(out var px))
                    {
                        if (!Owner.OwnerObject.Ref.Owner.IsNull)
                        {
                            if (!px.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner.Ref.Owner))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }).OrderBy(x => x.Ref.Base.GetCoords().DistanceFrom(targetCoord)).Take(10).ToList();

                int targetCount = 0;

                if (techno10.Count() > 0)
                {
                    foreach (var pobj in techno10)
                    {
                        CreateBulletToTarget(pobj.Convert<AbstractClass>());
                        targetCount++;
                    }
                }

                if (targetCount < 10)
                {
                    foreach (var pobj in techno10)
                    {
                        if (targetCount >= 10)
                        {
                            break;
                        }
                        CreateBulletToTarget(pobj.Convert<AbstractClass>());
                        targetCount++;
                    }
                }


                if(targetCount < 10)
                {
                    var left = 10 - targetCount;
                    for (var i = 0; i < left; i++)
                    {
                        
                        if(MapClass.Instance.TryGetCellAt(targetCoord + new CoordStruct(rd.Next(-Game.CellSize * 6, Game.CellSize * 6), rd.Next(-Game.CellSize * 6, Game.CellSize * 6),0),out var pcell))
                        {
                            CreateBulletToTarget(pcell.Convert<AbstractClass>());
                        }
                    }
                }

                Owner.OwnerObject.Ref.Base.UnInit();
            }
        }

        private void CreateBulletToTarget(Pointer<AbstractClass> target)
        {
            var bullet = pWeapon.Ref.Projectile.Ref.CreateBullet(target, Owner.OwnerObject.Ref.Owner.IsNull ? Pointer<TechnoClass>.Zero : Owner.OwnerObject.Ref.Owner, pWeapon.Ref.Damage, pWeapon.Ref.Warhead, pWeapon.Ref.Speed, pWeapon.Ref.Warhead.Ref.Bright);
            bullet.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords(), new BulletVelocity(rd.Next(-200, 200), rd.Next(-200, 200), rd.Next(-200, 200)));
            bullet.Ref.SetTarget(target);
        }

    }


    [ScriptAlias(nameof(DF41BulletSpitScript))]
    [Serializable]
    public class DF41BulletSpitScript : BulletScriptable
    {
        public DF41BulletSpitScript(BulletExt owner) : base(owner)
        {
        }

        private bool inited = false;

        private int delay = 10;


        public override void OnLateUpdate()
        {
            if (delay-- <= 0)
            {
                delay = 10;
                var velocity = Owner.OwnerObject.Ref.Velocity;
                var rd = new Random((int)(velocity.X + velocity.Y + velocity.Z));
                Owner.OwnerRef.Velocity = new BulletVelocity(rd.Next(-200, 200), rd.Next(-200, 200), rd.Next(-200, 200));
            }
        }


    }


    [ScriptAlias(nameof(DFLauncherScript))]
    [Serializable]
    public class DFLauncherScript : TechnoScriptable
    {
        public DFLauncherScript(TechnoExt owner) : base(owner)
        {
        }

        private int delay = 3000;

        public override void OnUpdate()
        {
            if(Owner.OwnerObject.Ref.Ammo==0)
            {
                if (delay-- <= 0)
                {
                    delay = 3000;
                    Owner.OwnerObject.Ref.Ammo = 1;
                }
            }
           
        }
    }

}
