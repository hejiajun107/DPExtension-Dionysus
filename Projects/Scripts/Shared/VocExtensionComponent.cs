using DynamicPatcher;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Extension.Shared
{
    [Serializable]
    public class VocExtensionComponent
    {
        TechnoExt Owner;

        public VocExtensionComponent(TechnoExt owner)
        {
            Owner = owner;
        }

        int VocSp1;
        int VocSp2;
        int VocSp3;
		int VocSp4;
		int VocSp5;



		public void Awake()
        {
            var ini = Owner.GameObject.CreateRulesIniComponentWith<VocExtData>(Owner.OwnerObject.Ref.Type.Ref.Base.Base.ID);
            if(!string.IsNullOrEmpty(ini.Data.VocSpecial1))
            {
                var item = ini.Data.VocSpecial1;
                VocSp1 = VocClass.FindIndex(item);
            }

            if (!string.IsNullOrEmpty(ini.Data.VocSpecial2))
            {
                var item = ini.Data.VocSpecial2;
                VocSp2 = VocClass.FindIndex(item);
            }

            if (!string.IsNullOrEmpty(ini.Data.VocSpecial3))
            {
                var item = ini.Data.VocSpecial3;
                VocSp3 = VocClass.FindIndex(item);
            }


			if (!string.IsNullOrEmpty(ini.Data.VocSpecial4))
			{
				var item = ini.Data.VocSpecial4;
				VocSp4 = VocClass.FindIndex(item);
			}


			if (!string.IsNullOrEmpty(ini.Data.VocSpecial5))
			{
				var item = ini.Data.VocSpecial5;
				VocSp5 = VocClass.FindIndex(item);
			}
		}

        private void LoadSounds(List<int> array,List<string> items)
        {
            foreach (var item in items)
            {
                var idx = VocClass.FindIndex(item);
                if (idx > -1)
                {
                    array.Add(idx);
                }
            }
        }

        /// <summary>
        /// 播放扩展音效 一般音效1为技能1 音效2为技能2  音效3为空蓝提示
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="ownerOnly"></param>
        public void PlaySpecialVoice(int idx,bool ownerOnly)
        {
            int spVoice = -1;
            if(idx == 1)
            {
                spVoice = VocSp1;
            }else if(idx == 2)
            {
                spVoice = VocSp2;
            }else if (idx == 3)
            {
                spVoice = VocSp3;
            }else if(idx == 4)
            {
                spVoice = VocSp4;
            }else if(idx == 5)
            {
                spVoice = VocSp5;
            }

            if(spVoice == -1)
            {
                return;
            }

            if (ownerOnly) 
            {
                if (!Owner.OwnerObject.Ref.Owner.Ref.IsControlledByCurrentPlayer())
                    return;
            }

            Owner.OwnerObject.Ref.QueueVoice(spVoice);
        }

    }

    [Serializable]
    public class VertenceyWatcher
	{
		TechnoExt Owner;
        VocExtensionComponent _voc;

		public VertenceyWatcher(TechnoExt owner,VocExtensionComponent voc)
		{
			Owner = owner;
            _voc = voc;
		}

        private int delay = 100;

        private float lastV = 0;

        public int GetLevel()
        {
            var val = Owner.OwnerObject.Ref.Veterancy.Veterancy;
            if (val >= 0 & val < 1)
            {
                return 0;
            }
            else if (val >= 1 & val < 2)
            {
                return 1;
            }
            else if (val >= 2)
            {
                return 2;
            }
            else
                return 0;
        }

		public void Update()
        {
            int cLevel = GetLevel();
            if (delay > 0)
            {
				delay--;
				lastV = cLevel;
                return;
            }
            

            if(cLevel > lastV)
            {
                if(cLevel == 1)
                {
                    _voc.PlaySpecialVoice(4, true);
                }else if(cLevel == 2)
                {
					_voc.PlaySpecialVoice(5, true);
				}
			}
            lastV = GetLevel();

		}
    }

    /// <summary>
    /// dummy
    /// </summary>
    [Serializable]
    [ScriptAlias(nameof(VocExtScriptable))]
    public class VocExtScriptable : TechnoScriptable
    {
        public VocExtScriptable(TechnoExt owner) : base(owner)
        {
        }
    }

    public class VocExtData : INIAutoConfig
    {
        [INIField(Key = "VoiceExt.Special1")]
        public string VocSpecial1;
        [INIField(Key = "VoiceExt.Special2")]
        public string VocSpecial2;
        [INIField(Key = "VoiceExt.Special3")]
        public string VocSpecial3;
		[INIField(Key = "VoiceExt.Special4")]
		public string VocSpecial4;
		[INIField(Key = "VoiceExt.Special5")]
		public string VocSpecial5;
	}
}
