using DynamicPatcher;
using Extension.EventSystems;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Japan
{
    [ScriptAlias(nameof(JALCScript))]
    [Serializable]
    public class JALCScript : TechnoScriptable
    {
        public JALCScript(TechnoExt owner) : base(owner)
        {
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
            var pBullet = pInviso.Ref.CreateBullet(pTarget, Owner.OwnerObject,1,WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("JALCAttachWH"),100, true);
            pBullet.Ref.Base.SetLocation(pTarget.Ref.GetCoords());
            var laserWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("JALCSLaser");
            Owner.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, laserWeapon, ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, new CoordStruct(30, -165, 25), false,1) );
            Owner.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, laserWeapon, ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, new CoordStruct(30, -165, 25), false, -1));
            pBullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
            base.OnFire(pTarget, weaponIndex);
        }
    }

    [ScriptAlias(nameof(NanoDeconstructionAttachEffectScript))]
    [Serializable]
    public class NanoDeconstructionAttachEffectScript : AttachEffectScriptable
    {
        public NanoDeconstructionAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        public TechnoExt Attacker;

        private int delay = 20;

        public int level = 0;

        public bool removed = false;

        public override void OnUpdate()
        {
            if (delay-- <= 0)
            {
                delay = 20;

                var mult = 1.0 + (level * 0.01);
                var damage = (int)(15.0 * mult);

                var pInviso = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
                var pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Attacker.IsNullOrExpired() ? Pointer<TechnoClass>.Zero : Attacker.OwnerObject,
                    (int)(damage * (Attacker.IsNullOrExpired() ? 1 : Attacker.OwnerObject.Ref.FirepowerMultiplier)),
                    WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("JALCDestWH"),
                    100, true);

                var coord = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                pBullet.Ref.DetonateAndUnInit(coord);
                for(var i = 0; i < 2; i++)
                {
                    YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("JALCDestExp"), coord + new CoordStruct(MathEx.Random.Next(-50, 50), MathEx.Random.Next(-50, 50), MathEx.Random.Next(0, 50)));
                }

            }
            base.OnUpdate();
        }

        public override void OnAttachEffectRemove()
        {
            if (!removed)
            {
                removed = true;
                if (Attacker.IsNullOrExpired())
                {
                    return;
                }

                var houseExt = Attacker.GetHouseGlobalExtension();
                var typeId = Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID.ToString();
                int levelUp = 3;
                int max = 300;
                if (!houseExt.DeconstructionLevels.ContainsKey(typeId))
                {
                    houseExt.DeconstructionLevels.Add(typeId, levelUp);
                }
                else
                {
                    var lastLevel = houseExt.DeconstructionLevels[typeId] + levelUp;
                    houseExt.DeconstructionLevels[typeId] = lastLevel < max ? lastLevel : max;
                }


                Pointer<BulletClass> bullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("JALCSBSeeker")
                    .Ref.CreateBullet(Attacker.OwnerObject.Convert<AbstractClass>(), Attacker.OwnerObject, 1,
                    WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("JALCSBWh"),
                    100, true);
                bullet.Ref.MoveTo(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, 150), new BulletVelocity(0, 0, 0));
                bullet.Ref.SetTarget(Attacker.OwnerObject.Convert<AbstractClass>());
            }
            base.OnAttachEffectRemove();
        }

        public override void OnRemove()
        {
            OnAttachEffectRemove();
            base.OnRemove();
        }

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            Duration = Duration;
            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
            if (pAttacker.IsNotNull)
            {
                Attacker = TechnoExt.ExtMap.Find(pAttacker.Convert<TechnoClass>());
                if (Attacker.IsNullOrExpired())
                    return;
                var houseExt = Attacker.GetHouseGlobalExtension();
                if (houseExt.DeconstructionLevels.ContainsKey(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID))
                {
                    level = houseExt.DeconstructionLevels[Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID];
                }
                else
                {
                    level = 0;
                }

                var displayer = Owner.GameObject.GetComponent<DeconstructionDisplayerScript>();
                if (displayer == null)
                {
                    var script = ScriptManager.GetScript(nameof(DeconstructionDisplayerScript));
                    var scriptComponent = ScriptManager.CreateScriptableTo(Owner.GameObject, script, Owner);
                    displayer = scriptComponent as DeconstructionDisplayerScript;
                }

                displayer.Duration = 50;
                displayer.Value = level;
                displayer.OwnerIndex = Attacker.OwnerObject.Ref.Owner.Ref.ArrayIndex;
            }
        }
    }

    [ScriptAlias(nameof(DeconstructionDisplayerScript))]
    [Serializable]
    public class DeconstructionDisplayerScript : TechnoScriptable
    {
        public DeconstructionDisplayerScript(TechnoExt owner) : base(owner)
        {
        }

        public int Duration { get; set; } = 50;

        public int Value { get; set; } = 0;

        public int OwnerIndex = 0;

        public override void Awake()
        {
            base.Awake();
            EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);

        }

        public override void OnDestroy()
        {
            EventSystem.GScreen.RemoveTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
        }

    
        public override void OnUpdate()
        {
            if(Duration-- <= 0)
            {
                EventSystem.GScreen.RemoveTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
                DetachFromParent();
            }
        }

        public void OnGScreenRender(object sender, EventArgs args)
        {
            if (args is GScreenEventArgs gScreenEvtArgs)
            {
                if (!gScreenEvtArgs.IsLateRender)
                {
                    return;
                }

                if (!Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap && HouseClass.Player.Ref.ArrayIndex == OwnerIndex)
                {
                    if (FileSystem.TyrLoadSHPFile("jpdecodebar.shp", out Pointer<SHPStruct> pCustomSHP))
                    {
                        Pointer<Surface> pSurface = Surface.Current;
                        RectangleStruct rect = pSurface.Ref.GetRect();
                        Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.BaseAbstract.GetCoords() + new CoordStruct(0, 0, 300));
                        {
                            var frame = (int)((Value / (double)300) * 100);

                            pSurface.Ref.DrawSHP(FileSystem.UNITx_PAL, pCustomSHP, frame, point, rect.GetThisPointer());
                        }
                    }
                }
            }
        }


    }

}
