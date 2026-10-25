using DynamicPatcher;
using Extension.UI.Core;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Extension.UI
{
    //
    // 摘要:
    //     单位教程模态面板。点击 helpbtn 后打开：
    //       1. 先播放一段过渡 SHP 动画（可选，配置缺失时直接跳页）；
    //       2. 再 1:1 原尺寸显示教程 PCX（多页可翻页）。
    //     面板尺寸 = 内容尺寸 + 8px 九宫格边框 + 底部按钮条，居中于 ViewBound；
    //     各页尺寸不同时面板按当前页尺寸变化（不做固定尺寸）。
    //     关闭方式只有底部「关闭」按钮（无 ESC / 右键）。选中的己方单位变化时自动关闭。
    //     模态：由 UIRoot.HitTest 在打开期间消费全部视口内点击，复用现有 MouseEvent 拦截。
    //     联机安全：只读本地选中集合与载入期配置，不发任何网络/游戏命令。
    public class TutorialPanel : Control
    {
        private enum State
        {
            Closed,
            Transition,
            Page
        }

        // 一页教程图
        private struct PageInfo
        {
            public string File;
            public Pointer<BSurface> Surface;
            public int Width;
            public int Height;

            // 解码页（jpg/png 等）自己持有的表面；PCX 页由引擎缓存，这里为 false
            public bool Owned;
            public IntPtr BufferPtr;
            public bool Raw => Owned;
        }

        // 九宫格边框宽度（共享 PanelFrame，固定 8px）
        private const int FrameInset = PanelFrame.Slice;

        // 固定内容区尺寸（不含边框）：过渡动画与教程页都用同一块 800×620 的框
        private const int ContentWidth = 800;
        private const int ContentHeight = 620;

        //
        // 摘要:
        // 底部按钮条高度（[TutorialUI] FooterHeight=），图片区高度 = 620 - 该值。
        // 钳制到 [0, ContentHeight]，避免配置异常把图片区压成负数。
        private static int FooterHeightPx
        {
            get
            {
                int value = TutorialConfig.FooterHeight;
                if (value < 0) return 0;
                if (value > ContentHeight) return ContentHeight;
                return value;
            }
        }

        // 过渡 SHP 的画布尺寸：固定 512×512，用于居中定位。
        // 不用 SHP 头部/帧 bounds，因为它们逐帧可变，会导致画面抖动并算偏。
        private const int TransitionWidth = 512;
        private const int TransitionHeight = 512;

        // 面板底色（纯黑）
        private static readonly ColorStruct BackgroundColor = new ColorStruct(0, 0, 0);

        // 按钮间/与边框的间距
        private const int ButtonGap = 9;
        private const int ButtonMargin = 4;

        // 底部页码点点（轮播图风格）：半径、间距、当前页/其它页颜色
        private const int DotRadius = 3;
        private const int DotGap = 6;
        private static readonly ColorStruct DotActiveColor = new ColorStruct(255, 255, 255);
        private static readonly ColorStruct DotInactiveColor = new ColorStruct(90, 90, 90);

        // 教程图淡入（抖动溶解）时长与扫描带高度
        private const int FadeDuration = 220;
        private const int FadeBandHeight = 2;
        private int _fadeStartTick = -1;

        private State _state = State.Closed;

        // 当前打开所对应的单位类型 ID，用于选中变化时自动关闭
        private string _typeId = string.Empty;

        // 教程页（按 INI 顺序；加载失败的页已剔除）
        private readonly List<PageInfo> _pages = new List<PageInfo>();
        private int _pageIndex;

        // 导航按钮（上一页 / 下一页 / 关闭）
        private readonly List<TutorialNavButton> _navButtons = new List<TutorialNavButton>();

        // 过渡动画
        private Pointer<SHPStruct> _transitionSHP = Pointer<SHPStruct>.Zero;
        private bool _transitionFailed;
        private int _transitionFrames = 1;
        private int _frameIndex;
        private int _lastFrameTick;

        // 上一次 ViewBound，用于检测屏幕区域变化以重新布局
        private RectangleStruct _lastViewBound;

        public TutorialPanel()
        {
            Visible = false;
            ClientRectangle = new RectangleStruct(0, 0, 0, 0);
        }

        //
        // 摘要:
        //     打开教程。typeId 用于选中变化时自动关闭；pcxList 为页序列（顺序即页序）。
        //     所有页都加载失败时不打开（保持面板隐藏）。
        public void Open(string typeId, List<string> pcxList)
        {
            try
            {
                ResetContent();

                _typeId = typeId ?? string.Empty;

                if (pcxList != null)
                {
                    foreach (string file in pcxList)
                    {
                        if (string.IsNullOrEmpty(file))
                        {
                            continue;
                        }
                        Pointer<BSurface> pSurface = LoadImage(file, out bool owned, out IntPtr bufferPtr);
                        if (pSurface.IsNull)
                        {
                            Logger.Log("[TutorialPanel] page load failed: " + file);
                            continue;
                        }
                        int w = pSurface.Ref.BaseSurface.Width;
                        int h = pSurface.Ref.BaseSurface.Height;
                        if (w <= 0 || h <= 0)
                        {
                            ReleaseSurface(pSurface, owned, bufferPtr);
                            continue;
                        }
                        _pages.Add(new PageInfo
                        {
                            File = file,
                            Surface = pSurface,
                            Width = w,
                            Height = h,
                            Owned = owned,
                            BufferPtr = bufferPtr
                        });
                    }
                }

                if (_pages.Count == 0)
                {
                    Visible = false;
                    ClientRectangle = new RectangleStruct(0, 0, 0, 0);
                    return;
                }

                _pageIndex = 0;
                Visible = true;
                _isDirty = true;

                // 有过渡 SHP 就先播动画，否则直接进第 1 页
                if (!TryStartTransition())
                {
                    _state = State.Page;
                    RebuildButtons();
                    StartFade();
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
                Close();
            }
        }

        //
        // 摘要:
        //     关闭面板并清空内容。过渡 SHP 的加载结果保留在缓存中，下次打开不再重试。
        public void Close()
        {
            _state = State.Closed;
            _typeId = string.Empty;
            DisposePages();
            _navButtons.Clear();
            ClearChildren();
            _pageIndex = 0;
            _frameIndex = 0;
            _lastFrameTick = 0;
            _fadeStartTick = -1;
            Visible = false;
            ClientRectangle = new RectangleStruct(0, 0, 0, 0);
        }

        //
        // 摘要:
        //     丢弃已加载的过渡 SHP（配置可能随地图变化）。场景切换后调用。
        public void ResetCache()
        {
            Close();
            _transitionSHP = Pointer<SHPStruct>.Zero;
            _transitionFailed = false;
            _transitionFrames = 1;
        }

        //
        // 摘要:
        //     清空上一次打开的页/按钮/状态（不动过渡 SHP 缓存）。
        private void ResetContent()
        {
            _state = State.Closed;
            DisposePages();
            _navButtons.Clear();
            ClearChildren();
            _pageIndex = 0;
            _frameIndex = 0;
            _lastFrameTick = 0;
            _fadeStartTick = -1;
        }

        private static Pointer<BSurface> LoadPCX(string file)
        {
            try
            {
                PCX.Instance.LoadFile(file);
                return PCX.Instance.GetSurface(file, Pointer<BytePalette>.Zero);
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
                return Pointer<BSurface>.Zero;
            }
        }

        //
        // 摘要:
        //     释放所有解码页持有的表面（PCX 页归引擎缓存，不动）。
        private void DisposePages()
        {
            foreach (PageInfo page in _pages)
            {
                try
                {
                    ReleaseSurface(page.Surface, page.Owned, page.BufferPtr);
                }
                catch (Exception ex)
                {
                    Logger.PrintException(ex);
                }
            }
            _pages.Clear();
        }

        //
        // 摘要:
        //     释放解码页：像素缓冲用 FreeHGlobal 配对 AllocHGlobal，表面对象用引擎
        //     operator delete 只回收内存、不走虚析构。
        //     不能调 YRClassHandle.Dispose()/BSurface.Destructor：该结构里 BufferSize 与
        //     VRAMmed/BufferAllocated 等字段重叠（offset 24/28），析构会去 Release 一个
        //     被写坏的 Surf 指针，导致关闭面板时崩溃。
        private static void ReleaseSurface(Pointer<BSurface> surface, bool owned, IntPtr bufferPtr)
        {
            if (!owned || surface.IsNull)
            {
                return;
            }
            if (bufferPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(bufferPtr);
            }
            surface.Ref.BaseSurface.Buffer = Pointer<byte>.Zero;
            YRMemory.Deallocate(surface);
        }

        //
        // 摘要:
        //     按扩展名分流加载：.pcx 走引擎 PCX 缓存；.jpg/.jpeg/.png/.bmp 走 GDI+ 解码，
        //     转成 16bpp RGB565 表面，owned=true 时由调用方负责 ReleaseSurface。
        private static Pointer<BSurface> LoadImage(string file, out bool owned, out IntPtr bufferPtr)
        {
            owned = false;
            bufferPtr = IntPtr.Zero;
            string ext = Path.GetExtension(file);
            if (string.IsNullOrEmpty(ext) || ext.Equals(".pcx", StringComparison.OrdinalIgnoreCase))
            {
                return LoadPCX(file);
            }
            if (!ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                && !ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                && !ext.Equals(".png", StringComparison.OrdinalIgnoreCase)
                && !ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase))
            {
                return Pointer<BSurface>.Zero;
            }
            return LoadDecoded(file, out owned, out bufferPtr);
        }

        //
        // 摘要:
        //     从散装文件/MIX 读字节 → GDI+ 解码 → 16bpp RGB565（Bayer 4×4 有序抖动）。
        //     Bitmap 在拷完像素后立即释放，不留缓存。
        private static Pointer<BSurface> LoadDecoded(string file, out bool owned, out IntPtr bufferPtr)
        {
            owned = false;
            bufferPtr = IntPtr.Zero;
            try
            {
                byte[] bytes = ReadFileBytes(file);
                if (bytes == null || bytes.Length == 0)
                {
                    return Pointer<BSurface>.Zero;
                }

                using (MemoryStream stream = new MemoryStream(bytes))
                using (Bitmap source = new Bitmap(stream))
                {
                    int width = source.Width;
                    int height = source.Height;
                    if (width <= 0 || height <= 0)
                    {
                        return Pointer<BSurface>.Zero;
                    }

                    Rectangle rect = new Rectangle(0, 0, width, height);
                    using (Bitmap converted = source.Clone(rect, PixelFormat.Format32bppArgb))
                    {
                        // 表面对象用引擎 operator new + Constructor(0x4AEC60)，指针自己持有，
                        // 不用 YRClassHandle（它的终结器会走虚析构，见 ReleaseSurface 注释）。
                        Pointer<BSurface> surface = YRMemory.Create<BSurface>(width, height);
                        if (surface.IsNull)
                        {
                            return Pointer<BSurface>.Zero;
                        }

                        IntPtr pBuffer = Marshal.AllocHGlobal(width * height * 2);
                        if (pBuffer == IntPtr.Zero)
                        {
                            YRMemory.Deallocate(surface);
                            return Pointer<BSurface>.Zero;
                        }

                        surface.Ref.BaseSurface.Buffer = pBuffer;
                        surface.Ref.BaseSurface.BytesPerPixel = 2;

                        BitmapData data = converted.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        try
                        {
                            CopyDither565(data, surface.Ref.BaseSurface.Buffer, width, height);
                        }
                        finally
                        {
                            converted.UnlockBits(data);
                        }

                        owned = true;
                        bufferPtr = pBuffer;
                        return surface;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
                return Pointer<BSurface>.Zero;
            }
        }

        //
        // 摘要:
        //     读取文件全部字节（散装文件或 MIX 内的同名资源）。
        //     走引擎的 LoadWholeFileEx（0x4A38D0），它覆盖散装 + MIX 的完整解析链。
        //     关键：MIX 命中的缓冲可能是档案映射视图而非堆内存，只有 outAllocated 为 true
        //     时才能释放，否则会把 MIX 视图当堆块 free 掉，造成堆损坏（关闭面板时崩溃）。
        private static byte[] ReadFileBytes(string file)
        {
            IntPtr pData = IntPtr.Zero;
            bool allocated = false;
            try
            {
                pData = FileSystem.LoadWholeFileEx(file, out allocated);
                if (pData == IntPtr.Zero)
                {
                    return null;
                }

                // 用图片结构自解析长度，不依赖 CCFileClass.FileSize
                int size = DetectImageLength(pData);
                if (size <= 0)
                {
                    return null;
                }

                byte[] bytes = new byte[size];
                Marshal.Copy(pData, bytes, 0, size);
                return bytes;
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
                return null;
            }
            finally
            {
                if (pData != IntPtr.Zero && allocated)
                {
                    YRMemory.Deallocate(pData);
                }
            }
        }

        //
        // 摘要:
        //     按文件头解析图片的字节长度（JPEG / PNG / BMP）。结构异常返回 0。
        private static unsafe int DetectImageLength(IntPtr pData)
        {
            byte* p = (byte*)pData;
            if (p[0] == 0xFF && p[1] == 0xD8)
            {
                return DetectJpegLength(p);
            }
            if (p[0] == 0x89 && p[1] == 0x50 && p[2] == 0x4E && p[3] == 0x47)
            {
                return DetectPngLength(p);
            }
            if (p[0] == (byte)'B' && p[1] == (byte)'M')
            {
                return p[2] | (p[3] << 8) | (p[4] << 16) | (p[5] << 24);
            }
            return 0;
        }

        // 上限 64MB，防止文件损坏时无限扫描
        private const int MaxScanLength = 64 * 1024 * 1024;

        //
        // 摘要:
        //     遍历 JPEG 段结构定位 EOI（0xFFD9）。会正确跳过熵编码数据里的
        //     0xFF00 填充与 RST 标记，也不会被 EXIF 内嵌缩略图的 EOI 误导。
        private static unsafe int DetectJpegLength(byte* p)
        {
            int offset = 2; // 跳过 SOI
            while (offset < MaxScanLength)
            {
                if (p[offset] != 0xFF)
                {
                    return 0;
                }
                byte marker = p[offset + 1];
                offset += 2;

                if (marker == 0xD9)
                {
                    return offset; // EOI
                }
                if (marker == 0xD8 || marker == 0x01 || (marker >= 0xD0 && marker <= 0xD7))
                {
                    continue; // 无长度的独立标记
                }

                int length = (p[offset] << 8) | p[offset + 1];
                if (length < 2)
                {
                    return 0;
                }

                if (marker == 0xDA)
                {
                    // SOS：之后是熵编码数据，扫描到下一个真实标记
                    int scan = offset + length;
                    while (scan < MaxScanLength)
                    {
                        if (p[scan] != 0xFF)
                        {
                            scan++;
                            continue;
                        }
                        byte next = p[scan + 1];
                        if (next == 0x00 || (next >= 0xD0 && next <= 0xD7))
                        {
                            scan += 2;
                            continue;
                        }
                        if (next == 0xD9)
                        {
                            return scan + 2;
                        }
                        offset = scan;
                        break;
                    }
                    continue;
                }

                offset += length;
            }
            return 0;
        }

        //
        // 摘要:
        //     遍历 PNG chunk 定位 IEND，返回文件总长度。
        private static unsafe int DetectPngLength(byte* p)
        {
            int offset = 8; // 跳过 PNG 签名
            while (offset < MaxScanLength)
            {
                int length = (p[offset] << 24) | (p[offset + 1] << 16) | (p[offset + 2] << 8) | p[offset + 3];
                if (length < 0)
                {
                    return 0;
                }
                uint type = ((uint)p[offset + 4] << 24) | ((uint)p[offset + 5] << 16)
                          | ((uint)p[offset + 6] << 8) | p[offset + 7];
                offset += 8 + length + 4; // 数据 + CRC
                if (type == 0x49454E44) // IEND
                {
                    return offset;
                }
            }
            return 0;
        }

        // 4×4 Bayer 抖动矩阵（0..15）
        private static readonly int[] Bayer4x4 =
        {
             0,  8,  2, 10,
            12,  4, 14,  6,
             3, 11,  1,  9,
            15,  7, 13,  5
        };

        //
        // 摘要:
        //     32bpp BGRA 像素 → 16bpp RGB565，带 4×4 有序抖动以减轻渐变色带。
        private static unsafe void CopyDither565(BitmapData data, Pointer<byte> pBuffer, int width, int height)
        {
            byte* pScan = (byte*)(IntPtr)data.Scan0;
            byte* pDst = (byte*)(IntPtr)pBuffer;
            int stride = data.Stride;

            for (int y = 0; y < height; y++)
            {
                byte* pSrc = pScan + y * stride;
                ushort* pRow = (ushort*)(pDst + y * width * 2);
                int bayerRow = (y & 3) * 4;

                for (int x = 0; x < width; x++)
                {
                    int threshold = Bayer4x4[bayerRow + (x & 3)];
                    // 5 位通道步长 8，6 位通道步长 4，抖动偏移取半个步长
                    int d5 = (threshold - 8) * 8 / 16;
                    int d6 = (threshold - 8) * 4 / 16;

                    int b = pSrc[x * 4 + 0];
                    int g = pSrc[x * 4 + 1];
                    int r = pSrc[x * 4 + 2];

                    int r5 = ClampToByte(r + d5) >> 3;
                    int g6 = ClampToByte(g + d6) >> 2;
                    int b5 = ClampToByte(b + d5) >> 3;

                    pRow[x] = (ushort)((r5 << 11) | (g6 << 5) | b5);
                }
            }
        }

        private static int ClampToByte(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return value;
        }

        //
        // 摘要:
        //     尝试进入过渡动画状态：加载并缓存 SHP。成功返回 true，失败（含文件名缺失）
        //     返回 false，由调用方直接进入 Page。失败后不再重试。
        private bool TryStartTransition()
        {
            if (_transitionFailed)
            {
                return false;
            }

            if (_transitionSHP.IsNull)
            {
                string name = TutorialConfig.TransitionSHP;
                if (string.IsNullOrEmpty(name))
                {
                    _transitionFailed = true;
                    return false;
                }
                if (!FileSystem.TyrLoadSHPFile(name, out _transitionSHP) || _transitionSHP.IsNull)
                {
                    _transitionFailed = true;
                    return false;
                }

                // 帧数用于播放推进
                _transitionFrames = Math.Max(1, (int)_transitionSHP.Ref.Frames);
            }

            _state = State.Transition;
            _frameIndex = 0;
            _lastFrameTick = Environment.TickCount;
            _isDirty = true;
            return true;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (!Visible || _state == State.Closed)
            {
                return;
            }

            try
            {
                // 选中的第一个己方类型变了（含取消选中）→ 自动关闭
                if (!IsCurrentSelection())
                {
                    Close();
                    return;
                }

                if (_state == State.Transition)
                {
                    AdvanceTransition();
                }

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
        //     当前选中的第一个己方类型是否仍与打开时一致。
        private bool IsCurrentSelection()
        {
            if (!UnitGroup.TryGetFirstLocalSelectedType(out Pointer<TechnoTypeClass> pType))
            {
                return false;
            }
            return string.Equals(UnitGroup.GetTypeId(pType), _typeId, StringComparison.OrdinalIgnoreCase);
        }

        //
        // 摘要:
        //     按 TransitionFPS 推进过渡帧；播完转 Page 并重建按钮。
        private void AdvanceTransition()
        {
            int fps = Math.Max(1, TutorialConfig.TransitionFPS);
            int interval = Math.Max(1, 1000 / fps);

            int now = Environment.TickCount;
            if (_lastFrameTick == 0)
            {
                _lastFrameTick = now;
            }
            int elapsed = unchecked(now - _lastFrameTick);
            if (elapsed < interval)
            {
                return;
            }

            int advance = elapsed / interval;
            _lastFrameTick = unchecked(_lastFrameTick + advance * interval);
            _frameIndex += advance;

            if (_frameIndex >= _transitionFrames)
            {
                _state = State.Page;
                _pageIndex = 0;
                RebuildButtons();
                StartFade();
                _isDirty = true;
            }
        }

        //
        // 摘要:
        //     重写 Arrange：忽略父级 finalRect，按 ViewBound 居中；隐藏/关闭时矩形置零。
        public override RectangleStruct Arrange(RectangleStruct finalRect)
        {
            if (!Visible || _state == State.Closed)
            {
                ClientRectangle = new RectangleStruct(0, 0, 0, 0);
                return ClientRectangle;
            }

            int width = ContentWidth + 2 * FrameInset;
            int height = ContentHeight + 2 * FrameInset;

            RectangleStruct view = Surface.ViewBound;
            int x = view.X + (view.Width - width) / 2;
            int y = view.Y + (view.Height - height) / 2;

            // 内容大于视口时贴左上，避免出现负坐标
            if (x < view.X) x = view.X;
            if (y < view.Y) y = view.Y;
            int overflowX = view.X + view.Width - (x + width);
            if (overflowX < 0) x += overflowX;
            int overflowY = view.Y + view.Height - (y + height);
            if (overflowY < 0) y += overflowY;

            ClientRectangle = new RectangleStruct(x, y, width, height);
            LayoutNavButtons();
            return ClientRectangle;
        }

        //
        // 摘要:
        //     在底部按钮条内右对齐排列导航按钮。
        private void LayoutNavButtons()
        {
            if (_navButtons.Count == 0)
            {
                return;
            }
            RectangleStruct rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            int buttonWidth = Math.Max(1, TutorialConfig.ButtonWidth);
            int footerHeight = FooterHeightPx;
            // 按钮高度受按钮条高度限制，避免压到上方图片区
            int buttonHeight = Math.Min(Math.Max(1, TutorialConfig.ButtonHeight), Math.Max(1, footerHeight));

            int footerTop = rect.Y + rect.Height - FrameInset - footerHeight;
            int totalWidth = _navButtons.Count * buttonWidth + (_navButtons.Count - 1) * ButtonGap;
            int x = rect.X + rect.Width - FrameInset - ButtonMargin - totalWidth;
            int y = footerTop + Math.Max(0, (footerHeight - buttonHeight) / 2);

            for (int i = 0; i < _navButtons.Count; i++)
            {
                _navButtons[i].Arrange(new RectangleStruct(x, y, buttonWidth, buttonHeight));
                x += buttonWidth + ButtonGap;
            }
        }

        //
        // 摘要:
        //     Page 状态下重建按钮：单页只有「关闭」，多页为「上一页」「下一页」「关闭」。
        private void RebuildButtons()
        {
            _navButtons.Clear();
            ClearChildren();

            if (_pages.Count <= 0)
            {
                return;
            }

            if (_pages.Count > 1)
            {
                AddNavButton(TutorialConfig.PrevPCX, PrevPage, _pageIndex > 0);
                AddNavButton(TutorialConfig.NextPCX, NextPage, _pageIndex < _pages.Count - 1);
            }
            AddNavButton(TutorialConfig.ClosePCX, Close, true);

            LayoutNavButtons();
        }

        private void AddNavButton(string iconPCX, Action action, bool enabled)
        {
            TutorialNavButton button = new TutorialNavButton(iconPCX, action);
            button.Enabled = enabled;
            _navButtons.Add(button);
            AddChild(button);
        }

        private void PrevPage()
        {
            if (_state != State.Page || _pageIndex <= 0)
            {
                return;
            }
            _pageIndex--;
            RebuildButtons();
            _isDirty = true;
        }

        private void NextPage()
        {
            if (_state != State.Page || _pageIndex >= _pages.Count - 1)
            {
                return;
            }
            _pageIndex++;
            RebuildButtons();
            _isDirty = true;
        }

        //
        // 摘要:
        //     铺黑底 + 完整四边边框，再按状态画过渡帧或教程页。
        protected override void OnDraw()
        {
            base.OnDraw();

            if (!Visible || _state == State.Closed)
            {
                return;
            }

            try
            {
                Pointer<Surface> pSurface = Surface.Current;
                RectangleStruct rect = ClientRectangle;
                if (pSurface.IsNull || rect.Width <= 0 || rect.Height <= 0)
                {
                    return;
                }

                // 黑底（边框透明处也不透出游戏画面）
                pSurface.Ref.FillRect(rect, Drawing.RGB2DWORD(BackgroundColor));
                PanelFrame.DrawFrame(pSurface, rect);

                if (_state == State.Transition)
                {
                    DrawTransition(pSurface, rect);
                }
                else
                {
                    DrawPage(pSurface, rect, _pages[_pageIndex]);
                    DrawFadeOverlay(pSurface, rect);
                    DrawPageDots(pSurface, rect);
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     图片区：内容区上方区域（固定 800 宽，高度 = 620 - FooterHeight）。
        private RectangleStruct GetImageRect(RectangleStruct rect)
        {
            return new RectangleStruct(
                rect.X + FrameInset,
                rect.Y + FrameInset,
                ContentWidth,
                ContentHeight - FooterHeightPx);
        }

        //
        // 摘要:
        //     绘制过渡动画当前帧（在图片区内居中，1:1 不缩放）。
        private void DrawTransition(Pointer<Surface> pSurface, RectangleStruct rect)
        {
            if (_transitionSHP.IsNull)
            {
                return;
            }

            RectangleStruct image = GetImageRect(rect);
            if (image.Width <= 0 || image.Height <= 0)
            {
                return;
            }

            // 固定 512×512 画布居中（左上角定位，与 cameo 同一套画法，不用 Centered 标志）
            int x = image.X + (image.Width - TransitionWidth) / 2;
            int y = image.Y + (image.Height - TransitionHeight) / 2;

            Pointer<ConvertClass> pPalette = TutorialConfig.ResolveTransitionPalette();
            RectangleStruct bound = pSurface.Ref.GetRect();
            pSurface.Ref.DrawSHP(pPalette, _transitionSHP, _frameIndex,
                new Point2D(x, y),
                Pointer<RectangleStruct>.AsPointer(ref bound),
                BlitterFlags.None);
        }

        //
        // 摘要:
        //     绘制当前教程页（原尺寸 1:1 居中于图片区，超出部分裁剪）。
        private void DrawPage(Pointer<Surface> pSurface, RectangleStruct rect, PageInfo page)
        {
            if (page.Surface.IsNull || page.Width <= 0 || page.Height <= 0)
            {
                return;
            }

            RectangleStruct content = GetImageRect(rect);
            if (content.Width <= 0 || content.Height <= 0)
            {
                return;
            }

            int x = content.X + Math.Max(0, (content.Width - page.Width) / 2);
            int y = content.Y + Math.Max(0, (content.Height - page.Height) / 2);

            RectangleStruct view = pSurface.Ref.GetRect();
            int left = Math.Max(Math.Max(x, view.X), content.X);
            int top = Math.Max(Math.Max(y, view.Y), content.Y);
            int right = Math.Min(Math.Min(x + page.Width, view.X + view.Width), content.X + content.Width);
            int bottom = Math.Min(Math.Min(y + page.Height, view.Y + view.Height), content.Y + content.Height);
            if (right <= left || bottom <= top)
            {
                return;
            }

            RectangleStruct clip = new RectangleStruct(left, top, right - left, bottom - top);
            if (page.Raw)
            {
                // 解码页是 16bpp RGB565，整块 blit（无调色板/透明色），源矩形按页内偏移裁剪
                RectangleStruct src = new RectangleStruct(
                    clip.X - x, clip.Y - y, clip.Width, clip.Height);
                pSurface.Ref.Blit(view, clip, page.Surface.Convert<Surface>(), clip, src, true, true);
            }
            else
            {
                PCX.Instance.BlitToSurface(clip.GetThisPointer(), pSurface.Convert<DSurface>(), page.Surface);
            }
        }

        //
        // 摘要:
        //     底部页码点：当前页实心白点，其余灰点，居中排在按钮左侧的空白里。
        //     只有多页时显示，单页不画。
        private void DrawPageDots(Pointer<Surface> pSurface, RectangleStruct rect)
        {
            int count = _pages.Count;
            if (count <= 1)
            {
                return;
            }

            int footerHeight = FooterHeightPx;
            if (footerHeight < 2 * DotRadius)
            {
                return;
            }

            int footerTop = rect.Y + rect.Height - FrameInset - footerHeight;
            int centerY = footerTop + footerHeight / 2;

            // 左右边界：左边贴边框，右边让给导航按钮，避免点点与按钮重叠
            int leftLimit = rect.X + FrameInset + ButtonMargin;
            int rightLimit = rect.X + rect.Width - FrameInset - ButtonMargin;
            if (_navButtons.Count > 0)
            {
                RectangleStruct first = _navButtons[0].ClientRectangle;
                if (first.X > leftLimit)
                {
                    rightLimit = first.X - ButtonGap;
                }
            }
            if (rightLimit <= leftLimit)
            {
                return;
            }

            int totalWidth = count * (2 * DotRadius) + (count - 1) * DotGap;
            int startX = leftLimit + (rightLimit - leftLimit - totalWidth) / 2;
            if (startX < leftLimit)
            {
                startX = leftLimit;
            }

            for (int i = 0; i < count; i++)
            {
                int cx = startX + i * (2 * DotRadius + DotGap) + DotRadius;
                ColorStruct color = i == _pageIndex ? DotActiveColor : DotInactiveColor;
                FillCircle(pSurface, cx, centerY, DotRadius, color);
            }
        }

        //
        // 摘要:
        //     开始一次教程图淡入（仅在教程首次出现时调用，翻页不淡入）。
        private void StartFade()
        {
            _fadeStartTick = Environment.TickCount;
        }

        //
        // 摘要:
        //     淡入覆盖：在图片区上按 2px 横向扫描带铺黑底，随时间按哈希顺序逐渐揭开，
        //     形成溶解淡入效果（引擎无逐像素 alpha，这里用分带覆盖模拟）。
        private void DrawFadeOverlay(Pointer<Surface> pSurface, RectangleStruct rect)
        {
            if (_fadeStartTick < 0)
            {
                return;
            }

            int elapsed = unchecked(Environment.TickCount - _fadeStartTick);
            if (elapsed >= FadeDuration)
            {
                _fadeStartTick = -1;
                return;
            }

            RectangleStruct image = GetImageRect(rect);
            if (image.Width <= 0 || image.Height <= 0)
            {
                return;
            }

            // 剩余黑底占比：1 → 0
            float blackRatio = 1f - (float)elapsed / FadeDuration;
            int dword = Drawing.RGB2DWORD(BackgroundColor);
            int bottom = image.Y + image.Height;

            for (int y = image.Y; y < bottom; y += FadeBandHeight)
            {
                int index = (y - image.Y) / FadeBandHeight;
                // 与顺序无关的打散阈值，揭开顺序看起来像随机溶解而非整块上移
                uint hash = unchecked((uint)index * 2654435761u);
                float threshold = (hash % 1024) / 1024f;
                if (threshold >= blackRatio)
                {
                    continue;
                }

                int height = Math.Min(FadeBandHeight, bottom - y);
                pSurface.Ref.FillRect(new RectangleStruct(image.X, y, image.Width, height), dword);
            }
        }

        //
        // 摘要:
        //     用逐行矩形拼一个实心圆（引擎无直接画圆接口）。
        private static void FillCircle(Pointer<Surface> pSurface, int cx, int cy, int radius, ColorStruct color)
        {
            int dword = Drawing.RGB2DWORD(color);
            for (int dy = -radius; dy <= radius; dy++)
            {
                int half = (int)Math.Sqrt(radius * radius - dy * dy);
                pSurface.Ref.FillRect(new RectangleStruct(cx - half, cy + dy, half * 2 + 1, 1), dword);
            }
        }
    }
}
