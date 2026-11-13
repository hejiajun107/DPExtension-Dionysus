using DynamicPatcher;
using PatcherYRpp;
using System;

namespace DP4CW.DPExtension_Dionysus.GeneralHooks
{
    /// <summary>
    /// 把世界渲染的中间表面（Composite / Tile）物理扩容到 1.5 倍，
    /// 但创建完立刻把结构体里的 Width/Height 改回原始值。
    ///
    /// 效果等价于"给表面加 padding"：DDraw 实际显存是 1.5 倍，
    /// 而引擎所有逻辑（GetRect/GetWidth/GetHeight/上屏 blit）看到的仍是原始尺寸。
    ///
    /// 注意：曾经试过"逻辑尺寸=物理尺寸"（让 rect 全部落在界内），
    /// 结果上屏 blit 的 dst 变成 2628x1800，画面/UI 全乱，已回滚。
    ///
    /// 该扩容只为缩放服务，缩放功能停用后 hook 一并注释，表面按引擎原始尺寸创建。
    /// </summary>
    public static class SurfaceScale
    {
        private const int ScaleNum = 3;
        private const int ScaleDen = 2;

        // ---- 第 1 组：把 CTOR 的 Width/Height 实参放大 1.5 倍 ----
        // sub_533FD0 内 4 处 `call DSurface::CTOR`，call 之前刚 push 过 Width、Height，
        // 所以 hook 点 esp+0 = Width，esp+4 = Height。
        private const int CompositeCtor = 0x534172;
        private const int TileCtor = 0x5341E8;
        private const int CompositeCtorVram = 0x534296;
        private const int TileCtorVram = 0x5342C4;

        // ---- 第 2 组：CTOR 返回后，把结构体里的逻辑尺寸改回原始值 ----
        // 这 4 处都是 `mov DSurface::Xxx, eax`（5 字节，绝对地址），
        // 此时 EAX = 刚创建好的表面指针；结构体偏移 +4 = Width，+8 = Height。
        private const int CompositeCreated = 0x53417B;
        private const int TileCreated = 0x5341F1;
        private const int CompositeCreatedVram = 0x53429B;
        private const int TileCreatedVram = 0x5342C9;

        /// <summary>未放大前的战术区尺寸，由第一次表面创建记录。</summary>
        private static int origWidth;
        private static int origHeight;

        // ---- hook 已随缩放功能停用：表面完全按引擎原始尺寸创建，不再扩容 ----
        /*
        [Hook(HookType.AresHook, Address = CompositeCtor, Size = 0x5)]
        public static unsafe UInt32 Composite_Ctor(REGISTERS* R) => ScaleArgs(R);

        [Hook(HookType.AresHook, Address = TileCtor, Size = 0x5)]
        public static unsafe UInt32 Tile_Ctor(REGISTERS* R) => ScaleArgs(R);

        [Hook(HookType.AresHook, Address = CompositeCtorVram, Size = 0x5)]
        public static unsafe UInt32 Composite_CtorVram(REGISTERS* R) => ScaleArgs(R);

        [Hook(HookType.AresHook, Address = TileCtorVram, Size = 0x5)]
        public static unsafe UInt32 Tile_CtorVram(REGISTERS* R) => ScaleArgs(R);

        [Hook(HookType.AresHook, Address = CompositeCreated, Size = 0x5)]
        public static unsafe UInt32 Composite_Created(REGISTERS* R) => RestoreLogicalSize(R);

        [Hook(HookType.AresHook, Address = TileCreated, Size = 0x5)]
        public static unsafe UInt32 Tile_Created(REGISTERS* R) => RestoreLogicalSize(R);

        [Hook(HookType.AresHook, Address = CompositeCreatedVram, Size = 0x5)]
        public static unsafe UInt32 Composite_CreatedVram(REGISTERS* R) => RestoreLogicalSize(R);

        [Hook(HookType.AresHook, Address = TileCreatedVram, Size = 0x5)]
        public static unsafe UInt32 Tile_CreatedVram(REGISTERS* R) => RestoreLogicalSize(R);
        */

        /// <summary>放大传给 DSurface::CTOR 的 Width/Height，让物理表面变成 1.5 倍。</summary>
        private static unsafe UInt32 ScaleArgs(REGISTERS* R)
        {
            try
            {
                int width = (int)R->Stack32(0);
                int height = (int)R->Stack32(4);

                if (width <= 0 || height <= 0)
                {
                    return 0;
                }

                if (origWidth == 0)
                {
                    origWidth = width;
                    origHeight = height;
                }

                int newWidth = width * ScaleNum / ScaleDen;
                int newHeight = height * ScaleNum / ScaleDen;

                R->Stack<uint>(0, (uint)newWidth);
                R->Stack<uint>(4, (uint)newHeight);

                Logger.Log("[SurfaceScale] physical {0}x{1} -> {2}x{3}", width, height, newWidth, newHeight);
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }

            return 0;
        }

        /// <summary>把刚建好的表面的逻辑尺寸改回原始值（DDraw 实际尺寸不变）。</summary>
        private static unsafe UInt32 RestoreLogicalSize(REGISTERS* R)
        {
            try
            {
                if (origWidth > 0 && origHeight > 0 && R->EAX != 0)
                {
                    uint* surface = (uint*)R->EAX;
                    surface[1] = (uint)origWidth;   // +4  Width
                    surface[2] = (uint)origHeight;  // +8  Height

                    Logger.Log("[SurfaceScale] logical restored to {0}x{1}", origWidth, origHeight);
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }

            return 0;
        }
    }
}
