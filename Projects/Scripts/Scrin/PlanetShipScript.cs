using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
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
        }

        private bool InIonStorm;
        private int Delay = 0;


        private static Pointer<AnimTypeClass> pblast => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("SCLIGHTNWAVE");
        private static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        private static Pointer<WarheadTypeClass> pWh => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("PlanetIonWh");


        public override void OnUpdate()
        {

            if (InIonStorm)
            {
                var height = Owner.OwnerObject.Ref.Base.GetHeight();
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords() - new CoordStruct(0,0,height);

                if (Delay % 20 == 0)
                {
                    var spread = 7 * Game.CellSize;
                    var target = new CoordStruct(location.X + MathEx.Random.Next(-spread, spread), location.Y + MathEx.Random.Next(-spread, spread), location.Z);
                    YRMemory.Create<AnimClass>(pblast, target);
                    var bullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 120, pWh, 100, true);
                    bullet.Ref.DetonateAndUnInit(target);
                }
                if (Delay-- <= 0)
                {
                    InIonStorm = false;
                }
            }
            base.OnUpdate();
        }

        public void StartIonStorm()
        {
            InIonStorm = true;
            Delay = 600;
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
