using Extension.CW;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Linq;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    [ScriptAlias(nameof(MeotorLauncherScript))]

    public class MeotorLauncherScript : TechnoScriptable
    {
        public MeotorLauncherScript(TechnoExt owner) : base(owner)
        {

        }

        public override void Awake()
        {
            var scriptArgs = "";
            var gext = Owner.GameObject.GetTechnoGlobalComponent();
            if (gext != null)
            {
                scriptArgs = gext.Data.ScriptArgs;
            }

            if (!string.IsNullOrEmpty(scriptArgs))
            {
                var args = scriptArgs.Split(',');
                remove = args[0];
                end = args[1];
            }
        }

        static Pointer<BulletTypeClass> bulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("InvisoMissile");
        //砸陨石的武器
        static Pointer<WeaponTypeClass> weapon => WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("ScrinMeotor");

        static Pointer<SuperWeaponTypeClass> sw2 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SWMeotorSpecial2");
        static Pointer<SuperWeaponTypeClass> sw3 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("SWMeotorSpecial3");

        static Pointer<SuperWeaponTypeClass> swLight => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("MockLightPurpleSpecial");




        //需要移除的单位
        private string remove;
        //结束表示
        private string end;

        private bool inited = false;


        private int delay = 5000;


        public override void OnUpdate()
        {
            base.OnUpdate();
            if (inited == false)
            {
                //移除前一阶段的引导者
                if (remove != "none")
                {
                    var technos = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, x => x.Ref.Type.Ref.Base.Base.ID == remove, FindRange.Owner);
                    if (technos != null)
                    {
                        foreach (var techno in technos)
                        {
                            if (!techno.IsNullOrExpired())
                            {
                                techno.OwnerObject.Ref.Base.UnInit();
                            }
                        }
                    }

                }

                inited = true;

                var swCount = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, x => (x.Ref.Type.Ref.Base.Base.ID == "RARIFT"
                    && x.Ref.Base.IsOnMap && !x.Ref.Base.InLimbo && x.Ref.IsInPlayfield
                ),FindRange.Owner).Count();

                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var cell = CellClass.Coord2Cell(location);

                double damageMultipler = 1;
                var range = 5;

                if (swCount>3)
                {
                    swCount = 3;
                }
                if (swCount <= 1)
                {
                    swCount = 1;
                }


                switch (swCount)
                {
                    case 1:
                        break;
                    case 2:
                        {
                            damageMultipler = 0.8;
                            range = 4;
                            break;
                        }
                    case 3:
                        {
                            damageMultipler = 0.7;
                            range = 6;
                            break;
                        }
                    default: 
                        break;
                }

                SpellMeotorAt(location, damageMultipler);

                if (swCount > 1)
                {
                    for (var i = 1; i < swCount; i++)
                    {
                        var angle = MathEx.Random.Next(0, 360);
                        var pos = new CoordStruct(location.X + (int)(range * Game.CellSize * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), location.Y + (int)(range * Game.CellSize * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), location.Z);
                        SpellMeotorAt(pos, damageMultipler);
                    }
                }



                if (Owner.OwnerObject.Ref.Owner != null)
                {
                    Pointer<HouseClass> pOwner = Owner.OwnerObject.Ref.Owner;
                    Pointer<SuperClass> pSuper2 = pOwner.Ref.FindSuperWeapon(sw2);
                    Pointer<SuperClass> pSuper3 = pOwner.Ref.FindSuperWeapon(sw3);
                    pSuper2.Ref.IsCharged = true;
                    pSuper3.Ref.IsCharged = true;


                    var pSW = pOwner.Ref.FindSuperWeapon(swLight);
                    pSW.Ref.IsCharged = true;
                    pSW.Ref.Launch(cell, true);
                    pSW.Ref.IsCharged = false;

                }



                return;
            }

            if (end == "yes")
            {
                Owner.OwnerObject.Ref.Base.UnInit();
                return;
            }

            //如果一直没有被使用的话直接进入冷却
            if (delay-- <= 0)
            {
                Owner.OwnerObject.Ref.Base.UnInit();
                return;
            }


        }

        private void SpellMeotorAt(CoordStruct location,double damageMultipler)
        {
            var warhead = weapon.Ref.Warhead ;
            var damage = (int)(weapon.Ref.Damage * damageMultipler);
            var cell = CellClass.Coord2Cell(location);
            if (MapClass.Instance.TryGetCellAt(cell, out var pCell))
            {
                var bullet = bulletType.Ref.CreateBullet(pCell.Convert<AbstractClass>(), Owner.OwnerObject, damage, warhead, 100, false);
                bullet.Ref.MoveTo(location + new CoordStruct(0, 0, 2000), new BulletVelocity(0, 0, 0));
            }
        }

    }
}
