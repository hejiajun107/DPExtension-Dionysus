using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(ParticleCannonBulletScript))]

    public class ParticleCannonBulletScript : BulletScriptable
    {
        public ParticleCannonBulletScript(BulletExt owner) : base(owner) { }

        public bool isActived = false;

        TechnoExt pTargetRef;

        //750

        public override void OnUpdate()
        {
            if (isActived == false)
            {
                var location = Owner.OwnerObject.Ref.Target.Ref.GetCoords();
                var currentCell = CellClass.Coord2Cell(location);
                var launcher = Owner.OwnerObject.Ref.Owner;
                var house = Owner.OwnerObject.Ref.Owner.Ref.Owner.Ref;
                var houseIdx = Owner.OwnerObject.Ref.Owner.Ref.Owner.Ref.ArrayIndex;

                CellSpreadEnumerator enumerator = new CellSpreadEnumerator(3);

                foreach (CellStruct offset in enumerator)
                {
                    CoordStruct where = CellClass.Cell2Coord(currentCell + offset, location.Z);

                    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                    {
                        Point2D p2d = new Point2D(60, 60);
                        Pointer<TechnoClass> target = pCell.Ref.FindTechnoNearestTo(p2d, false, launcher);

                        pTargetRef = (TechnoExt.ExtMap.Find(target));
                        if (!pTargetRef.IsNullOrExpired())
                        {
                            if (pTargetRef.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Building && pTargetRef.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.BuildingType)
                            {
                                if (pTargetRef.OwnerObject.Ref.Owner.Ref.ArrayIndex == houseIdx || house.IsAlliedWith(pTargetRef.OwnerObject.Ref.Owner))
                                {
                                    if (pTargetRef.GameObject.GetComponent(ParticleCannonUnitDecorator.ID) == null)
                                    {
                                        pTargetRef.GameObject.CreateScriptComponent(nameof(ParticleCannonUnitDecorator), ParticleCannonUnitDecorator.ID, "ParticleCannonUnitDecorator Decorator", pTargetRef);
                                    }
                                }
                            }
                        }
                    }


                }
            }
        }



        [Serializable]
        [ScriptAlias(nameof(ParticleCannonUnitDecorator))]

        public class ParticleCannonUnitDecorator : TechnoScriptable
        {
            public static int ID = 414002;
            public ParticleCannonUnitDecorator(TechnoExt self) : base(self)
            {
                Self = self;
                pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
            }

            TechnoExt Self;

            private int lifeTime = 800;

            private int attackRof = 0;

            private int attackedRof = 0;

            private int coolDown = 100;

            //光束颜色
            static ColorStruct innerColor = new ColorStruct(34, 177, 76);
            static ColorStruct outerColor = new ColorStruct(34, 177, 76);
            static ColorStruct outerSpread = new ColorStruct(34, 177, 76);

            static Pointer<WarheadTypeClass> blastWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ParticleCannonBlastGreen");
            static Pointer<WarheadTypeClass> beanWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ParticleCannonBlastBean");

            static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        
            private SwizzleablePointer<AnimClass> pAnim;

            static Pointer<WeaponTypeClass> laserWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ParticleLaser");

            static Pointer<AnimTypeClass> ballAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("PCBALL");

            static Pointer<AnimTypeClass> fireAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("PartBallFire");


            private int angle = 0;
            private int height = 150;
            private int radius = 256;
            private int stop = 0;


            public override void OnUpdate()
            {
                if (Self.IsNullOrExpired() || --lifeTime <= 0)
                {
                    DetachFromParent();
                    KillAnim();
                    return;
                }

                if (attackRof > 0)
                {
                    attackRof--;
                }

                if (attackedRof > 0)
                {
                    attackedRof--;
                }

                if (pAnim.IsNull)
                {
                    CreateAnim();
                }

                if(stop>0)
                {
                    stop--;
                }
                else
                {
                    angle += 2;
                    if (angle > 360)
                    {
                        angle = angle - 360;
                    }
                }
        

                var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var target = new CoordStruct(coord.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), coord.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), coord.Z + height);

                pAnim.Ref.Base.SetLocation(target);

            }


            public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
            {
                if (attackRof <= 0)
                {
                    attackRof = coolDown;
                    FireTo(pTarget.Ref.GetCoords());
                }
            }

            public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
            {
                if (pAttackingHouse.IsNull)
                {
                    return;
                }

                if (Self.IsNullOrExpired())
                {
                    return;
                }

                if (pAttackingHouse.Ref.ArrayIndex == Self.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                {
                    return;
                }
                if (attackedRof <= 0)
                {
                    attackedRof = coolDown;
                    if (pAttacker.CastToTechno(out var pAttackerTechno))
                    {
                        FireTo(pAttacker.Ref.Base.GetCoords());
                    }
                }
            }

            public override void OnRemove()
            {
                KillAnim();
            }

            public override void OnDestroy()
            {
                KillAnim();
            }

            private void FireTo(CoordStruct target)
            {
                var owner = Self;
                if (owner != null)
                {
                    int damage = 160;

                    var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                    var distance = coord.DistanceFrom(target);
                    if(double.IsNaN(distance) || distance > 256 * 15)
                    {
                        damage = 80;
                    }else if (distance > 256 * 15)
                    {
                        damage = 100;
                    }else if( distance > 256 * 10)
                    {
                        damage = 120;
                    }

                    var ntarget = new CoordStruct(target.X, target.Y, target.Z);

                    stop = 10;

                    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(owner.OwnerObject.Convert<AbstractClass>(), owner.OwnerObject, damage, blastWarhead, 100, true);
                    var start = new CoordStruct(coord.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), coord.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), coord.Z + height);

                    YRMemory.Create<AnimClass>(fireAnim, start);

                    pBullet.Ref.Base.SetLocation(target);
                    Owner.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, laserWeapon, start);
                    pBullet.Ref.DetonateAndUnInit(target);
                }
            }

            private void CreateAnim()
            {
                if (!pAnim.IsNull)
                {
                    KillAnim();
                }

                var anim = YRMemory.Create<AnimClass>(ballAnim, Owner.OwnerObject.Ref.Base.Base.GetCoords());
                pAnim.Pointer = anim;
            }

            private void KillAnim()
            {
                if (!pAnim.IsNull)
                {
                    pAnim.Ref.TimeToDie = true;
                    pAnim.Ref.Base.UnInit();
                    pAnim.Pointer = IntPtr.Zero;
                }
            }

        }
    }
}
