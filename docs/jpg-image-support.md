# 在 DynamicPatcher 扩展中显示 JPG / PNG 图片

面向"要在红警2尤里的复仇（gamemd.exe，YR 1.001 + Ares）的 DynamicPatcher C# 扩展里显示任意格式图片"的实现指南。
本文件只讲**通用做法与必须避开的坑**，不依赖具体业务（教程面板只是第一个用例）。

参考实现：`Projects/Extension.UI/TutorialPanel.cs`（`LoadImage` / `LoadDecoded` / `ReadFileBytes` / `CopyDither565` / `DrawPage` / `ReleaseSurface`）。

---

## 1. 总体流程

```
文件名字符串
  → ① 取字节（散装文件 or MIX 内资源）
  → ② GDI+ 解码为 Bitmap
  → ③ 建 16bpp 内存表面（引擎对象 + 自己分配的像素缓冲）
  → ④ 逐行拷像素（RGB565 + 可选抖动）
  → ⑤ 用 Surface.Blit 贴到当前帧
  → ⑥ 关闭时确定性释放
```

为什么不能直接显示 JPG：引擎只认 8 位调色板 SHP/PCX 或 16 位 RGB565 内存表面。
JPG/PNG 必须先解码并转成 **16bpp RGB565** 的 `BSurface`，再用 `Surface.Blit` 整块贴（走不了 `PCX.BlitToSurface`，那个只吃 8 位调色板 PCX）。

---

## 2. 关键步骤与要点

### ① 取字节：用 `LoadWholeFileEx`，不要用 `CCFileClass.Exists`

```csharp
IntPtr pData = IntPtr.Zero;
bool allocated = false;
try
{
    pData = FileSystem.LoadWholeFileEx(file, out allocated);   // 0x4A38D0
    if (pData == IntPtr.Zero) return null;                     // 散装 + MIX 都没找到

    int size = DetectImageLength(pData);                       // 见下：长度必须自己算
    if (size <= 0) return null;

    byte[] bytes = new byte[size];
    Marshal.Copy(pData, bytes, 0, size);
    return bytes;
}
finally
{
    // 只有 allocated == true 才能释放；MIX 命中的指针可能是档案映射视图
    if (pData != IntPtr.Zero && allocated) YRMemory.Deallocate(pData);
}
```

要点：

- **`CCFileClass.Exists()` 只查物理文件，查不到 MIX**。用它会得到"散装能显示、打进 MIX 就失效"的假故障。
- **绝不能不判断就 `Deallocate`**。MIX 命中的缓冲可能不是堆块，free 掉会破坏 MIX 内部状态：当时不崩，后续读文件静默失败，随后在任意 delete 处崩溃。
- **引擎不返回长度**，`CCFileClass.FileSize` 在 MIX 路径下不可靠。必须按文件头自解析：
  - JPEG：从 SOI(FFD8) 开始遍历段结构，遇 SOS(FFDA) 后要扫熵编码区，正确处理 `FF00` 填充与 `FFD0~FFD7` 重启标记，直到 EOI(FFD9)。**不要用 `LastIndexOf(FFD9)`**——EXIF 内嵌缩略图也有 EOI。
  - PNG：遍历 chunk（长度 4 + 类型 4 + 数据 + CRC 4），到 `IEND` 结束。
  - BMP：直接读偏移 2 的 4 字节文件长度。
  - 一律加上限（如 64MB）防止损坏文件导致死循环。

### ② 解码：GDI+，用完立刻释放

```csharp
using (MemoryStream stream = new MemoryStream(bytes))
using (Bitmap source = new Bitmap(stream))
using (Bitmap converted = source.Clone(rect, PixelFormat.Format32bppArgb))
{
    BitmapData data = converted.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    try { CopyDither565(data, buffer, width, height); }
    finally { converted.UnlockBits(data); }
}
```

- `System.Drawing` / `System.Drawing.Imaging` 是 .NET 自带能力，不需要第三方库。
- **必须给 csproj 加 `<Reference Include="System.Drawing" />`**，否则 DP 在游戏目录重新编译时会报类型找不到。
- 判断格式看文件头，不要只看扩展名（扩展名只用于"是否走这条分支"）。
- `Bitmap` 不缓存：拷完像素立即 `Dispose`，长期占用只剩 16bpp 表面（800×620 ≈ 1MB/页；峰值是解码瞬间的 32bpp Bitmap + 目标表面）。

### ③ 建 16bpp 表面：引擎对象 + 自己的缓冲

```csharp
Pointer<BSurface> surface = YRMemory.Create<BSurface>(width, height);  // ctor 0x4AEC60
IntPtr pBuffer = Marshal.AllocHGlobal(width * height * 2);
surface.Ref.BaseSurface.Buffer = pBuffer;      // 偏移 0x14
surface.Ref.BaseSurface.BytesPerPixel = 2;     // 偏移 0x10
```

要点：

- `BSurface` 构造函数（0x4AEC60）**只写 width / height / 基类 vtable，不分配任何内存**（IDA 反汇编只有 3 条指令、无 call）。所以覆写 `Buffer` 不会造成泄漏。
- 只写 `Buffer` 与 `BytesPerPixel` 两个字段。**不要再写 `BufferSize` / `BufferAllocated` / `Allocated` / `VRAMmed`**——原因见第 3 节。
- 表面对象用裸 `Pointer<BSurface>` 自己持有，**不要用 `YRClassHandle<BSurface>`**（它的 Dispose/终结器会执行虚析构）。

### ④ 拷像素：严格逐行

```csharp
byte* pSrc = (byte*)(IntPtr)data.Scan0;
byte* pDst = (byte*)(IntPtr)pBuffer;
for (int y = 0; y < height; y++)
{
    byte* row = pSrc + y * data.Stride;              // 注意 Stride，不是 width*4
    ushort* pRow = (ushort*)(pDst + y * width * 2);
    for (int x = 0; x < width; x++)
    {
        int b = row[x * 4 + 0], g = row[x * 4 + 1], r = row[x * 4 + 2];
        pRow[x] = (ushort)(((r >> 3) << 11) | ((g >> 2) << 5) | (b >> 3));
    }
}
```

- `32bppArgb` 的内存顺序是 **B, G, R, A**，别按 RGB 取。
- 必须按 `Stride` 逐行，整块拷 `Stride * Height` 会在宽度为奇数（Stride 有 padding）时写越界。
- 直接把 RGB 截位会出明显色带（照片尤其明显）。加 **4×4 Bayer 有序抖动**（5 位通道 ±4、6 位通道 ±2）可显著改善：
  ```csharp
  int d5 = (threshold - 8) * 8 / 16;   // threshold ∈ 0..15
  int d6 = (threshold - 8) * 4 / 16;
  r5 = clamp(r + d5) >> 3;  g6 = clamp(g + d6) >> 2;  b5 = clamp(b + d5) >> 3;
  ```

### ⑤ 绘制

```csharp
// 解码页（16bpp，无调色板）：整块 Blit
pSurface.Ref.Blit(viewRect, clipRect, srcSurface.Convert<Surface>(),
                  clipRect, srcRect, true, true);

// PCX 页（8bpp 调色板）：另一条通道，不能混用
PCX.Instance.BlitToSurface(clipPtr, pSurface.Convert<DSurface>(), pcxSurface);
```

- `Blit` 的参数顺序为 `(clipRect, clipRect2, pSrc, destRect, srcRect, unk1, unk2)`；在做裁剪显示时，`srcRect` 要填**页内偏移**（`clip.X - 图片x, clip.Y - 图片y`），否则裁剪后图像内容是错的。
- 两种来源的页要分开画，用页结构里的 `Owned`（或 `Raw`）标记分流。

### ⑥ 释放：分配器与释放器必须同源

```csharp
Marshal.FreeHGlobal(bufferPtr);        // 配对 AllocHGlobal
surface.Ref.BaseSurface.Buffer = Pointer<byte>.Zero;  // 防止任何后续路径重复释放
YRMemory.Deallocate(surface);          // 引擎 operator delete，只回收对象内存，不跑虚析构
```

- 关闭 / 重新打开 / 换场景时都要遍历页表逐个释放，并放在 try/finally 里保证异常路径也释放。
- 页结构要区分"引擎缓存的 PCX"（由引擎管理，我们**不能**释放）与"自己创建的解码表面"。

---

## 3. 必须避开的坑（每条都实际崩过）

### 坑 1：`Allocate(2)` + 析构 = 跨堆释放

`BSurface.Allocate()` 的像素缓冲是 **`Marshal.AllocHGlobal`**（CRT/进程堆），而虚析构里释放缓冲用的是**引擎 `operator delete`（0x7C8B3D）**。两者不同堆，释放即崩溃。

崩溃特征：`EIP = 007CF89A`，调用栈连续出现 `007C8B3D` / `007C8B46`（都在 `operator delete` 内部），发生在**关闭面板**而不是打开时。

### 坑 2：YRpp 的 `Surface` 结构字段偏移重叠（最关键）

引擎里这个结构（`BSurface`，真实大小 **32 字节**）的后半段是：

| 偏移 | 真实含义 |
|------|----------|
| 0x0C | LockLevel (int) |
| 0x10 | BytesPerPixel (int) |
| 0x14 | Buffer (void*) |
| 0x18 | **缓冲字节数 (int)** |
| 0x1C | **owns (byte，是否拥有 Buffer)** |

而 YRpp 的 `Surface` 声明把 `DSurface` 的字段混写了进来：

```
[FieldOffset(24)] Allocated;        [FieldOffset(24)] BufferSize;      // → 实际是 0x18 的 int，写它无害
[FieldOffset(25)] VRAMmed;          // 0x19，是上面那个 int 的第二个字节，写它等于写脏 BufferSize
[FieldOffset(28)] Surf;             [FieldOffset(28)] BufferAllocated; // → 实际是 0x1C 的 owns
```

所以 `BufferAllocated = true` 就是 **`owns = 1`**。owns 一旦为 1，虚析构必然走
"用引擎 delete 释放 Buffer"，把 CRT 堆的指针交给引擎 → 崩溃。

**规则：自己分配的缓冲，`owns` 必须保持 0；绝不写 `BufferAllocated`。**

### 坑 3：`BSurface.Destructor` / `YRClassHandle.Dispose` 的调用约定是错的

真实虚析构是 `0x411650`，签名是 `thiscall + 1 个 int 参数` 且结尾 `ret 4`（标量删除析构，参数 bit0 决定是否释放对象自身）。
YRpp 的 `BSurface.Destructor` 按**无参**调用，于是 .NET 侧压栈布局与 `ret 4` 不匹配，callee 会把栈上的垃圾当 flag：

- flag & 1 随机成立 → 对象被释放两次（每次开关随机崩，很难复现）；
- flag & 1 不成立 → 对象泄漏。

**规则：不要用 `YRClassHandle.Dispose()` / `YRMemory.Delete` 管理自己构造的 BSurface。**
用裸指针 + `YRMemory.Deallocate`（只回收内存、不执行析构）。

### 坑 4：`CCFileClass.ReadWholeFile` 的指针不一定可释放

MIX 命中的返回指针可能是档案映射视图。无条件 `YRMemory.Deallocate` 会静默破坏文件系统缓存，表现为"连续读第二个文件失败"+"之后某次关闭崩溃"。
用 `LoadWholeFileEx` 的 `out allocated` 作为唯一释放依据。

### 坑 5：文件名后缀被强行补 `.pcx`

如果代码里对配置列表做"没有后缀就补 `.pcx`"，`xxx.jpg` 会变成 `xxx.jpg.pcx`，读不到文件 → 页加载失败 → 面板不打开（`_pages` 为空），表现为**点击后毫无反应**。
规则：只在**完全没有扩展名**时补默认后缀。

### 坑 6：`BlitterFlags.Centered` 定位会偏移

绘制 SHP/Bitmap 类图集中定位时，`BlitterFlags.Centered` 会把图推向右下（引擎把尺寸补偿加在了坐标上）。
固定画布尺寸 + 左上角定位 + `BlitterFlags.None` 才可控；尺寸一律按**声明尺寸**算，不要用 `SHPStruct.GetFrameBounds()`（逐帧可变会导致抖动）。

---

## 4. 已验证的引擎事实（IDA 反汇编，YR 1.001）

| 地址 | 内容 |
|------|------|
| `0x4AEC60` | `Surface` 基类构造：`[eax+4]=w; [eax+8]=h; [eax]=7E2198h; ret 8`，**无 call，不分配内存** |
| `0x7C8E17` | 引擎 `operator new` |
| `0x7C8B3D` | 引擎 `operator delete` |
| `0x43AD00` | 挂接缓冲：`this = &surf->0x14`，参数 `(ptr, size)`；**ptr==0 且 size>0 时引擎 new 分配并置 owns=1**，否则 owns=0 |
| `0x7E2070[0] = 0x411650` | `BSurface` 标量删除析构：先 `0x43AE50(&surf->0x14)`，把 vtable 还原成基类，再按 flag&1 释放 this |
| `0x43AE50` | 读 `this+0x1C`(owns)，**仅当非 0** 才用引擎 `operator delete` 释放 Buffer；随后清零 0x14/0x18/0x1C。**不读 0x18，不调用 `IDirectDrawSurface::Release`** |
| `0x411630 / 0x411640` | `GetPitch = width * BytesPerPixel` |

真实 BSurface 创建序列（引擎内部，供对照）：

```
new(0x20) → 0x4AEC60(w,h) → LockLevel=0 → vtable=7E2104 → BytesPerPixel=1
          → 0x43AD00(this+0x14, 0, size)   // 引擎分配，owns=1
          → vtable=7E2070
```

---

## 5. 可选：最"正统"的实现路线

如果不想自己管 CRT 堆，可以完全复刻引擎流程：

1. `YRMemory.Create<BSurface>(w, h)`；
2. `surface.Ref.BaseSurface.BytesPerPixel = 2;`
3. 调 `0x43AD00(&surf->0x14, IntPtr.Zero, size)`，让引擎分配并持有缓冲（owns=1）；
4. 拷像素进 `surface.Ref.BaseSurface.Buffer`；
5. 释放时直接调 `0x411650(this, 1)`（一次调用同时释放缓冲与对象）。

优点：缓冲与对象都在引擎堆，owns 语义天然正确。
代价：多依赖两个硬编码地址，且析构为 `ret 4`，绑定必须用 `thiscall + int` 保证栈平衡。

当前项目采用的是前一种（CRT 分配 + 跳过虚析构），已通过多次开关验证；两种都不要混用。

---

## 6. 关于 MIX 打包与调试

- **MIX 目录项里只有 `ID(uint CRC) / Offset / Size`**，没有名字和扩展名。扩展名只参与哈希计算、**不参与合法性校验**，所以 `xxx.jpg` 可以正常打进 MIX 并被引擎按名查到。
- **散装文件优先级高于 MIX**，调试期把图片丢在游戏根目录即可，不用每次重新打包。
- 名字打错（多个字符、子目录、全角符号）只会得到"查不到"，不会有明显报错；调试期建议在页加载失败处打一条日志（每次打开最多一条，不会刷屏）。
- RA2 的 MIX 目录是加密的，**外部脚本无法直接扫描目录项做交叉验证**，以引擎日志为准。

---

## 7. 自检清单

- [ ] 散装文件能显示，打进 MIX 后也能显示（验证 `Exists` 没有被误用）
- [ ] 连续打开 → 翻页 → 关闭 ×10，不崩溃（验证 owns/释放路径）
- [ ] 关闭后再次打开，图片内容正确（验证缓冲没被提前释放或复用）
- [ ] 宽高为奇数的图片显示正常（验证 `Stride` 逐行拷贝）
- [ ] 多页列表全部显示（验证后缀补全逻辑与每页独立释放）
- [ ] 大图/高压缩比照片无肉眼可见色带（验证抖动）
- [ ] 打开瞬间无长时间卡顿（解码耗时；需要时改懒加载）
