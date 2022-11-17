
using Extension.Ext;
using Extension.Script;
using Extension.Shared;
using PatcherYRpp;
using System;

namespace Scripts
{

    [Serializable]
    [ScriptAlias(nameof(BorisScript))]

    public class BorisScript : TechnoScriptable
    {
        public BorisScript(TechnoExt owner) : base(owner)
        {
            _manaCounter = new ManaCounter();
        }

        private ManaCounter _manaCounter;



        Random random = new Random(113542);


        static Pointer<BulletTypeClass> pBulletType => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("BorisParaBullet");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("BorisParaDropperWH");


        public override void OnUpdate()
        {
            _manaCounter.OnUpdate(Owner);
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {

        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH,
      Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            try
            {
                if (pAttackingHouse.IsNull)
                {
                    return;
                }
                if (pAttackingHouse.Ref.ArrayIndex != Owner.OwnerObject.Ref.Owner.Ref.ArrayIndex)
                {
                    if (_manaCounter.Cost(100))
                    {
                        CreateParadDrop(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                    }
                }
            }
            catch (Exception) {; }
        }


        private void CreateParadDrop(CoordStruct center)
        {
            for (int i = 0; i < 2; i++)
            {
                var ntarget = new CoordStruct(center.X + random.Next(-500, 500), center.Y + random.Next(-500, 500), 2000);
                var ntargetGround = new CoordStruct(ntarget.X, ntarget.Y, center.Z);

                if (MapClass.Instance.TryGetCellAt(ntargetGround, out Pointer<CellClass> pCell))
                {
                    Pointer<BulletClass> pBullet = pBulletType.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, warhead, 50, true);
                    BulletVelocity velocity = new BulletVelocity(0, 0, 0);
                    pBullet.Ref.MoveTo(ntarget, velocity);
                    pBullet.Ref.SetTarget(pCell.Convert<AbstractClass>());
                }
            }
        }
    }
}