﻿using Extension.CW;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.Script;
using PatcherYRpp;
using System;

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
        static Pointer<WarheadTypeClass> pDebuffWh1 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MirrorDebuffWh1");
        static Pointer<WarheadTypeClass> pDebuffWh2 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MirrorDebuffWh2");


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
                    if (pTechno.Ref.Type.Ref.Cost >= Owner.OwnerObject.Ref.Type.Ref.Cost)
                        return;
                }

                if (pTarget.Ref.WhatAmI() != AbstractType.Unit)
                    return;
                var technoExt = TechnoExt.ExtMap.Find(pTechno);
                if (technoExt == null)
                    return;

                var gext = technoExt.GameObject.GetTechnoGlobalComponent();
                if (gext == null)
                    return;

                if (!gext.Data.Copyable)
                    return;


                int health = Owner.OwnerObject.Ref.Base.Health;
                bool isSelected = Owner.OwnerObject.Ref.Base.IsSelected;

                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                var height = Owner.OwnerObject.Ref.Base.GetHeight();

                Pointer<TechnoTypeClass> copyType;
                if(string.IsNullOrEmpty(gext.Data.CopyAs))
                {
                    copyType = pTechno.Ref.Type;
                }
                else
                {
                    copyType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(gext.Data.CopyAs);
                }

                var techno = copyType.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner).Convert<TechnoClass>();
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

                    if (IsMkIIUpdated)
                    {
                        techno.Ref.Veterancy.SetElite();
                    }

                    var bullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 0, pDebuffWh, 100, false);
                    bullet.Ref.DetonateAndUnInit(createLocation);
                    var bullet1 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 0, pDebuffWh1, 100, false);
                    bullet1.Ref.DetonateAndUnInit(createLocation);
                    var bullet2 = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 0, pDebuffWh2, 100, false);
                    bullet2.Ref.DetonateAndUnInit(createLocation);
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
