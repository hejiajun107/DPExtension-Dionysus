using Extension.Decorators;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                                if(pTargetRef.OwnerObject.Ref.Owner.Ref.ArrayIndex == houseIdx || house.IsAlliedWith(pTargetRef.OwnerObject.Ref.Owner))
                                {
                                    if (pTargetRef.GameObject.GetComponent(ParticleCannonUnitDecorator.ID) == null)
                                    {
                                        pTargetRef.GameObject.CreateScriptComponent(nameof(ParticleCannonUnitDecorator),ParticleCannonUnitDecorator.ID, "ParticleCannonUnitDecorator Decorator", pTargetRef);
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


            public override void OnUpdate()
            {
                if (Self.IsNullOrExpired() || --lifeTime <= 0)
                {
                    DetachFromParent();
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
                if(pAttackingHouse.IsNull)
                {
                    return;
                }

                if(Self.IsNullOrExpired())
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
                    if(pAttacker.CastToTechno(out var pAttackerTechno))
                    {
                        FireTo(pAttacker.Ref.Base.GetCoords());
                    }
                }
            }


            private void FireTo(CoordStruct target)
            {
                var owner = Self;
                if(owner!=null)
                {
                    var ntarget = new CoordStruct(target.X , target.Y, target.Z);

                    //for (var angle = 0; angle < 360; angle += 60)
                    //{
                    //    var pos = new CoordStruct(ntarget.X + (int)(15 * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), ntarget.Y + (int)(15 * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), ntarget.Z);
                    //    Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(pos + new CoordStruct(0, 0, 3000), pos, innerColor, outerColor, outerSpread, 8);
                    //    pLaser.Ref.Thickness = 10;
                    //    pLaser.Ref.IsHouseColor = false;
                    //}

                    Pointer<BulletClass> pBean = pBulletType.Ref.CreateBullet(owner.OwnerObject.Convert<AbstractClass>(), owner.OwnerObject, 1, beanWarhead, 100, false);
                    pBean.Ref.DetonateAndUnInit(ntarget);

                    int damage = 160;
                    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(owner.OwnerObject.Convert<AbstractClass>(), owner.OwnerObject, damage, blastWarhead, 100, true);
                    pBullet.Ref.DetonateAndUnInit(ntarget);
                }
            }

        }
    }
}
