using System;
using System.Collections.Generic;
using UnityEngine;

namespace CMS2026UITKFramework
{
    /// <summary>
    /// Controls when scroll is active.
    /// Auto  — activates only when content exceeds viewport height (default).
    /// Always — viewport always scrollable regardless of content height.
    /// </summary>
    public enum ScrollMode { Auto, Always }

    public class UIPanel
    {
        // ── Layout ─────────────────────────────────────────────────────────
        private GameObject _go;
        private IntPtr _panelPtr;
        private IntPtr _rootPtr;
        private IntPtr _viewportPtr;   // clipping window — fixed height
        private IntPtr _contentPtr;    // grows with added elements

        private float _x, _y, _width, _height;
        private bool _visible = true;
        private string _title = "Panel";

        private bool _dragging;
        private Vector2 _dragOffset;

        private float _currentY = 0f;

        private const float TitleH = 24f;
        private const float Pad = 6f;
        private const float ElemH = 26f;
        private const float ElemGap = 4f;
        private const float SbW = 6f;    // scrollbar width
        private const float ScrollStep = 40f;

        private float ViewportH => _height - TitleH - Pad * 2;
        private float ContentW => _width - Pad * 2;

        // ── Scroll ─────────────────────────────────────────────────────────
        private float _scrollY = 0f;
        private ScrollMode _scrollMode = ScrollMode.Auto;
        private bool _showScrollbar = false;
        private bool _dragWhenScrollable = false;
        private IntPtr _scrollTrackPtr;
        private IntPtr _scrollThumbPtr;

        /// <summary>True when content is currently scrollable (based on mode and content height).</summary>
        private bool IsScrollable =>
            _scrollMode == ScrollMode.Always || _currentY > ViewportH;

        // ── Factory ────────────────────────────────────────────────────────
        public static UIPanel Create(string title, float x, float y,
                                     float width, float height)
        {
            if (!UIRuntime.IsAvailable)
            {
                FrameworkPlugin.Log.Warning("[UIPanel] UIRuntime not available");
                return null;
            }
            var p = new UIPanel
            {
                _title = title,
                _x = x,
                _y = y,
                _width = width,
                _height = height
            };
            p.Build();
            return p;
        }

        // ── Build ──────────────────────────────────────────────────────────
        private void Build()
        {
            var ps = UIRuntime.CreatePanelSettings();
            _go = new GameObject($"UITK_Panel_{_title}");
            UnityEngine.Object.DontDestroyOnLoad(_go);

            var docType = UIRuntime.UIDocumentType;
            var docRaw = _go.AddComponent(Il2CppInterop.Runtime.Il2CppType.From(docType));
            var docWrap = Activator.CreateInstance(docType,
                              new object[] { ((Component)docRaw).Pointer });

            docType.GetProperty("panelSettings").SetValue(docWrap, ps);

            var root = docType.GetProperty("rootVisualElement").GetValue(docWrap);
            _rootPtr = UIRuntime.GetPtr(root);

            BuildPanel(root);
            FrameworkPlugin.ActivePanels.Add(this);
        }

        private void BuildPanel(object root)
        {
            var panel = UIRuntime.NewVE();
            var s = UIRuntime.GetStyle(panel);
            S.Position(s, "Absolute");
            S.Left(s, _x); S.Top(s, _y);
            S.Width(s, _width); S.Height(s, _height);
            S.BgColor(s, new Color(0.08f, 0.08f, 0.10f, 0.93f));
            S.Overflow(s, "Hidden");
            UIRuntime.AddChild(root, panel);
            _panelPtr = UIRuntime.GetPtr(panel);

            BuildTitleBar(panel);
            BuildContentArea(panel);
            BuildScrollbar(panel);
            ApplyDisplay(_visible);
        }

        private void BuildTitleBar(object panel)
        {
            var lbl = Activator.CreateInstance(UIRuntime.LabelType);
            var s = UIRuntime.GetStyle(lbl);
            S.Position(s, "Absolute");
            S.Left(s, 0f); S.Top(s, 0f);
            S.Width(s, _width); S.Height(s, TitleH);
            S.BgColor(s, new Color(0.13f, 0.13f, 0.20f, 1f));
            S.Color(s, Color.white);
            S.Font(s);
            S.TextAlign(s, TextAnchor.MiddleLeft);
            UIRuntime.LabelType.GetProperty("text").SetValue(lbl, $"  {_title}");
            UIRuntime.AddChild(panel, lbl);
        }

        private void BuildContentArea(object panel)
        {
            // ── Viewport — fixed-size clipping window ──────────────────────
            var viewport = UIRuntime.NewVE();
            var vs = UIRuntime.GetStyle(viewport);
            S.Position(vs, "Absolute");
            S.Left(vs, Pad); S.Top(vs, TitleH + Pad);
            S.Width(vs, ContentW); S.Height(vs, ViewportH);
            S.Overflow(vs, "Hidden");
            UIRuntime.AddChild(panel, viewport);
            _viewportPtr = UIRuntime.GetPtr(viewport);

            // ── Content — grows as elements are added ──────────────────────
            var content = UIRuntime.NewVE();
            var cs = UIRuntime.GetStyle(content);
            S.Position(cs, "Absolute");
            S.Left(cs, 0f); S.Top(cs, 0f);
            S.Width(cs, ContentW);
            // Height intentionally not set — grows with _currentY
            UIRuntime.AddChild(viewport, content);
            _contentPtr = UIRuntime.GetPtr(content);
        }

        private void BuildScrollbar(object panel)
        {
            // ── Track — always built, hidden by default ────────────────────
            var track = UIRuntime.NewVE();
            var ts = UIRuntime.GetStyle(track);
            S.Position(ts, "Absolute");
            S.Left(ts, _width - Pad - SbW); S.Top(ts, TitleH + Pad);
            S.Width(ts, SbW); S.Height(ts, ViewportH);
            S.BgColor(ts, new Color(0.12f, 0.12f, 0.18f, 0.85f));
            S.Overflow(ts, "Hidden");
            S.Display(ts, false);   // hidden until modder calls SetScrollbarVisible(true)
            UIRuntime.AddChild(UIRuntime.WrapVE(_panelPtr), track);
            _scrollTrackPtr = UIRuntime.GetPtr(track);

            // ── Thumb ──────────────────────────────────────────────────────
            var thumb = UIRuntime.NewVE();
            var ths = UIRuntime.GetStyle(thumb);
            S.Position(ths, "Absolute");
            S.Left(ths, 1f); S.Top(ths, 0f);
            S.Width(ths, SbW - 2f); S.Height(ths, ViewportH);
            S.BgColor(ths, new Color(0.40f, 0.40f, 0.55f, 0.9f));
            UIRuntime.AddChild(track, thumb);
            _scrollThumbPtr = UIRuntime.GetPtr(thumb);
        }

        // ══════════════════════════════════════════════════════════════════
        //  PUBLIC SCROLL CONFIGURATION API
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Sets when scrolling is active.
        /// <c>Auto</c>   — only when content height exceeds viewport (default).
        /// <c>Always</c> — always, regardless of content size.
        /// </summary>
        public UIPanel SetScrollMode(ScrollMode mode)
        {
            _scrollMode = mode;
            return this;
        }

        /// <summary>
        /// Shows or hides the scrollbar indicator strip.
        /// Default: false — no visual, scroll still works via mouse wheel.
        /// </summary>
        public UIPanel SetScrollbarVisible(bool visible)
        {
            _showScrollbar = visible;
            RefreshScrollbarVisibility();
            return this;
        }

        /// <summary>
        /// Overrides scrollbar track and thumb colors.
        /// Call after <see cref="SetScrollbarVisible"/> for predictable results.
        /// </summary>
        public UIPanel SetScrollbarColors(Color track, Color thumb)
        {
            if (_scrollTrackPtr != IntPtr.Zero)
                S.BgColor(UIRuntime.GetStyle(UIRuntime.WrapVE(_scrollTrackPtr)), track);
            if (_scrollThumbPtr != IntPtr.Zero)
                S.BgColor(UIRuntime.GetStyle(UIRuntime.WrapVE(_scrollThumbPtr)), thumb);
            return this;
        }

        /// <summary>
        /// When <c>true</c>, title-bar drag is allowed even while panel is scrollable.
        /// Default: <c>false</c> — drag is suppressed when content is scrollable
        /// to prevent accidental panel movement during scroll.
        /// </summary>
        public UIPanel SetDragWhenScrollable(bool allow)
        {
            _dragWhenScrollable = allow;
            return this;
        }

        /// <summary>Scrolls content to an absolute Y position (clamped to valid range).</summary>
        public void ScrollTo(float y)
        {
            float max = Mathf.Max(0f, _currentY - ViewportH);
            _scrollY = Mathf.Clamp(y, 0f, max);
            ApplyScroll();
            UpdateScrollbar();
        }

        /// <summary>Scrolls to the very top of the content.</summary>
        public void ScrollToTop() => ScrollTo(0f);

        /// <summary>Scrolls to the very bottom of the content.</summary>
        public void ScrollToBottom() => ScrollTo(float.MaxValue);

        // ══════════════════════════════════════════════════════════════════
        //  FLUENT ELEMENT API
        // ══════════════════════════════════════════════════════════════════

        public UILabelHandle AddLabel(string text,
                                      Color? color = null,
                                      float height = ElemH)
        {
            var lbl = Activator.CreateInstance(UIRuntime.LabelType);
            var s = UIRuntime.GetStyle(lbl);
            S.Position(s, "Absolute");
            S.Left(s, 0f); S.Top(s, _currentY);
            S.Width(s, ContentW); S.Height(s, height);
            S.Color(s, color ?? Color.white);
            S.Font(s);
            UIRuntime.LabelType.GetProperty("text").SetValue(lbl, text);
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), lbl);

            var handle = new UILabelHandle(UIRuntime.GetPtr(lbl));
            _currentY += height + ElemGap;
            return handle;
        }

        public UIButtonHandle AddButton(string label,
                                        Action onClick,
                                        Color? bgColor = null,
                                        float height = ElemH)
        {
            var btn = Activator.CreateInstance(UIRuntime.ButtonType);
            var s = UIRuntime.GetStyle(btn);
            S.Position(s, "Absolute");
            S.Left(s, 0f); S.Top(s, _currentY);
            S.Width(s, ContentW); S.Height(s, height);
            S.BgColor(s, bgColor ?? new Color(0.18f, 0.28f, 0.48f, 1f));
            S.Color(s, Color.white);
            S.Font(s);
            S.TextAlign(s, TextAnchor.MiddleCenter);
            S.Padding(s, 0f);
            UIRuntime.ButtonType.GetProperty("text").SetValue(btn, label);

            if (onClick != null)
            {
                var clickable = UIRuntime.ButtonType.GetProperty("clickable").GetValue(btn);
                var il2Action = Il2CppInterop.Runtime.DelegateSupport
                                    .ConvertDelegate<Il2CppSystem.Action>(onClick);
                UIRuntime.ClickableType.GetMethod("add_clicked")
                    .Invoke(clickable, new object[] { il2Action });
            }

            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), btn);
            var handle = new UIButtonHandle(UIRuntime.GetPtr(btn));
            _currentY += height + ElemGap;
            return handle;
        }

        public UIToggleHandle AddToggle(string label,
                                        bool initial = false,
                                        Action<bool> onChange = null)
        {
            var handle = new UIToggleHandle(initial, onChange);

            var lbl = Activator.CreateInstance(UIRuntime.LabelType);
            var ls = UIRuntime.GetStyle(lbl);
            S.Position(ls, "Absolute");
            S.Left(ls, 0f); S.Top(ls, _currentY);
            S.Width(ls, ContentW - 70f); S.Height(ls, ElemH);
            S.Color(ls, Color.white);
            S.Font(ls);
            UIRuntime.LabelType.GetProperty("text").SetValue(lbl, label);
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), lbl);

            var btn = Activator.CreateInstance(UIRuntime.ButtonType);
            var bs = UIRuntime.GetStyle(btn);
            S.Position(bs, "Absolute");
            S.Left(bs, ContentW - 66f); S.Top(bs, _currentY);
            S.Width(bs, 64f); S.Height(bs, ElemH);
            S.BgColor(bs, initial
                ? new Color(0.18f, 0.58f, 0.28f, 1f)
                : new Color(0.50f, 0.15f, 0.15f, 1f));
            S.Color(bs, Color.white);
            S.Font(bs);
            S.TextAlign(bs, TextAnchor.MiddleCenter);
            S.Padding(bs, 0f);
            UIRuntime.ButtonType.GetProperty("text").SetValue(btn, initial ? "ON" : "OFF");

            Action clickHandler = () => handle.Toggle();
            var clickable = UIRuntime.ButtonType.GetProperty("clickable").GetValue(btn);
            var il2Action = Il2CppInterop.Runtime.DelegateSupport
                                .ConvertDelegate<Il2CppSystem.Action>(clickHandler);
            UIRuntime.ClickableType.GetMethod("add_clicked")
                .Invoke(clickable, new object[] { il2Action });

            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), btn);
            handle.Init(UIRuntime.GetPtr(btn));

            _currentY += ElemH + ElemGap;
            return handle;
        }

        public UISliderHandle AddSlider(string label,
                                        float min, float max,
                                        float initial,
                                        Action<float> onChange = null,
                                        float step = 1f)
        {
            float clamped = Mathf.Clamp(initial, min, max);

            const float BtnW = 28f;
            const float BtnH = 20f;
            const float Gap = 4f;
            float trackW = ContentW - BtnW * 2 - Gap * 2;
            float row2Y = _currentY + ElemH + Gap;

            // Row 1: name label
            var nameLbl = Activator.CreateInstance(UIRuntime.LabelType);
            var nls = UIRuntime.GetStyle(nameLbl);
            S.Position(nls, "Absolute");
            S.Left(nls, 0f); S.Top(nls, _currentY);
            S.Width(nls, ContentW - 56f); S.Height(nls, ElemH);
            S.Color(nls, Color.white);
            S.Font(nls);
            UIRuntime.LabelType.GetProperty("text").SetValue(nameLbl, label);
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), nameLbl);

            // Row 1: value label (right-aligned)
            var valLbl = Activator.CreateInstance(UIRuntime.LabelType);
            var vls = UIRuntime.GetStyle(valLbl);
            S.Position(vls, "Absolute");
            S.Left(vls, ContentW - 52f); S.Top(vls, _currentY);
            S.Width(vls, 52f); S.Height(vls, ElemH);
            S.Color(vls, new Color(0.80f, 1.00f, 0.80f, 1f));
            S.Font(vls);
            S.TextAlign(vls, TextAnchor.MiddleRight);
            string initStr = (step < 1f) ? clamped.ToString("F1") : ((int)clamped).ToString();
            UIRuntime.LabelType.GetProperty("text").SetValue(valLbl, initStr);
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), valLbl);

            // Row 2: [−] track/fill [+]
            var btnMinus = MakeSmallBtn("−", 0f, row2Y, BtnW, BtnH,
                               new Color(0.40f, 0.15f, 0.15f, 1f));

            var track = UIRuntime.NewVE();
            var trackS = UIRuntime.GetStyle(track);
            S.Position(trackS, "Absolute");
            S.Left(trackS, BtnW + Gap); S.Top(trackS, row2Y);
            S.Width(trackS, trackW); S.Height(trackS, BtnH);
            S.BgColor(trackS, new Color(0.18f, 0.18f, 0.22f, 1f));
            S.Overflow(trackS, "Hidden");
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), track);

            var fill = UIRuntime.NewVE();
            var fillS = UIRuntime.GetStyle(fill);
            S.Position(fillS, "Absolute");
            S.Left(fillS, 0f); S.Top(fillS, 0f);
            float initT = (max > min) ? (clamped - min) / (max - min) : 0f;
            S.Width(fillS, trackW * initT); S.Height(fillS, BtnH);
            S.BgColor(fillS, new Color(0.20f, 0.50f, 0.80f, 1f));
            UIRuntime.AddChild(track, fill);

            var btnPlus = MakeSmallBtn("+", BtnW + Gap + trackW + Gap, row2Y,
                              BtnW, BtnH, new Color(0.15f, 0.40f, 0.15f, 1f));

            var handle = new UISliderHandle(
                UIRuntime.GetPtr(fill),
                UIRuntime.GetPtr(valLbl),
                min, max, clamped, step, trackW, onChange);

            WireClick(btnMinus, () => handle.Step(-step));
            WireClick(btnPlus, () => handle.Step(+step));

            _currentY = row2Y + BtnH + ElemGap;
            return handle;
        }

        public UILabelHandle AddHeader(string text, Color? color = null)
        {
            var h = AddLabel(text, color ?? new Color(0.55f, 0.80f, 1.00f, 1f), 22f);
            AddSeparator();
            return h;
        }

        public void AddSeparator(Color? color = null)
        {
            var sep = UIRuntime.NewVE();
            var s = UIRuntime.GetStyle(sep);
            S.Position(s, "Absolute");
            S.Left(s, 0f); S.Top(s, _currentY + 4f);
            S.Width(s, ContentW); S.Height(s, 1f);
            S.BgColor(s, color ?? new Color(0.30f, 0.30f, 0.40f, 0.8f));
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), sep);
            _currentY += 10f + ElemGap;
        }

        public void AddSpace(float pixels = 8f) => _currentY += pixels;

        // ── Helpers ────────────────────────────────────────────────────────

        private object MakeSmallBtn(string label, float x, float y,
                                    float w, float h, Color bg)
        {
            var btn = Activator.CreateInstance(UIRuntime.ButtonType);
            var s = UIRuntime.GetStyle(btn);
            S.Position(s, "Absolute");
            S.Left(s, x); S.Top(s, y);
            S.Width(s, w); S.Height(s, h);
            S.BgColor(s, bg);
            S.Color(s, Color.white);
            S.Font(s);
            S.TextAlign(s, TextAnchor.MiddleCenter);
            S.Padding(s, 0f);
            UIRuntime.ButtonType.GetProperty("text").SetValue(btn, label);
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), btn);
            return btn;
        }

        private void WireClick(object btn, Action onClick)
        {
            var clickable = UIRuntime.ButtonType.GetProperty("clickable").GetValue(btn);
            var il2Action = Il2CppInterop.Runtime.DelegateSupport
                                .ConvertDelegate<Il2CppSystem.Action>(onClick);
            UIRuntime.ClickableType.GetMethod("add_clicked")
                .Invoke(clickable, new object[] { il2Action });
        }

        // ── Public panel API ───────────────────────────────────────────────

        public bool IsVisible => _visible;

        public void SetVisible(bool v) { _visible = v; ApplyDisplay(v); }
        public void Toggle() => SetVisible(!_visible);

        public void Destroy()
        {
            FrameworkPlugin.ActivePanels.Remove(this);
            if (_go != null) UnityEngine.Object.Destroy(_go);
        }

        // Called every frame by UIKitUpdater
        public void OnUpdate()
        {
            if (!_visible) return;
            HandleScroll();
            HandleDrag();
        }

        // ── Internals ──────────────────────────────────────────────────────

        private void ApplyDisplay(bool show)
        {
            var ve = UIRuntime.WrapVE(_panelPtr);
            S.Display(UIRuntime.GetStyle(ve), show);
        }

        private void HandleScroll()
        {
            if (!IsScrollable) return;

            float delta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(delta) < 0.01f) return;

            // Only scroll when cursor is inside this panel
            Vector2 mp = Input.mousePosition;
            float uitY = Screen.height - mp.y;
            bool inPanel = mp.x >= _x && mp.x <= _x + _width
                        && uitY >= _y && uitY <= _y + _height;
            if (!inPanel) return;

            float max = Mathf.Max(0f, _currentY - ViewportH);
            _scrollY = Mathf.Clamp(_scrollY - delta * ScrollStep, 0f, max);
            ApplyScroll();
            UpdateScrollbar();
        }

        private void ApplyScroll()
        {
            var content = UIRuntime.WrapVE(_contentPtr);
            S.Top(UIRuntime.GetStyle(content), -_scrollY);
        }

        /// <summary>
        /// Recomputes and applies scrollbar track visibility and thumb geometry.
        /// Called after every scroll step and after SetScrollbarVisible().
        /// </summary>
        private void RefreshScrollbarVisibility()
        {
            if (_scrollTrackPtr == IntPtr.Zero) return;
            bool show = _showScrollbar && IsScrollable;
            S.Display(UIRuntime.GetStyle(UIRuntime.WrapVE(_scrollTrackPtr)), show);
        }

        private void UpdateScrollbar()
        {
            if (_scrollTrackPtr == IntPtr.Zero || _scrollThumbPtr == IntPtr.Zero) return;

            bool show = _showScrollbar && IsScrollable;
            S.Display(UIRuntime.GetStyle(UIRuntime.WrapVE(_scrollTrackPtr)), show);
            if (!show) return;

            float max = Mathf.Max(0f, _currentY - ViewportH);
            float ratio = ViewportH / Mathf.Max(_currentY, ViewportH + 0.1f);
            float thumbH = Mathf.Max(16f, ViewportH * ratio);
            float thumbY = max > 0f
                ? (_scrollY / max) * (ViewportH - thumbH)
                : 0f;

            var thumb = UIRuntime.WrapVE(_scrollThumbPtr);
            S.Height(UIRuntime.GetStyle(thumb), thumbH);
            S.Top(UIRuntime.GetStyle(thumb), thumbY);
        }

        private void HandleDrag()
        {
            // Default: suppress drag when panel is scrollable (prevents
            // accidental moves during scroll). Modder can override with
            // SetDragWhenScrollable(true).
            if (IsScrollable && !_dragWhenScrollable) return;

            Vector2 mp = Input.mousePosition;
            float uitY = Screen.height - mp.y;
            bool inTitle = mp.x >= _x && mp.x <= _x + _width
                        && uitY >= _y && uitY <= _y + TitleH;

            if (Input.GetMouseButtonDown(0) && inTitle)
            {
                _dragging = true;
                _dragOffset = new Vector2(mp.x - _x, uitY - _y);
            }
            if (Input.GetMouseButtonUp(0)) _dragging = false;
            if (!_dragging || !Input.GetMouseButton(0)) return;

            float uitYNow = Screen.height - Input.mousePosition.y;
            _x = Mathf.Clamp(Input.mousePosition.x - _dragOffset.x,
                              0f, Screen.width - _width);
            _y = Mathf.Clamp(uitYNow - _dragOffset.y,
                              0f, Screen.height - _height);

            var s = UIRuntime.GetStyle(UIRuntime.WrapVE(_panelPtr));
            S.Left(s, _x); S.Top(s, _y);
        }
    }
}