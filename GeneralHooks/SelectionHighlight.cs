using System;
using System.Collections.Generic;
using DynamicPatcher;
using Extension.EventSystems;
using PatcherYRpp;

namespace GeneralHooks
{
    //  选中柔和高亮：移植自 SelectionHighlightCore.h / SelectionHighlightRuntime.h
    //      - 100ms 平滑提亮 + 420ms 平滑淡出，总长 520ms；峰值提亮 45%（最大 +450，上限 1500）
    //      - 亮度标度按 YR 正常日光约 1000 设计，只改本次绘制参数，不写回永久光照缓存
    //      - 覆盖步兵/SHP载具/建筑（共享 SHP 本体绘制）、载具 Voxel、飞机等公共 Voxel
    //      - 建筑附带的运行动画走独立的 AnimClass 绘制流程，按建筑动画槽单独接入亮度，
    //        但共用建筑同一次高亮进度，不另起计时器
    //      - 隐形/显隐过渡、铁幕、力场护盾期间不叠加
    //      - 取消选择时先按 地址 + UniqueID 从存活集合核对，再请求本体重绘（静止建筑必须重绘）
    //      - 联机安全：只读本地选中状态与本地对象状态，不发网络事件、不用同步随机数
    public static class SelectionHighlight
    {
        const uint RiseMilliseconds = 100;
        const uint DurationMilliseconds = 520;
        const int StrengthScale = 1000;
        const int PeakPercent = 45;
        const int PeakExtraIntensity = 450;
        const int MaxIntensity = 1500;

        // TechnoClass 字段偏移（YR 1.001）
        const int OFS_ABSTRACT_FLAGS = 0x14;      // AbstractClass::AbstractFlags
        const int OFS_IRON_CURTAIN_TIMER = 0x18C; // CDTimerClass
        const int OFS_FORCE_SHIELDED = 0x1C4;     // int (0/1)
        const int OFS_CLOAK_STATE = 0x220;        // CloakState

        // BuildingClass::Anims[21]，含 Active/Idle/生产/升级/低电量/特殊动画等槽
        const int OFS_BUILDING_ANIMS = 0x55C;
        const int BUILDING_ANIM_SLOTS = 0x15;

        // 动画绘制函数内的亮度栈参数（与 Phobos AnimClass_Draw_TintColor 同一个槽）
        const int STACK_ANIM_INTENSITY = 0x38;

        struct Entry
        {
            public int Id;
            public uint Started;
            public int Value;
            public uint SeenAt;
            public bool Finished;
        }

        struct AnimBinding
        {
            public int Id;
            public int Strength;
        }

        static readonly Dictionary<IntPtr, Entry> _entries = new Dictionary<IntPtr, Entry>(64);
        static readonly Dictionary<IntPtr, AnimBinding> _animBindings = new Dictionary<IntPtr, AnimBinding>(128);
        static readonly List<IntPtr> _restoreKeys = new List<IntPtr>(16);
        static readonly List<int> _restoreIds = new List<int>(16);
        static readonly List<IntPtr> _removeKeys = new List<IntPtr>(16);

        static uint _now;

        // DP 逐文件编译，不能从 General.cs / Savegame.cs 直接调用本类，
        // 这里改订阅它们已经发出的关卡清理与读档结束事件
        static SelectionHighlight()
        {
            EventSystem.General.AddPermanentHandler(
                EventSystem.General.ScenarioClearClassesEvent,
                (sender, e) => Reset());
            EventSystem.SaveGame.AddPermanentHandler(
                EventSystem.SaveGame.LoadGameEvent,
                (sender, e) =>
                {
                    SaveLoadEventArgs args = e as SaveLoadEventArgs;
                    if (args != null && (args.IsEnd || args.IsEndInStream))
                    {
                        Reset();
                    }
                });
        }

        // ---- Core ----

        static int Strength(uint elapsed)
        {
            if (elapsed >= DurationMilliseconds)
            {
                return 0;
            }
            double t = elapsed < RiseMilliseconds
                ? (double)elapsed / RiseMilliseconds
                : (double)(DurationMilliseconds - elapsed) / (DurationMilliseconds - RiseMilliseconds);
            return (int)(StrengthScale * t * t * (3.0 - 2.0 * t) + 0.5);
        }

        static int Brighten(int intensity, int strength)
        {
            // 保留暗场景（<=0）与全亮/特殊亮度（>=上限），不降低原亮度
            if (intensity <= 0 || intensity >= MaxIntensity || strength <= 0)
            {
                return intensity;
            }
            int extra = Math.Min(PeakExtraIntensity, intensity * PeakPercent / 100)
                * Math.Min(Math.Max(strength, 0), StrengthScale) / StrengthScale;
            return Math.Min(MaxIntensity, intensity + extra);
        }

        // ---- Adapter ----

        static bool IsUsable(ref ObjectClass obj)
        {
            return obj.IsAlive && obj.IsOnMap && !obj.InLimbo;
        }

        static unsafe bool IsSuppressed(Pointer<TechnoClass> pTechno)
        {
            if (((Pointer<int>)pTechno.RawOffset(OFS_CLOAK_STATE)).Ref != 0)
            {
                return true; // Uncloaked=0，其余为隐形/显隐过渡
            }
            if (((Pointer<int>)pTechno.RawOffset(OFS_FORCE_SHIELDED)).Ref != 0)
            {
                return true;
            }
            if (((Pointer<TimerStruct>)pTechno.RawOffset(OFS_IRON_CURTAIN_TIMER)).Ref.GetTimeLeft() > 0)
            {
                return true;
            }
            return false;
        }

        static int Apply(Pointer<TechnoClass> pTechno, int intensity)
        {
            if (pTechno.IsNull)
            {
                return intensity;
            }
            ref ObjectClass obj = ref pTechno.Ref.Base;
            if (!IsUsable(ref obj) || !obj.IsSelected)
            {
                return intensity;
            }
            if (IsSuppressed(pTechno))
            {
                return intensity;
            }
            return Brighten(intensity, GetStrength(pTechno, obj.Base.UniqueID));
        }

        // ---- Tracker ----

        static bool Observe(Pointer<TechnoClass> pTechno, int id)
        {
            IntPtr address = pTechno;
            if (_entries.TryGetValue(address, out Entry entry) && entry.Id == id)
            {
                entry.SeenAt = _now;
                if (!entry.Finished)
                {
                    uint elapsed = _now - entry.Started;
                    int value = Strength(elapsed);
                    bool changed = value != entry.Value;
                    entry.Value = value;
                    entry.Finished = elapsed >= DurationMilliseconds;
                    _entries[address] = entry;
                    return changed;
                }
                _entries[address] = entry;
                return false;
            }

            entry.Id = id;
            entry.Started = _now;
            entry.Value = 0;
            entry.SeenAt = _now;
            entry.Finished = false;
            _entries[address] = entry;
            return true;
        }

        static int GetStrength(Pointer<TechnoClass> pTechno, int id)
        {
            return _entries.TryGetValue(pTechno, out Entry entry) && entry.Id == id ? entry.Value : 0;
        }

        public static void Reset()
        {
            _entries.Clear();
            _animBindings.Clear();
            _removeKeys.Clear();
            _restoreKeys.Clear();
            _restoreIds.Clear();
        }

        // ---- 建筑附带动画 ----

        static bool IsBuilding(Pointer<TechnoClass> pTechno)
        {
            return pTechno.Ref.BaseAbstract.WhatAmI() == AbstractType.Building;
        }

        static unsafe Pointer<AnimClass> GetBuildingAnim(Pointer<BuildingClass> pBuilding, int slot)
        {
            return ((Pointer<Pointer<AnimClass>>)pBuilding.RawOffset(OFS_BUILDING_ANIMS))[slot];
        }

        // 从建筑自己的动画槽确定归属：不按地图格推断，也不要求 OwnerObject 有值
        static void BindBuildingAnims(Pointer<TechnoClass> pTechno, int strength)
        {
            Pointer<BuildingClass> pBuilding = pTechno.Convert<BuildingClass>();
            for (int slot = 0; slot < BUILDING_ANIM_SLOTS; slot++)
            {
                Pointer<AnimClass> pAnim = GetBuildingAnim(pBuilding, slot);
                if (pAnim.IsNull)
                {
                    continue;
                }
                AnimBinding binding;
                binding.Id = pAnim.Ref.Base.Base.UniqueID;
                binding.Strength = strength;
                _animBindings[pAnim] = binding;
            }
        }

        // 建筑重绘时，让它的运行动画一起重绘，暂停的动画也能淡出
        static void MarkBuildingAnimsForRedraw(Pointer<TechnoClass> pTechno)
        {
            Pointer<BuildingClass> pBuilding = pTechno.Convert<BuildingClass>();
            for (int slot = 0; slot < BUILDING_ANIM_SLOTS; slot++)
            {
                Pointer<AnimClass> pAnim = GetBuildingAnim(pBuilding, slot);
                if (!pAnim.IsNull)
                {
                    pAnim.Ref.Base.MarkForRedraw();
                }
            }
        }

        // ---- 每帧更新：早于地图绘制与脏区收集 ----

        static void Update()
        {
            _now = (uint)Environment.TickCount;
            _animBindings.Clear(); // 绑定表每帧清空重建，动画中途替换也会接着当前进度淡出

            // ① 完整遍历当前选中集合（含建筑），推进脉冲；强度变化时请求本体重绘
            ref DynamicVectorClass<Pointer<ObjectClass>> selected = ref ObjectClass.CurrentObjects;
            int selectedCount = selected.Count;
            for (int i = 0; i < selectedCount; i++)
            {
                Pointer<ObjectClass> pObject = selected[i];
                if (pObject.IsNull)
                {
                    continue;
                }
                if ((((Pointer<int>)pObject.RawOffset(OFS_ABSTRACT_FLAGS)).Ref
                    & (int)AbstractFlags.Techno) == 0)
                {
                    continue;
                }

                Pointer<TechnoClass> pTechno = pObject.Convert<TechnoClass>();
                ref ObjectClass obj = ref pTechno.Ref.Base;
                if (!IsUsable(ref obj) || !obj.IsSelected)
                {
                    continue;
                }

                int id = obj.Base.UniqueID;
                bool changed = Observe(pTechno, id);
                if (changed)
                {
                    obj.MarkForRedraw();
                }

                int strength = GetStrength(pTechno, id);
                if (strength > 0 && IsBuilding(pTechno))
                {
                    BindBuildingAnims(pTechno, strength);
                    if (changed)
                    {
                        MarkBuildingAnimsForRedraw(pTechno);
                    }
                }
            }

            // End：取本帧未出现（已取消选择/已销毁）的对象身份
            if (_entries.Count == 0)
            {
                return;
            }
            _removeKeys.Clear();
            _restoreKeys.Clear();
            _restoreIds.Clear();
            foreach (KeyValuePair<IntPtr, Entry> pair in _entries)
            {
                if (pair.Value.SeenAt == _now)
                {
                    continue;
                }
                _removeKeys.Add(pair.Key);
                if (pair.Value.Value > 0)
                {
                    // 仍可能有高亮像素残留在画面缓存上
                    _restoreKeys.Add(pair.Key);
                    _restoreIds.Add(pair.Value.Id);
                }
            }
            if (_removeKeys.Count == 0)
            {
                return;
            }
            for (int i = 0; i < _removeKeys.Count; i++)
            {
                _entries.Remove(_removeKeys[i]);
            }
            if (_restoreKeys.Count == 0)
            {
                return;
            }

            // ② 只在真正存活的 Techno 集合里核对 地址 + UniqueID 后重绘本体，
            //    建筑取消选择后不在选中集合里，只能从 TechnoClass.Array 找到它
            ref DynamicVectorClass<Pointer<TechnoClass>> live = ref TechnoClass.Array;
            int liveCount = live.Count;
            for (int i = 0; i < liveCount; i++)
            {
                Pointer<TechnoClass> pTechno = live[i];
                if (pTechno.IsNull)
                {
                    continue;
                }
                int index = _restoreKeys.IndexOf(pTechno);
                if (index < 0)
                {
                    continue;
                }
                ref ObjectClass obj = ref pTechno.Ref.Base;
                if (!IsUsable(ref obj) || obj.Base.UniqueID != _restoreIds[index])
                {
                    continue; // 已销毁或地址被其他对象复用
                }
                obj.MarkForRedraw();
                if (IsBuilding(pTechno))
                {
                    MarkBuildingAnimsForRedraw(pTechno);
                }
            }
        }

        // ---- Hooks ----

        [Hook(HookType.AresHook, Address = 0x4F4483, Size = 5)] // GScreenClass::Render 入口，早于地图绘制与脏区收集
        public static unsafe UInt32 GScreenClass_Render_SelectionHighlight(REGISTERS* R)
        {
            try
            {
                Update();
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x7063EB, Size = 7)] // 共享 SHP 本体（步兵/SHP载具/建筑）：ESI=Techno，EBP=亮度
        public static unsafe UInt32 TechnoClass_DrawSHP_Brightness(REGISTERS* R)
        {
            try
            {
                int intensity = (int)R->EBP;
                int result = Apply(new Pointer<TechnoClass>((int)R->ESI), intensity);
                if (result != intensity)
                {
                    R->EBP = (uint)result;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x73C166, Size = 6)] // 载具 Voxel：EBP=Techno，亮度在 ECX 与 [ESP+1E0]
        public static unsafe UInt32 UnitClass_DrawVXL_Brightness(REGISTERS* R)
        {
            try
            {
                int intensity = (int)R->ECX;
                int result = Apply(new Pointer<TechnoClass>((int)R->EBP), intensity);
                if (result != intensity)
                {
                    // 原始字节 push ecx 会把 ECX 压入作为绘制参数，同时同步栈槽
                    R->ECX = (uint)result;
                    *(int*)((byte*)R->ESP + 0x1E0) = result;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x7067E4, Size = 8)] // 公共 Voxel（飞机等）：EBP=Techno，EDI=亮度
        public static unsafe UInt32 TechnoClass_DrawVXL_Brightness(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = new Pointer<TechnoClass>((int)R->EBP);
                // Unit 已在载具 Voxel 入口处理，这里跳过避免叠加两次
                if (!pTechno.IsNull && pTechno.Ref.BaseAbstract.WhatAmI() != AbstractType.Unit)
                {
                    int intensity = (int)R->EDI;
                    int result = Apply(pTechno, intensity);
                    if (result != intensity)
                    {
                        R->EDI = (uint)result;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        // 建筑运行动画（AnimClass）的独立绘制流程：ESI=AnimClass，亮度在栈槽 0x38
        // 紧接 Phobos AnimClass_Draw_TintColor(0x4235D3, 6) 之后，且不与其重叠
        [Hook(HookType.AresHook, Address = 0x4235D9, Size = 6)]
        public static unsafe UInt32 AnimClass_Draw_BuildingSelectionHighlight(REGISTERS* R)
        {
            try
            {
                if (_animBindings.Count == 0)
                {
                    return 0;
                }
                Pointer<AnimClass> pAnim = new Pointer<AnimClass>((int)R->ESI);
                if (pAnim.IsNull)
                {
                    return 0;
                }
                if (!_animBindings.TryGetValue(pAnim, out AnimBinding binding))
                {
                    return 0;
                }
                if (pAnim.Ref.Base.Base.UniqueID != binding.Id)
                {
                    return 0; // 地址被复用，不是当初绑定的那个动画
                }
                int* pIntensity = (int*)((byte*)R->ESP + STACK_ANIM_INTENSITY);
                *pIntensity = Brighten(*pIntensity, binding.Strength);
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
    }
}
