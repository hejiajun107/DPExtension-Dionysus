using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    public class ScarabTargetScript:TechnoScriptable
    {
        public ScarabTargetScript(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<WeaponTypeClass> jumpWeapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ScarabJumpWeapon");

        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private static Pointer<WarheadTypeClass> pJumpWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ScrabJumpExpWh");

        private static Pointer<WarheadTypeClass> pDebuffWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ScrabDebuffWh");

        private bool inited { get; set; }

        private int delay = 100;

        public override void OnUpdate()
        {
            base.OnUpdate();


            if (!inited)
            {
                inited = true;
                //跳跃的目标点
                var target = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                if(Owner.OwnerObject.Ref.Owner == null)
                {
                    return;
                }

                var houseIndex = Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex;

                //寻找附近的空点
                var currentCell = CellClass.Coord2Cell(target);

                CellSpreadEnumerator enumeratorTarget = new CellSpreadEnumerator(5);

                List<Pointer<CellClass>> emptyCells = new List<Pointer<CellClass>>();

                foreach (CellStruct offset in enumeratorTarget)
                {
                    CoordStruct where = CellClass.Cell2Coord(currentCell + offset, target.Z);

                    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                    {
                        if (pCell.IsNull)
                        {
                            continue;
                        }

                        Point2D p2d = new Point2D(60, 60);
                        Pointer<TechnoClass> ptargetTechno = pCell.Ref.FindTechnoNearestTo(p2d, false, Owner.OwnerObject);

                        if (TechnoExt.ExtMap.Find(ptargetTechno) == null)
                        {
                            emptyCells.Add(pCell);
                            continue;
                        }
                    }
                }

                int indexTarget = 0;

                if (emptyCells.Count() > 0)
                {
                    CellSpreadEnumerator enumeratorSource = new CellSpreadEnumerator(24);
                    foreach (CellStruct offset in enumeratorSource)
                    {
                        CoordStruct where = CellClass.Cell2Coord(currentCell + offset, target.Z);

                        if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                        {
                            if (pCell.IsNull)
                            {
                                continue;
                            }

                            Point2D p2d = new Point2D(60, 60);
                            Pointer<TechnoClass> ptargetTechno = pCell.Ref.FindTechnoNearestTo(p2d, false, Owner.OwnerObject);

                            var technoExt = TechnoExt.ExtMap.Find(ptargetTechno);
                            if (technoExt == null)
                            {
                                continue;
                            }

                            if (technoExt.OwnerObject.Ref.Owner.IsNull)
                            {
                                continue;
                            }

                            if(technoExt.OwnerObject.Ref.Owner.Ref.ArrayIndex == houseIndex && (technoExt.OwnerObject.Ref.Type.Ref.Base.Base.ID == "SCARAB" || technoExt.OwnerObject.Ref.Type.Ref.Base.Base.ID == "SCARAB4AI"))
                            {
                                if(indexTarget+1>=emptyCells.Count())
                                {
                                    indexTarget = 0;
                                }
                                JumpTo(technoExt.OwnerObject, emptyCells[indexTarget].Convert<AbstractClass>());
                                indexTarget++;
                            }
                        }
                    }

                }

            }


            if (delay--<0)
            {
                Owner.OwnerObject.Ref.Base.UnInit();
                return;
            }


        }


        private void JumpTo(Pointer<TechnoClass> techno, Pointer<AbstractClass> target)
        {


            var mission = techno.Convert<MissionClass>();

            //techno.Ref.SetTarget(default);
            //techno.Ref.SetDestination(default, false);

            //mission.Ref.QueueMission(Mission.Stop, false);
            //mission.Ref.NextMission();

            techno.Ref.Ammo = 1;
            techno.Ref.SetTarget(target);
            mission.Ref.ForceMission(Mission.Attack);

            if (techno.CastToFoot(out Pointer<FootClass> pfoot))
            {
                var dir = GameUtil.Point2Dir(techno.Ref.Base.Base.GetCoords(), target.Ref.GetCoords());
                var tdir = new DirStruct(16,(short)DirStruct.TranslateFixedPoint(3, 16, (uint)dir));
                techno.Ref.Facing.set(tdir);
                //pfoot.Ref.Locomotor.Push(tdir);
                //pfoot.Ref.Locomotor.Push(tdir);

                //pfoot.Ref.MoveTo(techno.Ref.Base.Base.GetCoords() + verctor);

            }


            Pointer<BulletClass> pjumpBullet = pInviso.Ref.CreateBullet(techno.Convert<AbstractClass>(), techno, 1, pJumpWarhead, 100, true);
            pjumpBullet.Ref.DetonateAndUnInit(techno.Ref.Base.Base.GetCoords());

            Pointer<BulletClass> pDebuffBullet = pInviso.Ref.CreateBullet(techno.Convert<AbstractClass>(), techno, 1, pDebuffWh, 100, true);
            pDebuffBullet.Ref.DetonateAndUnInit(techno.Ref.Base.Base.GetCoords());
            

            Pointer<BulletClass> bullet = jumpWeapon.Ref.Projectile.Ref.CreateBullet(target, techno, jumpWeapon.Ref.Damage, jumpWeapon.Ref.Warhead, 60, true);
            bullet.Ref.MoveTo(techno.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 50), new BulletVelocity(0, 0, 800));
            bullet.Ref.SetTarget(target);


        }
    }
}
