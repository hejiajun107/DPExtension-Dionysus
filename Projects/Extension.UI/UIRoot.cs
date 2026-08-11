using DynamicPatcher;
using Extension.EventSystems;
using Extension.UI.Core;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.UI
{
    public class UIRoot : Panel
    {
        private static readonly UIRoot _instance = new UIRoot();

        public static UIRoot Instance => _instance;

        private EventHandler _renderHandler;
        private EventHandler _mouseMoveHandler;
        private EventHandler _mouseDownHandler;
        private EventHandler _mouseUpHandler;

        //
        // 摘要:
        //     最近一次鼠标事件是否被 UI 消费（命中了子控件）。事件处理器设置后由 MouseEvent Hook 读取。
        public bool IsMouseEventConsumed => _mouseConsumed;

        //
        // 摘要:
        //     当前被悬停的控件。鼠标移动时通过 HitTest 更新。
        private Control _hoveredControl;

        //
        // 摘要:
        //     是否需要把鼠标切成手/指针形。OnMouseMove 置状态，OnGScreenRender 每帧写 WWMouseClass.PointerFrame，
        //     这样不会被游戏 Sidebar / Tactical 每帧的指针设置覆盖。
        private bool _uiCursorActive;

        // 诊断用：每 ~60 帧打一次 PointerFrame 候选地址当前值
        private int _cursorDiagCounter;

        //
        // 摘要:
        //     MouseDown 时命中的控件。MouseUp 时若命中同一控件则触发 OnClick。
        private Control _pressedControl;

        //
        // 摘要:
        //     最近一次鼠标事件是否被 UI 消费（命中了控件）。
        //     Broadcast 在 ConsumeMouseHandler 调用之前执行，因此此值已被事件处理器设置。
        private bool _mouseConsumed;

        //
        // 摘要:
        //     当前悬停的 OnHover 是否已触发，避免重复触发。
        private bool _hoverTriggered;

        //
        // 摘要:
        //     Tooltip 实例。Hover 触发时设置内容并显示，鼠标移出控件时隐藏。
        //     在 Draw() 之后调用 _tooltip.Draw() 以保证位于最上层。
        private Tooltip _tooltip = new Tooltip();

        private UIRoot()
        {
        }

        //
        // 摘要:
        //     订阅 GScreenRenderEvent 和鼠标事件，注册 ConsumeMouseHandler。
        //     幂等：内部先取消已有订阅再重新订阅，避免重复注册。
        public void Init()
        {
            Unsubscribe();

            _renderHandler = OnGScreenRender;
            EventSystem.GScreen.AddTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, _renderHandler);

            _mouseMoveHandler = OnMouseMove;
            _mouseDownHandler = OnMouseDown;
            _mouseUpHandler = OnMouseUp;
            EventSystem.MouseEventSystem.AddTemporaryHandler(EventSystem.MouseEventSystem.MouseMoveEvent, _mouseMoveHandler);
            EventSystem.MouseEventSystem.AddTemporaryHandler(EventSystem.MouseEventSystem.MouseDownEvent, _mouseDownHandler);
            EventSystem.MouseEventSystem.AddTemporaryHandler(EventSystem.MouseEventSystem.MouseUpEvent, _mouseUpHandler);

            // 创建超武面板并添加到控件树
            ClearChildren();
            try
            {
                SuperWeaponPanel swPanel = new SuperWeaponPanel();
                AddChild(swPanel);
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     移除所有订阅并清空子控件及鼠标路由状态。供场景结束时调用。
        public void Cleanup()
        {
            Unsubscribe();
            ClearChildren();

            _hoveredControl = null;
            _pressedControl = null;
            _mouseConsumed = false;
            _hoverTriggered = false;
            _tooltip.IsVisible = false;
            _tooltip.Target = null;
        }

        private void Unsubscribe()
        {
            if (_renderHandler != null)
            {
                EventSystem.GScreen.RemoveTemporaryHandler(EventSystem.GScreen.GScreenRenderEvent, _renderHandler);
                _renderHandler = null;
            }
            if (_mouseMoveHandler != null)
            {
                EventSystem.MouseEventSystem.RemoveTemporaryHandler(EventSystem.MouseEventSystem.MouseMoveEvent, _mouseMoveHandler);
                _mouseMoveHandler = null;
            }
            if (_mouseDownHandler != null)
            {
                EventSystem.MouseEventSystem.RemoveTemporaryHandler(EventSystem.MouseEventSystem.MouseDownEvent, _mouseDownHandler);
                _mouseDownHandler = null;
            }
            if (_mouseUpHandler != null)
            {
                EventSystem.MouseEventSystem.RemoveTemporaryHandler(EventSystem.MouseEventSystem.MouseUpEvent, _mouseUpHandler);
                _mouseUpHandler = null;
            }
        }

        private void OnGScreenRender(object sender, EventArgs e)
        {
            if (e is GScreenEventArgs args && args.IsLateRender)
            {
                Update();
                Draw();
                // Tooltip 在所有控件绘制之后绘制，保证位于最上层
                _tooltip?.Draw();

                // ---- 每帧三层保险写入手型 ----
                // MouseEvent Hook (UpdateCursor 内) 已写一次；游戏后续 SetCursor 可能覆盖，
                // LateRender 是当前渲染循环最晚的阶段，这里再写一次确保最终绘制生效。
                // 三层写入: 1)MouseClass.MouseCursorIndex(0x555C) 2)MouseCursorCurrentFrame(0x5564) 3)WWMouseClass.ImageFrameIndex(+0x08)
                try
                {
                    if (_uiCursorActive)
                    {
                        WWMouseClass.SetPointerFrameSelectHand();
                    }

                    // 诊断日志（每 ~60 帧）
                    _cursorDiagCounter++;
                    if (_cursorDiagCounter >= 60)
                    {
                        _cursorDiagCounter = 0;
                        long pInst = WWMouseClass.ReadInstancePointerRaw;
                        int imgFrame = WWMouseClass.ReadImageFrameIndex;
                        int mcIdx = SidebarClass.ReadMouseCursorIndex;
                        int mcFrame = SidebarClass.ReadMouseCursorCurrentFrame;
                        Logger.Log(
                            "[Cursor] uiActive={0}  pWWMouseClass=0x{1:X}  ImageFrameIndex={2}  MC_Index(0x555C)={3}  MC_Frame(0x5564)={4}",
                            _uiCursorActive, pInst, imgFrame, mcIdx, mcFrame);
                    }
                }
                catch { /* 忽略 */ }
            }
        }

        //
        // 摘要:
        //     UIRoot 自身不消费鼠标事件，仅检查子控件。
        //     鼠标不在任何子控件上时返回 null，使游戏正常处理鼠标。
        public override Control HitTest(Point2D pos)
        {
            RectangleStruct rect = ClientRectangle;
            if (pos.X < rect.X || pos.X >= rect.X + rect.Width ||
                pos.Y < rect.Y || pos.Y >= rect.Y + rect.Height)
            {
                return null;
            }

            for (int i = _children.Count - 1; i >= 0; i--)
            {
                Control hit = _children[i].HitTest(pos);
                if (hit != null)
                {
                    return hit;
                }
            }

            return null;
        }

        //
        // 摘要:
        //     处理鼠标移动：HitTest 找到悬停控件，管理 MouseEnter/Leave 切换。
        private void OnMouseMove(object sender, EventArgs e)
        {
            try
            {
                if (e is MouseEventArgs args)
                {
                    Control hit = HitTest(args.Position);
                    _mouseConsumed = (hit != null);

                    if (hit != _hoveredControl)
                    {
                        if (_hoveredControl != null)
                        {
                            _hoveredControl.IsHovered = false;
                            _hoveredControl._hoverFrameCount = 0;
                            _hoveredControl.OnMouseLeave();
                        }
                        // 悬停控件切换或移出时隐藏 Tooltip
                        _tooltip.IsVisible = false;
                        _hoveredControl = hit;
                        _hoverTriggered = false;
                        // 更新指针形状状态：悬停在任何命中控件上时显示手型
                        _uiCursorActive = (hit != null);
                        if (hit != null)
                        {
                            hit.IsHovered = true;
                            hit._hoverFrameCount = 0;
                            hit.OnMouseEnter();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     处理鼠标按下：HitTest 后对命中控件触发 OnMouseDown，记录 _pressedControl。
        private void OnMouseDown(object sender, EventArgs e)
        {
            try
            {
                if (e is MouseEventArgs args)
                {
                    Control hit = HitTest(args.Position);
                    _mouseConsumed = (hit != null);
                    _pressedControl = hit;
                    if (hit != null)
                    {
                        hit.OnMouseDown(args);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        //
        // 摘要:
        //     处理鼠标释放：HitTest 后对命中控件触发 OnMouseUp；
        //     若 Down 和 Up 命中同一控件则触发 OnClick。
        private void OnMouseUp(object sender, EventArgs e)
        {
            try
            {
                if (e is MouseEventArgs args)
                {
                    Control hit = HitTest(args.Position);
                    _mouseConsumed = (hit != null);
                    if (hit != null)
                    {
                        hit.OnMouseUp(args);
                        if (hit == _pressedControl)
                        {
                            hit.OnClick();
                        }
                    }
                    _pressedControl = null;
                }
            }
            catch (Exception ex)
            {
                Logger.PrintException(ex);
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // 窗口可能 resize，每帧将 ClientRectangle 同步到 Surface.ViewBound
            ClientRectangle = Surface.ViewBound;

            // Hover 计时：悬停超过 30 帧触发 OnHover（仅触发一次）
            if (_hoveredControl != null && !_hoverTriggered)
            {
                _hoveredControl._hoverFrameCount++;
                if (_hoveredControl._hoverFrameCount > 30)
                {
                    _hoverTriggered = true;
                    _hoveredControl.OnHover();

                    // 检查控件是否有 Tooltip 内容，有则显示
                    List<string> tooltipLines = _hoveredControl.GetTooltipLines();
                    if (tooltipLines != null && tooltipLines.Count > 0)
                    {
                        _tooltip.Target = _hoveredControl;
                        _tooltip.Lines = tooltipLines;
                        _tooltip.IsVisible = true;
                    }
                }
            }
        }
    }
}
