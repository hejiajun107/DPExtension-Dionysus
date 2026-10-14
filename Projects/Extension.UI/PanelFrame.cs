using DynamicPatcher;
using PatcherYRpp;
using System;
using System.Collections.Generic;

namespace Extension.UI
{
    //
    // 摘要:
    //     共享的九宫格边框（素材 uiext.pcx）。UnitFilterPanel 与 SuperWeaponPanel 共用
    //     同一份素材与切片缓存，避免重复加载与重复的 unsafe 拷贝实现。
    //
    //     实现要点（均为踩坑后的结论）：
    //       - PCX 透明色按“精确值”匹配（0xF81F，565 下的 #FF00FF），不是容差/alpha，
    //         素材四角的抗锯齿过渡像素会漏过 colorkey，需在切片时重写（FixCornerFringe）；
    //       - 切片必须用 unsafe 直接内存拷贝，不能用 Surface.Blit——BSurface 是纯内存 buffer，
    //         Blit 虚函数不会写入其 buffer，且 AllocHGlobal 不清零会画出堆垃圾（白线/乱色）；
    //       - 边切片在目标长度上生成、绘制时 1:1 blit，不依赖引擎的截断/缩放语义。
    //
    //     用法：DrawFrame(surface, rect, left, right, top, bottom)。
    //     left/right/top/bottom 控制是否绘制对应方向的边与角（去掉某一边的边框），
    //     关闭某方向时该侧的边条会延伸到最外沿，从而不留下缺口。
    public static class PanelFrame
    {
        // 九宫格边宽（固定 8px）：uiext.pcx 内 4 角 / 4 边 / 中心 的切线距离。
        public const int Slice = 8;

        // BSurface 每像素字节数。YR 的 BSurface 恒为 16bpp（游戏内部渲染 565，
        // BSurface::Allocate 硬编码 2 字节/像素），不要用 GetBytesPerPixel 虚函数。
        private const int Bpp = 2;

        private static bool _init;
        private static Pointer<BSurface> _src;
        private static int _srcW, _srcH, _edgeW, _edgeH;

        // 4 角切片（8×8），固定尺寸，初始化时生成一次
        private static YRClassHandle<BSurface> _cornerTL;
        private static YRClassHandle<BSurface> _cornerTR;
        private static YRClassHandle<BSurface> _cornerBL;
        private static YRClassHandle<BSurface> _cornerBR;

        // 边切片缓存：横向（上/下边条）按目标宽度缓存，纵向（左/右边条）按目标高度缓存。
        // 键 = 长度 * 2 + 方向位（0=上/左，1=下/右）。
        private static readonly Dictionary<long, YRClassHandle<BSurface>> _hEdges =
            new Dictionary<long, YRClassHandle<BSurface>>();
        private static readonly Dictionary<long, YRClassHandle<BSurface>> _vEdges =
            new Dictionary<long, YRClassHandle<BSurface>>();

        //
        // 摘要:
        //     懒加载素材并预切 4 角。素材缺失或尺寸不足时保持未就绪（即无边框）。
        public static void EnsureInit()
        {
            if (_init)
            {
                return;
            }
            _init = true;

            try
            {
                PCX.Instance.LoadFile("uiext.pcx");
                _src = PCX.Instance.GetSurface("uiext.pcx", Pointer<BytePalette>.Zero);
                if (_src.IsNull)
                {
                    return;
                }

                _srcW = _src.Ref.BaseSurface.Width;
                _srcH = _src.Ref.BaseSurface.Height;
                if (_srcW < 2 * Slice || _srcH < 2 * Slice)
                {
                    return;
                }

                _edgeW = _srcW - 2 * Slice;
                _edgeH = _srcH - 2 * Slice;

                _cornerTL = CreateCornerSlice(_src, 0, 0);
                _cornerTR = CreateCornerSlice(_src, _srcW - Slice, 0);
                _cornerBL = CreateCornerSlice(_src, 0, _srcH - Slice);
                _cornerBR = CreateCornerSlice(_src, _srcW - Slice, _srcH - Slice);
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     绘制九宫格外边框到 rect：4 角贴角点、4 边贴边、中心不画。
        public static void DrawFrame(Pointer<Surface> pSurface, RectangleStruct rect,
            bool left = true, bool right = true, bool top = true, bool bottom = true)
        {
            EnsureInit();
            if (_cornerTL == null || pSurface.IsNull || rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            int bs = Slice;

            // 顶/底边条的横向范围：关闭左/右边时向外延伸到 rect 边缘，避免缺口
            int edgeStartX = left ? rect.X + bs : rect.X;
            int edgeEndX = right ? rect.X + rect.Width - bs : rect.X + rect.Width;
            int hLen = edgeEndX - edgeStartX;

            // 左/右边条的纵向范围：关闭上/下边时向外延伸到 rect 边缘
            int edgeStartY = top ? rect.Y + bs : rect.Y;
            int edgeEndY = bottom ? rect.Y + rect.Height - bs : rect.Y + rect.Height;
            int vLen = edgeEndY - edgeStartY;

            if (top && hLen > 0)
            {
                DrawSlice(pSurface, GetHEdge(true, hLen), edgeStartX, rect.Y);
            }
            if (bottom && hLen > 0)
            {
                DrawSlice(pSurface, GetHEdge(false, hLen), edgeStartX, rect.Y + rect.Height - bs);
            }
            if (left && vLen > 0)
            {
                DrawSlice(pSurface, GetVEdge(true, vLen), rect.X, edgeStartY);
            }
            if (right && vLen > 0)
            {
                DrawSlice(pSurface, GetVEdge(false, vLen), rect.X + rect.Width - bs, edgeStartY);
            }

            if (top && left) DrawSlice(pSurface, _cornerTL, rect.X, rect.Y);
            if (top && right) DrawSlice(pSurface, _cornerTR, rect.X + rect.Width - bs, rect.Y);
            if (bottom && left) DrawSlice(pSurface, _cornerBL, rect.X, rect.Y + rect.Height - bs);
            if (bottom && right) DrawSlice(pSurface, _cornerBR, rect.X + rect.Width - bs, rect.Y + rect.Height - bs);
        }

        //
        // 摘要:
        //     取（并缓存）横向边切片：长度 len、高 Slice。上边取源顶部条，下边取源底部条。
        private static YRClassHandle<BSurface> GetHEdge(bool isTop, int len)
        {
            long key = (long)len * 2 + (isTop ? 0 : 1);
            if (_hEdges.TryGetValue(key, out YRClassHandle<BSurface> cached))
            {
                return cached;
            }
            var slice = CreateHEdgeSlice(isTop, len);
            _hEdges[key] = slice;
            return slice;
        }

        //
        // 摘要:
        //     取（并缓存）纵向边切片：宽 Slice、长度 len。左边取源左侧条，右边取源右侧条。
        private static YRClassHandle<BSurface> GetVEdge(bool isLeft, int len)
        {
            long key = (long)len * 2 + (isLeft ? 0 : 1);
            if (_vEdges.TryGetValue(key, out YRClassHandle<BSurface> cached))
            {
                return cached;
            }
            var slice = CreateVEdgeSlice(isLeft, len);
            _vEdges[key] = slice;
            return slice;
        }

        //
        // 摘要:
        //     从 PCX 源 1:1 截取 (sx,sy) 处的 Slice×Slice 到新 BSurface，并修掉四角洋红过渡像素。
        private static unsafe YRClassHandle<BSurface> CreateCornerSlice(Pointer<BSurface> src, int sx, int sy)
        {
            var slice = new YRClassHandle<BSurface>(Slice, Slice);
            slice.Ref.Allocate(Bpp);

            byte* srcBase = (byte*)src.Ref.BaseSurface.Buffer.Value;
            byte* dstBase = (byte*)slice.Ref.BaseSurface.Buffer.Value;
            int srcPitch = _srcW * Bpp;
            int rowBytes = Slice * Bpp;

            for (int y = 0; y < Slice; y++)
            {
                byte* s = srcBase + (sy + y) * srcPitch + sx * Bpp;
                Buffer.MemoryCopy(s, dstBase + y * rowBytes, rowBytes, rowBytes);
            }
            FixCornerFringe(slice, Slice, Slice);
            return slice;
        }

        //
        // 摘要:
        //     生成横向边切片（len × Slice）。源边条 >= 目标时中心裁剪（保持对称设计的对称性），
        //     否则平铺重复。
        private static unsafe YRClassHandle<BSurface> CreateHEdgeSlice(bool isTop, int len)
        {
            int sy = isTop ? 0 : _srcH - Slice;
            var slice = new YRClassHandle<BSurface>(len, Slice);
            slice.Ref.Allocate(Bpp);

            byte* srcBase = (byte*)_src.Ref.BaseSurface.Buffer.Value;
            byte* dstBase = (byte*)slice.Ref.BaseSurface.Buffer.Value;
            int srcPitch = _srcW * Bpp;
            int dstPitch = len * Bpp;
            int sw = _edgeW;

            if (sw >= len)
            {
                int offset = (sw - len) / 2;
                for (int y = 0; y < Slice; y++)
                {
                    byte* s = srcBase + (sy + y) * srcPitch + (Slice + offset) * Bpp;
                    Buffer.MemoryCopy(s, dstBase + y * dstPitch, dstPitch, dstPitch);
                }
            }
            else
            {
                for (int y = 0; y < Slice; y++)
                {
                    byte* sRow = srcBase + (sy + y) * srcPitch + Slice * Bpp;
                    byte* dRow = dstBase + y * dstPitch;
                    for (int x = 0; x < len; x += sw)
                    {
                        int chunk = Math.Min(sw, len - x) * Bpp;
                        Buffer.MemoryCopy(sRow, dRow + x * Bpp, chunk, chunk);
                    }
                }
            }
            return slice;
        }

        //
        // 摘要:
        //     生成纵向边切片（Slice × len）。源边条 >= 目标时中心裁剪，否则平铺重复。
        private static unsafe YRClassHandle<BSurface> CreateVEdgeSlice(bool isLeft, int len)
        {
            int sx = isLeft ? 0 : _srcW - Slice;
            var slice = new YRClassHandle<BSurface>(Slice, len);
            slice.Ref.Allocate(Bpp);

            byte* srcBase = (byte*)_src.Ref.BaseSurface.Buffer.Value;
            byte* dstBase = (byte*)slice.Ref.BaseSurface.Buffer.Value;
            int srcPitch = _srcW * Bpp;
            int dstPitch = Slice * Bpp;
            int sh = _edgeH;

            if (sh >= len)
            {
                int offset = (sh - len) / 2;
                for (int y = 0; y < len; y++)
                {
                    byte* s = srcBase + (Slice + offset + y) * srcPitch + sx * Bpp;
                    Buffer.MemoryCopy(s, dstBase + y * dstPitch, dstPitch, dstPitch);
                }
            }
            else
            {
                for (int y = 0; y < len; y++)
                {
                    byte* s = srcBase + (Slice + y % sh) * srcPitch + sx * Bpp;
                    Buffer.MemoryCopy(s, dstBase + y * dstPitch, dstPitch, dstPitch);
                }
            }
            return slice;
        }

        //
        // 摘要:
        //     修掉角切片里漏过 colorkey 的洋红抗锯齿像素：不再抠成透明（会让圆角失去过渡、
        //     看起来被“切掉”），而是按“向黑底混合”重写——取邻域最近的非洋红像素作为前景色 F，
        //     用绿通道估算前景占比（洋红 G=0），再按比例把 F 混向黑，得到自然的深色抗锯齿过渡。
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
        //     用 PCX.BlitToSurface 1:1 绘制一个切片到目标位置（目标矩形 = 切片尺寸），
        //     引擎自动处理透明色（0xF81F）。
        private static void DrawSlice(Pointer<Surface> pSurface, YRClassHandle<BSurface> slice, int x, int y)
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
