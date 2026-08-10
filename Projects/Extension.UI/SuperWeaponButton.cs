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
    //     超武按钮控件。绘制 cameo 图标、充能进度遮罩、就绪闪烁，
    //     并响应点击发射超武。
    public class SuperWeaponButton : Control
    {
        //
        // 摘要:
        //     持有的超武对象指针。
        public Pointer<SuperClass> OwnerObject;

        //
        // 摘要:
        //     Tooltip 描述文本（由 Task 7 的 Tooltip 机制使用）。
        public string Description;

        //
        // 摘要:
        //     供 Tooltip 机制使用的悬停标志。OnHover 置 true，OnMouseLeave 置 false。
        public bool ShowTooltip;

        //
        // 摘要:
        //     就绪闪烁帧计数器，每帧 OnDraw 递增。
        private int _flashFrame;

        //
        // 摘要:
        //     默认按钮尺寸（与原版 sidebar cameo 一致）。
        private const int DefaultWidth = 60;
        private const int DefaultHeight = 48;

        // 充能遮罩颜色（FillRect 不支持半透明，用深色实心遮罩近似）
        private static readonly ColorStruct ChargeMaskColor = new ColorStruct(10, 10, 10);
        // 就绪边框颜色（亮黄）
        private static readonly ColorStruct ReadyBorderColor = new ColorStruct(255, 255, 100);

        public SuperWeaponButton(Pointer<SuperClass> owner) : this(owner, string.Empty)
        {
        }

        public SuperWeaponButton(Pointer<SuperClass> owner, string description)
        {
            OwnerObject = owner;
            Description = description ?? string.Empty;
            ClientRectangle = new RectangleStruct(0, 0, DefaultWidth, DefaultHeight);
        }

        //
        // 摘要:
        //     绘制 cameo、充能遮罩、就绪闪烁。用 try-catch 包裹超武 API 调用保证异常安全。
        protected override void OnDraw()
        {
            base.OnDraw();

            if (OwnerObject.IsNull)
            {
                return;
            }

            try
            {
                Pointer<Surface> pSurface = Surface.Current;
                if (pSurface.IsNull)
                {
                    return;
                }

                RectangleStruct rect = ClientRectangle;
                RectangleStruct bound = pSurface.Ref.GetRect();
                Point2D pos = new Point2D(rect.X, rect.Y);

                // 获取 cameo SHP：SuperClass.Type -> SuperWeaponTypeClass.SidebarImage
                Pointer<SuperWeaponTypeClass> pType = OwnerObject.Ref.Type;
                if (pType.IsNull)
                {
                    return;
                }
                Pointer<SHPStruct> pCameo = pType.Ref.SidebarImage;
                if (pCameo.IsNull)
                {
                    return;
                }

                bool isCharged = OwnerObject.Ref.IsCharged;

                // 绘制 cameo（调色板用 CAMEO_PAL，frame=0，BlitterFlags.None 不居中）
                pSurface.Ref.DrawSHP(FileSystem.CAMEO_PAL, pCameo, 0, pos, bound.GetThisPointer(), BlitterFlags.None);

                // 充能进度遮罩：未充满且需要显示进度时，在底部绘制深色遮罩
                if (!isCharged && OwnerObject.Ref.ShouldDrawProgress())
                {
                    double progress = GetChargeProgress();
                    // 遮罩从底部向上，高度 = (1 - progress) * Height
                    int maskHeight = (int)((1.0 - progress) * rect.Height);
                    if (maskHeight > 0)
                    {
                        RectangleStruct maskRect = new RectangleStruct(
                            rect.X,
                            rect.Y + rect.Height - maskHeight,
                            rect.Width,
                            maskHeight);
                        // FillRect 不支持 BlitterFlags 半透明，用深色实心遮罩近似
                        pSurface.Ref.FillRect(maskRect, Drawing.RGB2DWORD(ChargeMaskColor));
                    }
                }

                // 就绪闪烁
                if (isCharged)
                {
                    _flashFrame++;

                    // 每 16 帧一个周期，后半周期（>8）增亮绘制
                    if (_flashFrame % 16 > 8)
                    {
                        // bright=2000 表示 2 倍亮度，实现闪烁高亮
                        pSurface.Ref.DrawSHP(
                            FileSystem.CAMEO_PAL, pCameo, 0, pos, bound.GetThisPointer(),
                            BlitterFlags.None, 2000, 0);
                    }

                    // 绘制就绪边框
                    pSurface.Ref.DrawRect(rect, Drawing.RGB2DWORD(ReadyBorderColor));
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     计算充能进度 [0,1]。基于 RechargeTimer 剩余时间与总充能时间。
        private double GetChargeProgress()
        {
            try
            {
                int rechargeTime = OwnerObject.Ref.GetRechargeTime();
                if (rechargeTime <= 0)
                {
                    return 1.0;
                }
                int timeLeft = OwnerObject.Ref.RechargeTimer.GetTimeLeft();
                double progress = 1.0 - (double)timeLeft / rechargeTime;
                if (progress < 0) progress = 0;
                if (progress > 1) progress = 1;
                return progress;
            }
            catch
            {
                return 0;
            }
        }

        //
        // 摘要:
        //     点击进入超武选中状态（与原版 sidebar 点击超武一致）：
        //     设置 Game.CurrentSWType 后由原生逻辑接管——光标变为超武形态，
        //     玩家点击目标位置时由原生 DisplayClass 处理发射，而非此处直接 Launch。
        //     同时清除其他 UI 模式（建造/维修/出售/规划等），避免模式冲突。
        protected internal override void OnClick()
        {
            base.OnClick();

            if (OwnerObject.IsNull)
            {
                return;
            }

            try
            {
                if (!OwnerObject.Ref.IsCharged || !OwnerObject.Ref.CanFire())
                {
                    return;
                }

                Pointer<SuperWeaponTypeClass> pType = OwnerObject.Ref.Type;
                if (pType.IsNull)
                {
                    return;
                }

                // 进入选中状态：原生根据 CurrentSWType 显示对应光标并等待玩家点击目标
                Game.CurrentSWType = pType.Ref.ArrayIndex;

                // 清除其他互斥的 UI 模式，确保原生逻辑进入超武待发射状态
                DisplayClass display = DisplayClass.Instance.Ref;
                display.PendingObject = Pointer<BuildingTypeClass>.Zero;
                display.PendingHouse = -1;
                display.RepairMode = false;
                display.SellMode = false;
                display.PowerToggleMode = false;
                display.PlanningMode = false;
                display.PlaceBeaconMode = false;
                MapClass.UnselectAll();
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     鼠标悬停时设置 ShowTooltip 标志供 Tooltip 机制使用。
        protected internal override void OnHover()
        {
            base.OnHover();
            ShowTooltip = true;
        }

        //
        // 摘要:
        //     鼠标离开时清除 ShowTooltip 标志。
        protected internal override void OnMouseLeave()
        {
            base.OnMouseLeave();
            ShowTooltip = false;
        }

        public override List<string> GetTooltipLines()
        {
            try
            {
                List<string> lines = new List<string>();

                // 第1行：超武名称（用 INI ID）
                if (!OwnerObject.IsNull && !OwnerObject.Ref.Type.IsNull)
                {
                    lines.Add(OwnerObject.Ref.Type.Ref.Base.ID.ToString());
                }

                // 第2行：充能状态
                if (!OwnerObject.IsNull)
                {
                    if (OwnerObject.Ref.IsCharged)
                    {
                        lines.Add("Ready");
                    }
                    else
                    {
                        int timeLeft = OwnerObject.Ref.RechargeTimer.GetTimeLeft();
                        lines.Add("Charging: " + timeLeft + " frames");
                    }
                }

                // 第3行+：INI Description
                if (!string.IsNullOrEmpty(Description))
                {
                    lines.Add(Description);
                }

                return lines.Count > 0 ? lines : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
