﻿using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(PLAScript))]
    public class PLAScript : TechnoScriptable
    {
        public PLAScript(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<TechnoTypeClass> originType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("E9");
        private static Pointer<TechnoTypeClass> targetType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("GATP2");

        public bool inited = false;
		public int AmmoType { get; set; } = 1;

		public override void Awake()
		{
			AmmoType = MathEx.Random.Next(100) > 50 ? 1 : 2;
		}


		public override void OnUpdate()
        {
            if (!inited)
            {
                inited = true;

                if (Owner.OwnerObject.Ref.Owner.IsNull)
                    return;

                if (Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                {
                    if (Owner.OwnerObject.Ref.Type == originType)
                    {
                        ref DynamicVectorClass<Pointer<BuildingClass>> buildings = ref Owner.OwnerObject.Ref.Owner.Ref.Buildings;

                        bool hasBuilding = false;

                        for (int i = buildings.Count - 1; i >= 0; i--)
                        {
                            Pointer<BuildingClass> pBuilding = buildings.Get(i);

                            if (pBuilding.Ref.Type.Ref.Base.Base.Base.ID == "ZGBGJD" && pBuilding.Ref.Base.Base.IsOnMap == true && !pBuilding.Ref.Base.Base.InLimbo)
                            {
                                hasBuilding = true;
                                break;
                            }
                        }

                        if (hasBuilding)
                        {
                            Owner.OwnerObject.Convert<InfantryClass>().Ref.Type = targetType.Convert<InfantryTypeClass>();
                        }
                    }
                }
            }
        }


    }

	[Serializable]
	[ScriptAlias(nameof(PLAUltraBulletScript))]
	public class PLAUltraBulletScript : BulletScriptable
	{
		public PLAUltraBulletScript(BulletExt owner) : base(owner)
		{
		}

        private bool inited = false;

		public override void OnUpdate()
		{
            if (inited) {
                return;
            }

            inited = true;

            var ammoType = 0;

            if(Owner.OwnerObject.Ref.Owner.IsNotNull)
            {
                var technoExt = TechnoExt.ExtMap.Find(Owner.OwnerObject.Ref.Owner);
                if(!technoExt.IsNullOrExpired())
                {
                    var plaScript = technoExt.GameObject.GetComponent<PLAScript>();
                    
					if (plaScript != null)
                    {
                        ammoType = plaScript.AmmoType;
                    }
                }
            }


            if (ammoType == 1)
            {
				Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(Owner.OwnerObject.Ref.SourceCoords, Owner.OwnerObject.Ref.TargetCoords, new ColorStruct(255, 0, 0), new ColorStruct(0, 0, 0), new ColorStruct(0, 0, 0), 1);
				var pb = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("GATPFlameAE"), 100, true);
				pb.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
			}
			else if (ammoType == 2)
            {
				Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(Owner.OwnerObject.Ref.SourceCoords, Owner.OwnerObject.Ref.TargetCoords, new ColorStruct(0, 0, 255), new ColorStruct(0, 0, 0), new ColorStruct(0, 0, 0), 1);
				var pb = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject.Ref.Owner, 1, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("GATPBoltAE"), 100, true);
				pb.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
			}
			else
            {
				Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(Owner.OwnerObject.Ref.SourceCoords, Owner.OwnerObject.Ref.TargetCoords, new ColorStruct(200, 200, 160), new ColorStruct(0, 0, 0), new ColorStruct(0, 0, 0), 1);
			}


			base.OnUpdate();
		}
	}




	[Serializable]
	[ScriptAlias(nameof(PlaFlameAttachEffectScript))]
	public class PlaFlameAttachEffectScript : AttachEffectScriptable
	{
	
		private int count = 1;

		private Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");


		public PlaFlameAttachEffectScript(TechnoExt owner) : base(owner)
		{
		}

		public override void OnUpdate()
		{
			
		}

		public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
		{

		}

		public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
		{
			base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
		}

		//重置duration
		public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
		{
			if (count < 9)
			{
				count += 1;
			}
			else
			{
				if(pAttacker.IsNotNull)
				{
					if(pAttacker.CastToTechno(out var pattckTechno))
					{
						count = 0;
						var pb = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pattckTechno, 30, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("GATPFlameWh"), 100, true);
						pb.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
					}
				}
			}

			Duration = duration;
		}

		public override void OnRemove()
		{
			Duration = 0;
			base.OnRemove();
		}

		public override void OnAttachEffectRemove()
		{
			base.OnAttachEffectRemove();
		}
	}







	[Serializable]
	[ScriptAlias(nameof(PlaThunderAttachEffectScript))]
	public class PlaThunderAttachEffectScript : AttachEffectScriptable
	{

		private int count = 1;

		private Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");


		public PlaThunderAttachEffectScript(TechnoExt owner) : base(owner)
		{
		}

		public override void OnUpdate()
		{

		}

		public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
		{

		}

		public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
		{
			base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
		}

		//重置duration
		public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
		{
			if (count < 9)
			{
				count += 1;
			}
			else
			{
				if (pAttacker.IsNotNull)
				{
					if (pAttacker.CastToTechno(out var pattckTechno))
					{
						count = 0;
						var pb = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pattckTechno, 10, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("GATPBoltWh"), 100, true);
						pb.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
					}
				}
			}

			Duration = duration;
		}

		public override void OnRemove()
		{
			Duration = 0;
			base.OnRemove();
		}

		public override void OnAttachEffectRemove()
		{
			base.OnAttachEffectRemove();
		}
	}




}
