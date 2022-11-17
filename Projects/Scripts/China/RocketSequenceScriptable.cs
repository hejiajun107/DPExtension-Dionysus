using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(RocketSequenceScriptable))]
    public class RocketSequenceScriptable : TechnoScriptable
    {
        public RocketSequenceScriptable(TechnoExt owner) : base(owner)
        {

        }

        private List<CoordStruct> FLHS = new List<CoordStruct>();

        private Queue<TechnoExt> targets = new Queue<TechnoExt>();

        public override void Awake()
        {
            var settingINI = this.CreateRulesIniComponentWith<RocketSequenceData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            var data = settingINI.Data;
            rof = data.RocketRof;
            if (data.FLH0 != null)
            {
                FLHS.Add(ConvertToPosition(data.FLH0));
            }

            if (data.FLH1 != null)
            {
                FLHS.Add(ConvertToPosition(data.FLH1));
            }


            if (data.FLH2 != null)
            {
                FLHS.Add(ConvertToPosition(data.FLH2));
            }

            if (data.FLH3 != null)
            {
                FLHS.Add(ConvertToPosition(data.FLH3));
            }

            if (data.FLH4 != null)
            {
                FLHS.Add(ConvertToPosition(data.FLH4));
            }

            if (data.FLH5 != null)
            {
                FLHS.Add(ConvertToPosition(data.FLH5));
            }

            if (data.FLH6 != null)
            {
                FLHS.Add(ConvertToPosition(data.FLH6));
            }

            if (data.FLH7 != null)
            {
                FLHS.Add(ConvertToPosition(data.FLH7));
            }
        }

        private CoordStruct ConvertToPosition(int[] arr)
        {
            if (arr.Length >= 3)
            {
                return new CoordStruct(arr[0], arr[1], arr[2]);
            }
            else
            {
                return new CoordStruct(0, 0, 0);
            }
        }

        private int delay = 0;

        private int rof = 5;

        private int currentBurst = 0;

        public override void OnUpdate()
        {

            if (delay <= 0)
            {
                if (targets.Count() > 0)
                {
                    var target = targets.Dequeue();
                    if (!target.IsNullOrExpired())
                    {
                        if (currentBurst > FLHS.Count() - 1)
                            currentBurst = 0;

                        delay = rof;

                        var flh = FLHS[currentBurst];
                        var currentPosition = Owner.OwnerObject.Ref.Base.Base.GetCoords();
                        var firePosition = ExHelper.GetFLHAbsoluteCoords(Owner.OwnerObject, flh, false);

                        Owner.OwnerObject.Ref.Base.SetLocation(firePosition);
                        Owner.OwnerObject.Ref.Fire_NotVirtual(target.OwnerObject.Convert<AbstractClass>(), 1);
                        Owner.OwnerObject.Ref.Base.SetLocation(currentPosition);

                        currentBurst++;
                    }
                }
            }
            else
            {
                delay--;
            }
            base.OnUpdate();
        }

        public void AddTarget(TechnoExt ext)
        {
            targets.Enqueue(ext);
        }

    }

    [Serializable]

    public class RocketSequenceData : INIAutoConfig
    {
        [INIField(Key = "RocketSequence.Rof")]
        public int RocketRof = 5;

        [INIField(Key = "RocketSequence.FLH0")]
        public int[] FLH0 = null;

        [INIField(Key = "RocketSequence.FLH1")]
        public int[] FLH1 = null;

        [INIField(Key = "RocketSequence.FLH2")]
        public int[] FLH2 = null;

        [INIField(Key = "RocketSequence.FLH3")]
        public int[] FLH3 = null;

        [INIField(Key = "RocketSequence.FLH4")]
        public int[] FLH4 = null;

        [INIField(Key = "RocketSequence.FLH5")]
        public int[] FLH5 = null;

        [INIField(Key = "RocketSequence.FLH6")]
        public int[] FLH6 = null;

        [INIField(Key = "RocketSequence.FLH7")]
        public int[] FLH7 = null;

    }


    [Serializable]
    [ScriptAlias(nameof(GYCRocketAttachEffectScript))]
    public class GYCRocketAttachEffectScript : AttachEffectScriptable
    {
        public GYCRocketAttachEffectScript(TechnoExt owner) : base(owner)
        {
        }

        private TechnoExt attacker;

        bool inited = false;

        public override void OnUpdate()
        {

            if (!inited)
            {
                inited = true;
                if (attacker != null && !attacker.IsNullOrExpired())
                {
                    var rocketTechno = attacker.GameObject.GetComponent<RocketSequenceScriptable>();
                    if (rocketTechno != null)
                        rocketTechno.AddTarget(Owner);
                }
            }


            base.OnUpdate();
        }

        public override void OnAttachEffectPut(Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            //获取发射者
            if (!pAttacker.IsNull)
            {
                if (pAttacker.CastToTechno(out var ptAttacker))
                {
                    attacker = (TechnoExt.ExtMap.Find(ptAttacker));
                }
            }
            base.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
        }

        public override void OnAttachEffectRecieveNew(int duration, Pointer<int> pDamage, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            return;
        }


    }


}
