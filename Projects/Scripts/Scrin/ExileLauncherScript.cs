using Extension.CW;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(ExileLauncherScript))]
    public class ExileLauncherScript : TechnoScriptable
    {
        public ExileLauncherScript(TechnoExt owner) : base(owner)
        {
        }

        private int delay = 900;

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> moneyWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ExileMoneyWh");

        static Pointer<AnimTypeClass> illusionAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("IllusionEffect");

        static Pointer<WarheadTypeClass> peaceWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("PeaceKillWh");


        public override void OnUpdate()
        {

            if (Owner.OwnerObject.Ref.Passengers.NumPassengers <= 0)
            {
                delay = 900;
                return;
            }

            if (delay-- <= 0)
            {
                var passenger = Owner.OwnerObject.Ref.Passengers.GetFirstPassenger();
                if (!passenger.IsNull)
                {

                    if (passenger.Convert<AbstractClass>().CastToTechno(out Pointer<TechnoClass> pPassenger))
                    {
                        if (!pPassenger.Ref.Owner.IsNull)
                        {
                            if (!pPassenger.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                            {
                                var technoExt = TechnoExt.ExtMap.Find(pPassenger);
                                if (technoExt != null)
                                {
                                    var gext = technoExt.GameObject.GetComponent<TechnoGlobalExtension>();
                                    if (gext != null)
                                    {
                                        if (gext.Data.Copyable != false)
                                        {
                                            var techno = pPassenger.Ref.Type.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();
                                            if (techno != null)
                                            {
                                                CellSpreadEnumerator enumerator = new CellSpreadEnumerator(5);
                                                var p2d = new Point2D(60, 60);

                                                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                                                var cell = CellClass.Coord2Cell(location);

                                                bool putted = false;

                                                foreach (CellStruct offset in enumerator)
                                                {
                                                    CoordStruct where = CellClass.Cell2Coord(cell + offset, location.Z);

                                                    if (MapClass.Instance.TryGetCellAt(where, out Pointer<CellClass> pCell))
                                                    {
                                                        if (pCell.IsNull)
                                                            continue;

                                                        if (pCell.Ref.FirstObject.IsNull)
                                                        {
                                                            var emptyCell = pCell;
                                                            var putLocation = pCell.Ref.Base.GetCoords();
                                                            putted = techno.Ref.Base.Put(putLocation, Direction.S);

                                                            YRMemory.Create<AnimClass>(illusionAnim, putLocation);

                                                            break;
                                                        }
                                                    }
                                                }

                                                if (!putted)
                                                {
                                                    techno.Ref.Base.UnInit();
                                                }
                                            }
                                        }
                                    }





                                }
                            }


                            var ext = TechnoExt.ExtMap.Find(pPassenger);
                            bool destoryed = false;
                            if (ext != null)
                            {
                                var gext = ext.GameObject.GetComponent<TechnoGlobalExtension>();
                                if (gext != null)
                                {
                                    if (gext.Data.IsHarvester)
                                    {
                                        pPassenger.Ref.Base.TakeDamage(5000, peaceWarhead, true);
                                    }
                                }

                            }
                            if (!destoryed)
                            {
                                pPassenger.Ref.Base.UnInit();
                            }

                            var moneyBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, moneyWarhead, 100, true);
                            moneyBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 900));

                            delay = 900;
                        }
                    }
                }


                base.OnUpdate();
            }
        }
    }
}
