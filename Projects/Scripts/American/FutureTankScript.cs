using Extension.Coroutines;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.American
{
    [ScriptAlias(nameof(FutureTankScript))]
    [Serializable]
    public class FutureTankScript : TechnoScriptable
    {
        public FutureTankScript(TechnoExt owner) : base(owner)
        {
        }

        private int coolDown = 500;

        private int captureDelay = 100;

        public List<Snap> Snaps { get; set; } = new List<Snap>();

        private static Pointer<AnimTypeClass> anim => AnimTypeClass.ABSTRACTTYPE_ARRAY.Find("TIMECUTMINI");

        public override void OnUpdate()
        {
            if (coolDown > 0)
                coolDown--;

            if (captureDelay-- <= 0)
            {
                captureDelay = 100;
                if (Snaps.Count > 5)
                {
                    Snaps.RemoveAt(0);
                }
                Snaps.Add(new Snap(Owner.OwnerRef.Base.Health));
            }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if(coolDown<=0)
            {
                var estimateDamage = MapClass.GetTotalDamage(pDamage.Ref, pWH, Owner.OwnerRef.Type.Ref.Base.Armor, DistanceFromEpicenter);
                if (estimateDamage > 0)
                {
                    var hpmax = Owner.OwnerObject.Ref.Type.Ref.Base.Strength;
                    var health = Owner.OwnerRef.Base.Health;
                    if (health < hpmax * 0.3)
                    {
                        if(Snaps.Count()>0)
                        {
                            YRMemory.Create<AnimClass>(anim, Owner.OwnerObject.Ref.Base.Base.GetCoords());
                            coolDown = 1500;
                            var snap = Snaps[0];
                            Owner.GameObject.StartCoroutine(TimeRevert(snap.Health));
                        }
                    }
                }
            }   
        }

        IEnumerator TimeRevert(int health)
        {
            yield return new WaitForFrames(200);
            if (Owner.OwnerRef.Base.Health < health && !Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap)
            {
                Owner.OwnerRef.Base.Health = health;
            }
        }
    }

    [Serializable]
    public class Snap
    {
        public Snap(int health)
        {
            Health = health;
            //Coord = coord;
        }

        public int Health { get; set; }

        //public CoordStruct Coord { get; set; }
    }
}
