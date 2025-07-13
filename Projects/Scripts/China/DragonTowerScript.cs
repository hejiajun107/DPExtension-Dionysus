using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(DragonTowerScript))]
    class DragonTowerScript : TechnoScriptable
    {
        public DragonTowerScript(TechnoExt owner) : base(owner) { }



        static ColorStruct innerColor1 = new ColorStruct(255, 0, 0);
        static ColorStruct outerColor1 = new ColorStruct(128, 0, 0);

        static ColorStruct innerColor2 = new ColorStruct(0, 255, 0);
        static ColorStruct outerColor2 = new ColorStruct(0, 128, 0);

        static ColorStruct outerSpread = new ColorStruct(0, 0, 0);

        private int height = 420;

        TechnoExt pTargetRef;


        static Pointer<BulletTypeClass> bullet => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ZAFKWH");

        static Pointer<WarheadTypeClass> supWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ZAFKSupAOEWH");

        //static Pointer<WarheadTypeClass> supWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ZAFKSupWH");

        static Pointer<WarheadTypeClass> animWarhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("ZAFKMockAnimWH");

        static Pointer<WarheadTypeClass> mk2Warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAttachWh");

        static Pointer<WarheadTypeClass> burnWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("FKTOWRBurnWH");


        static List<string> supportIds = new List<string>()
        {
            "ZAFKTR","ZAFKTR4AI"
        };

        private bool IsMkIIUpdated = false;

        private int coolDown = 0;

        private int batteStage = 0;

        public override void OnUpdate()
        {
            if (coolDown > 0)
            {
                coolDown--;
            }

            if (batteStage > 0)
            {
                batteStage--;
            }

            if (batteStage <= 0)
            {
                Owner.OwnerObject.Ref.Ammo = 0;
               
            }
        }



        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            if (weaponIndex == 0)
            {
                coolDown = 25;
                Pointer<BulletClass> supBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, supWarhead, 100, false);
                supBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            }
            else if(weaponIndex==1)
            {
                if (Owner.OwnerRef.Base.Base.GetCoords().DistanceFrom(pTarget.Ref.GetCoords()) < 10 * Game.CellSize)
                {
                    coolDown = 25;
                    Owner.OwnerObject.Ref.Ammo = 0;
                }
            }

            if (IsMkIIUpdated)
            {
                Pointer<BulletClass> burnBullet = bullet.Ref.CreateBullet(pTarget, Owner.OwnerObject, 1, burnWh, 100, false);
                burnBullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
            }


            batteStage = 50;


            //if (coolDown <= 0)
            //{

            //    DrawLaser(Owner.OwnerObject.Ref.Base.Base.GetCoords(), pTarget.Ref.GetCoords(), innerColor1, outerColor1);
            //    Pointer<BulletClass> damageBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 8, warhead, 100, false);
            //    damageBullet.Ref.DetonateAndUnInit(pTarget.Ref.GetCoords());
            //    coolDown = 10;
            //    if (pTarget.CastToTechno(out Pointer<TechnoClass> pTechno))
            //    {
            //        var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            //        var currentCell = CellClass.Coord2Cell(location);


            //        Pointer<BulletClass> supBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), pTechno, 1, supWarhead, 100, false);
            //        supBullet.Ref.DetonateAndUnInit(location);


            //    }
            //}
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (IsMkIIUpdated == false)
            {
                //判断是否来自升级弹头
                if (pWH.Ref.Base.ID.ToString() == "MarkIISpWh")
                {
                    IsMkIIUpdated = true;
                    Pointer<TechnoClass> pTechno = Owner.OwnerObject;
                    CoordStruct currentLocation = pTechno.Ref.Base.Base.GetCoords();
                    Pointer<BulletClass> mk2bullet = bullet.Ref.CreateBullet(pTechno.Convert<AbstractClass>(), Owner.OwnerObject, 1, mk2Warhead, 100, false);
                    mk2bullet.Ref.DetonateAndUnInit(currentLocation);
                }
            }

            if (coolDown <= 0)
            {
                //if (Owner.OwnerObject.Ref.Target.IsNull)
                //{
                if (pAttacker.IsNull)
                {
                    return;
                }
                if (pWH.Ref.Base.ID == supWarhead.Ref.Base.ID || pWH.Ref.Base.ID == "ZAFKSupAOEWH")
                {
                    //DrawLaser(Owner.OwnerObject.Ref.Base.Base.GetCoords(), pAttacker.Ref.Base.GetCoords(), innerColor2, outerColor2);
                    //Pointer<BulletClass> damageBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 6, warhead, 100, false);
                    //damageBullet.Ref.DetonateAndUnInit(pAttacker.Ref.Base.GetCoords());

                    //Pointer<BulletClass> animBullet = bullet.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, animWarhead, 100, false);
                    //animBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords() + new CoordStruct(0, 0, height));

                    Owner.OwnerRef.Ammo = 4;


                    coolDown = 10;
                    batteStage = 30;
                }
                //}
            }
        }


        private void DrawLaser(CoordStruct from, CoordStruct to, ColorStruct innerColor, ColorStruct outerColor)
        {
            CoordStruct center = from + new CoordStruct(0, 0, height);
            CoordStruct focus = center + new CoordStruct(0, 0, 150);

            var radius = 100;

            for (var angle = -180 - 45; angle < 180 - 45; angle += 90)
            {
                var pos = new CoordStruct(center.X + (int)(radius * Math.Round(Math.Cos(angle * Math.PI / 180), 5)), center.Y + (int)(radius * Math.Round(Math.Sin(angle * Math.PI / 180), 5)), center.Z);
                Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(pos, focus, innerColor, outerColor, outerSpread, 15);
            }

            Pointer<LaserDrawClass> pLaser2 = YRMemory.Create<LaserDrawClass>(focus, to, innerColor, outerColor, outerSpread, 15);
            pLaser2.Ref.IsHouseColor = true;
            pLaser2.Ref.Thickness = 2;

        }

       

    }

}
