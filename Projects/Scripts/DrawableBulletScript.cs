using DynamicPatcher;
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

    [Serializable]
    [ScriptAlias(nameof(DrawableBulletScript))]
    public class DrawableBulletScript : BulletScriptable
    {
        public DrawableBulletScript(BulletExt owner) : base(owner)
        {
        }

        private bool inited = false;

        public INIComponentWith<DrawableBulletData> ini;


        public override void Awake()
        {
            ini = this.CreateRulesIniComponentWith<DrawableBulletData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
        }

        public override void OnUpdate()
        {
            if(!inited)
            {
                inited = true;

                var data = ini.Data;

                if (data.IsLaser == true)
                {

                    Pointer<LaserDrawClass> pLaser = YRMemory.Create<LaserDrawClass>(Owner.OwnerObject.Ref.SourceCoords, Owner.OwnerObject.Ref.TargetCoords, 
                        data.LaserInnerColor == null ? new ColorStruct(0, 0, 0) : new ColorStruct(data.LaserInnerColor[0], data.LaserInnerColor[1], data.LaserInnerColor[2]), 
                        data.LaserOuterColor == null ? new ColorStruct(0, 0, 0) : new ColorStruct(data.LaserOuterColor[0], data.LaserOuterColor[1], data.LaserOuterColor[2]), 
                        data.LaserOuterSpread == null ? new ColorStruct(0, 0, 0) : new ColorStruct(data.LaserOuterSpread[0], data.LaserOuterSpread[1], data.LaserOuterSpread[2]), 
                        data.LaserDuration);
                    pLaser.Ref.IsHouseColor = data.IsHouseColor;
                    pLaser.Ref.Thickness = data.LaserThickness;
                }

            }
        }
    }

    [Serializable]
    public class DrawableBulletData:INIAutoConfig
    {
        [INIField(Key = "Bullet.IsLaser")]
        public bool IsLaser;
        [INIField(Key = "Bullet.LaserInnerColor")]
        public int[] LaserInnerColor = null;
        [INIField(Key = "Bullet.LaserOuterColor")]
        public int[] LaserOuterColor = null;
        [INIField(Key = "Bullet.LaserOuterSpread")]
        public int[] LaserOuterSpread = null;
        [INIField(Key = "Bullet.IsHouseColor")]
        public bool IsHouseColor = false;
        [INIField(Key = "Bullet.LaserThickness")]
        public int LaserThickness = 2;
        [INIField(Key = "Bullet.LaserDuration")]
        public int LaserDuration = 10;

    }
}
