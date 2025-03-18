using Extension.Ext;
using Extension.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension.CW;
using Extension.Ext4CW;
using PatcherYRpp;
using Extension.Utilities;
using DynamicPatcher;
using PatcherYRpp.Utilities;

namespace Scripts.Scrin
{
    [ScriptAlias(nameof(SCDoorUnitScript))]
    [Serializable]
    public class SCDoorUnitScript : TechnoScriptable
    {
        public SCDoorUnitScript(TechnoExt owner) : base(owner)
        {
            pAnim = new SwizzleablePointer<AnimClass>(IntPtr.Zero);

        }

        private SwizzleablePointer<AnimClass> pAnim;

        private bool _isOpeningDoor = false;

        public bool IsOpeningDoor { get
            {
                return _isOpeningDoor;
            }
            private set
            {
                if (value)
                {
                    CreateAnim();
                }
                else
                {
                    KillAnim();
                }

                _isOpeningDoor = value;
            }
        }

        public override void Awake()
        {
            if (Owner.TryGetHouseGlobalExtension(out var houseExt))
            {
                houseExt.SCDoorUnit.Add(Owner);
            }
        }

        public override void OnDestroy()
        {
            if (Owner.TryGetHouseGlobalExtension(out var houseExt))
            {
                houseExt.SCDoorUnit.Remove(Owner);
            }
        }

        
        public override void OnUpdate()
        {
            var mission = Owner.OwnerObject.Convert<MissionClass>();

            if(mission.Ref.CurrentMission != Mission.Unload)
            {
                if (IsOpeningDoor) 
                {
                    IsOpeningDoor = false;
                }
            }
            else
            {
                if (!IsOpeningDoor)
                {
                    IsOpeningDoor = true;
                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 1)
            {
                var technos = ObjectFinder.FindTechnosNear(pTarget.Ref.GetCoords(), (int)(Game.CellSize * 1.5)).Where(x => !x.Ref.InLimbo && !x.Ref.Base.IsInAir() && x.Ref.Base.WhatAmI() != AbstractType.Building).ToList().Select(x => x.Convert<TechnoClass>());

                var matched = new List<SCWarpFlagScript>();
                foreach (var techno in technos)
                {
                    if (MapClass.GetTotalDamage(1000, WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("SA"), Owner.OwnerObject.Ref.Type.Ref.Base.Armor, 0) <= 0)
                        continue;

                    var ext = TechnoExt.ExtMap.Find(techno);
                    if (ext.IsNullOrExpired())
                    {
                        continue;
                    }

                    SCWarpFlagScript script = null;
                    script = ext.GameObject.GetComponent<SCWarpFlagScript>();

                    if (script is null) 
                    {
                        ext.GameObject.CreateScriptComponent(nameof(SCWarpFlagScript), SCWarpFlagScript.UniqueId, nameof(SCWarpFlagScript), ext, Owner);
                        script = ext.GameObject.GetComponent<SCWarpFlagScript>();
                    }

                    if (script is null)
                        continue;

                    if(script.Master != Owner)
                    {
                        script.ChangeMaster(Owner);
                    }

                    if (script.CanWrap) {
                        matched.Add(script);
                    }

                }

                if(matched.Any())
                {
                    var houseExt = Owner.GetHouseGlobalExtension();

                    if (houseExt is null)
                        return;

                    var targetDoor = houseExt.SCDoorUnit.Where(x =>
                        {
                            if (x.IsNullOrExpired())
                                return false;

                            if (x.OwnerObject.Ref.Base.InLimbo)
                                return false;

                            if (x == Owner)
                                return false;

                            var doorScript = x.GameObject.GetComponent<SCDoorUnitScript>();

                            if (doorScript is null)
                                return false;

                            if (doorScript.IsOpeningDoor)
                                return true;

                            return false;
                        }
                    ).OrderByDescending(x=>x.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(Owner.OwnerObject.Ref.Base.Base.GetCoords())).FirstOrDefault();

                    if (targetDoor is null)
                        return;

                    var targetCoord = targetDoor.OwnerObject.Ref.Base.Base.GetCoords() - new CoordStruct(0, 0, targetDoor.OwnerObject.Ref.Base.GetHeight());

                    if(MapClass.Instance.TryGetCellAt(targetCoord, out var cell))
                    {
                        if (cell.Ref.ContainsBridge())
                            return;

                        if (cell.Ref.LandType == LandType.Water)
                            return;

                        if (cell.Ref.GetBuilding().IsNotNull)
                            return;
                        
                        var targetCenter = cell.Ref.Base.GetCoords();

                        foreach(var item in matched)
                        {
                            item.WrapTo(targetCenter);
                        }

                        YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SCTELETG"), pTarget.Ref.GetCoords());
                        YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SCTELEAR"), targetCenter);
                    }
                }
            }
        }


        public override void OnRemove()
        {
            KillAnim();
        }

        private void CreateAnim()
        {
            if (!pAnim.IsNull)
            {
                KillAnim();
            }

            var anim = YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SCTELEFD"), Owner.OwnerObject.Ref.Base.Base.GetCoords() - new CoordStruct(0, 0, Owner.OwnerObject.Ref.Base.GetHeight()));
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

    [ScriptAlias(nameof(SCWarpFlagScript))]
    [Serializable]
    public class SCWarpFlagScript : TechnoScriptable
    {
        public static int UniqueId = 2025031722;

        public SCWarpFlagScript(TechnoExt owner,TechnoExt master) : base(owner)
        {
            Master = master;
        }

        public void ChangeMaster(TechnoExt master)
        {
            Master = master;
            delay = 500;
            stayDuration = 0;
        }

        public void WrapTo(CoordStruct target)
        {
            delay = 500;
            stayDuration = 0;

            var pTechno = Owner.OwnerObject;
            var mission = pTechno.Convert<MissionClass>();
            mission.Ref.ForceMission(Mission.Stop);
            var currentLocation = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            //位置
            if (pTechno.CastToFoot(out Pointer<FootClass> pfoot))
            {
                if (MapClass.Instance.TryGetCellAt(target, out Pointer<CellClass> pCell))
                {
                    var source = pTechno.Ref.Base.Base.GetCoords();
                    pfoot.Ref.Locomotor.Mark_All_Occupation_Bits(0);
                    pfoot.Ref.Locomotor.Force_Track(-1, source);
                    pTechno.Ref.Base.UnmarkAllOccupationBits(source);
                    pTechno.Ref.Base.SetLocation(target);
                    pTechno.Ref.Base.UnmarkAllOccupationBits(target);
                    pTechno.Ref.Base.Scatter(target, true, true);
                }
            }

        }

        public TechnoExt Master { get; private set; }

        private int delay = 500;

        public int stayDuration = 0;

        public bool CanWrap { get { return stayDuration >= 300; } } 


        public override void OnUpdate()
        {
            if(delay -- <=0)
            {
                DetachFromParent();
            }

            if (Master.IsNullOrExpired())
            {
                stayDuration = 0;
                return;
            }


            var doorUnit = Master.GameObject.GetComponent<SCDoorUnitScript>();
            if(doorUnit is null)
            {
                stayDuration = 0;
                return;
            }

            if (!doorUnit.IsOpeningDoor)
            {
                stayDuration = 0;
                return;
            }
            
            if((Owner.OwnerObject.Ref.Base.Base.GetCoords().BigDistanceForm(Master.OwnerObject.Ref.Base.Base.GetCoords() - new CoordStruct(0,0, Master.OwnerObject.Ref.Base.GetHeight())) > Game.CellSize * 2))
            {
                stayDuration = 0;
                return;
            }
            else
            {
                delay = 500;
                stayDuration = stayDuration > 300 ? 300 : stayDuration + 1;
            }


        }

        
    }
}
