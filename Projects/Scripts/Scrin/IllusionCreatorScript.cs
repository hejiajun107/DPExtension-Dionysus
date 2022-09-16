using Extension.CW;
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
using static System.Net.Mime.MediaTypeNames;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    public class IllusionCreatorScript : TechnoScriptable
    {
        public IllusionCreatorScript(TechnoExt owner) : base(owner)
        {
        }

        private bool actived = false;

        private int delay = 1200;

        List<TechnoExt> targets = new List<TechnoExt>();

        List<TechnoExt> illusions = new List<TechnoExt>();


        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private static Pointer<WarheadTypeClass> inWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("IllusionInWh");

        private static Pointer<WarheadTypeClass> outWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("IllusionOutWh");

        public override void OnUpdate()
        {
            base.OnUpdate();

            var current = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var height = Owner.OwnerObject.Ref.Base.GetHeight();

            if (actived == false)
            {
                actived = true;

                if (Owner.OwnerObject.Ref.Owner.IsNull)
                    return;

                var cell = CellClass.Coord2Cell(current);

                var houseIndex = Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex;

                CellSpreadEnumerator enumerator = new CellSpreadEnumerator(5);
                var p2d = new Point2D(60, 60);

                //寻找附近的单位
                foreach (CellStruct offset in enumerator)
                {
                    CoordStruct where = CellClass.Cell2Coord(cell + offset, current.Z);


                    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                    {
                        if (pCell.IsNull)
                            continue;

                        var techno = pCell.Ref.FindTechnoNearestTo(p2d, false, Owner.OwnerObject);

                        if (TechnoExt.ExtMap.Find(techno) == null)
                        {
                            continue;
                        }

                        TechnoExt tref = default;

                        tref.Set(TechnoExt.ExtMap.Find(techno));

                        if (tref.TryGet(out TechnoExt ptechno))
                        {
                            if (ptechno.OwnerObject.Ref.Owner.IsNull)
                                continue;
                            if (houseIndex != ptechno.OwnerObject.Ref.Owner.Ref.ArrayIndex && !ptechno.OwnerObject.Ref.Owner.Ref.IsAlliedWith(houseIndex))
                                continue;
                            if (ptechno.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Unit)
                                continue;

                            var gext = ptechno.GameObject.GetComponent<TechnoGlobalExtension>();
                            if (gext == null)
                                continue;

                            if (!gext.Data.Copyable)
                                continue;


                            targets.Add(tref);
                        }

                    }
                }
            }
            else if (actived && delay > 0)
            {
                //根据目标放置镜像
                if (targets.Count > 0)
                {
                    var target = targets[0];
                    targets.RemoveAt(0);
                    if (target.TryGet(out var pTarget))
                    {
                        var health = pTarget.OwnerObject.Ref.Base.Health;
                        var selected = pTarget.OwnerObject.Ref.Base.IsSelected;

                        if (pTarget.OwnerObject.Ref.Owner.IsNull)
                            return;

                        var techno = pTarget.OwnerObject.Ref.Type.Ref.Base.CreateObject(pTarget.OwnerObject.Ref.Owner).Convert<TechnoClass>();
                        if (techno == null)
                            return;


                        if (techno.Ref.Base.Health > health)
                        {
                            techno.Ref.Base.Health = health;
                        }


                        //寻找空地
                        var location = pTarget.OwnerObject.Ref.Base.Base.GetCoords();

                        var cell = CellClass.Coord2Cell(location);

                        bool putted = false;


                        CellSpreadEnumerator enumerator = new CellSpreadEnumerator(8);
                        var p2d = new Point2D(60, 60);
                        foreach (CellStruct offset in enumerator)
                        {
                            CoordStruct where = CellClass.Cell2Coord(cell + offset, location.Z);

                            if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                            {
                                if (pCell.IsNull)
                                    continue;

                                Pointer<TechnoClass> ptargetTechno = pCell.Ref.FindTechnoNearestTo(p2d, false, Owner.OwnerObject);

                                if (TechnoExt.ExtMap.Find(ptargetTechno) == null)
                                {
                                    //找到空地
                                    var targetLocation = pCell.Ref.Base.GetCoords();
                                    if (techno.Ref.Base.Put(targetLocation, Direction.N))
                                    {
                                        putted = true;
                                        TechnoExt illusion = default;
                                        illusion.Set(TechnoExt.ExtMap.Find(techno));
                                        illusions.Add(illusion);

                                        var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, inWarhead, 100, false);
                                        bullet.Ref.DetonateAndUnInit(targetLocation);

                                        if (selected)
                                        {
                                            if (techno.Ref.Base.CanBeSelected())
                                            {
                                                techno.Ref.Base.Select();
                                            }
                                        }


                                        var mission = techno.Convert<MissionClass>();
                                        if (Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                                        {
                                            mission.Ref.QueueMission(Mission.Guard, false);
                                        }
                                        else
                                        {
                                            mission.Ref.ForceMission(Mission.Hunt);
                                        }
                                    }
                                   
                                    break;
                                }
                            }
                        }

                        if (putted == false)
                        {
                            techno.Ref.Base.UnInit();
                        }
                    }
                }
            }


            if (delay-- <= 0)
            {
                if (illusions.Count > 0)
                {
                    var illusion = illusions[0];
                    illusions.RemoveAt(0);
                    if (illusion.TryGet(out var pTarget))
                    {
                        var targetLocation = pTarget.OwnerObject.Ref.Base.Base.GetCoords();
                        var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, outWarhead, 100, false);
                        bullet.Ref.DetonateAndUnInit(targetLocation);
                        pTarget.OwnerObject.Ref.Base.Remove();
                        pTarget.OwnerObject.Ref.Base.UnInit();
                    }
                }
                else
                {
                    Owner.OwnerObject.Ref.Base.UnInit();
                }
            }
        }
    }
}
