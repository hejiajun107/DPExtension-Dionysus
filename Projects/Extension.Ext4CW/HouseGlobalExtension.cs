using DynamicPatcher;
using Extension.CWUtilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.CW
{

    [Serializable]
    [GlobalScriptable(typeof(HouseExt))]
    public partial class HouseGlobalExtension : HouseScriptable
    {
        public HouseGlobalExtension(HouseExt owner) : base(owner)
        {
        }

        public Dictionary<string, int> TechnoMaxRank = new Dictionary<string, int>();

        public int NatashaNukeCount = 0;

        public override void OnUpdate()
        {
            if(NeedRecalcNumOfThunderBird == true)
            {
                NeedRecalcNumOfThunderBird = false;
                RecalcNumOfThunderBird();
            }
        }

        /// <summary>
        /// 羲和阳炎攻击位置
        /// </summary>
        public CoordStruct XHSunstrikeTarget1 { get; set; }
        public CoordStruct XHSunstrikeTarget2 { get; set; }
        public CoordStruct XHSunstrikeTarget3 { get; set; }
        public CoordStruct XHSunstrikeTarget4 { get; set; }


        /// <summary>
        /// 机场补贴发放次数
        /// </summary>
        public int AirportPaybackTime { get; set; } = 0;


        #region 雷鸟运输机
        /// <summary>
        /// 雷鸟运输机的数量
        /// </summary>
        public int NumOfThunderBird { get; set; } = 0;
        /// <summary>
        /// 是否需要重新计算雷鸟运输机的数量
        /// </summary>
        public bool NeedRecalcNumOfThunderBird { get; set; } = false;

        public void RecalcNumOfThunderBird()
        {
            NumOfThunderBird = Finder.FindTechno(Owner.OwnerObject, x => (x.Ref.Type.Ref.Base.Base.ID == "ORCAB" || x.Ref.Type.Ref.Base.Base.ID == "ORCAB2") && !x.Ref.Base.InLimbo && x.Ref.IsInPlayfield == true, FindRange.Owner).Count();
        }
        #endregion


    }


}
