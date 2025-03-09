using DynamicPatcher;
using Extension.EventSystems;
using Extension.Ext;
using Extension.Ext4CW;
using Extension.Script;
using Extension.Utilities;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using System;
using System.Runtime.InteropServices;

namespace DpLib.Scripts.China
{
    [Serializable]
    [ScriptAlias(nameof(SupportCenterScript))]

    public class SupportCenterScript : TechnoScriptable
    {
        public SupportCenterScript(TechnoExt owner) : base(owner)
        {
        }

        private static Pointer<TechnoTypeClass> targetType => TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find("GATP2");

        private int uavCount = 0;
        private int uavRof = 300;



		public override void OnPut(CoordStruct coord, Direction faceDir)
        {
            base.OnPut(coord, faceDir);

            if (Owner.OwnerObject.Ref.Owner.IsNull)
                return;

            if (Owner.OwnerObject.Ref.Owner.Ref.ControlledByHuman())
            {
                var exts = Finder.FindTechno(Owner.OwnerObject.Ref.Owner, t => t.Ref.Type.Ref.Base.Base.ID == "E9" && t.Ref.IsInPlayfield == true, FindRange.Owner);

                foreach (var ext in exts)
                {
                    if (!ext.IsNullOrExpired())
                    {
                        ext.OwnerObject.Convert<InfantryClass>().Ref.Type = targetType.Convert<InfantryTypeClass>();
                    }
                }
            }
        }

		public override void OnUpdate()
		{
			if (uavRof > 0)
			{
				uavRof--;
			}
          
            if(Owner.TryGetHouseGlobalExtension(out var houseExt))
            {
                if(uavRof <= 0)
                {
					uavRof = 300;
					houseExt.UAVCount = houseExt.UAVCount >= 100 ? 100 : (houseExt.UAVCount + 1);

				}
				uavCount = houseExt.UAVCount;
			}
		}


		public override void Awake()
		{
			EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
		}

		public override void OnDestroy()
		{
			EventSystem.GScreen.RemoveTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
		}


		public void OnGScreenRender(object sender, EventArgs args)
		{
			if (args is GScreenEventArgs gScreenEvtArgs)
			{
				if (!gScreenEvtArgs.IsLateRender)
				{
					return;
				}

				if (!Owner.OwnerObject.Ref.Base.InLimbo && Owner.OwnerObject.Ref.Base.IsOnMap && Owner.OwnerObject.Ref.Owner == HouseClass.Player && Owner.OwnerObject.Ref.Base.IsSelected)
				{
					if (FileSystem.TyrLoadSHPFile("uavcountbar.shp", out Pointer<SHPStruct> pCustomSHP))
					{
						Pointer<Surface> pSurface = Surface.Current;
						RectangleStruct rect = pSurface.Ref.GetRect();
						Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.BaseAbstract.GetCoords() + new CoordStruct(0, 0, 1000));
						{
							var frame = uavCount;

							pSurface.Ref.DrawSHP(FileSystem.UNITx_PAL, pCustomSHP, frame, point, rect.GetThisPointer());
						}
					}
				}
			}
		}

	}
}
