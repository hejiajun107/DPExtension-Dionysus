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
    [ScriptAlias(nameof(KeepMoveSpeedScript))]
    [Serializable]
    public class KeepMoveSpeedScript : TechnoScriptable
    {
        public KeepMoveSpeedScript(TechnoExt owner) : base(owner)
        {
        }

        private int duration = 100;

        private int targetSpeed = 4;

        private int speed = 4;

        private double initMultiper = 1.0;

        private bool inited = false;

        public override void Awake()
        {
            var ini = GameObject.CreateRulesIniComponentWith<KeepMoveSpeedData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            speed = ini.Data.OSpeed;
            targetSpeed = ini.Data.Speed;
            duration = ini.Data.Duration;

            base.Awake();
        }


        public override void OnUpdate()
        {
            var pfoot = Owner.OwnerObject.Convert<FootClass>();
            if (!inited) {
                inited = true;
                initMultiper = pfoot.Ref.SpeedMultiplier;
            }

            if (duration-- > 0)
            {
                pfoot.Ref.SpeedMultiplier = ((double)targetSpeed / (double)speed);
            }
            else
            {
                pfoot.Ref.SpeedMultiplier = initMultiper;
                DetachFromParent();
            }
        }
    }

    
    public class KeepMoveSpeedData:INIAutoConfig
    {
        [INIField(Key = "KeepMoveSpeed.Duration")]
        public int Duration = 100;

        [INIField(Key = "KeepMoveSpeed.Speed")]
        public int Speed = 4;

        [INIField(Key = "Speed")]
        public int OSpeed = 4;
    }
}
