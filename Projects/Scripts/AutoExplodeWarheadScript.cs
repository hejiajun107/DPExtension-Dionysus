using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts
{
    [ScriptAlias(nameof(AutoExplodeWarheadScript))]
    [Serializable]
    public class AutoExplodeWarheadScript : TechnoScriptable
    {
        public AutoExplodeWarheadScript(TechnoExt owner) : base(owner)
        {
        }

        static Pointer<BulletTypeClass> pInviso => BulletTypeClass.ABSTRACTTYPE_ARRAY.Find("Invisible");

        public string warhead;
        public int damage;
        public int delay;
        public int current;

        public override void Awake()
        {
            var ini = this.CreateRulesIniComponentWith<AutoExplodeWarheadData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            damage = ini.Data.Damage;
            warhead = ini.Data.Warhead;
            delay = ini.Data.Delay;
            current = delay;
        }

        public override void OnUpdate()
        {
            if (current-- > 0)
                return;

            current = delay;

            if (Owner.OwnerObject.Ref.Base.IsAlive && Owner.OwnerObject.Ref.Base.IsOnMap && Owner.OwnerObject.Ref.Base.InLimbo == false)
            {
                var wh = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find(warhead);
                if (wh.IsNotNull)
                {
                    Pointer<BulletClass> pBullet = pInviso.Ref.CreateBullet(Owner.OwnerObject.Convert<AbstractClass>(), Owner.OwnerObject, damage, wh, 100, false);
                    pBullet.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
            }
        }
    }

    public class AutoExplodeWarheadData : INIAutoConfig
    {
        [INIField(Key = "AutoExplode.Damage")]
        public int Damage = 10;

        [INIField(Key = "AutoExplode.Warhead")]
        public string Warhead = "";

        [INIField(Key = "AutoExplode.Delay")]
        public int Delay = 5;

    }
}
