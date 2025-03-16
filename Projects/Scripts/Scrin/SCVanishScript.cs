using Extension.Ext;
using Extension.Ext4CW;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Scrin
{
	[ScriptAlias(nameof(SCVanishScript))]
	[Serializable]
	public class SCVanishScript : TechnoScriptable
	{
		public SCVanishScript(TechnoExt owner) : base(owner)
		{
		}

		private int supportDelay = 60;

		public bool CanSupport()
		{
			return supportDelay <= 0;
		}

        public override void Awake()
        {
			if(Owner.TryGetHouseGlobalExtension(out var houseExt))
			{
				houseExt.ScrinVanish.Add(Owner);
            }
        }

        public override void OnUpdate()
        {
            if (supportDelay > 0)
            {
                supportDelay--;
            }
        }

        public override void OnDestroy()
        {
            if (Owner.TryGetHouseGlobalExtension(out var houseExt))
            {
                houseExt.ScrinVanish.Remove(Owner);
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
		{
			if (weaponIndex == 0) 
			{
			   var pWp = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SCVanSupport");

                if (Owner.TryGetHouseGlobalExtension(out var houseExt))
                {
					int extraDamage = 0;

                    var myCoord = ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, new CoordStruct(60, 0, 100), false);
                    var target = pTarget.Ref.GetCoords();
                    var tcoord = new CoordStruct((target.X + myCoord.X) / 2, (target.Y + myCoord.Y) / 2, (target.Z + myCoord.Z) / 2);

                    foreach (var vanishExt in houseExt.ScrinVanish)
					{
						if (vanishExt.IsNullOrExpired())
							continue;

						if (vanishExt == Owner)
							continue;

						if(vanishExt.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords()) > 4 * Game.CellSize)
							continue;

                        var dir = GameUtil.Point2Dir(vanishExt.OwnerObject.Ref.Base.Base.GetCoords(), pTarget.Ref.GetCoords());
                        var face = GameUtil.Facing2Dir(vanishExt.OwnerObject.Ref.Facing);
                        var diff = Math.Abs((int)face - (int)dir);

                        if (diff > 4)
                            diff = 8 - diff;

                        if (Math.Abs(diff) >= 2)
							continue;

						var component = vanishExt.GameObject.GetComponent<SCVanishScript>();

						if (component is null)
							continue;

						if (!component.CanSupport())
							continue;


						
						component.Attack(tcoord);

                        extraDamage += vanishExt.OwnerObject.Ref.Veterancy.IsElite() ? pWp.Ref.Damage * 2 : pWp.Ref.Damage;
                    }


                    if (extraDamage > 0)
					{
                        var bullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(pTarget, Owner.OwnerObject, extraDamage, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SCVanExtWH"), 100, false);
                        bullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
						YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("BlazeMiniX"), tcoord);
                    }
                }
            }

			if (weaponIndex != 1)
			{
				return;
			}

			if (Owner.OwnerObject.Ref.Base.InLimbo)
			{
				return;
			}

			if (pTarget.CastToTechno(out var ptechno))
			{
                if (!ptechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                {
					return;
                }
                TrySetLocation(ptechno, pTarget.Ref.GetCoords());
			}
		}

		public void Attack(CoordStruct target)
		{
			supportDelay = 60;
			var bullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(),Pointer<TechnoClass>.Zero, 0,WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("Special"),100,false);
            bullet.Ref.Base.SetLocation(target);
            Owner.OwnerObject.Ref.CreateLaser(bullet.Convert<ObjectClass>(), 0, WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SCVanSupport"), ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject,new CoordStruct(60, 0, 100),false));
            bullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
        }

        private bool TrySetLocation(Pointer<TechnoClass> techno, CoordStruct location)
		{
			if (!Owner.OwnerObject.Ref.Owner.IsNull)
			{
				var pTechno = Owner.OwnerObject;
				var mission = pTechno.Convert<MissionClass>();
				mission.Ref.ForceMission(Mission.Stop);
				var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();

				//位置
				if (pTechno.CastToFoot(out Pointer<FootClass> pfoot))
				{
					if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
					{
						//源动画
						var source = pTechno.Ref.Base.Base.GetCoords();

						YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("QGROCX03"), source);

						pfoot.Ref.Locomotor.Mark_All_Occupation_Bits(0);
						pfoot.Ref.Locomotor.Force_Track(-1, source);
						pTechno.Ref.Base.UnmarkAllOccupationBits(source);
						//var cLocal = pCell.Ref.Base.GetCoords();
						//var pLocal = new CoordStruct(cLocal.X, cLocal.Y, location.Z);
						pTechno.Ref.Base.SetLocation(location);
						pTechno.Ref.Base.UnmarkAllOccupationBits(location);

						YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("QGROCX03"), location);

						pTechno.Ref.Base.Scatter(location, true, true);


					}
				}

				return true;
			}


			return false;
		}
	}
}
