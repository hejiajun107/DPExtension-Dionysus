using Extension.EventSystems;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;
using Scripts.Tavern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Tavern
{
    [ScriptAlias(nameof(CardComponent))]
    [Serializable]
    public class CardComponent : TechnoScriptable
    {
        public CardComponent(TechnoExt owner) : base(owner)
        {
        }

        public CardType CardType { get; set; }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            
        }

        public override void Awake()
        {
            EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
            base.Awake();
        }
        public override void OnDeploy()
        {
            EventSystem.GScreen.RemovePermanentHandler(EventSystem.GScreen.GScreenRenderEvent, OnGScreenRender);
            base.OnDestroy();
        }

        public void OnGScreenRender(object sender, EventArgs args)
        {
            if (args is GScreenEventArgs gScreenEvtArgs)
            {
                if (!gScreenEvtArgs.IsLateRender)
                {
                    return;
                }

                if(CardType is not null)
                {
                    RenderPCX(CardType.Cameo, 0, 0, 0);
                }

                if(Owner.OwnerObject.Ref.Owner == HouseClass.Player && Owner.OwnerObject.Ref.Base.IsSelected)
                {
                    RenderDesc();
                }
            }
        }



        private void RenderPCX(string pcxName, int offsetX, int offsetY, int offsetZ)
        {
            var loaded = PCX.Instance.LoadFile(pcxName);
            var pcx = PCX.Instance.GetSurface(pcxName, Pointer<BytePalette>.Zero);
            RectangleStruct pcxBounds = new RectangleStruct(0, 0, pcx.Ref.Base.Base.Width, pcx.Ref.Base.Base.Height);
            Pointer<Surface> pSurface = Surface.Current;
            RectangleStruct rect = pSurface.Ref.GetRect();
            Point2D point = TacticalClass.Instance.Ref.CoordsToClient(Owner.OwnerObject.Ref.BaseAbstract.GetCoords() + new CoordStruct(offsetX, offsetY, offsetZ));
            var source = new RectangleStruct(point.X, point.Y, pcx.Ref.Base.Base.Width, pcx.Ref.Base.Base.Height);
            PCX.Instance.BlitToSurface(source.GetThisPointer(), pSurface.Convert<DSurface>(), pcx);
        }

        private void RenderDesc()
        {
            return;
        }

    }

    [Serializable]
    public class CardType
    {
        /// <summary>
        /// 卡牌注册名，需要唯一，可与单位注册名一致
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 对应单位的注册名
        /// </summary>
        public string TechnoType { get; set; }

        /// <summary>
        /// 图标,仅PCX
        /// </summary>
        public string Cameo { get; set; }
    }
}
