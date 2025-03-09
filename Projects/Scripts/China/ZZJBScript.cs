using DynamicPatcher;
using Extension.Coroutines;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.China
{
    [ScriptAlias(nameof(ZZJBScript))]
    [Serializable]
    public class ZZJBScript : TechnoScriptable
    {
        public ZZJBScript(TechnoExt owner) : base(owner)
        {
        }

        public TechnoExt Flying;

        private bool locked = false;


        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if(mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);

                if(Flying.IsNullOrExpired() && !locked)
                {
                    if(Owner.TryGetHouseGlobalExtension(out var houseExt))
                    {
                        if (houseExt.UAVCount > 0) {
							locked = true;
                            houseExt.UAVCount--;
							var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
							var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BuyDJIFreeWH"), 100, false);
							pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
							Owner.GameObject.StartCoroutine(CallDJI());
						}
						else if (Owner.OwnerObject.Ref.Owner.Ref.Available_Money() > 400)
						{
							locked = true;
							var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
							var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BuyDJIWH"), 100, false);
							pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
							Owner.GameObject.StartCoroutine(CallDJI());
						}
					}
                }
            }
          

            base.OnUpdate();
        }

   
        IEnumerator CallDJI()
        {
            yield return new WaitForFrames(300);
            if (Flying.IsNullOrExpired())
            {
                var type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("CNDJI");
                var techno = type.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();
                if (TechnoPlacer.PlaceTechnoNear(techno, CellClass.Coord2Cell(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 500))))
                {
                    Flying = TechnoExt.ExtMap.Find(techno);
                    Flying.GameObject.CreateScriptComponent(nameof(CNDJIScript), CNDJIScript.UniqueId, nameof(CNDJIScript), Flying, Owner);
                }
            }
            locked = false;
        }

        public override void OnRemove()
        {
            //if (!Flying.IsNullOrExpired())
            //{
            //    Flying.OwnerObject.Ref.Base.Remove();
            //}
        }

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {

        }


    }

    [ScriptAlias(nameof(CNDJIScript))]
    [Serializable]
    public class CNDJIScript : TechnoScriptable
    {
        public TechnoExt Master;

        public static int UniqueId = 2024090701;

        private int pointIdx = 0;

        internal static readonly CoordStruct[] AroundPoints = new[] {
            new CoordStruct(350, 350, 0),
            new CoordStruct(350, -350, 0),
            new CoordStruct(-350, -350, 0),
            new CoordStruct(-350, 350, 0),
        };

        public CNDJIScript(TechnoExt owner, TechnoExt master) : base(owner)
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

                if(Owner.OwnerObject.Ref.Target.IsNull)
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
                    if (Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(follower.Ref.Base.Base.GetCoords()) > 8 * Game.CellSize)
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
                if (follower.Ref.Target.Ref.GetCoords().BigDistanceForm(follower.Ref.Base.Base.GetCoords()) <= 15 * Game.CellSize)
                {
                    var pfoot = Owner.OwnerObject.Convert<FootClass>();
                    pfoot.Ref.Base.Attack(follower.Ref.Target);
                }
                else
                {
                    isIdle = true;
                }
            }

            if (Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(follower.Ref.Base.Base.GetCoords()) > 15 * Game.CellSize)
            {
                isIdle = true;
            }

            if (isIdle)
            {
                Owner.OwnerObject.Ref.SetTarget(Pointer<AbstractClass>.Zero);
                var pfoot = Owner.OwnerObject.Convert<FootClass>();
                pfoot.Ref.MoveTo(GetDestination(follower));
            }
        }

        public Pointer<TechnoClass> FindFollower()
        {
            var current = Master.OwnerObject;
            while(current.Ref.Transporter.IsNotNull)
            {
                current = current.Ref.Transporter;
            }
            return current;
        }

        public CoordStruct GetDestination(Pointer<TechnoClass> follower)
        {
            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var target = follower.Ref.Base.Base.GetCoords() + AroundPoints[pointIdx];
            if (location.BigDistanceForm(new CoordStruct(target.X,target.Y, location.Z)) >= 256)
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

            List<Pointer<ObjectClass>> list = ObjectFinder.FindTechnosNear(coord, 5 * Game.CellSize)
                .Where(x=>!x.Ref.Base.GetOwningHouse().Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner) && !zhongli.Contains(x.Ref.Base.GetOwningHouse().Ref.Type.Ref.Base.ID))
                .Where(x=> Owner.OwnerObject.Ref.CanAttack(x))
                .OrderBy(x=>x.Ref.Base.GetCoords().DistanceFrom(coord))
                .ToList();

            if (list.Count > 0) 
            {
                return list.FirstOrDefault().Convert<AbstractClass>();
            }

            return Pointer<AbstractClass>.Zero;
        }
    }
}
