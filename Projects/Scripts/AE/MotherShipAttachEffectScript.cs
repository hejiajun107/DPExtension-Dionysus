using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;

namespace DpLib.Scripts.AE
{
    [Serializable]
    [ScriptAlias(nameof(MotherShipAttachEffectScript))]
    public class MotherShipAttachEffectScript : AttachEffectScriptable
    {
        public MotherShipAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        private static Pointer<WarheadTypeClass> damageWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MoDamageWh");

        private int level = 1;

        private static Pointer<WarheadTypeClass> spreadWh1 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MoShipExtendWh");
        private static Pointer<WarheadTypeClass> spreadWh2 => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MoShipExtendWh2");


        private int delay = 200;
        private bool inited = false;

        private TechnoExt attacker;

        private CoordStruct lastCoord;

        public override void OnUpdate()
        {
            if (inited) return;


            var currentCoord = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            if (lastCoord != null && lastCoord != default)
            {
                if (double.IsNaN(currentCoord.DistanceFrom(lastCoord)) || currentCoord.DistanceFrom(lastCoord) >= 2560)
                {
                    inited = true;
                    //瞬间移动范围过大，立刻在上次的位置引爆
                    TakeSpreadAt(getAttacker(), lastCoord);
                    if (currentCoord != null && currentCoord != default)
                    {
                        TakeDamageAt(getAttacker(), currentCoord);
                    }
                }
            }
            lastCoord = currentCoord;

            if (delay-- <= 0)
            {
                inited = true;

                TakeSpreadAt(getAttacker(), currentCoord);
                TakeDamageAt(getAttacker(), currentCoord);
                return;
            }
        }

        private Pointer<TechnoClass> getAttacker()
        {
            Pointer<TechnoClass> launcher;

            //引爆
            //设定发射者
            if (!attacker.IsNullOrExpired())
            {
                launcher = attacker.OwnerObject;
            }
            else
            {
                launcher = Pointer<TechnoClass>.Zero;
            }
            return launcher;
        }

        private void TakeDamageAt(Pointer<TechnoClass> launcher, CoordStruct coord)
        {
            var damage = level == 1 ? 2000 : 1000;
            //if(launcher == Owner.OwnerObject && !Owner.Type.IsEpicUnit)
            //{
            //    Owner.OwnerObject.Ref.Base.TakeDamage(damage, damageWarhead, true);
            //}
            //else
            //{
            var bullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), launcher, damage, damageWarhead, 100, true);
            bullet.Ref.DetonateAndUnInit(coord);
            //}
        }

        private void TakeSpreadAt(Pointer<TechnoClass> launcher, CoordStruct coord)
        {
            var warhead = level == 1 ? spreadWh1 : spreadWh2;
            var bullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), launcher, 5, warhead, 100, true);
            bullet.Ref.DetonateAndUnInit(coord);
        }

        public override void OnRemove()
        {
            if(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID == "YAREFN" || Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID == "SMIN" || Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID == "SLAV")
            {
                return;
            }

            //如果从地图上提前移除则直接引爆
            if (inited == false)
            {
                inited = true;
                var coord = lastCoord != default ? lastCoord : Owner.OwnerObject.Ref.Base.Base.GetCoords();
                //直接引爆
                TakeSpreadAt(getAttacker(), coord);
                TakeDamageAt(getAttacker(), coord);
            }
            //直接清理AE
            Duration = 0;
            base.OnRemove();
        }

        //当AE被贴上时获取发射者
        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {

            //获取发射者
            if (!pAttacker.IsNull)
            {
                if (pAttacker.CastToTechno(out var ptAttcker))
                {
                    attacker = TechnoExt.ExtMap.Find(ptAttcker);
                }
            }

            if (!pWH.IsNull)
            {
                if (pWH.Ref.Base.ID == "MoShipExtendWh" || pWH.Ref.Base.ID == "MoShipBlastWh" || pWH.Ref.Base.ID == "MoShipBlastWh2")
                {
                    level = 1;
                }
                else
                {
                    level = 2;
                }
            }

            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
        }

        //这里直接return屏蔽接收新的AE效果
        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            return;
        }

        public override void OnAttachEffectRemove()
        {
            base.OnAttachEffectRemove();
        }

    }
}
