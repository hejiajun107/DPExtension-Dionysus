using DynamicPatcher;
using Extension.CW;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts.Scrin
{
    [Serializable]
    public class MeotorLauncherScript : TechnoScriptable
    {
        public MeotorLauncherScript(TechnoExt owner) : base(owner)
        {
            var scriptArgs = "";
            var gext = owner.GameObject.GetComponent<TechnoGlobalExtension>();
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
            if(inited == false)
            {
                //移除前一阶段的引导者
                if(remove != "none")
                {
                    var technos = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, x => x.Ref.Type.Ref.Base.Base.ID == remove, FindRange.Owner);
                    if(technos!=null)
                    {
                        foreach(var techno in technos)
                        {
                            if(techno.TryGet(out var pTechno))
                            {
                                pTechno.OwnerObject.Ref.Base.UnInit();
                            }
                        }
                    }

                }

                inited = true;

                var warhead = weapon.Ref.Warhead;
                var damage = weapon.Ref.Damage;
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                var cell = CellClass.Coord2Cell(location);
                if (MapClass.Instance.TryGetCellAt(cell, out var pCell))
                {
                    var bullet = bulletType.Ref.CreateBullet(pCell.Convert<AbstractClass>(), Owner.OwnerObject, damage, warhead, 100, false);
                    bullet.Ref.MoveTo(location + new CoordStruct(0, 0, 2000), new BulletVelocity(0, 0, 0));
                }

                if(Owner.OwnerObject.Ref.Owner!=null)
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

            if(end == "yes")
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
    }
}
