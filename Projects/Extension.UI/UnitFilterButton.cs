using DynamicPatcher;
using Extension.UI.Core;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using System;
using System.Collections.Generic;

namespace Extension.UI
{
    //
    // 摘要:
    //     单位过滤按钮。显示单位 cameo 图标，点击后从当前选中里过滤只保留该类型。
    //     pType 为空时画纯黑色矩形占位（不足 9 格时保留布局）。
    //     联机安全：仅修改本地 CurrentObjects 选中状态，不触发网络事件。
    public class UnitFilterButton : Control
    {
        //
        // 摘要:
        //     该按钮对应的单位类型指针。空表示空格占位。
        public Pointer<TechnoTypeClass> TypePointer;

        //
        // 摘要:
        //     分组键（来自 UnitGroup：读 [单位ID] GroupAs=，未配置时默认为单位自身 ID）。
        //     有 GroupAs 的单位视作同一类，点击时保留同组所有单位。
        public string GroupKey;

        //
        // 摘要:
        //     默认按钮尺寸（与原版 sidebar cameo 一致）。
        private const int DefaultWidth = 60;
        private const int DefaultHeight = 48;

        // 空格填充色（纯黑）
        private static readonly ColorStruct EmptyColor = new ColorStruct(0, 0, 0);
        // 图标全部解析失败时使用的 UI 占位图标（PCX），避免格子出现纯黑
        private const string NoCameoPCX = "uinocameo.pcx";
        // 选中高亮边框色（亮黄）
        private static readonly ColorStruct HighlightColor = new ColorStruct(255, 255, 100);

        // cameo 名（不含扩展名）→ 已加载 SHP 的缓存，避免每帧重新读盘。
        // 加载失败也缓存 Zero，表示不用再试。
        private static readonly Dictionary<string, Pointer<SHPStruct>> _shpCache =
            new Dictionary<string, Pointer<SHPStruct>>();

        public UnitFilterButton() : this(Pointer<TechnoTypeClass>.Zero, null)
        {
        }

        public UnitFilterButton(Pointer<TechnoTypeClass> pType, string groupKey)
        {
            TypePointer = pType;
            GroupKey = groupKey;
            ClientRectangle = new RectangleStruct(0, 0, DefaultWidth, DefaultHeight);
        }

        //
        // 摘要:
        //     绘制 cameo 或黑色空格，并在当前选中含该类型时画高亮边框。
        //     图标名解析（与游戏/Ares 一致）：按 rules [ID] Image= 定位 art 节（缺省=ID），
        //     从 art INI 取 CameoPCX / Cameo。
        //     绘制优先级：CameoPCX(PCX) → Cameo(SHP) → CameoFile → uinocameo.pcx → 引擎默认 missing cameo → 黑底。
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

                RectangleStruct rect = ClientRectangle;
                Point2D pos = new Point2D(rect.X, rect.Y);
                RectangleStruct bound = pSurface.Ref.GetRect();

                // 空格：纯黑色矩形
                if (TypePointer.IsNull)
                {
                    pSurface.Ref.FillRect(rect, Drawing.RGB2DWORD(EmptyColor));
                    return;
                }

                bool drew = false;

                // 定位 art 节：rules [ID] Image=，缺省为 ID 本身
                string artSection;
                if (!UnitGroup.TryGetIdAndArtSection(TypePointer, out _, out artSection))
                {
                    artSection = string.Empty;
                }

                // 1) CameoPCX（Ares 扩展，art INI [Image] CameoPCX=，文件名含 .pcx）
                if (!string.IsNullOrEmpty(artSection))
                {
                    string cameoPCX = UnitGroup.ReadArtString(artSection, "CameoPCX");
                    if (!string.IsNullOrEmpty(cameoPCX))
                    {
                        drew = DrawPCX(cameoPCX, pSurface, rect);
                    }
                }

                // 2) 原版 SHP cameo（art INI [Image] Cameo=，名字不含扩展名，加载时补 .shp）
                if (!drew && !string.IsNullOrEmpty(artSection))
                {
                    string cameoName = UnitGroup.ReadArtString(artSection, "Cameo");
                    if (!string.IsNullOrEmpty(cameoName))
                    {
                        Pointer<SHPStruct> pSHP = LoadCameoSHP(cameoName);
                        if (!pSHP.IsNull)
                        {
                            pSurface.Ref.DrawSHP(FileSystem.CAMEO_PAL, pSHP, 0, pos, bound.GetThisPointer(), BlitterFlags.None);
                            drew = true;
                        }
                    }
                }

                // 2b) 兜底：引擎已解析好的 cameo 文件名（TechnoTypeClass.CameoFile 字段，
                //     由游戏从 art INI 读入）。纯字段读取，无 INI/引擎调用风险。
                if (!drew)
                {
                    string filedCameo = TypePointer.Ref.CameoFile.ToString();
                    if (!string.IsNullOrEmpty(filedCameo))
                    {
                        Pointer<SHPStruct> pFiled = LoadCameoSHP(filedCameo);
                        if (!pFiled.IsNull)
                        {
                            pSurface.Ref.DrawSHP(FileSystem.CAMEO_PAL, pFiled, 0, pos, bound.GetThisPointer(), BlitterFlags.None);
                            drew = true;
                        }
                    }
                }

                // 3) 兜底：先用 UI 的 no-cameo 占位图（uinocameo.pcx）取代引擎的 missing cameo
                if (!drew)
                {
                    drew = DrawPCX(NoCameoPCX, pSurface, rect);
                }

                // 4) 占位图也加载不到时，才退回引擎默认 missing cameo
                //    （类型初始化时 Cameo 字段默认指向全局占位 SHP）
                if (!drew)
                {
                    Pointer<SHPStruct> pDefault = TypePointer.Ref.Cameo;
                    if (!pDefault.IsNull)
                    {
                        pSurface.Ref.DrawSHP(FileSystem.CAMEO_PAL, pDefault, 0, pos, bound.GetThisPointer(), BlitterFlags.None);
                        drew = true;
                    }
                }

                // 5) 仍然拿不到才画纯黑
                if (!drew)
                {
                    pSurface.Ref.FillRect(rect, Drawing.RGB2DWORD(EmptyColor));
                }

                // 选中高亮：当前 CurrentObjects 含该类型
                if (IsTypeInCurrentSelection())
                {
                    pSurface.Ref.DrawRect(rect, Drawing.RGB2DWORD(HighlightColor));
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     检查当前选中列表是否包含该分组的单位（按 GroupKey 比较）。
        private bool IsTypeInCurrentSelection()
        {
            if (TypePointer.IsNull || string.IsNullOrEmpty(GroupKey))
            {
                return false;
            }
            var currentObjs = ObjectClass.CurrentObjects;
            for (int i = 0; i < currentObjs.Count; i++)
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
                if (UnitGroup.GetGroupKey(pType) == GroupKey)
                {
                    return true;
                }
            }
            return false;
        }

        //
        // 摘要:
        //     用 PCX 管理器加载并绘制指定文件到按钮位置。按 PCX 源图实际尺寸 1:1 blit
        //     （cameo PCX 通常即 60×48），默认 colorkey 透明。成功返回 true。
        private bool DrawPCX(string pcxFile, Pointer<Surface> pSurface, RectangleStruct rect)
        {
            PCX.Instance.LoadFile(pcxFile);
            Pointer<BSurface> pPCX = PCX.Instance.GetSurface(pcxFile, Pointer<BytePalette>.Zero);
            if (pPCX.IsNull)
            {
                return false;
            }

            int w = pPCX.Ref.Base.Base.Width;
            int h = pPCX.Ref.Base.Base.Height;
            RectangleStruct blitRect = new RectangleStruct(rect.X, rect.Y, w, h);

            // 与目标表面视口求交，避免越界 blit
            RectangleStruct view = pSurface.Ref.GetRect();
            int x = Math.Max(blitRect.X, view.X);
            int y = Math.Max(blitRect.Y, view.Y);
            int right = Math.Min(blitRect.X + blitRect.Width, view.X + view.Width);
            int bottom = Math.Min(blitRect.Y + blitRect.Height, view.Y + view.Height);
            if (right <= x || bottom <= y)
            {
                return false;
            }
            RectangleStruct clipRect = new RectangleStruct(x, y, right - x, bottom - y);

            PCX.Instance.BlitToSurface(clipRect.GetThisPointer(), pSurface.Convert<DSurface>(), pPCX);
            return true;
        }

        //
        // 摘要:
        //     按 cameo 名（art INI Cameo= 的值，不含扩展名）加载 SHP，结果全局缓存。
        //     参照 GameUtil.RenderCameoSHP：FileSystem.TyrLoadSHPFile(name + ".shp")。
        //     加载失败缓存 Zero，不再重复尝试。
        private Pointer<SHPStruct> LoadCameoSHP(string cameoName)
        {
            if (_shpCache.TryGetValue(cameoName, out Pointer<SHPStruct> cached))
            {
                return cached;
            }

            Pointer<SHPStruct> pSHP = Pointer<SHPStruct>.Zero;
            try
            {
                string file = cameoName.EndsWith(".shp", StringComparison.OrdinalIgnoreCase)
                    ? cameoName
                    : cameoName + ".shp";
                FileSystem.TyrLoadSHPFile(file, out pSHP);
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
            _shpCache[cameoName] = pSHP;
            return pSHP;
        }

        //
        // 摘要:
        //     点击过滤当前选中：只保留同分组单位，其他 Deselect。
        //     有 GroupAs 的同组单位全部保留。先快照 CurrentObjects 再遍历。
        //     联机安全：Deselect 只改本地选中状态，不进网络包。
        protected internal override void OnClick()
        {
            base.OnClick();

            if (TypePointer.IsNull || string.IsNullOrEmpty(GroupKey))
            {
                return;
            }

            try
            {
                // 快照当前选中列表
                var snapshot = new System.Collections.Generic.List<Pointer<ObjectClass>>();
                var currentObjs = ObjectClass.CurrentObjects;
                for (int i = 0; i < currentObjs.Count; i++)
                {
                    snapshot.Add(currentObjs[i]);
                }

                // 非同分组单位取消选中
                foreach (var pObj in snapshot)
                {
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
                    if (pType.IsNull || UnitGroup.GetGroupKey(pType) != GroupKey)
                    {
                        pObj.Ref.Deselect();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }
    }
}
