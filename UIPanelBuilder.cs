using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        public string Title => _title;
        private GameObject _go;
        private IntPtr _panelPtr;

        public IntPtr GetPanelRawPtr() => _panelPtr;

        private IntPtr _rootPtr;
        private IntPtr _viewportPtr;   // clipping window — fixed height
        private IntPtr _contentPtr;    // grows with added elements

        private float _x, _y, _width, _height;
        private bool _visible = true;
        private string _title = "Panel";

        private bool _dragging;
        private Vector2 _dragOffset;

        private object _panelSettings;
        private float _currentY = 0f;

        private const float TitleH = 24f;
        private const float Pad = 6f;
        private const float ElemH = 26f;
        private const float ElemGap = 4f;
        private const float SbW = 6f;    // scrollbar width
        private const float ScrollStep = 40f;

        private float ViewportH => _height - TitleH - Pad * 2;
        private float ContentW => _width - Pad * 3 - SbW;

        // ── Scroll ─────────────────────────────────────────────────────────
        private float _scrollY = 0f;
        private ScrollMode _scrollMode = ScrollMode.Auto;
        private bool _showScrollbar = false;
        private bool _dragWhenScrollable = false;
        private bool _draggable = true;
        private IntPtr _scrollTrackPtr;
        private IntPtr _scrollThumbPtr;



        // ── Dropdown ─────────────────────────────────────────────────────────
        private readonly List<UIDropdownHandle> _dropdownHandles = new();


        // ── Update callback ────────────────────────────────────────────────────────
        private Action<float> _updateCallback;

        /// <summary>True when content is currently scrollable (based on mode and content height).</summary>
        private bool IsScrollable =>
            _scrollMode == ScrollMode.Always || _currentY > ViewportH;

        // ── Factory ────────────────────────────────────────────────────────
        public static UIPanel Create(string title, float x, float y,
                                     float width, float height,
                                     int sortOrder = 9999)
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

            //p.Build(); przenosimy to zewnetrznego publika ponizej
            p.SetSortOrder(sortOrder);
            return p;
        }

        // ── Internal accessors for UIRowBuilder dropdowns ──────────────────
        //internal void AddOverlayToPanel(object ve)
        //    => UIRuntime.AddChild(UIRuntime.WrapVE(_panelPtr), ve); //duplikat 

        internal float GetScrollY() => _scrollY;

        internal void RegisterDropdownHandle(UIDropdownHandle dd)
            => _dropdownHandles.Add(dd);




        // ── Build ──────────────────────────────────────────────────────────
        public UIPanel Build(int sortOrder = 9999)//odteraz jest publiczne -> chodzi o pozniejsze dobudowywanie przyciskow do belki
        {
            _panelSettings = UIRuntime.CreatePanelSettings();
            var ps = _panelSettings;
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

            SetSortOrder(sortOrder); // Ustawiamy to po zbudowaniu!

            return this;
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
            var bar = UIRuntime.NewVE();
            var bs = UIRuntime.GetStyle(bar);
            S.Position(bs, "Absolute");
            S.Left(bs, 0f); S.Top(bs, 0f);
            S.Width(bs, _width); S.Height(bs, TitleH);
            S.BgColor(bs, new Color(0.13f, 0.13f, 0.20f, 1f));
            S.Overflow(bs, "Hidden");
            UIRuntime.AddChild(panel, bar);

            // Title label
            var lbl = Activator.CreateInstance(UIRuntime.LabelType);
            var ls = UIRuntime.GetStyle(lbl);
            S.Position(ls, "Absolute");
            S.Left(ls, 6f); S.Top(ls, 0f);
            S.Width(ls, _width - 6f); S.Height(ls, TitleH);
            S.Color(ls, Color.white);
            S.Font(ls);
            S.TextAlign(ls, TextAnchor.MiddleLeft);
            UIRuntime.LabelType.GetProperty("text").SetValue(lbl, _title);
            UIRuntime.AddChild(bar, lbl);

            // Control buttons — right to left
            float xRight = _width;
            const float BtnW = 28f;

            foreach (var (label, onClick, bg) in
                     System.Linq.Enumerable.Reverse(_titleButtons))
            {
                xRight -= BtnW;
                var btn = Activator.CreateInstance(UIRuntime.ButtonType);
                var s = UIRuntime.GetStyle(btn);
                S.Position(s, "Absolute");
                S.Left(s, xRight); S.Top(s, 0f);
                S.Width(s, BtnW); S.Height(s, TitleH);
                S.BgColor(s, bg);
                S.Color(s, Color.white);
                S.Font(s);
                S.TextAlign(s, TextAnchor.MiddleCenter);
                S.Padding(s, 0f);
                UIRuntime.ButtonType.GetProperty("text").SetValue(btn, label);
                WireClick(btn, onClick);

                Color hover = new Color(
                    Mathf.Min(bg.r + 0.15f, 1f),
                    Mathf.Min(bg.g + 0.15f, 1f),
                    Mathf.Min(bg.b + 0.15f, 1f), bg.a);
                WireHoverPress(btn, bg, hover,
                    new Color(bg.r * 0.7f, bg.g * 0.7f, bg.b * 0.7f, bg.a));

                UIRuntime.AddChild(bar, btn);
            }
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

        /// <summary>
        /// Called every frame while panel is visible.
        /// deltaTime = Time.deltaTime — use for animations, live data refresh.
        /// </summary>
        public UIPanel SetUpdateCallback(Action<float> callback)
        {
            _updateCallback = callback;
            return this;
        }

        /// <summary>
        /// Creates a horizontal row container.
        /// Add elements via the returned UIRowBuilder — they are placed left-to-right.
        /// </summary>
        /// <param name="height">Row height in pixels (default 26)</param>
        /// <param name="gap">Vertical gap after the row (default ElemGap)</param>
        public UIRowBuilder AddRow(float height = ElemH, float gap = ElemGap)
        {
            var container = UIRuntime.NewVE();
            var s = UIRuntime.GetStyle(container);
            S.Position(s, "Absolute");
            S.Left(s, 0f); S.Top(s, _currentY);
            S.Width(s, ContentW); S.Height(s, height);
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), container);

            float rowTopInPanel = TitleH + Pad + _currentY;
            _currentY += height + gap;

            return new UIRowBuilder(                         // <-- rozszerzona sygnatura
                UIRuntime.GetPtr(container),
                ContentW, height,
                this, rowTopInPanel);
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

        /// <summary>
        /// Enables or disables title-bar dragging entirely.
        /// Default: true. SetDraggable(false) overrides SetDragWhenScrollable.
        /// </summary>
        public UIPanel SetDraggable(bool draggable)
        {
            _draggable = draggable;
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

        /// <summary>
        /// Zmienia kolejność renderowania panelu.
        /// Wyższy numer = na wierzchu. Domyślnie 9999.
        /// </summary>
        public UIPanel SetSortOrder(int order)
        {
            if (_panelSettings != null)
                UIRuntime.PanelSettingsType
                    .GetProperty("sortingOrder")
                    .SetValue(_panelSettings, order);
            return this;
        }

        // ── Title bar control buttons ──────────────────────────────────────
        private readonly List<(string label, Action onClick, Color bg)> _titleButtons = new();

        /// <summary>
        /// Adds a button to the RIGHT side of the title bar.
        /// Call before Build() — or rebuild title bar after.
        /// Buttons are added right-to-left (last added = rightmost).
        /// </summary>
        public UIPanel AddTitleButton(string label, Action onClick,
                                       Color? bgColor = null, float width = 28f)
        {
            _titleButtons.Add((label, onClick, bgColor ?? new Color(0.25f, 0.25f, 0.35f, 1f)));
            return this;
        }

        public void SetSize(float width, float height)
        {
            _width = width;
            _height = height;

            // 1. KLUCZ: Aktualizacja PanelSettings (to steruje rozmiarem "okna" na ekranie)
            if (_panelSettings != null)
            {
                try
                {
                    var psType = _panelSettings.GetType();
                    // Musimy ustawić referenceResolution, aby fizyczny obszar renderowania UI się zmienił
                    var resProp = psType.GetProperty("referenceResolution");
                    if (resProp != null)
                    {
                        // UIToolkit wymaga Vector2Int dla rozdzielczości
                        resProp.SetValue(_panelSettings, new Vector2Int((int)_width, (int)_height));
                    }

                    // Opcjonalnie: upewnij się, że scaleMode to ConstantPixelSize, 
                    // żeby 500px zawsze było 500px
                    var scaleModeProp = psType.GetProperty("scaleMode");
                    if (scaleModeProp != null)
                    {
                        // Pobieramy typ enum PanelScaleMode z assembly UIElements
                        var psmType = UIRuntime.UEAsm.GetType("UnityEngine.UIElements.PanelScaleMode");
                        if (psmType != null)
                        {
                            scaleModeProp.SetValue(_panelSettings, Enum.Parse(psmType, "ConstantPixelSize"));
                        }
                    }
                }
                catch (Exception ex) { FrameworkPlugin.Log.Error($"[SetSize] PS Error: {ex.Message}"); }
            }

            // 2. Aktualizacja kontenerów VisualElement (tak jak w Twojej konsoli)
            if (_rootPtr != IntPtr.Zero)
            {
                var rootVE = UIRuntime.WrapVE(_rootPtr);
                var style = UIRuntime.GetStyle(rootVE);

                // Ustawiamy wymiary na sztywno
                S.Width(style, _width);
                S.Height(style, _height);

                // 3. Aktualizacja Viewportu (żeby scrollbar i ucinanie treści pasowało do nowej wielkości)
                if (_viewportPtr != IntPtr.Zero)
                {
                    var viewVE = UIRuntime.WrapVE(_viewportPtr);
                    var vStyle = UIRuntime.GetStyle(viewVE);
                    S.Width(vStyle, _width);
                    S.Height(vStyle, _height - TitleH); // TitleH to Twój pasek tytułowy (24f)
                }

                // 4. Content - jeśli masz rzędy (AddRow), one korzystają z _width panelu przy tworzeniu,
                // ale stare rzędy nie zmienią szerokości same z siebie (mają Absolute).
                // Możemy jednak wymusić szerokość kontenera treści:
                if (_contentPtr != IntPtr.Zero)
                {
                    var contentVE = UIRuntime.WrapVE(_contentPtr);
                    var cStyle = UIRuntime.GetStyle(contentVE);
                    S.Width(cStyle, _width);
                }
            }
        }


        // ══════════════════════════════════════════════════════════════════
        //  FLUENT ELEMENT API
        // ══════════════════════════════════════════════════════════════════

        public UILabelHandle AddLabel(string text, Color? color = null, float height = ElemH)
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

            Color bg = bgColor ?? new Color(0.18f, 0.28f, 0.48f, 1f);
            Color hoverC = new Color(
                Mathf.Min(bg.r + 0.12f, 1f),
                Mathf.Min(bg.g + 0.12f, 1f),
                Mathf.Min(bg.b + 0.12f, 1f), bg.a);
            Color pressC = new Color(
                bg.r * 0.70f,
                bg.g * 0.70f,
                bg.b * 0.70f, bg.a);
            WireHoverPress(btn, bg, hoverC, pressC);

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

        public UISliderHandle AddSlider(string label, float min, float max, float initial, Action<float> onChange = null, float step = 1f)
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
            RegisterDragOnTrack(track, trackW, lx => handle.SeekToPosition(lx)); // ← SeekToPosition, nie SeekChannel

            _currentY = row2Y + BtnH + ElemGap;
            return handle;
        }




        public UILabelHandle AddHeader(string text, Color? color = null)
        {
            var h = AddLabel(text, color ?? new Color(0.55f, 0.80f, 1.00f, 1f), 22f);
            AddSeparator();
            return h;
        }

        /// <summary>
        /// Single-line text input field.
        /// onSubmit fires when user presses Enter.
        /// </summary>
        public UITextInputHandle AddTextInput(string placeholder = "",
                                               Action<string> onSubmit = null,
                                               float height = ElemH)
        {
            var tf = Activator.CreateInstance(UIRuntime.TextFieldType);
            var s = UIRuntime.GetStyle(tf);
            S.Position(s, "Absolute");
            S.Left(s, 0f); S.Top(s, _currentY);
            S.Width(s, ContentW); S.Height(s, height);
            S.BgColor(s, new Color(0.04f, 0.04f, 0.08f, 1f));
            S.Color(s, new Color(0.85f, 1f, 0.85f, 1f));
            S.Font(s);
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), tf);

            var handle = new UITextInputHandle(UIRuntime.GetPtr(tf));

            if (!string.IsNullOrEmpty(placeholder))
                handle.SetPlaceholder(placeholder);

            if (onSubmit != null)
            {
                var trickleType = UIRuntime.UEAsm.GetType("UnityEngine.UIElements.TrickleDown");
                var regMethod = UIRuntime.VisualElementType.GetMethods()
                    .First(m => m.Name == "RegisterCallback"
                             && m.IsGenericMethod
                             && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(UnityEngine.UIElements.KeyDownEvent));

                Action<UnityEngine.UIElements.KeyDownEvent> handler = evt =>
                {
                    if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                        onSubmit(handle.GetValue());
                };

                var il2cb = Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.KeyDownEvent>>(handler);

                regMethod.Invoke(tf, new object[] { il2cb, Enum.Parse(trickleType, "TrickleDown") });
            }

            _currentY += height + ElemGap;
            return handle;
        }

        /// <summary>
        /// RGB color picker: preview swatch + three compact channel sliders.
        /// step — change per click in 0–255 scale (default 5).
        /// </summary>
        public UIColorPickerHandle AddColorPicker(string label, Color initial, Action<Color> onChange = null, int step = 5)
        {
            const float BtnW = 22f;
            const float BtnH = 18f;
            const float ValW = 30f;
            const float Gap = 3f;
            const float ChGap = 3f;
            float trackW = ContentW - 16f - BtnW * 2 - Gap * 3 - ValW;

            // ── Row 1: name label + color preview ─────────────────────────────
            var nameLbl = Activator.CreateInstance(UIRuntime.LabelType);
            var nls = UIRuntime.GetStyle(nameLbl);
            S.Position(nls, "Absolute");
            S.Left(nls, 0f); S.Top(nls, _currentY);
            S.Width(nls, ContentW - 32f); S.Height(nls, ElemH);
            S.Color(nls, Color.white); S.Font(nls);
            UIRuntime.LabelType.GetProperty("text").SetValue(nameLbl, label);
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), nameLbl);

            var preview = UIRuntime.NewVE();
            var pvs = UIRuntime.GetStyle(preview);
            S.Position(pvs, "Absolute");
            S.Left(pvs, ContentW - 28f); S.Top(pvs, _currentY + 3f);
            S.Width(pvs, 26f); S.Height(pvs, ElemH - 6f);
            S.BgColor(pvs, initial);
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), preview);

            float rowY = _currentY + ElemH + ElemGap;

            // Arrays filled in loop — handle holds references to the same arrays
            var fillPtrs = new IntPtr[3];
            var valuePtrs = new IntPtr[3];

            var channelColors = new Color[]
            {
        new Color(0.80f, 0.20f, 0.20f, 1f),
        new Color(0.20f, 0.70f, 0.20f, 1f),
        new Color(0.20f, 0.35f, 0.80f, 1f)
            };
            string[] chLabels = { "R", "G", "B" };
            float[] chValues = { initial.r, initial.g, initial.b };
            float delta = step / 255f;

            var handle = new UIColorPickerHandle(initial,
                UIRuntime.GetPtr(preview),
                fillPtrs, valuePtrs,
                trackW, onChange);

            for (int i = 0; i < 3; i++)
            {
                float y = rowY + i * (BtnH + ChGap);
                int channel = i;

                // Channel letter
                var chLbl = Activator.CreateInstance(UIRuntime.LabelType);
                var cls = UIRuntime.GetStyle(chLbl);
                S.Position(cls, "Absolute");
                S.Left(cls, 0f); S.Top(cls, y);
                S.Width(cls, 14f); S.Height(cls, BtnH);
                S.Color(cls, channelColors[i]); S.Font(cls);
                UIRuntime.LabelType.GetProperty("text").SetValue(chLbl, chLabels[i]);
                UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), chLbl);

                // [−]
                var btnMinus = MakeSmallBtn("−", 16f, y, BtnW, BtnH,
                                   new Color(0.40f, 0.12f, 0.12f, 1f));

                // Track
                var track = UIRuntime.NewVE();
                var ts = UIRuntime.GetStyle(track);
                S.Position(ts, "Absolute");
                S.Left(ts, 16f + BtnW + Gap); S.Top(ts, y);
                S.Width(ts, trackW); S.Height(ts, BtnH);
                S.BgColor(ts, new Color(0.18f, 0.18f, 0.22f, 1f));
                S.Overflow(ts, "Hidden");
                UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), track);

                // Fill
                var fill = UIRuntime.NewVE();
                var fs = UIRuntime.GetStyle(fill);
                S.Position(fs, "Absolute");
                S.Left(fs, 0f); S.Top(fs, 0f);
                S.Width(fs, trackW * chValues[i]); S.Height(fs, BtnH);
                S.BgColor(fs, channelColors[i]);
                UIRuntime.AddChild(track, fill);
                fillPtrs[i] = UIRuntime.GetPtr(fill);

                // [+]
                var btnPlus = MakeSmallBtn("+", 16f + BtnW + Gap + trackW + Gap, y, BtnW, BtnH, new Color(0.12f, 0.40f, 0.12f, 1f));

                // Value label
                var valLbl = Activator.CreateInstance(UIRuntime.LabelType);
                var vls = UIRuntime.GetStyle(valLbl);
                S.Position(vls, "Absolute");
                S.Left(vls, 16f + BtnW + Gap + trackW + Gap + BtnW + Gap); S.Top(vls, y);
                S.Width(vls, ValW); S.Height(vls, BtnH);
                S.Color(vls, Color.white); S.Font(vls);
                S.TextAlign(vls, TextAnchor.MiddleRight);
                UIRuntime.LabelType.GetProperty("text").SetValue(valLbl, Mathf.RoundToInt(chValues[i] * 255f).ToString());
                UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), valLbl);
                valuePtrs[i] = UIRuntime.GetPtr(valLbl);

                WireClick(btnMinus, () => handle.StepChannel(channel, -delta));
                WireClick(btnPlus, () => handle.StepChannel(channel, +delta));

                int capturedChannel = channel;
                float capturedW = trackW;
                RegisterDragOnTrack(track, capturedW, lx => handle.SeekChannel(capturedChannel, lx));
            }



            _currentY = rowY + 3 * (BtnH + ChGap) + ElemGap;
            return handle;
        }

        /// <summary>
        /// Image display element.
        /// width = 0 means full content width.
        /// </summary>
        public UIImageHandle AddImage(Texture2D texture = null,
                                       float width = 0f,
                                       float height = 60f)
        {
            float w = width > 0f ? width : ContentW;

            var ve = UIRuntime.NewVE();
            var s = UIRuntime.GetStyle(ve);
            S.Position(s, "Absolute");
            S.Left(s, 0f); S.Top(s, _currentY);
            S.Width(s, w); S.Height(s, height);
            S.BgColor(s, new Color(0f, 0f, 0f, 0f));
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), ve);

            if (texture != null)
                UIRuntime.SetBackgroundImage(ve, texture);

            var handle = new UIImageHandle(UIRuntime.GetPtr(ve));
            _currentY += height + ElemGap;
            return handle;
        }

        /// <summary>
        /// Horizontal progress bar. value: 0.0–1.0
        /// </summary>
        public UIProgressBarHandle AddProgressBar(string label,
                                                   float initial = 0f,
                                                   Color? fillColor = null,
                                                   float height = ElemH)
        {
            Color fc = fillColor ?? new Color(0.20f, 0.65f, 0.30f, 1f);
            float clamped = Mathf.Clamp01(initial);

            // Label row
            var nameLbl = Activator.CreateInstance(UIRuntime.LabelType);
            var nls = UIRuntime.GetStyle(nameLbl);
            S.Position(nls, "Absolute");
            S.Left(nls, 0f); S.Top(nls, _currentY);
            S.Width(nls, ContentW - 44f); S.Height(nls, ElemH);
            S.Color(nls, Color.white); S.Font(nls);
            UIRuntime.LabelType.GetProperty("text").SetValue(nameLbl, label);
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), nameLbl);

            // Percent label (right side)
            var pctLbl = Activator.CreateInstance(UIRuntime.LabelType);
            var pls = UIRuntime.GetStyle(pctLbl);
            S.Position(pls, "Absolute");
            S.Left(pls, ContentW - 40f); S.Top(pls, _currentY);
            S.Width(pls, 40f); S.Height(pls, ElemH);
            S.Color(pls, new Color(0.80f, 1.00f, 0.80f, 1f)); S.Font(pls);
            S.TextAlign(pls, TextAnchor.MiddleRight);
            UIRuntime.LabelType.GetProperty("text")
                .SetValue(pctLbl, Mathf.RoundToInt(clamped * 100f) + "%");
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), pctLbl);

            float trackY = _currentY + ElemH + ElemGap;

            // Track
            var track = UIRuntime.NewVE();
            var ts = UIRuntime.GetStyle(track);
            S.Position(ts, "Absolute");
            S.Left(ts, 0f); S.Top(ts, trackY);
            S.Width(ts, ContentW); S.Height(ts, height);
            S.BgColor(ts, new Color(0.15f, 0.15f, 0.18f, 1f));
            S.Overflow(ts, "Hidden");
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), track);

            // Fill
            var fill = UIRuntime.NewVE();
            var fs = UIRuntime.GetStyle(fill);
            S.Position(fs, "Absolute");
            S.Left(fs, 0f); S.Top(fs, 0f);
            S.Width(fs, ContentW * clamped); S.Height(fs, height);
            S.BgColor(fs, fc);
            UIRuntime.AddChild(track, fill);

            _currentY = trackY + height + ElemGap;

            return new UIProgressBarHandle(
                UIRuntime.GetPtr(fill),
                UIRuntime.GetPtr(pctLbl),
                ContentW, clamped, fc);
        }




        /// <summary>
        /// Dropdown list. Expands downward over other content when open.
        /// maxVisible — how many options shown before list clips (default 5).
        /// </summary>
        public UIDropdownHandle AddDropdown(string label,
                                     string[] options,
                                     int selectedIndex = 0,
                                     Action<int> onChange = null,
                                     int maxVisible = 5)
        {
            options ??= Array.Empty<string>();
            int sel = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, options.Length - 1));

            const float BtnH = 26f;
            const float OptionH = 24f;

            // Nazwa sekcji
            var nameLbl = Activator.CreateInstance(UIRuntime.LabelType);
            var nls = UIRuntime.GetStyle(nameLbl);
            S.Position(nls, "Absolute");
            S.Left(nls, 0f); S.Top(nls, _currentY);
            S.Width(nls, ContentW); S.Height(nls, ElemH);
            S.Color(nls, Color.white); S.Font(nls);
            UIRuntime.LabelType.GetProperty("text").SetValue(nameLbl, label);
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), nameLbl);

            float btnY = _currentY + ElemH + ElemGap;

            // Header button
            var headerBtn = Activator.CreateInstance(UIRuntime.ButtonType);
            var hbs = UIRuntime.GetStyle(headerBtn);
            S.Position(hbs, "Absolute");
            S.Left(hbs, 0f); S.Top(hbs, btnY);
            S.Width(hbs, ContentW); S.Height(hbs, BtnH);
            S.BgColor(hbs, new Color(0.12f, 0.18f, 0.28f, 1f));
            S.Color(hbs, Color.white); S.Font(hbs);
            S.TextAlign(hbs, TextAnchor.MiddleLeft);
            S.Padding(hbs, 4f);
            UIRuntime.ButtonType.GetProperty("text")
                .SetValue(headerBtn, $"{(options.Length > 0 ? options[sel] : "—")}  ▼");
            UIRuntime.AddChild(UIRuntime.WrapVE(_contentPtr), headerBtn);

            // Lista — dodana do PANELU (nie content) żeby nie była clipped przez viewport
            float listH = Mathf.Min(options.Length, maxVisible) * OptionH;

            // Pozycja listy w przestrzeni panelu
            float listTopInPanel = TitleH + Pad + btnY + BtnH;

            var listContainer = UIRuntime.NewVE();
            var lcs = UIRuntime.GetStyle(listContainer);
            S.Position(lcs, "Absolute");
            S.Left(lcs, Pad);
            S.Top(lcs, listTopInPanel);
            S.Width(lcs, ContentW);
            S.Height(lcs, listH);
            S.BgColor(lcs, new Color(0.10f, 0.14f, 0.22f, 0.98f));
            S.Overflow(lcs, "Hidden");
            S.Display(lcs, false);
            // ← dodaj do PANELU, nie do contentPtr
            UIRuntime.AddChild(UIRuntime.WrapVE(_panelPtr), listContainer);

            var handle = new UIDropdownHandle(
                UIRuntime.GetPtr(headerBtn),
                UIRuntime.GetPtr(listContainer),
                options, sel, onChange,
                listTopInPanel,
                () => _scrollY);

            WireClick(headerBtn, () => handle.Toggle());

            // Typy do hover
            var ue = UIRuntime.UEAsm;
            var trickle = ue.GetType("UnityEngine.UIElements.TrickleDown");
            var enterType = ue.GetType("UnityEngine.UIElements.PointerEnterEvent");
            var leaveType = ue.GetType("UnityEngine.UIElements.PointerLeaveEvent");
            var regBase = UIRuntime.VisualElementType.GetMethods()
                                .First(m => m.Name == "RegisterCallback"
                                         && m.IsGenericMethod
                                         && m.GetParameters().Length == 2);

            for (int i = 0; i < options.Length; i++)
            {
                int idx = i;
                var optBtn = Activator.CreateInstance(UIRuntime.ButtonType);
                var obs = UIRuntime.GetStyle(optBtn);
                S.Position(obs, "Absolute");
                S.Left(obs, 0f); S.Top(obs, i * OptionH);
                S.Width(obs, ContentW); S.Height(obs, OptionH);
                S.BgColor(obs, i == sel
                    ? new Color(0.20f, 0.35f, 0.55f, 1f)
                    : new Color(0.10f, 0.14f, 0.22f, 1f));
                S.Color(obs, Color.white); S.Font(obs);
                S.TextAlign(obs, TextAnchor.MiddleLeft);
                S.Padding(obs, 4f);
                UIRuntime.ButtonType.GetProperty("text").SetValue(optBtn, options[i]);
                WireClick(optBtn, () => handle.Select(idx));

                // Hover
                try
                {
                    var enterReg = regBase.MakeGenericMethod(enterType);
                    Action<UnityEngine.UIElements.PointerEnterEvent> enterH =
                        _ => handle.OnHoverEnter(idx);
                    enterReg.Invoke(optBtn, new object[] {
                Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<
                    UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerEnterEvent>>(enterH),
                Enum.Parse(trickle, "TrickleDown") });

                    var leaveReg = regBase.MakeGenericMethod(leaveType);
                    Action<UnityEngine.UIElements.PointerLeaveEvent> leaveH =
                        _ => handle.OnHoverLeave(idx);
                    leaveReg.Invoke(optBtn, new object[] {
                Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerLeaveEvent>>(leaveH),
                Enum.Parse(trickle, "TrickleDown") });
                }
                catch { }

                UIRuntime.AddChild(listContainer, optBtn);
                handle.AddOptionPtr(UIRuntime.GetPtr(optBtn));
            }

            _dropdownHandles.Add(handle);
            _currentY = btnY + BtnH + ElemGap;
            return handle;
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

        private void WireHoverPress(object ve, Color normal, Color hover, Color press)
        {
            try
            {
                var ue = UIRuntime.UEAsm;
                var trickle = ue.GetType("UnityEngine.UIElements.TrickleDown");
                var enterType = ue.GetType("UnityEngine.UIElements.PointerEnterEvent");
                var leaveType = ue.GetType("UnityEngine.UIElements.PointerLeaveEvent");
                var downType = ue.GetType("UnityEngine.UIElements.PointerDownEvent");
                var upType = ue.GetType("UnityEngine.UIElements.PointerUpEvent");

                var regBase = UIRuntime.VisualElementType.GetMethods()
                    .First(m => m.Name == "RegisterCallback"
                             && m.IsGenericMethod
                             && m.GetParameters().Length == 2);
                var td = Enum.Parse(trickle, "TrickleDown");

                var enterReg = regBase.MakeGenericMethod(enterType);
                Action<UnityEngine.UIElements.PointerEnterEvent> enterH =
                    _ => S.BgColor(UIRuntime.GetStyle(ve), hover);
                enterReg.Invoke(ve, new object[] {
                    Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerEnterEvent>>(enterH), td });

                var leaveReg = regBase.MakeGenericMethod(leaveType);
                Action<UnityEngine.UIElements.PointerLeaveEvent> leaveH =
                    _ => S.BgColor(UIRuntime.GetStyle(ve), normal);
                leaveReg.Invoke(ve, new object[] {
                    Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerLeaveEvent>>(leaveH), td });

                var downReg = regBase.MakeGenericMethod(downType);
                Action<UnityEngine.UIElements.PointerDownEvent> downH =
                    _ => S.BgColor(UIRuntime.GetStyle(ve), press);
                downReg.Invoke(ve, new object[] {
                    Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerDownEvent>>(downH), td });

                var upReg = regBase.MakeGenericMethod(upType);
                Action<UnityEngine.UIElements.PointerUpEvent> upH =
                    _ => S.BgColor(UIRuntime.GetStyle(ve), hover);
                upReg.Invoke(ve, new object[] {
                    Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerUpEvent>>(upH), td });
            }
            catch (Exception ex) { FrameworkPlugin.Log.Warning("[HoverPress] " + ex.Message); }
        }

        /// <summary>
        /// Podpina hover/press feedback na dowolny element przez jego ptr.
        /// Działa na Label, Button, lub surowym VE pobranym przez GetRawPtr().
        /// </summary>
        public void WireHover(IntPtr ptr, Color normal, Color hover, Color press)
        {
            var ve = UIRuntime.WrapVE(ptr);
            WireHoverPress(ve, normal, hover, press);
        }


        // ── NEW: overlay helpers ──────────────────────────────────────────────

        /// <summary>Adds a VE directly to panel root — not clipped by viewport.</summary>
        public void AddOverlayToPanel(object ve)
            => UIRuntime.AddChild(UIRuntime.WrapVE(_panelPtr), ve);

        /// <summary>Wires PointerUp click on any raw VE (labels, images, custom VEs).</summary>
        public void WireClick(IntPtr ptr, Action onClick)
        {
            try
            {
                var ve = UIRuntime.WrapVE(ptr);
                var ue = UIRuntime.UEAsm;
                var trickle = ue.GetType("UnityEngine.UIElements.TrickleDown");
                var pUpType = ue.GetType("UnityEngine.UIElements.PointerUpEvent");
                var regBase = UIRuntime.VisualElementType.GetMethods()
                                 .First(m => m.Name == "RegisterCallback"
                                          && m.IsGenericMethod
                                          && m.GetParameters().Length == 2);
                Action<UnityEngine.UIElements.PointerUpEvent> upH = _ => onClick?.Invoke();
                regBase.MakeGenericMethod(pUpType).Invoke(ve, new object[] {
                    Il2CppInterop.Runtime.DelegateSupport
                        .ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerUpEvent>>(upH),
                    Enum.Parse(trickle, "TrickleDown") });
            }
            catch (Exception ex) { FrameworkPlugin.Log.Warning("[WireClick] " + ex.Message); }
        }

        /// <summary>Creates a Button in an arbitrary container VE. Returns raw pointer.</summary>
        public IntPtr AddButtonToContainer(object container, string text,float x, float y, float w, float h, Color bg, Action onClick)
        {
            var btn = Activator.CreateInstance(UIRuntime.ButtonType);
            var s = UIRuntime.GetStyle(btn);
            S.Position(s, "Absolute");
            S.Left(s, x); S.Top(s, y);
            S.Width(s, w); S.Height(s, h);
            S.BgColor(s, bg); S.Color(s, Color.white);
            S.Font(s); S.TextAlign(s, TextAnchor.MiddleCenter); S.Padding(s, 0f);
            UIRuntime.ButtonType.GetProperty("text").SetValue(btn, text);
            if (onClick != null)
            {
                var cl = UIRuntime.ButtonType.GetProperty("clickable").GetValue(btn);
                var il2a = Il2CppInterop.Runtime.DelegateSupport
                                .ConvertDelegate<Il2CppSystem.Action>(onClick);
                UIRuntime.ClickableType.GetMethod("add_clicked").Invoke(cl, new object[] { il2a });
            }
            UIRuntime.AddChild(container, btn);
            return UIRuntime.GetPtr(btn);
        }

        public IntPtr AddButtonToContainer(IntPtr containerPtr, string text,float x, float y, float w, float h, Color bg, Action onClick) => AddButtonToContainer(UIRuntime.WrapVE(containerPtr), text, x, y, w, h, bg, onClick);
        

        /// <summary>Creates a Label in an arbitrary container VE. Returns UILabelHandle.</summary>
        public UILabelHandle AddLabelToContainer(object container, string text,
            float x, float y, float w, float h, Color color)
        {
            var lbl = Activator.CreateInstance(UIRuntime.LabelType);
            var s = UIRuntime.GetStyle(lbl);
            S.Position(s, "Absolute");
            S.Left(s, x); S.Top(s, y);
            S.Width(s, w); S.Height(s, h);
            S.Color(s, color); S.Font(s);
            UIRuntime.LabelType.GetProperty("text").SetValue(lbl, text);
            UIRuntime.AddChild(container, lbl);
            return new UILabelHandle(UIRuntime.GetPtr(lbl));
        }

        public UILabelHandle AddLabelToContainer(IntPtr containerPtr, string text,float x, float y, float w, float h, Color color) => AddLabelToContainer(UIRuntime.WrapVE(containerPtr), text, x, y, w, h, color);


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
            _updateCallback?.Invoke(Time.deltaTime);
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

            Vector2 mp = Input.mousePosition;
            float uitY = Screen.height - mp.y;
            bool inPanel = mp.x >= _x && mp.x <= _x + _width
                        && uitY >= _y && uitY <= _y + _height;
            if (!inPanel) return;

            // ── Jeśli kursor jest nad otwartą listą dropdown — nie scrolluj ───────
            foreach (var dd in _dropdownHandles)
            {
                if (dd.IsOpenAndContains(uitY, _y))
                    return;   // scroll pochłonięty przez dropdown, nic nie rób
            }

            // ── Zamknij dropdowny i scrolluj ──────────────────────────────────────
            foreach (var dd in _dropdownHandles) dd.ForceClose();

            float max = Mathf.Max(0f, _currentY - ViewportH);
            _scrollY = Mathf.Clamp(_scrollY - delta * ScrollStep, 0f, max);
            ApplyScroll();
            UpdateScrollbar();
        }



        private void RegisterDragOnTrack(object track, float trackW, Action<float> onLocalX)
        {
            try
            {
                var ue = UIRuntime.UEAsm;
                var trickle = ue.GetType("UnityEngine.UIElements.TrickleDown");
                var pDownType = ue.GetType("UnityEngine.UIElements.PointerDownEvent");
                var pMoveType = ue.GetType("UnityEngine.UIElements.PointerMoveEvent");
                var pUpType = ue.GetType("UnityEngine.UIElements.PointerUpEvent");
                var pLeaveType = ue.GetType("UnityEngine.UIElements.PointerLeaveEvent"); 

                var regBase = UIRuntime.VisualElementType.GetMethods()
                                    .First(m => m.Name == "RegisterCallback"
                                             && m.IsGenericMethod
                                             && m.GetParameters().Length == 2);
                var capMethod = UIRuntime.VisualElementType.GetMethod("CapturePointer");
                var relMethod = UIRuntime.VisualElementType.GetMethod("ReleasePointer");

                // Metoda StopPropagation na EventBase — blokuje scroll gdy przeciągamy
                var stopPropMethod = ue.GetType("UnityEngine.UIElements.EventBase")
                                       ?.GetMethod("StopPropagation");

                bool pressing = false;

                // ── PointerDown ────────────────────────────────────────────────────
                var downReg = regBase.MakeGenericMethod(pDownType);
                Action<UnityEngine.UIElements.PointerDownEvent> downH = evt =>
                {
                    pressing = true;
                    onLocalX(Mathf.Clamp(evt.localPosition.x, 0f, trackW));
                    capMethod?.Invoke(track, new object[] { evt.pointerId });
                    try { stopPropMethod?.Invoke(evt, null); } catch { }  // zatrzymaj scroll
                };
                downReg.Invoke(track, new object[] {
            Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerDownEvent>>(downH),
            Enum.Parse(trickle, "TrickleDown") });

                // ── PointerMove ────────────────────────────────────────────────────
                var moveReg = regBase.MakeGenericMethod(pMoveType);
                Action<UnityEngine.UIElements.PointerMoveEvent> moveH = evt =>
                {
                    // Guard: jeśli LMB już nie jest wciśnięty — przerwij
                    if (!Input.GetMouseButton(0))
                    {
                        if (pressing)
                        {
                            pressing = false;
                            try { relMethod?.Invoke(track, new object[] { evt.pointerId }); } catch { }
                        }
                        return;
                    }
                    if (!pressing) return;
                    onLocalX(Mathf.Clamp(evt.localPosition.x, 0f, trackW));
                };
                moveReg.Invoke(track, new object[] {
            Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerMoveEvent>>(moveH),
            Enum.Parse(trickle, "TrickleDown") });

                // ── PointerUp ──────────────────────────────────────────────────────
                var upReg = regBase.MakeGenericMethod(pUpType);
                Action<UnityEngine.UIElements.PointerUpEvent> upH = evt =>
                {
                    pressing = false;
                    try { relMethod?.Invoke(track, new object[] { evt.pointerId }); } catch { }
                };
                upReg.Invoke(track, new object[] {
            Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerUpEvent>>(upH),
            Enum.Parse(trickle, "TrickleDown") });

                // ── PointerLeave — guard gdy kursor wyjedzie za element ────────────
                var leaveReg = regBase.MakeGenericMethod(pLeaveType);
                Action<UnityEngine.UIElements.PointerLeaveEvent> leaveH = evt =>
                {
                    // Zwalniamy tylko jeśli LMB już nie wciśnięty
                    // (jeśli wciśnięty — capture trzyma eventy, kontynuujemy)
                    if (!Input.GetMouseButton(0))
                    {
                        pressing = false;
                        try { relMethod?.Invoke(track, new object[] { evt.pointerId }); } catch { }
                    }
                };
                leaveReg.Invoke(track, new object[] {
            Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerLeaveEvent>>(leaveH),Enum.Parse(trickle, "TrickleDown") });
            }
            catch (Exception ex) { FrameworkPlugin.Log.Warning("[DragTrack] " + ex.Message); }
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
            if (!_draggable) return; //api turnoff

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