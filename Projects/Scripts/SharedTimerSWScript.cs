using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using System;

namespace Scripts
{
    [Serializable]

    public class SharedTimerSWScript : SuperWeaponScriptable
    {
        public SharedTimerSWScript(SuperWeaponExt owner) : base(owner)
        {
        }

        private SharedTimerSWData data;

        public override void Awake()
        {
            var ini = this.CreateRulesIniComponentWith<SharedTimerSWData>(Owner.OwnerObject.Ref.Type.Ref.Base.ID);
            data = ini.Data;
        }

        public override void OnLaunch(CellStruct cell, bool isPlayer)
        {
            var pSuper = Owner.OwnerObject;
            var owner = Owner.OwnerObject.Ref.Owner;


            //Pointer<SuperClass> pSuper = Owner.OwnerObject;
            //DecoratorComponent decorator = Owner.DecoratorComponent;

            //if (pSuper.Ref.IsCharged)
            //{
            //    if (decorator.GetValue<int>(LevelTag) > 0)
            //    {
            //        decorator.SetValue(LaunchedTag, true);
            //    }
            //    else
            //    {
            //        decorator.SetValue(BaseChargedTag, false);
            //        pSuper.Ref.UIName = pSuper.Ref.Type.Ref.Base.UIName;
            //    }
            //}
        }
    }

    [Serializable]
    public class SharedTimerSWData : INIAutoConfig
    {
        [INIField(Key = "SuperWeapons")]
        public string[] SuperWeapons;

        [INIField(Key = "SuperWeapon")]
        public string SuperWeapon;

        [INIField(Key = "RechargeTime")]
        public double RechargeTime;
    }

}
