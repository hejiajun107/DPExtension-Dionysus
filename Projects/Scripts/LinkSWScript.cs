using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DpLib.Scripts
{
    [Serializable]
    public class LinkSWScript : TechnoScriptable
    {
        public LinkSWScript(TechnoExt owner) : base(owner)
        {

        }

        private bool inited = false;

        static Pointer<SuperWeaponTypeClass> sw1 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("MultiTargetSpeical1");
        static Pointer<SuperWeaponTypeClass> sw2 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("MultiTargetSpeical2");
        static Pointer<SuperWeaponTypeClass> sw3 => SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("MultiTargetSpeical3");

        private string technoId1 = "SWLinkUnit1";
        private string technoId2 = "SWLinkUnit2";

        private static Pointer<BulletTypeClass> pbullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("HuimieLaserCannon");
        private static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("AP");

        public override void OnUpdate()
        {
            if(!inited)
            {
                inited = true;

                var id = Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID.ToString();
                int num = Int32.Parse(id.Last().ToString());

                //移除之前投递的目标
                var li = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, techno => techno.Ref.Type.Ref.Base.Base.ID == id && techno != Owner.OwnerObject, FindRange.Owner);
                foreach(var item in li)
                {
                    if (item.IsNullOrExpired())
                    {
                        item.OwnerObject.Ref.Base.UnInit();
                    }
                }

                switch (num)
                {
                    case 1:
                        {
                            Logger.Log("丢1");

                            Pointer<SuperClass> pSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw1);
                            pSuper.Ref.IsCharged = true;

                            Pointer<SuperClass> pSuper2 = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw2);
                            pSuper2.Ref.IsCharged = true;
                            Game.CurrentSWType = sw2.Ref.ArrayIndex;
                            break;
                        }
                    case 2:
                        {
                            Logger.Log("丢2");

                            Pointer<SuperClass> pSuper3 = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw3);
                            pSuper3.Ref.IsCharged = true;
                            Game.CurrentSWType = sw3.Ref.ArrayIndex;
                            break;
                        }
                    case 3:
                        {
                            Logger.Log("丢3");

                            //sw1进入冷却
                            Pointer<SuperClass> pSuper = Owner.OwnerObject.Ref.Owner.Ref.FindSuperWeapon(sw1);
                            pSuper.Ref.IsCharged = false;
                            pSuper.Ref.RechargeTimer.Start(600);
                            pSuper.Ref.CameoChargeState = 0;

                            var technos = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, techno => techno.Ref.Type.Ref.Base.Base.ID == technoId1 || techno.Ref.Type.Ref.Base.Base.ID == technoId2, FindRange.Owner);

                            var techno1 = technos.Where(t => t.OwnerObject.Ref.Type.Ref.Base.Base.ID == technoId1).FirstOrDefault();
                            var techno2 = technos.Where(t => t.OwnerObject.Ref.Type.Ref.Base.Base.ID == technoId2).FirstOrDefault();

                            if (!techno1.IsNullOrExpired())
                            {

                                var t1location = techno1.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0,0,2000);
                                if (!techno2.IsNullOrExpired())
                                {
                                    var bullet1 = pbullet.Ref.CreateBullet(techno2.OwnerObject.Convert<AbstractClass>(), techno1.OwnerObject, 1, warhead, 50, false);
                                    bullet1.Ref.MoveTo(t1location, new BulletVelocity(0, 0, 0));
                                    bullet1.Ref.SetTarget(techno2.OwnerObject.Convert<AbstractClass>());
                                }

                                var bullet2 = pbullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), techno1.OwnerObject, 1, warhead, 50, false);
                                bullet2.Ref.MoveTo(t1location, new BulletVelocity(0, 0, 0));
                                bullet2.Ref.SetTarget(Owner.OwnerObject.Convert<AbstractClass>());

                            }

                            break;
                        }
                    default:
                        break;
                }

            }
            base.OnUpdate();
        }
    }
}


