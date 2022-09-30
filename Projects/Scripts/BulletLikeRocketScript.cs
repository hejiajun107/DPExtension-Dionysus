using Extension.Ext;
using Extension.Ext4CW.Untilities;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;

namespace Scripts
{

    [Serializable]
    public class BulletLikeRocketArgs : INIAutoConfig
    {
        [INIField(Key = "BulletLikeRocket.Type")]
        public string BulletLikeRocketType;
    }

    [Serializable]
    [ScriptAlias(nameof(BulletLikeRocketScript))]
    public class BulletLikeRocketScript : BulletScriptable
    {
        public BulletLikeRocketScript(BulletExt owner) : base(owner)
        {
        }

        private string technoType;

        public override void Awake()
        {
            //获取本体的INI
            var ini = this.CreateRulesIniComponentWith<BulletLikeRocketArgs>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            technoType = ini.Data.BulletLikeRocketType;
        }

        public override void Start()
        {
            var type = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(technoType);
            if (type == null)
                return;
            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;
            var techno = type.Ref.Base.CreateObject(Owner.OwnerObject.Ref.Owner.Ref.Owner).Convert<TechnoClass>();
            if (techno == null)
                return;
            var technoExt = TechnoExt.ExtMap.Find(techno);
            if (technoExt.IsNullOrExpired())
                return;

         
            var location = Owner.OwnerObject.Ref.Base.Base.GetCoords();
            var velocity = Owner.OwnerObject.Ref.Velocity;
            ++Game.IKnowWhatImDoing;
            technoExt.OwnerObject.Ref.Base.Put(Owner.OwnerObject.Ref.Base.Base.GetCoords(), GameUtil.Point2Dir(location, new CoordStruct(location.X + (int)velocity.X, location.Y + (int)velocity.Y, location.Z + (int)velocity.Z)));
            --Game.IKnowWhatImDoing;

            technoExt.GameObject.CreateScriptComponent(nameof(BulletRokcetScript), BulletRokcetScript.UniqueId, "BulletRokcetScript", technoExt, Owner);


        }


    }

    [Serializable]
    [ScriptAlias(nameof(BulletRokcetScript))]
    public class BulletRokcetScript : TechnoScriptable
    {
        public BulletRokcetScript(TechnoExt owner,BulletExt bullet) : base(owner)
        {
            Master = bullet;
        }

        public static int UniqueId = 202209071; 

        public BulletExt Master;

        public override void OnUpdate()
        {
            if(!Master.IsNullOrExpired())
            {
                //同步位置
                var location = Master.OwnerObject.Ref.Base.Base.GetCoords();
                var velocity = Master.OwnerObject.Ref.Velocity;


                if (Owner.OwnerObject.Ref.Base.Base.WhatAmI() != AbstractType.Building)
                {
                    Owner.OwnerObject.Convert<FootClass>().Ref.Locomotor.Lock();
                }

                Owner.OwnerObject.Ref.Base.SetLocation(location);
                //Owner.OwnerObject.Ref.Facing.set(GameUtil.Point2Dir(location,Master.OwnerObject.Ref.TargetCoords).ToDirStruct());
                Owner.OwnerObject.Ref.Facing.set(GameUtil.Point2Dir(location, new CoordStruct(location.X + (int)velocity.X, location.Y + (int)velocity.Y, location.Z + (int)velocity.Z)).ToDirStruct());

                if (Owner.OwnerObject.Ref.Base.Base.WhatAmI() == AbstractType.Building)
                {
                    Owner.OwnerObject.Ref.Base.MarkForRedraw();
                }
                else
                {
                    Owner.OwnerObject.Convert<FootClass>().Ref.Locomotor.Lock();
                }
            }
            else
            {
                DetachFromParent();
                Owner.OwnerObject.Ref.Base.UnInit();
            }
        }

        public override void OnRemove()
        {
            if(!Master.IsNullOrExpired())
            {
                if(Master.OwnerRef.Base.Health<=0)
                {
                    //强制引爆
                    Master.OwnerObject.Ref.DetonateAndUnInit(Owner.OwnerObject.Ref.Base.Base.GetCoords());
                }
                else
                {
                    Master.OwnerObject.Ref.Base.UnInit();
                }
            }
        }
    }
}
