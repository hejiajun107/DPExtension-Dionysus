# gamemd.exe 只读逆向：GScreenClass::DrawOnTop / SidebarClass::Draw_It / DSurface pitch-lock

- Image base: 0x400000 (IDA database `F:\ra2tools\idb\gamemd.exe.i64`)
- 分析方式：只读。使用 IDA MCP 的 `disasm` / `get_bytes` / `decompile` / `py_eval`（仅 idc/idautils 读取），未修改 IDB、未打补丁。
- 相关全局：
  - `DSurface::Primary = 0x887308`
  - `DSurface::Temp = 0x887314`
  - `DSurface::Composite = 0x88731C`
  - `DSurface::Sidebar = 0x887300`
  - `DSurface::Tile = 0x8872FC`
- DSurface 虚表 `DSurface_vtbl_ = 0x7E85D4`：
  - `+0x08 CopyFromPart(0x4BB080)`、`+0x5C Lock(0x4BAD80)`、`+0x60 Unlock(0x4BAF40)`、`+0x64 CanLock(0x4BAEC0)`、`+0x70 GetBytesPerPixel(0x4BAD60)`、`+0x74 GetPitch(0x4BAD70)`、`+0x7C GetWidth(0x411540)`、`+0x80 GetHeight(0x411550)`。
- MapClass 虚表 `MapClass_vtbl_ = 0x7E1964`（由 MapClass 构造链设置；继承自 Sidebar→Tab→Scroll）：
  - `+0x3C DrawOnTop(0x4F4480)`、`+0x40 Draw(0x6D0A20, TabClass::Draw)`、`+0x44 vt_entry_44(0x4F45B0, 上屏)`。
- SidebarClass 虚表 `??_7SidebarClass@@6B@ = 0x7F3058`：
  - `+0x3C DrawOnTop(0x4F4480)`、`+0x40 Draw = SidebarClass::Draw_It(0x6A6C30)`、`+0x44 0x4F45B0`。

## F. GScreenClass::DrawOnTop (0x4F4480)

函数实际结束于 0x4F45A8（0x4F45A8 `retn` 后为对齐；0x4F45B0 是虚表槽 +0x44 的另一个函数）。完整反汇编：

```asm
0x4F4480 sub     esp, 8
0x4F4483 mov     eax, DSurface__Composite
0x4F4488 push    ebp
0x4F4489 mov     ebp, DSurface__Temp
0x4F448F push    esi
0x4F4490 mov     esi, ecx
0x4F4492 mov     DSurface__Temp, eax
0x4F4497 mov     ecx, WWMouseClass__Instance
0x4F449D push    0
0x4F449F push    eax
0x4F44A0 mov     edx, [ecx]
0x4F44A2 call    dword ptr [edx+40h]        ; WWMouse::SetCursorSurface(Composite, 0)
0x4F44A5 mov     eax, [esi+0Ch]
0x4F44A8 mov     dword ptr [esi+0Ch], 0
0x4F44AF test    eax, eax
0x4F44B1 setnz   cl
0x4F44B4 cmp     eax, 2
0x4F44B7 mov     byte ptr [esp+10h+var_8], cl
0x4F44BB setz    dl
0x4F44BE mov     byte ptr [esp+10h+var_4], dl
0x4F44C2 call    sub_53BAE0
0x4F44C7 test    al, al
0x4F44C9 jnz     short loc_4F451B
0x4F44CB mov     eax, DSurface__Composite
0x4F44D0 mov     ecx, pPStm
0x4F44D6 push    edi
0x4F44D7 mov     edi, [esp+14h+var_8]
0x4F44DB push    0
0x4F44DD push    edi
0x4F44DE push    eax
0x4F44DF call    sub_6D3D10                 ; (Composite, var_8, 0)
0x4F44E4 mov     ecx, DSurface__Composite
0x4F44EA push    1
0x4F44EC push    edi
0x4F44ED push    ecx
0x4F44EE mov     ecx, pPStm
0x4F44F4 call    sub_6D3D10                 ; (Composite, var_8, 1) 世界 pass
0x4F44F9 mov     eax, [esp+14h+var_4]
0x4F44FD mov     edx, [esi]
0x4F44FF push    eax
0x4F4500 mov     ecx, esi
0x4F4502 call    dword ptr [edx+40h]        ; this->Draw() -> 0x6D0A20 -> SidebarClass::Draw_It
0x4F4505 mov     ecx, DSurface__Composite
0x4F450B push    2
0x4F450D push    edi
0x4F450E push    ecx
0x4F450F mov     ecx, pPStm
0x4F4515 call    sub_6D3D10                 ; (Composite, var_8, 2) overlay pass
0x4F451A pop     edi
0x4F451B mov     al, byte_B0B519
0x4F4520 test    al, al
0x4F4522 jz      short loc_4F4547
0x4F4524 mov     al, Unsorted__ArmageddomMode
0x4F4529 test    al, al
0x4F452B jnz     short loc_4F4547
0x4F452D mov     ecx, WWMouseClass__Instance
0x4F4533 mov     eax, DSurface__Sidebar
0x4F4538 push    1
0x4F453A push    eax
0x4F453B mov     edx, [ecx]
0x4F453D call    dword ptr [edx+40h]        ; SetCursorSurface(Sidebar, 1)
0x4F4540 mov     byte_B0B519, 0
0x4F4547 mov     ecx, dwUnk_0
0x4F454D test    ecx, ecx
0x4F454F jz      short loc_4F4558
0x4F4551 mov     edx, [ecx]
0x4F4553 push    0
0x4F4555 call    dword ptr [edx+2Ch]
0x4F4558 mov     ecx, (offset SessionClass__Instance.unknown_8AA+17Eh)
0x4F455D call    sub_5D49A0
0x4F4562 mov     ecx, dword_887368
0x4F4568 test    ecx, ecx
0x4F456A jz      short loc_4F4573
0x4F456C mov     eax, [ecx]
0x4F456E push    0
0x4F4570 call    dword ptr [eax+0Ch]
0x4F4573 mov     al, byte ptr SessionClass__Instance.unknown_310+36Ch
0x4F4578 test    al, al
0x4F457A jz      short loc_4F4583
0x4F457C xor     cl, cl
0x4F457E call    sub_55F1E0
0x4F4583 mov     ecx, WWMouseClass__Instance
0x4F4589 mov     eax, DSurface__Composite
0x4F458E push    0
0x4F4590 push    eax
0x4F4591 mov     edx, [ecx]
0x4F4593 call    dword ptr [edx+3Ch]        ; SetCursorSurface(Composite, 0) 再次
0x4F4596 mov     edx, [esi]
0x4F4598 mov     ecx, esi
0x4F459A call    dword ptr [edx+44h]        ; vt_entry_44 = 0x4F45B0，上屏
0x4F459D mov     DSurface__Temp, ebp       ; 恢复旧 Temp
0x4F45A3 pop     esi
0x4F45A4 pop     ebp
0x4F45A5 add     esp, 8
0x4F45A8 retn
```

### F1
入口序言就是：

```asm
sub esp, 8
mov eax, DSurface__Composite
push ebp
mov ebp, DSurface__Temp
push esi
mov esi, ecx
mov DSurface__Temp, eax
```

`ebp` 保存的是旧的 `DSurface::Temp`（0x887314）。0x4F4492 的字节是 `A3 14 73 88 00`，即 `mov dword ptr [887314h], eax`，确实写 `DSurface::Temp = DSurface::Composite`。

### F2
0x4F459D 原始字节：`89 2D 14 73 88 00`（随后 `5E 5D` 是 `pop esi; pop ebp`）。

- `89` = MOV r/m32, r32
- `2D` = ModRM（mod=00, reg=101=ebp, r/m=101 -> disp32）
- `14 73 88 00` = 0x00887314

即 `mov dword ptr [887314h], ebp`，写的是 `DSurface::Temp`（0x887314）。

### F3
`sub_6D3D10` 是 `__thiscall`：`ecx = pPStm`，3 个栈参数：

| 调用点 | 实参 |
|---|---|
| 0x4F44DF | `arg0 = DSurface::Composite`，`arg1 = var_8`，`arg2 = 0` |
| 0x4F44F4 | `arg0 = DSurface::Composite`，`arg1 = var_8`，`arg2 = 1` |
| 0x4F4515 | `arg0 = DSurface::Composite`，`arg1 = var_8`，`arg2 = 2` |

其中 `var_8 = (旧 [esi+0Ch] != 0)`（0x4F44A5-0x4F44B7）；`var_4 = (旧 [esi+0Ch] == 2)`（0x4F44B4-0x4F44BE），`var_4` 作为 0x4F4502 `call [esi+0x40]`（Draw）的参数。

世界渲染期间 `DSurface::Temp` 指向 **Tile**，不是 Composite。`sub_6D3D10` 内部：

```asm
0x6D40F3 mov edx, DSurface__Temp      ; 保存旧值（此时是 DrawOnTop 设置的 Composite）
0x6D40FB mov [esp+104h+var_E4], edx
0x6D40FF mov edx, DSurface__Tile
0x6D4105 mov DSurface__Temp, edx      ; Temp = Tile(0x8872FC)
0x6D4110 mov ecx, DSurface__Tile
0x6D4117 call dword ptr [edx+5Ch]     ; Tile->Lock(0,0)
```

世界 pass 结束恢复（0x6D462E / 0x6D4639）；overlay pass（arg2=2）则在 0x6D4556-0x6D456C 把 `Temp` 设回保存值（Composite）并在 0x6D4934 恢复。也就是说：**世界绘制用 Tile，overlay/合成用 Composite**；DrawOnTop 入口的 `Temp=Composite` 只是保存/默认值。

### F4
0x4F459A `call [edx+44h]` 之后到返回只有：

```asm
0x4F459D mov DSurface__Temp, ebp
0x4F45A3 pop esi
0x4F45A4 pop ebp
0x4F45A5 add esp, 8
0x4F45A8 retn
```

没有任何对 `DSurface::Primary`（0x887308）的写，也没有 `DSurface::Temp = DSurface::Primary`。上屏发生在这个虚调用内部：`[edx+44h] -> 0x4F45B0`；非 D3D 路径在 0x4F4757 调用 `sub_4F4780(Composite, 0, cl)`，后者在 0x4F4B7B 执行 `DSurface::Primary->CopyFromPart(Primary, ..., Composite)`（DSurface 虚表 +0x08）。0x4F45B0 本身也不改 `DSurface::Temp`。

## G. SidebarClass::Draw_It 的绘制目标与调用顺序

**入口实际是 0x6A6C30**（SidebarClass 虚表 0x7F3058 的 +0x40；MapClass 的 +0x40 是 0x6D0A20，后者在 0x6D0E38 直接 `call sub_6A6C30`）。0x6A70AE 不是入口，它只是函数尾部的 `call sub_6A70E0`。

入口与表面切换：

```asm
0x6A6C30 sub     esp, 24h
0x6A6C33 push    ebx
0x6A6C34 mov     eax, dword ptr rect1_0
0x6A6C39 push    esi
0x6A6C3A push    edi
0x6A6C3B mov     edi, ecx
...
0x6A6C59 mov     ecx, DSurface__Temp
0x6A6C65 mov     [esp+30h+var_1C], ecx   ; 保存旧 Temp
0x6A6C69 mov     ecx, DSurface__Sidebar
0x6A6C74 mov     DSurface__Temp, ecx     ; Temp = Sidebar(0x887300)
0x6A6C7A mov     edx, [ecx]
0x6A6C86 call    dword ptr [edx+80h]     ; Sidebar->GetHeight()
0x6A6C8C mov     ecx, DSurface__Sidebar
0x6A6C96 call    dword ptr [eax+7Ch]     ; Sidebar->GetWidth()
...
0x6A6D33 mov     ecx, DSurface__Sidebar ; Surface
0x6A6D42 call    CC_Draw_Shape
```

该函数内所有 `CC_Draw_Shape` 的表面实参都是显式的 `DSurface::Sidebar`：

- 0x6A6D33 -> 0x6A6D42
- 0x6A6E5D -> 0x6A6E6F
- 0x6A6F0B -> 0x6A6F1C
- 0x6A6F56 -> 0x6A6F67
- 0x6A6FA1 -> 0x6A6FB2

它不是读取 `DSurface::Temp` 作为绘制目标，也不接收显式表面参数；`Temp=Sidebar` 只是让期间可能间接使用 `Temp` 的绘制函数也落到 Sidebar。

尾部：

```asm
0x6A70A6 mov     eax, 1
0x6A70AB push    eax
0x6A70AC mov     ecx, edi
0x6A70AE call    sub_6A70E0        ; 特殊/动画重绘
0x6A70B3 mov     eax, [esp+30h+var_1C]
0x6A70B7 mov     [edi+53A6h], bl
0x6A70BD mov     [edi+53A7h], bl
0x6A70C3 pop     edi
0x6A70C4 pop     esi
0x6A70C5 mov     DSurface__Temp, eax   ; 恢复旧 Temp
0x6A70CA pop     ebx
0x6A70CB add     esp, 24h
0x6A70CE retn    4
```

正常路径 `Draw_It` 本身**不 blit 到 Primary**。只有 `sub_6A70E0` 内的特殊分支（`Unsorted::WTFMode` 且侧边栏动画状态满足时）才做 Sidebar→Primary 的拷贝（`DSurface` 虚表 +0x08 = CopyFromPart）：

```asm
0x6A71F7 mov     edx, DSurface__Sidebar
0x6A7201 mov     ecx, DSurface__Primary
0x6A722C call    dword ptr [eax+8]   ; Primary->CopyFromPart(..., Sidebar, ...)
...
0x6A7460 mov     ecx, DSurface__Primary
0x6A747E call    dword ptr [edx+8]   ; Primary->CopyFromPart(..., Sidebar, ...)
```

顺序（关键）：`DrawOnTop` 自己会在 0x4F4502 通过 `call [esi+0x40]` 调用 `this->Draw()`。对 MapClass 而言 `+0x40 = 0x6D0A20`，后者在 0x6D0E38 调用 `SidebarClass::Draw_It(0x6A6C30)`。因此：

1. 0x4F44DF `sub_6D3D10(...,0)`（滚动准备）
2. 0x4F44F4 `sub_6D3D10(...,1)`（世界绘制到 Tile，再 Tile→Composite）
3. **0x4F4502 `call [esi+0x40]` → 0x6D0A20 → 0x6A6C30（Sidebar 画到 DSurface::Sidebar）**
4. 0x4F4515 `sub_6D3D10(...,2)`（overlay 合成到 Composite）
5. 0x4F459A `call [esi+0x44]`（上屏 Composite→Primary）

所以两者在**同一帧、同一次 DrawOnTop 调用内**，SidebarClass::Draw_It 在前（世界 pass 之后、overlay 与上屏之前），DrawOnTop 整体包裹它；不是两个独立渲染循环。

## H. DSurface 的 pitch / Lock 边界

### GetPitch（0x4BAD70，虚表 +0x74）

```asm
0x4BAD70 mov     eax, [ecx+20h]   ; Data.VideoSurfaceDescription (DDSURFACEDESC2*)
0x4BAD73 mov     eax, [eax+10h]   ; DDSURFACEDESC2.lPitch (union dwLinearSize)
0x4BAD76 retn
```

返回的是 **DDraw 真实 pitch**（DDSURFACEDESC2 偏移 +0x10），不是 `Width*2` 计算值。

证据链：CTOR 0x4BA5A0 在 0x4BA67A 通过 `call [ecx+58h]`（IDirectDrawSurface::GetSurfaceDesc）把物理表面描述写入 `this->Data.VideoSurfaceDescription`（对象 +0x20）；Lock 在 0x4BAE4D-0x4BAE59 又把 `Lock` 返回的 0x6C 字节描述整体拷到同一结构。CTOR 同时把参数写入 `[esi+4]=Width`、`[esi+8]=Height`（0x4BA5B5/0x4BA5B8），但 pitch 来自 DDraw，而不是这两个字段。

### Lock（0x4BAD80，虚表 +0x5C）

```asm
0x4BAD80 mov     al, byte_89F978
0x4BAD85 sub     esp, 6Ch
...
0x4BADD5 mov     esi, [ebx+0Ch]     ; LockLevel
0x4BADDC mov     edx, [ebx]
0x4BADDE xor     ecx, ecx
0x4BADE0 mov     [ebx+0Ch], ecx
0x4BADE3 push    ecx
0x4BADE4 push    eax
0x4BADE5 mov     ecx, ebx
0x4BADE7 call    dword ptr [edx+5Ch] ; 递归/重新 Lock（LockLevel 处理）
...
0x4BADFB mov     eax, [esp+7Ch+X]
0x4BAE02 test    eax, eax
0x4BAE04 jl      loc_4BAEAF          ; 只检查 X < 0
0x4BAE0A mov     ebp, [esp+7Ch+Y]
0x4BAE11 test    ebp, ebp
0x4BAE13 jl      loc_4BAEAF          ; 只检查 Y < 0
0x4BAE19 mov     eax, [ebx+0Ch]
0x4BAE1C test    eax, eax
0x4BAE1E jnz     short loc_4BAE7C
0x4BAE20 ...                          ; LockLevel==0 时构造 DDSURFACEDESC2
0x4BAE38 mov     [esp+84h+var_6C.dwSize], 6Ch
0x4BAE40 mov     ecx, [eax]
0x4BAE42 push    edx
0x4BAE43 push    0
0x4BAE45 push    eax
0x4BAE46 call    dword ptr [ecx+64h]  ; IDirectDrawSurface::Lock(NULL,&desc,1,0)
0x4BAE4B jnz     short loc_4BAEAF
0x4BAE4D mov     edi, [ebx+20h]
0x4BAE59 rep movsd                    ; 拷贝 0x6C 字节 DDSURFACEDESC2
0x4BAE5B mov     eax, [ebx+20h]
0x4BAE5E mov     ecx, [eax+54h]       ; dwRGBBitCount
0x4BAE61 add     ecx, 7
0x4BAE64 shr     ecx, 3
0x4BAE67 mov     [ebx+10h], ecx       ; BytesPerPixel
0x4BAE76 mov     eax, [eax+24h]       ; lpSurface
0x4BAE79 mov     [ebx+14h], eax       ; Data.Buffer
0x4BAE7C mov     edx, [ebx+0Ch]
0x4BAE82 mov     [ebx+0Ch], edx
0x4BAE87 call    dword ptr [edx+74h]  ; GetPitch()
0x4BAE8A mov     esi, eax
0x4BAE90 imul    esi, ebp             ; Y * pitch
0x4BAE93 call    dword ptr [eax+70h]  ; GetBytesPerPixel()
0x4BAE96 imul    eax, [esp+7Ch+X]     ; X * bpp
0x4BAE9E add     esi, eax
0x4BAEA0 mov     eax, [ebx+14h]
0x4BAEA3 add     eax, esi             ; Buffer + Y*pitch + X*bpp
0x4BAEA5 pop     edi
...
0x4BAEAC retn    8
```

结论：`Lock(X,Y)` **只检查 X<0 / Y<0**，没有任何 `X<Width` / `Y<Height` 的边界检查；返回指针按物理 pitch 计算。

### CTOR 之后把 +4 Width / +8 Height 改小的影响

- `GetPitch`：不受影响。它读的是 `VideoSurfaceDescription(+0x20)->lPitch(+0x10)`，该值由 DDraw 的 `GetSurfaceDesc`（CTOR 0x4BA67A）或 `Lock` 返回的描述刷新，等于物理表面 pitch；与结构体 +4/+8 无关。
- `Lock`：不受影响。它只检查 X,Y >= 0，然后用物理 pitch 和物理基址做 `Buffer + Y*pitch + X*bpp`。把 +4/+8 改小不会让它拒绝或裁剪越界的 X/Y（只要真实物理表面足够大），也不会改变行跨度。
- 真正的后果只出现在**依赖 +4/+8 做裁剪/blit 的其它代码**（例如 GetWidth/GetHeight 包装、CopyFromPart 目标矩形等）：逻辑尺寸变小会被这些路径用于裁剪，但 `Lock`/`GetPitch` 本身仍按物理表面工作。因此“CTOR 后改小 Width/Height、物理表面保持放大尺寸”不会破坏 Lock/GetPitch 的指针计算；但如果直接 Lock 后按小尺寸遍历，会写出逻辑边界之外、但仍在物理表面之内的像素。

## 复现步骤（只读）

1. 连接运行中的 IDA 数据库（gamemd.exe.i64）。
2. `disasm 0x4F4480` 取 DrawOnTop 全量；`get_bytes 0x4F459D` 取原始字节。
3. `disasm 0x6A6C30` / `0x6A70A0` / `0x6A7440` 取 SidebarClass::Draw_It 入口、尾部与 Primary 拷贝分支；`xrefs_to 0x6A6C30` 确认只被 0x6D0A20 与虚表引用。
4. `decompile 0x6D3D10` 与 `py_eval` 读取 0x6D40F3/0x6D40FF/0x6D4105/0x6D462E/0x6D456C 等指令，确认 Temp 在 Tile/Composite 之间切换。
5. `disasm 0x4BAD70` / `0x4BAD80` 与 `decompile 0x4BA5A0`，结合虚表 `0x7E85D4`（+0x5C/+0x70/+0x74/+0x08）确认 pitch 与 Lock 语义。
6. 全程未执行 `set_comment` / `rename` / `patch`，IDB 未变更。
