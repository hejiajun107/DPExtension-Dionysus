using Extension.Ext;
using Extension.Script;
using PatcherYRpp.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.Utilities;
using Extension.Coroutines;
using System.Collections;
using DynamicPatcher;

namespace Scripts.China
{
    [ScriptAlias(nameof(J20SScript))]
    [Serializable]
    public class J20SScript : TechnoScriptable
    {
        public TechnoExt Flying1;
        public TechnoExt Flying2;

        private int flying1Respawn = 0;
        private int flying2Respawn = 0;

        public J20SScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnUpdate()
        {
            if (flying1Respawn > 0)
            {
                flying1Respawn--;
            }

            if (flying2Respawn > 0)
            {
                flying2Respawn--;
            }

            if (flying1Respawn <= 0)
            {
                if (Flying1.IsNullOrExpired())
                {
                    flying1Respawn = 500;
                    Flying1 = CallFeiHong(1);
                }
            }

            if (flying2Respawn <= 0)
            {
                if (Flying2.IsNullOrExpired())
                {
                    flying2Respawn = 500;
                    Flying2 = CallFeiHong(2);
                }
            }

            base.OnUpdate();
        }

        public TechnoExt CallFeiHong(int no)
        {
            var type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("FH97");
            var techno = type.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();
            if (TechnoPlacer.PlaceTechnoNear(techno, CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(no == 1 ? 500 : -500, no == 1 ? 500 : -500, 500))))
            {
                var ext = TechnoExt.ExtMap.Find(techno);
                ext.GameObject.CreateScriptComponent(nameof(FeiHongScript), FeiHongScript.UniqueId, nameof(FeiHongScript), ext, Owner);
                return ext;
            }
            else { return null; }
        }

    }

    [ScriptAlias(nameof(FeiHongScript))]
    [Serializable]
    public class FeiHongScript : TechnoScriptable
    {
        public FeiHongScript(TechnoExt owner) : base(owner)
        {
        }

        public TechnoExt Master;

        public static int UniqueId = 2024120819;

        private int pointIdx = 0;

        internal static readonly CoordStruct[] AroundPoints = new[] {
            new CoordStruct(500, 500, 0),
            new CoordStruct(500, -500, 0),
            new CoordStruct(-500, -500, 0),
            new CoordStruct(-500, 500, 0),
        };

        public FeiHongScript(TechnoExt owner, TechnoExt master) : base(owner)
        {
            Master = master;
        }

        public override void OnUpdate()
        {
            if (Master.IsNullOrExpired())
            {
                Owner.OwnerObject.Ref.Base.TakeDamage(1000, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Super"), true);
                return;
            }

            bool isIdle = false;

            var follower = FindFollower();

            if (follower.Ref.Target.IsNull)
            {
                var pick = FindTarget(follower.Ref.Base.Base.GetCoords());

                if (Owner.OwnerObject.Ref.Target.IsNull)
                {
                    if (pick.IsNull)
                    {
                        isIdle = true;
                    }
                    else
                    {
                        var pfoot = Owner.OwnerObject.Convert<FootClass>();
                        pfoot.Ref.Base.Attack(pick);
                    }
                }
                else
                {
                    if (Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(follower.Ref.Base.Base.GetCoords()) > 15 * Game.CellSize)
                    {
                        if (pick.IsNull)
                        {
                            isIdle = true;
                        }
                        else
                        {
                            var pfoot = Owner.OwnerObject.Convert<FootClass>();
                            pfoot.Ref.Base.Attack(pick);
                        }
                    }
                }
            }
            else
            {
                if (follower.Ref.Target.Ref.GetCoords().BigDistanceForm(follower.Ref.Base.Base.GetCoords()) <= 20 * Game.CellSize)
                {
                    var pfoot = Owner.OwnerObject.Convert<FootClass>();
                    pfoot.Ref.Base.Attack(follower.Ref.Target);
                }
                else
                {
                    isIdle = true;
                }
            }

            if (Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(follower.Ref.Base.Base.GetCoords()) > 20 * Game.CellSize)
            {
                isIdle = true;
            }

            if (isIdle)
            {
                Owner.OwnerObject.Ref.SetTarget(Pointer<AbstractClass>.Zero);
                var pfoot = Owner.OwnerObject.Convert<FootClass>();
                pfoot.Ref.MoveTo(GetDestination(follower));
            }
            else
            {
                if(Owner.OwnerObject.Ref.Ammo == 0)
                {
                    if(!Owner.OwnerObject.Ref.Target.IsNull)
                    {
                        if(Owner.OwnerObject.Ref.Target.CastToTechno(out var ptechno))
                        {
                            var pfoot = Owner.OwnerObject.Convert<FootClass>();
                            pfoot.Ref.MoveTo(GetDestination(ptechno));
                        }
                    }
                }
            }
        }

        public Pointer<TechnoClass> FindFollower()
        {
            var current = Master.OwnerObject;
            while (current.Ref.Transporter.IsNotNull)
            {
                current = current.Ref.Transporter;
            }
            return current;
        }

        public CoordStruct GetDestination(Pointer<TechnoClass> follower)
        {
            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var target = follower.Ref.Base.Base.GetCoords() + AroundPoints[pointIdx];
            if (location.BigDistanceForm(new CoordStruct(target.X, target.Y, location.Z)) >= 256)
            {
                return target;
            }
            else
            {
                pointIdx++;
                if (pointIdx > AroundPoints.Length - 1)
                {
                    pointIdx = 0;
                }
            }
            return follower.Ref.Base.Base.GetCoords() + AroundPoints[pointIdx];
        }

        private int findRate = 10;

        public Pointer<AbstractClass> FindTarget(CoordStruct coord)
        {
            if (findRate-- <= 0)
            {
                findRate = 10;
            }

            var zhongli = new[]
            {
                "Special",
                "Neutral"
            };

            List<Pointer<ObjectClass>> list = ObjectFinder.FindTechnosNear(coord, 8 * Game.CellSize)
                .Where(x => !x.Ref.Base.GetOwningHouse().Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner) && !zhongli.Contains(x.Ref.Base.GetOwningHouse().Ref.Type.Ref.Base.ID))
                .Where(x => Owner.OwnerObject.Ref.CanAttack(x))
                .OrderBy(x => x.Ref.Base.GetCoords().DistanceFrom(coord))
                .ToList();

            if (list.Count > 0)
            {
                return list.FirstOrDefault().Convert<AbstractClass>();
            }

            return Pointer<AbstractClass>.Zero;
        }
    }
}
