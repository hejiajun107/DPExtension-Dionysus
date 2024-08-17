using DynamicPatcher;
using Extension.Coroutines;
using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Scrin
{
    [ScriptAlias(nameof(PlanetShipScript))]
    [Serializable]
    public class PlanetShipScript : TechnoScriptable
    {
        public PlanetShipScript(TechnoExt owner) : base(owner)
        {
            pMana = new SwizzleablePointer<AnimClass>(IntPtr.Zero);
        }

        private bool InIonStorm;
        private int Delay = 0;

        private SwizzleablePointer<AnimClass> pMana;


        private static Pointer<AnimTypeClass> pblast => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SCLIGHTNWAVE");
        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        private static Pointer<WarheadTypeClass> pWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("PlanetIonWh");

        private int charge = 0;

        private int chargeMax = 3000;//3000;


        public override void OnUpdate()
        {

            var mission = Owner.OwnerObject.Convert<MissionClass>();
            if(mission.Ref.CurrentMission == Mission.Unload)
            {
                mission.Ref.ForceMission(Mission.Stop);
                if (charge >= chargeMax)
                {
                    charge = 0;
                    Owner.GameObject.StartCoroutine(DoIonStorm());
                }
            }

            if (charge < chargeMax && !InIonStorm)
            {
                charge++;
            }

            UpdateAnim();

            //if (InIonStorm)
            //{
            //    var height = Owner.OwnerObject.Ref.Base.GetHeight();
            //    var location = Owner.OwnerObject.Ref.Base.Base.GetCoords() - new CoordStruct(0,0,height);

            //    if (Delay % 20 == 0)
            //    {
            //        var spread = 7 * Game.CellSize;
            //        var target = new CoordStruct(location.X + MathEx.Random.Next(-spread, spread), location.Y + MathEx.Random.Next(-spread, spread), location.Z);
            //        YRMemory.Create<AnimClass>(pblast, target);
            //        var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 120, pWh, 100, true);
            //        bullet.Ref.DetonateAndUnInit(target);
            //    }
            //    if (Delay-- <= 0)
            //    {
            //        InIonStorm = false;
            //    }
            //}
            base.OnUpdate();
        }

        IEnumerator DoIonStorm()
        {
            InIonStorm = true;
            var spread = Game.CellSize * 6;

            for (var i = 0;i<= 30; i++)
            {
                var height = Owner.OwnerObject.Ref.Base.GetHeight();
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                var sky = new CoordStruct(location.X + MathEx.Random.Next(-spread, spread), location.Y + MathEx.Random.Next(-spread, spread), location.Z + 300);

                var anim = YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("PSIONCLOUD"), sky);
                yield return new WaitForFrames(5);

                var techno = ObjectFinder.FindTechnosNear(sky - new CoordStruct(0, 0, height + 300), 5 * Game.CellSize).Select(x => x.Convert<TechnoClass>()).Where(x =>
                    !x.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner) && !x.Ref.Base.InLimbo && GameUtil.CanAffectTarget(Owner.OwnerObject, x)
                ).OrderBy(x=>x.Ref.Base.Base.GetCoords().DistanceFrom(sky)).FirstOrDefault();

                if(techno != null && techno.IsNotNull)
                {
                    var pTarget = techno.Convert<AbstractClass>();
                    FlashTo(sky, techno.Ref.Base.Base.GetCoords(), pTarget);
                }
                else
                {
                    var targetLoction = new CoordStruct(location.X + MathEx.Random.Next(-spread, spread), location.Y + MathEx.Random.Next(-spread, spread), location.Z - height);
                    if(MapClass.Instance.TryGetCellAt(targetLoction,out var pCell))
                    {
                        FlashTo(sky, targetLoction, pCell.Convert<AbstractClass>());
                    }
                }


                yield return new WaitForFrames(20);
            }


            InIonStorm = false;
        }

        private void FlashTo(CoordStruct from,CoordStruct to,Pointer<AbstractClass> pTarget)
        {
            var laserWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find("PlanetLaser");
            Pointer<BulletClass> pBullet = BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible").Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 40, pWh, 100, true);

            var dx = from.X - to.X;
            var dy = from.Y - to.Y;
            var dz = from.Z - to.Z;
            var pos1 = from - new CoordStruct((int)(0.3 * dx), (int)(0.3 * dy), (int)(0.3 * dz)) + new CoordStruct(MathEx.Random.Next(-600, 600), MathEx.Random.Next(-600, 600), MathEx.Random.Next(-600, 600));
            pBullet.Ref.Base.SetLocation(from);
            Owner.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, laserWeapon, pos1);
            var pos2 = from - new CoordStruct((int)(0.6 * dx), (int)(0.6 * dy), (int)(0.6 * dz)) + new CoordStruct(MathEx.Random.Next(-600, 600), MathEx.Random.Next(-600, 600), MathEx.Random.Next(-600, 600));
            pBullet.Ref.Base.SetLocation(pos1);
            Owner.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, laserWeapon, pos2);
            pBullet.Ref.Base.SetLocation(pos2);
            Owner.OwnerObject.Ref.CreateLaser(pBullet.Convert<ObjectClass>(), 0, laserWeapon, to);
            pBullet.Ref.DetonateAndUnInit(to);
        }


        public void StartIonStorm()
        {
            InIonStorm = true;
            Delay = 600;
        }

        private void UpdateAnim()
        {
            CheckAnim();




            if (!Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap && Owner.OwnerObject.Ref.Owner == HouseClass.Player && Owner.OwnerObject.Ref.Base.IsSelected)
            {
                pMana.Ref.Invisible = false;
            }
            else
            {
                pMana.Ref.Invisible = true;
            }

            pMana.Ref.Base.SetLocation(Owner.OwnerObject.Ref.Base.Base.GetCoords());
            var frame = (int)((charge / (double)chargeMax) * 20 - 1);
            if(frame<0)
            {
                frame = 0;
            }else if (frame > 19)
            {
                frame = 19;
            }
            pMana.Ref.Animation.Value = frame;
            pMana.Ref.Pause();
        }


        private void CheckAnim()
        {
            if (pMana.IsNull)
            {
                CreateAnim();
            }

        }

        private void CreateAnim()
        {
            if (!pMana.IsNull)
            {
                KillAnim();
            }

            var anim1 = YRMemory.Create<AnimClass>(AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("UnitChargeBar"), Owner.OwnerObject.Ref.Base.Base.GetCoords());
          
            pMana.Pointer = anim1;
        }

        private void KillAnim()
        {
            if (!pMana.IsNull)
            {
                pMana.Ref.TimeToDie = true;
                pMana.Ref.Base.UnInit();
                pMana.Pointer = IntPtr.Zero;
            }
        }

        public override void OnRemove()
        {
            KillAnim();
        }

    }

    [ScriptAlias(nameof(IonStormLauncherScript))]
    [Serializable]
    public class IonStormLauncherScript : SuperWeaponScriptable
    {
        public IonStormLauncherScript(SuperWeaponExt owner) : base(owner)
        {
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            var pojbs = ObjectFinder.FindTechnosNear(CellClass.Cell2Coord(cell), Game.CellSize * 6);
            foreach(var obj in pojbs)
            {
                if(obj.CastToTechno(out var ptechno))
                {
                    if (!ptechno.Ref.Owner.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                        continue;

                    var technoExt = TechnoExt.ExtMap.Find(ptechno);
                    if(!technoExt.IsNullOrExpired())
                    {
                        var component = technoExt.GameObject.GetComponent<PlanetShipScript>();
                        if (component != null)
                        {
                            component.StartIonStorm();
                        }
                    }
                }
            }
        }
    }
}
