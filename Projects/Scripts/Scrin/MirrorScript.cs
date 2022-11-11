using DynamicPatcher;
using Extension.CW;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(MirrorScript))]
    public class MirrorScript : TechnoScriptable
    {
        public MirrorScript(TechnoExt owner) : base(owner)
        {
        }

        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");

        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        static Pointer<WarheadTypeClass> pDebuffWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MirrorDebuffWh");

        static Pointer<AnimTypeClass> pAnim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("FIELDFX");


        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            base.OnFire(pTarget, weaponIndex);

            if (pTarget.CastToTechno(out var pTechno))
            {
                if (pTechno.Ref.Owner.IsNull)
                    return;
                if (Owner.OwnerObject.Ref.Owner.IsNull)
                    return;
                if (Owner.OwnerObject.Ref.Base.OnBridge)
                    return;
                if (pTechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                {
                    if(pTechno.Ref.Type.Ref.Cost >= Owner.OwnerObject.Ref.Type.Ref.Cost)
                    return;
                }
                    
                if (pTarget.Ref.WhatAmI() != AbstractType.Unit)
                    return;
                var technoExt = TechnoExt.ExtMap.Find(pTechno);
                if (technoExt == null)
                    return;

                var gext = technoExt.GameObject.GetComponent<TechnoGlobalExtension>();
                if (gext == null)
                    return;

                if (!gext.Data.Copyable)
                    return;


                int health = Owner.OwnerObject.Ref.Base.Health;
                bool isSelected = Owner.OwnerObject.Ref.Base.IsSelected;

                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                var height = Owner.OwnerObject.Ref.Base.GetHeight();

                var techno = pTechno.Ref.Type.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();
                if (techno == null)
                    return;

                Owner.OwnerObject.Ref.Base.Remove();

                if (techno.Ref.Base.Health > health + 140)
                {
                    techno.Ref.Base.Health = health + 140;
                }


                var createLocation = location + new CoordStruct(0, 0, -height);

                if (techno.Ref.Base.Put(createLocation, Direction.N))
                {
                    if (isSelected)
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
                        mission.Ref.QueueMission(Mission.Hunt, false);
                    }

                    if(IsMkIIUpdated)
                    {
                        techno.Ref.Veterancy.SetElite(); 
                    }

                    var bullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 0, pDebuffWh, 100, false);
                    bullet.Ref.DetonateAndUnInit(createLocation);
                    YRMemory.Create<AnimClass>(pAnim, createLocation);
                        
                }

                Owner.OwnerObject.Ref.Base.UnInit();
            }
        }

        private bool IsMkIIUpdated = false;

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
        Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (IsMkIIUpdated == false)
            {
                //判断是否来自升级弹头
                if (pWH.Ref.Base.ID.ToString() == "MarkIISpWh")
                {
                    IsMkIIUpdated = true;
                    Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                    CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                    Pointer<BulletClass> mk2bullet = pBulletType.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, mk2Warhead, 100, false);
                    mk2bullet.Ref.DetonateAndUnInit(currentLocation);
                }
            }
        }
    }
}
