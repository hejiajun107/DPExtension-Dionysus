using DynamicPatcher;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripts
{

    [Serializable]
    public class AttachEffectWarheadConfig : INIAutoConfig
    {

        [INIField(Key = "AttachEffect.Scripts")]
        public string AttachEffectScript = string.Empty;

        [INIField(Key = "AttachEffect.Scripts.Duration")]
        public int AttachEffectDuration = 0;

        [INIField(Key = "AttachEffect.Scripts.Cumulative")]
        public bool AttachEffectCumulative = false;

        
        //Ares
        [INIField(Key = "AllowZeroDamage")]
        public bool AllowZeroDamage = false;

        [INIField(Key = "AffectsEnemies")]
        public bool AffectsEnemies = true;


    }

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    public partial class AttachEffectScriptExtension : TechnoScriptable
    {
        private List<AttachEffectScriptable> _attachEffectScriptables = new List<AttachEffectScriptable>();


        INIComponentWith<AttachEffectWarheadConfig> INI;

        public AttachEffectScriptExtension(TechnoExt owner) : base(owner)
        {
        }

        public override void Awake()
        {
            INI = this.CreateRulesIniComponentWith<AttachEffectWarheadConfig>("Special");
        }

        public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            foreach (var ae in _attachEffectScriptables)
            {
                ae.OnPut(coord, faceDir);
            }
        }

        public override void OnUpdate()
        {
            foreach (var ae in _attachEffectScriptables)
            {
                if (ae.Duration > 0)
                {
                    ae.Duration--;
                    ae.OnUpdate();
                }
            }
            ClearExpiredAttachEffect();
        }

        public override void OnRemove()
        {
            foreach (var ae in _attachEffectScriptables)
            {
                ae.OnRemove();
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            foreach (var ae in _attachEffectScriptables)
            {
                ae.OnFire(pTarget, weaponIndex);
            }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int DistanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool IgnoreDefenses, bool PreventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            foreach (var ae in _attachEffectScriptables)
            {
                ae.OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse);
            }

            INI.Section = pWH.Ref.Base.ID;
            var data = INI.Data;

            if (pAttackingHouse.IsNull)
                return;

            if (data != null)
            {
                if (pAttackingHouse.Ref.IsAlliedWith(Owner.OwnerObject.Ref.Owner))
                {
                    if (!pWH.Ref.AffectsAllies)
                        return;
                }
                else
                {
                    if (!data.AffectsEnemies)
                        return;
                }

                if (MapClass.GetTotalDamage(10000, pWH, Owner.OwnerObject.Ref.Type.Ref.Base.Armor, DistanceFromEpicenter) != 0 || data.AllowZeroDamage)
                {
                    if (!string.IsNullOrEmpty(data.AttachEffectScript))
                    {
                        var currentScript = _attachEffectScriptables.Where(s => s.ScriptName == data.AttachEffectScript).FirstOrDefault();

                        if (currentScript != null && !data.AttachEffectCumulative)
                        {
                            currentScript.OnAttachEffectRecieveNew(data.AttachEffectDuration, pDamage, pWH, pAttacker, pAttackingHouse);
                        }
                        else
                        {
                            if (data.AttachEffectDuration > 0)
                            {
                                var script = ScriptManager.GetScript(data.AttachEffectScript);
                                currentScript = ScriptManager.CreateScriptable(script, Owner) as AttachEffectScriptable;
                                currentScript.Duration = data.AttachEffectDuration;
                                currentScript.OnAttachEffectPut(pDamage, pWH, pAttacker, pAttackingHouse);
                                _attachEffectScriptables.Add(currentScript);
                            }
                        }
                    }

                }

            }

        }


        private void ClearExpiredAttachEffect()
        {
            if (_attachEffectScriptables.Any())
            {
                for (var i = _attachEffectScriptables.Count() - 1; i >= 0; i--)
                {
                    var ae = _attachEffectScriptables[i];
                    if (ae.Duration <= 0)
                    {
                        ae.OnAttachEffectRemove();
                        _attachEffectScriptables.Remove(ae);
                    }
                }
            }
        }

    }
}
