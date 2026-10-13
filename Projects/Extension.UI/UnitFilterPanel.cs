using DynamicPatcher;
using Extension.UI.Core;
using PatcherYRpp;
using System;
using System.Collections.Generic;

namespace Extension.UI
{
    //
    // 摘要:
    //     单位过滤面板。3×3 网格，扫描当前选中单位（CurrentObjects）提取不同类型，
    //     每类一格显示 cameo，不足 9 格时保留布局用黑色空格占位。
    //     点击某格过滤当前选中只保留该类型。
    //     外边框用 uiext.pcx 九宫格切片绘制：4 角原尺寸贴角点、4 边单方向平铺、中心不画（被按钮占）。
    //     切片边宽固定 8px（BorderSlice）。
    //     位置：屏幕左侧（ViewBound 左边缘），底部对齐超武栏底部，超武栏右移腾位。
    //     联机安全：仅操作本地 CurrentObjects 选中状态。
    public class UnitFilterPanel : Panel
    {
        // 每格尺寸（与原版 cameo 一致）
        private const int CellWidth = 60;
        private const int CellHeight = 48;
        // 格间距
        private const int CellInterval = 2;
        // 贴底显示（面板底部对齐 ViewBound 底边）
        private const int BottomMargin = 0;

        // 3×3 按钮区域尺寸（不含边框）
        public const int GridWidth = 3 * CellWidth + 2 * CellInterval;   // 184
        public const int GridHeight = 3 * CellHeight + 2 * CellInterval;  // 148

        // 九宫格切片边宽（固定 8px）：uiext.pcx 内 4 角 / 4 边 / 中心 的切线距离。
        // 素材宽高须 >= 2 * BorderSlice。
        public const int BorderSlice = 8;

        // 面板总尺寸（含边框），供超武栏定位时引用，避免重叠
        public const int TotalWidth = GridWidth + 2 * BorderSlice;    // 200
        public const int TotalHeight = GridHeight + 2 * BorderSlice;  // 164

        // 与超武栏的水平间距
        public const int SiblingGap = 4;

        // 面板底色（纯黑）
        private static readonly ColorStruct BackgroundColor = new ColorStruct(0, 0, 0);

        // 边框素材（懒加载）：uiext.pcx 整张图，加载为 BSurface
        private Pointer<BSurface> _pBorderPCX;
        private bool _borderLoaded;

        // 九宫格 8 个切片 surface（4 角 + 4 边，中心不画）。
        // 预切片后每块用 PCX.BlitToSurface 绘制，由引擎处理透明色（洋红 0xF81F）。
        private YRClassHandle<BSurface> _sliceCornerTL;
        private YRClassHandle<BSurface> _sliceCornerTR;
        private YRClassHandle<BSurface> _sliceCornerBL;
        private YRClassHandle<BSurface> _sliceCornerBR;
        private YRClassHandle<BSurface> _sliceEdgeTop;
        private YRClassHandle<BSurface> _sliceEdgeBottom;
        private YRClassHandle<BSurface> _sliceEdgeLeft;
        private YRClassHandle<BSurface> _sliceEdgeRight;

        // 上次扫描到的分组键列表（GroupAs 字符串），用于检测变化、避免每帧重建按钮
        private List<string> _lastGroupKeys = new List<string>();

        // 上一次 ViewBound，用于检测屏幕区域变化以重新定位
        private RectangleStruct _lastViewBound;

        static UnitFilterPanel()
        {
            // 默认纵向堆叠，但我们重写 ArrangeChildren 做 3×3 布局
        }

        public UnitFilterPanel()
        {
            Orientation = PanelOrientation.Vertical;
            ClientRectangle = new RectangleStruct(0, 0, GridWidth, GridHeight);
        }

        //
        // 摘要:
        //     懒加载边框素材 uiext.pcx（整张图），并预切成 8 个九宫格切片 surface。
        //     切片后每块用 PCX.BlitToSurface 绘制，由引擎处理透明色（洋红 0xF81F）。
        //     加载失败则无边框（厚度 0），按钮区域 = 整个面板。
        private void EnsureBorderLoaded()
        {
            if (_borderLoaded)
            {
                return;
            }
            _borderLoaded = true;

            try
            {
                PCX.Instance.LoadFile("uiext.pcx");
                _pBorderPCX = PCX.Instance.GetSurface("uiext.pcx", Pointer<BytePalette>.Zero);
                if (_pBorderPCX.IsNull)
                {
                    return;
                }

                int srcW = _pBorderPCX.Ref.BaseSurface.Width;
                int srcH = _pBorderPCX.Ref.BaseSurface.Height;
                int bs = BorderSlice;

                if (srcW < 2 * bs || srcH < 2 * bs)
                {
                    return;
                }

                int edgeW = srcW - 2 * bs;
                int edgeH = srcH - 2 * bs;

                // 4 角：1:1 裁剪
                _sliceCornerTL = CreateCornerSlice(_pBorderPCX, 0, 0, bs, bs);
                _sliceCornerTR = CreateCornerSlice(_pBorderPCX, srcW - bs, 0, bs, bs);
                _sliceCornerBL = CreateCornerSlice(_pBorderPCX, 0, srcH - bs, bs, bs);
                _sliceCornerBR = CreateCornerSlice(_pBorderPCX, srcW - bs, srcH - bs, bs, bs);

                // 4 边：直接做成目标尺寸（面板尺寸固定），源边条 >= 目标时中心裁剪，< 目标时平铺
                _sliceEdgeTop = CreateEdgeSlice(_pBorderPCX, bs, 0, edgeW, bs, GridWidth, bs);
                _sliceEdgeBottom = CreateEdgeSlice(_pBorderPCX, bs, srcH - bs, edgeW, bs, GridWidth, bs);
                _sliceEdgeLeft = CreateEdgeSlice(_pBorderPCX, 0, bs, bs, edgeH, bs, GridHeight);
                _sliceEdgeRight = CreateEdgeSlice(_pBorderPCX, srcW - bs, bs, bs, edgeH, bs, GridHeight);

                // 边框尺寸变化，面板总尺寸需更新 → 触发重排
                _isDirty = true;
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        // BSurface 每像素字节数。YR 的 BSurface 恒为 16bpp（游戏内部渲染 565，
        // BSurface::Allocate 硬编码 2 字节/像素），不要用 GetBytesPerPixel 虚函数。
        private const int SliceBpp = 2;

        // 引擎的 PCX 透明色是按“精确值”匹配的（0xF81F = 565 下的 #FF00FF），不是容差、
        // 也不是 alpha：只有与 0xF81F 完全相等的像素才会被剔除，因此素材里的抗锯齿过渡
        // 像素会漏过 colorkey，渲染成粉色残点。
        //
        // uiext.pcx 里这类像素只有 17 个，且全部集中在画布四个角，构成边框“圆角”的
        // 抗锯齿过渡（斜向过渡带）。如果简单地把它们抠成透明，圆角会失去过渡、看起来
        // 被“切掉一块”。这里改成：把它们按“向面板黑底混合”的方式重写——
        // 取邻域内最近的非洋红像素作为前景色 F，用绿通道估算前景占比（洋红 G=0），
        // 再按该比例把 F 混向黑，从而得到一条自然的（深色）抗锯齿过渡。
        //
        // 只对角切片调用；直线边条上不存在这类像素，不做任何处理。
        private static unsafe void FixCornerFringe(YRClassHandle<BSurface> slice, int w, int h)
        {
            ushort* p = (ushort*)slice.Ref.BaseSurface.Buffer.Value;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    ushort v = p[y * w + x];
                    int r = (v >> 11) & 0x1F;
                    int g = (v >> 5) & 0x3F;
                    int b = v & 0x1F;
                    if (!(r >= 10 && b >= 10 && g <= 8))
                    {
                        continue; // 非洋红过渡像素：不动
                    }

                    // 在 5×5 邻域内找最近的非洋红、非全黑像素作为前景参考色
                    int fr = -1, fg = 0, fb = 0, best = int.MaxValue;
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            if (dx == 0 && dy == 0) { continue; }
                            int nx = x + dx, ny = y + dy;
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h) { continue; }
                            ushort nv = p[ny * w + nx];
                            if (nv == 0) { continue; }
                            int nr = (nv >> 11) & 0x1F;
                            int ng = (nv >> 5) & 0x3F;
                            int nb = nv & 0x1F;
                            if (nr >= 10 && nb >= 10 && ng <= 8) { continue; } // 仍是洋红过渡
                            int d = dx * dx + dy * dy;
                            if (d < best) { best = d; fr = nr; fg = ng; fb = nb; }
                        }
                    }

                    if (fr < 0)
                    {
                        p[y * w + x] = 0; // 无参考：直接按黑底
                        continue;
                    }

                    // 前景占比：洋红的绿通道为 0，故用 绿/绿F 估算过渡程度，再混向黑
                    int ratio = fg > 0 ? Math.Min(63, g * 63 / fg) : 0;
                    p[y * w + x] = (ushort)(((fr * ratio / 63) << 11)
                                          | ((fg * ratio / 63) << 5)
                                          | (fb * ratio / 63));
                }
            }
        }

        //
        // 摘要:
        //     从 PCX 源 surface 1:1 截取 (sx,sy,sw,sh) 区域到新 BSurface。
        //     直接内存拷贝（含透明色像素），不走 Surface.Blit——
        //     BSurface 是纯内存 buffer surface，Blit 虚函数不会写入其 buffer，
        //     且 AllocHGlobal 不清零，会画出堆垃圾（白线/乱色）。
        private unsafe YRClassHandle<BSurface> CreateCornerSlice(Pointer<BSurface> pSrcPCX, int sx, int sy, int sw, int sh)
        {
            var slice = new YRClassHandle<BSurface>(sw, sh);
            slice.Ref.Allocate(SliceBpp);

            byte* srcBase = (byte*)pSrcPCX.Ref.BaseSurface.Buffer.Value;
            byte* dstBase = (byte*)slice.Ref.BaseSurface.Buffer.Value;
            int srcPitch = pSrcPCX.Ref.BaseSurface.Width * SliceBpp;
            int rowBytes = sw * SliceBpp;

            for (int y = 0; y < sh; y++)
            {
                byte* s = srcBase + (sy + y) * srcPitch + sx * SliceBpp;
                Buffer.MemoryCopy(s, dstBase + y * rowBytes, rowBytes, rowBytes);
            }
            FixCornerFringe(slice, sw, sh);
            return slice;
        }

        //
        // 摘要:
        //     从 PCX 源 surface 的边条区域 (sx,sy,sw,sh) 生成目标尺寸 (dstW,dstH) 的边切片。
        //     源条 >= 目标：中心裁剪（保持对称设计的对称性）；源条 < 目标：平铺重复。
        //     切片在加载时一次生成目标尺寸，绘制时 1:1 blit，
        //     不依赖引擎 BlitToSurface 的截断/缩放语义。
        private unsafe YRClassHandle<BSurface> CreateEdgeSlice(Pointer<BSurface> pSrcPCX,
            int sx, int sy, int sw, int sh, int dstW, int dstH)
        {
            var slice = new YRClassHandle<BSurface>(dstW, dstH);
            slice.Ref.Allocate(SliceBpp);

            byte* srcBase = (byte*)pSrcPCX.Ref.BaseSurface.Buffer.Value;
            byte* dstBase = (byte*)slice.Ref.BaseSurface.Buffer.Value;
            int srcPitch = pSrcPCX.Ref.BaseSurface.Width * SliceBpp;
            int dstRowBytes = dstW * SliceBpp;

            // 横向边条（上/下边）：沿 X 轴填充
            if (sw >= sh && dstW >= dstH)
            {
                if (sw >= dstW)
                {
                    // 中心裁剪
                    int offset = (sw - dstW) / 2;
                    for (int y = 0; y < dstH; y++)
                    {
                        byte* s = srcBase + (sy + y) * srcPitch + (sx + offset) * SliceBpp;
                        Buffer.MemoryCopy(s, dstBase + y * dstRowBytes, dstRowBytes, dstRowBytes);
                    }
                }
                else
                {
                    // 平铺
                    for (int y = 0; y < dstH; y++)
                    {
                        byte* sRow = srcBase + (sy + y) * srcPitch + sx * SliceBpp;
                        byte* dRow = dstBase + y * dstRowBytes;
                        for (int x = 0; x < dstW; x += sw)
                        {
                            int chunk = Math.Min(sw, dstW - x) * SliceBpp;
                            Buffer.MemoryCopy(sRow, dRow + x * SliceBpp, chunk, chunk);
                        }
                    }
                }
            }
            // 纵向边条（左/右边）：沿 Y 轴填充
            else
            {
                if (sh >= dstH)
                {
                    // 中心裁剪
                    int offset = (sh - dstH) / 2;
                    for (int y = 0; y < dstH; y++)
                    {
                        byte* s = srcBase + (sy + offset + y) * srcPitch + sx * SliceBpp;
                        Buffer.MemoryCopy(s, dstBase + y * dstRowBytes, dstRowBytes, dstRowBytes);
                    }
                }
                else
                {
                    // 平铺
                    for (int y = 0; y < dstH; y++)
                    {
                        byte* s = srcBase + (sy + y % sh) * srcPitch + sx * SliceBpp;
                        Buffer.MemoryCopy(s, dstBase + y * dstRowBytes, dstRowBytes, dstRowBytes);
                    }
                }
            }
            return slice;
        }

        //
        // 摘要:
        //     重写 Arrange：根据 Surface.ViewBound 计算绝对位置——
        //     左对齐 ViewBound 左边缘，底部对齐超武栏底部（距底 BottomMargin）。
        //     面板总尺寸 = 3×3 网格 + 边框厚度。
        public override RectangleStruct Arrange(RectangleStruct finalRect)
        {
            RectangleStruct view = Surface.ViewBound;

            int x = view.X;
            int y = view.Y + view.Height - TotalHeight - BottomMargin;

            RectangleStruct absolute = new RectangleStruct(x, y, TotalWidth, TotalHeight);
            ClientRectangle = absolute;
            ArrangeChildren(ClientRectangle);
            return ClientRectangle;
        }

        //
        // 摘要:
        //     重写 ArrangeChildren：3×3 网格布局，从边框内沿开始排列。
        protected override void ArrangeChildren(RectangleStruct clientRect)
        {
            int startX = clientRect.X + BorderSlice;
            int startY = clientRect.Y + BorderSlice;

            int count = Math.Min(_children.Count, 9);
            for (int i = 0; i < count; i++)
            {
                int row = i / 3;
                int col = i % 3;
                int x = startX + col * (CellWidth + CellInterval);
                int y = startY + row * (CellHeight + CellInterval);
                _children[i].Arrange(new RectangleStruct(x, y, CellWidth, CellHeight));
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            try
            {
                // 懒加载边框素材
                EnsureBorderLoaded();

                // 扫描 CurrentObjects 提取不同类型
                RebuildButtons();

                // 检测 ViewBound 变化
                RectangleStruct view = Surface.ViewBound;
                if (view != _lastViewBound)
                {
                    _lastViewBound = view;
                    _isDirty = true;
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     扫描 CurrentObjects，按分组键（GroupAs 字符串）去重提取类型（最多 9 个）。
        //     分组键来自 UnitGroup：读 [单位ID] GroupAs=，未配置时默认为单位自身 ID。
        //     类型列表变化时重建按钮；不足 9 格补空格按钮保留布局。
        private void RebuildButtons()
        {
            var typeMap = new Dictionary<string, Pointer<TechnoTypeClass>>();
            var currentObjs = ObjectClass.CurrentObjects;

            for (int i = 0; i < currentObjs.Count && typeMap.Count < 9; i++)
            {
                var pObj = currentObjs[i];
                if (pObj.IsNull)
                {
                    continue;
                }
                var pTechno = pObj.Convert<TechnoClass>();
                if (pTechno.IsNull)
                {
                    continue;
                }
                var pType = pTechno.Ref.Type;
                if (pType.IsNull)
                {
                    continue;
                }
                string groupKey = UnitGroup.GetGroupKey(pType);
                if (!typeMap.ContainsKey(groupKey))
                {
                    typeMap[groupKey] = pType;
                }
            }

            // 比较分组键列表是否变化
            var newKeys = new List<string>(typeMap.Keys);
            newKeys.Sort(StringComparer.Ordinal);
            if (IsSameList(_lastGroupKeys, newKeys))
            {
                return;
            }
            _lastGroupKeys = newKeys;

            // 重建按钮
            ClearChildren();
            // 注意：AddChild 只进队列，_children 下一帧 ProcessQueues 才更新，
            // 必须用局部计数器判断补齐数量，不能用 _children.Count（会死循环）
            int added = 0;
            foreach (var kv in typeMap)
            {
                AddChild(new UnitFilterButton(kv.Value, kv.Key));
                added++;
            }
            // 不足 9 格补空格
            for (; added < 9; added++)
            {
                AddChild(new UnitFilterButton());
            }

            _isDirty = true;
        }

        private static bool IsSameList(List<string> a, List<string> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        //
        // 摘要:
        //     先铺黑底，再画外边框（uiext.pcx 九宫格切片），最后由 base.Draw() 画子按钮。
        protected override void OnDraw()
        {
            base.OnDraw();

            try
            {
                Pointer<Surface> pSurface = Surface.Current;
                if (pSurface.IsNull)
                {
                    return;
                }

                // 整个面板区域铺黑底（边框透明部分也显黑，不透游戏画面）
                pSurface.Ref.FillRect(ClientRectangle, Drawing.RGB2DWORD(BackgroundColor));

                if (_sliceCornerTL == null)
                {
                    return;
                }

                DrawBorder(pSurface, ClientRectangle);
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     用预切好的 8 个九宫格切片绘制面板外边框（切片已是目标尺寸，1:1 各画一次）：
        //       4 角贴角点，4 边贴边，中心不画（3×3 按钮区已占）。
        //     每块用 PCX.BlitToSurface 绘制，由引擎处理透明色（洋红 0xF81F）。
        private void DrawBorder(Pointer<Surface> pSurface, RectangleStruct rect)
        {
            int bs = BorderSlice;

            // 4 个角
            DrawSlice(pSurface, _sliceCornerTL, rect.X, rect.Y);
            DrawSlice(pSurface, _sliceCornerTR, rect.X + rect.Width - bs, rect.Y);
            DrawSlice(pSurface, _sliceCornerBL, rect.X, rect.Y + rect.Height - bs);
            DrawSlice(pSurface, _sliceCornerBR, rect.X + rect.Width - bs, rect.Y + rect.Height - bs);

            // 4 条边（边切片尺寸 = 网格区尺寸，正好填满角之间的区域）
            DrawSlice(pSurface, _sliceEdgeTop, rect.X + bs, rect.Y);
            DrawSlice(pSurface, _sliceEdgeBottom, rect.X + bs, rect.Y + rect.Height - bs);
            DrawSlice(pSurface, _sliceEdgeLeft, rect.X, rect.Y + bs);
            DrawSlice(pSurface, _sliceEdgeRight, rect.X + rect.Width - bs, rect.Y + bs);
        }

        //
        // 摘要:
        //     用 PCX.BlitToSurface 1:1 绘制一个切片到目标位置（目标矩形 = 切片尺寸），
        //     引擎自动处理透明色（0xF81F）。
        private void DrawSlice(Pointer<Surface> pSurface, YRClassHandle<BSurface> slice, int x, int y)
        {
            if (slice == null || slice.Pointer.IsNull)
            {
                return;
            }
            RectangleStruct rect = new RectangleStruct(x, y,
                slice.Ref.BaseSurface.Width, slice.Ref.BaseSurface.Height);
            PCX.Instance.BlitToSurface(rect.GetThisPointer(), pSurface.Convert<DSurface>(), slice.Pointer);
        }
    }
}
