using Extension.CW;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Data;
using System.Linq;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(Mk2UpdateSpecialScript))]
    public class Mk2UpdateSpecialScript : TechnoScriptable
    {
        public Mk2UpdateSpecialScript(TechnoExt owner) : base(owner)
        {

        }

        private static Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        private static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIIAnimWh");


        private int delay = 10;

        private bool inited = false;

        public override void OnUpdate()
        {
            if (!inited)
            {
                //如果是AI释放的
                if (!Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
                {
                    var technos4AI = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, p =>
                    {
                        var ext = TechnoExt.ExtMap.Find(p);
                        if (ext == null)
                            return false;

                        if (!p.Ref.Base.IsOnMap || p.Ref.Base.InLimbo)
                            return false;

                        var gext = ext.GameObject.GetTechnoGlobalComponent();
                        if (gext == null)
                            return false;

                        if (!gext.Data.CanMk2Update)
                            return false;

                        if (gext.MKIIUpdated)
                            return false;

                        return true;
                    }, FindRange.Allies).ToList();

                    if (technos4AI.Count() > 0)
                    {
                        var techno = technos4AI.OrderByDescending(t =>
                            {
                                if (!t.IsNullOrExpired())
                                {
                                    var location = t.OwnerObject.Ref.Base.Base.GetCoords();

                                    var count = technos4AI.Where(tAI =>
                                    {
                                        if (!tAI.IsNullOrExpired())
                                        {
                                            var tcoord = tAI.OwnerObject.Ref.Base.Base.GetCoords();
                                            var distance = new CoordStruct(tcoord.X, tcoord.Y, location.Z).DistanceFrom(location);
                                            return !double.IsNaN(distance) && distance <= 1000;
                                        }
                                        return false;
                                    }).Count();
                                    return count;
                                }
                                return 0;
                            }
                        ).FirstOrDefault();


                        if (!techno.IsNullOrExpired())
                        {
                            var ailocation = techno.OwnerObject.Ref.Base.Base.GetCoords();
                            Owner.OwnerObject.Ref.Base.SetLocation(ailocation);
                        }
                    }
                }
            }

            if (!inited)
            {
                inited = true;
                var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

                var technos = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, p =>
                {
                    var ext = TechnoExt.ExtMap.Find(p);
                    if (ext == null)
                        return false;

                    if (!p.Ref.Base.IsOnMap || p.Ref.Base.InLimbo)
                        return false;

                    var gext = ext.GameObject.GetTechnoGlobalComponent();
                    if (gext == null)
                        return false;

                    if (!gext.Data.CanMk2Update)
                        return false;

                    if(gext.MKIIUpdated)
                        return false;

                    var mycoords = p.Ref.Base.Base.GetCoords();
                    var distance = new CoordStruct(mycoords.X, mycoords.Y, location.Z).DistanceFrom(location);
                    if (double.IsNaN(distance) || distance > 1000)
                        return false;

                    return true;
                }, FindRange.Allies
                 ).OrderBy(
                         p =>
                         {
                             if (!p.IsNullOrExpired())
                             {
                                 var distance = p.OwnerObject.Ref.Base.Base.GetCoords().DistanceFrom(location);
                                 if (double.IsNaN(distance))
                                     return 65535;
                                 return distance;
                             }
                             else
                             {
                                 return 65535;
                             }
                         }
                 ).Take(9).ToList();

                foreach (var techno in technos)
                {
                    if (!techno.IsNullOrExpired())
                    {
                        if (techno.GameObject.GetComponent<ToBeMkUpdatedScript>() == null)
                        {
                            var pBullet = inviso.Ref.CreateBullet(techno.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, warhead, 100, false);
                            pBullet.Ref.DetonateAndUnInit(techno.OwnerObject.Ref.Base.Base.GetCoords());
                            techno.GameObject.CreateScriptComponent(nameof(ToBeMkUpdatedScript), "Delay To Attach MKII Update", techno);
                        }
                        //var pBullet = inviso.Ref.CreateBullet(techno.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, warhead, 100, false);
                        //pBullet.Ref.DetonateAndUnInit(techno.OwnerObject.Ref.Base.Base.GetCoords());
                        //techno.GameObject.GetTechnoGlobalComponent().MKIIUpdated = true;
                    }
                }
            }



            if (delay-- <= 0)
            {
                Owner.OwnerObject.Ref.Base.UnInit();
            }
            base.OnUpdate();
        }

    }

    [Serializable]
    [ScriptAlias(nameof(ToBeMkUpdatedScript))]
    public class ToBeMkUpdatedScript : TechnoScriptable
    {
        public ToBeMkUpdatedScript(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<BulletTypeClass> inviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");
        private static Pointer<WarheadTypeClass> warhead => WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find("MarkIISpWh");

        private int delay = 200;

        public override void OnUpdate()
        {
            if(Owner.IsNullOrExpired())
            {
                DetachFromParent();
                return;
            }

            if(delay--<=0)
            {
                var pBullet = inviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, 1, warhead, 100, false);
                pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                Owner.GameObject.GetTechnoGlobalComponent().MKIIUpdated = true;
                DetachFromParent();
            }
            
        }
    }
}
