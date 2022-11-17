using Extension.Ext;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Linq;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(NCannonTargetScript))]
    public class NCannonTargetScript : TechnoScriptable
    {
        public NCannonTargetScript(TechnoExt owner) : base(owner) { }

        private bool Inited = false;

        private int delay = 150;

        public override void OnUpdate()
        {
            base.OnUpdate();

            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();

            if (Inited == false)
            {
                Inited = true;
                //计算水平距离
                var technos = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, t => (t.Ref.Type.Ref.Base.Base.ID == "NCANNON" || t.Ref.Type.Ref.Base.Base.ID == "GYCBC" || t.Ref.Type.Ref.Base.Base.ID == "V5" || t.Ref.Type.Ref.Base.Base.ID == "GYCCANNONBU") && t.Ref.Base.InLimbo == false && t.Ref.Base.Base.GetCoords().DistanceFrom(new CoordStruct(location.X, location.Y, t.Ref.Base.Base.GetCoords().Z)) <= 16080, FindRange.Owner);

                if (technos != null && technos.Count() > 0)
                {
                    var target = Owner.OwnerObject.Ref.Target;

                    for (var i = technos.Count - 1; i >= 0; i--)
                    {
                        var techno = technos[i];

                        if (techno.OwnerObject.Ref.Type.Ref.Base.Base.ID == "GYCBC")
                        {
                            techno.OwnerObject.Ref.SetTarget(default);
                            continue;
                        }

                        if (!techno.IsNullOrExpired())
                        {
                            techno.OwnerObject.Ref.SetTarget(Owner.OwnerObject.Convert<AbstractClass>());
                        }
                    }
                }
            }

            if (delay-- <= 0)
            {
                Owner.OwnerObject.Ref.Base.UnInit();
            }

        }

    }
}
