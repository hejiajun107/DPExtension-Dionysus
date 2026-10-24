using DynamicPatcher;
using PatcherYRpp;
using System;

namespace Extension.UI
{
    //
    // 摘要:
    //     教程面板底部的导航按钮（上一页 / 下一页 / 关闭）。
    //     图标为 PCX（文件名来自 [TutorialUI] 配置），加载失败时画占位色方块，便于先看布局。
    //     置灰（Enabled = false）时什么都不画（保持面板底色），不响应点击。
    public class TutorialNavButton : Control
    {
        // 按钮图标（PCX 文件名，空表示不画图标）
        public string IconPCX;

        // 是否可用（分页到边界时置灰）
        public bool Enabled = true;

        // 点击回调
        private readonly Action _clickAction;

        // 可用但无图标时的占位色（暗灰）
        private static readonly ColorStruct PlaceholderColor = new ColorStruct(90, 90, 90);

        public TutorialNavButton(string iconPCX, Action clickAction)
        {
            IconPCX = iconPCX ?? string.Empty;
            _clickAction = clickAction;
            ClientRectangle = new RectangleStruct(0, 0,
                TutorialConfig.ButtonWidth, TutorialConfig.ButtonHeight);
        }

        protected override void OnDraw()
        {
            base.OnDraw();

            try
            {
                Pointer<Surface> pSurface = Surface.Current;
                if (pSurface.IsNull || ClientRectangle.Width <= 0 || ClientRectangle.Height <= 0)
                {
                    return;
                }

                // 置灰：什么都不画，露出面板底色
                if (!Enabled)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(IconPCX) && DrawIcon(pSurface))
                {
                    return;
                }

                // 无图标素材：占位方块
                pSurface.Ref.FillRect(ClientRectangle, Drawing.RGB2DWORD(PlaceholderColor));
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     在按钮矩形内居中绘制 PCX 图标，裁剪到按钮矩形与视口内。成功返回 true。
        private bool DrawIcon(Pointer<Surface> pSurface)
        {
            PCX.Instance.LoadFile(IconPCX);
            Pointer<BSurface> pPCX = PCX.Instance.GetSurface(IconPCX, Pointer<BytePalette>.Zero);
            if (pPCX.IsNull)
            {
                return false;
            }

            int w = pPCX.Ref.BaseSurface.Width;
            int h = pPCX.Ref.BaseSurface.Height;
            if (w <= 0 || h <= 0)
            {
                return false;
            }

            RectangleStruct area = ClientRectangle;
            int x = area.X + Math.Max(0, (area.Width - w) / 2);
            int y = area.Y + Math.Max(0, (area.Height - h) / 2);

            RectangleStruct view = pSurface.Ref.GetRect();
            int left = Math.Max(Math.Max(x, view.X), area.X);
            int top = Math.Max(Math.Max(y, view.Y), area.Y);
            int right = Math.Min(Math.Min(x + w, view.X + view.Width), area.X + area.Width);
            int bottom = Math.Min(Math.Min(y + h, view.Y + view.Height), area.Y + area.Height);
            if (right <= left || bottom <= top)
            {
                return false;
            }

            RectangleStruct clip = new RectangleStruct(left, top, right - left, bottom - top);
            PCX.Instance.BlitToSurface(clip.GetThisPointer(), pSurface.Convert<DSurface>(), pPCX);
            return true;
        }

        protected internal override void OnClick()
        {
            if (!Enabled)
            {
                return;
            }
            try
            {
                _clickAction?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }
    }
}
