using System;
using System.Linq;
using UnityEngine;

namespace CMS2026UITKFramework
{
    /// <summary>
    /// Horizontal layout container. Get one via panel.AddRow().
    /// Elements are placed left-to-right with absolute positioning.
    /// </summary>
    public class UIRowBuilder
    {
        private readonly IntPtr _containerPtr;
        private readonly float _totalWidth;
        private readonly float _height;
        private float _currentX = 0f;

        private readonly UIPanel _panel;
        private readonly float _rowTopInPanel;
        private const float Pad = 6f;   // musi zgadzać się z UIPanel.Pad

        internal UIRowBuilder(IntPtr containerPtr, float totalWidth, float height,UIPanel panel = null, float rowTopInPanel = 0f)
        {
            _containerPtr = containerPtr;
            _totalWidth = totalWidth;
            _height = height;
            _panel = panel;
            _rowTopInPanel = rowTopInPanel;
        }

        // ── AddDropdown ────────────────────────────────────────────────────
        /// <summary>
        /// Inline dropdown w wierszu.
        /// Lista otwiera się nad/pod wierszem i nie jest przycinana przez viewport.
        /// Wymaga, aby UIRowBuilder był tworzony przez panel.AddRow() (nie ręcznie).
        /// </summary>
        public UIDropdownHandle AddDropdown(string label,string[] options,int selectedIndex = 0,Action<int> onChanged = null,float width = 100f,int maxVisible = 5)
        {
            options ??= Array.Empty<string>();
            int sel = Mathf.Clamp(selectedIndex, 0,
                                  Mathf.Max(0, options.Length - 1));

            //const float BtnH = 24f;
            const float OptionH = 24f;

            // ── Header button w wierszu ────────────────────────────────────
            var headerBtn = Activator.CreateInstance(UIRuntime.ButtonType);
            var hbs = UIRuntime.GetStyle(headerBtn);
            S.Position(hbs, "Absolute");
            S.Left(hbs, _currentX); S.Top(hbs, 0f);
            S.Width(hbs, width); S.Height(hbs, _height);
            S.BgColor(hbs, new Color(0.12f, 0.18f, 0.28f, 1f));
            S.Color(hbs, Color.white);
            S.Font(hbs);
            S.TextAlign(hbs, TextAnchor.MiddleLeft);
            S.Padding(hbs, 4f);
            UIRuntime.ButtonType.GetProperty("text")
                .SetValue(headerBtn, $"{(options.Length > 0 ? options[sel] : "—")}  ▼");
            UIRuntime.AddChild(UIRuntime.WrapVE(_containerPtr), headerBtn);

            // ── Lista — doklejona do roota panelu, poza viewportem ─────────
            float listH = Mathf.Min(options.Length, maxVisible) * OptionH;
            float listTopInPanel = _rowTopInPanel + _height;   // tuż pod wierszem
            float listLeftInPanel = Pad + _currentX;            // wyrównanie do przycisku

            var listContainer = UIRuntime.NewVE();
            var lcs = UIRuntime.GetStyle(listContainer);
            S.Position(lcs, "Absolute");
            S.Left(lcs, listLeftInPanel);
            S.Top(lcs, listTopInPanel);
            S.Width(lcs, width);
            S.Height(lcs, listH);
            S.BgColor(lcs, new Color(0.10f, 0.14f, 0.22f, 0.98f));
            S.Overflow(lcs, "Hidden");
            S.Display(lcs, false);

            // Fallback gdy builder tworzony poza panelem (nie powinno się zdarzyć)
            if (_panel == null)
                UIRuntime.AddChild(UIRuntime.WrapVE(_containerPtr), listContainer);
            else
                _panel.AddToRoot(listContainer);   // ← root zamiast overlay

            var handle = new UIDropdownHandle(
            UIRuntime.GetPtr(headerBtn),
            UIRuntime.GetPtr(listContainer),
            options, sel, onChanged,
            listTopInPanel,
            listLeftInPanel,
            _panel != null ? () => _panel.GetScrollY() : (Func<float>)(() => 0f),
            _panel != null ? () => _panel.GetPanelX() : (Func<float>)(() => 0f),
            _panel != null ? () => _panel.GetPanelY() : (Func<float>)(() => 0f));
            // default(IntPtr) — brak section label w row dropdownie, pomijamy

            WireClick(headerBtn, () => handle.Toggle());

            // ── Opcje z hover ──────────────────────────────────────────────
            var ue = UIRuntime.UEAsm;
            var trickle = ue.GetType("UnityEngine.UIElements.TrickleDown");
            var enterT = ue.GetType("UnityEngine.UIElements.PointerEnterEvent");
            var leaveT = ue.GetType("UnityEngine.UIElements.PointerLeaveEvent");
            var regBase = UIRuntime.VisualElementType.GetMethods()
                              .First(m => m.Name == "RegisterCallback"
                                       && m.IsGenericMethod
                                       && m.GetParameters().Length == 2);
            var td = Enum.Parse(trickle, "TrickleDown");

            for (int i = 0; i < options.Length; i++)
            {
                int idx = i;
                var optBtn = Activator.CreateInstance(UIRuntime.ButtonType);
                var obs = UIRuntime.GetStyle(optBtn);
                S.Position(obs, "Absolute");
                S.Left(obs, 0f); S.Top(obs, i * OptionH);
                S.Width(obs, width); S.Height(obs, OptionH);
                S.BgColor(obs, i == sel
                    ? new Color(0.20f, 0.35f, 0.55f, 1f)
                    : new Color(0.10f, 0.14f, 0.22f, 1f));
                S.Color(obs, Color.white);
                S.Font(obs);
                S.TextAlign(obs, TextAnchor.MiddleLeft);
                S.Padding(obs, 4f);
                UIRuntime.ButtonType.GetProperty("text").SetValue(optBtn, options[i]);
                WireClick(optBtn, () => handle.Select(idx));

                try
                {
                    var er = regBase.MakeGenericMethod(enterT);
                    Action<UnityEngine.UIElements.PointerEnterEvent> eh =
                        _ => handle.OnHoverEnter(idx);
                    er.Invoke(optBtn, new object[] {
                    Il2CppInterop.Runtime.DelegateSupport
                        .ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerEnterEvent>>(eh), td });

                    var lr = regBase.MakeGenericMethod(leaveT);
                    Action<UnityEngine.UIElements.PointerLeaveEvent> lh =
                        _ => handle.OnHoverLeave(idx);
                    lr.Invoke(optBtn, new object[] {
                    Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerLeaveEvent>>(lh), td });
                }
                catch { }

                UIRuntime.AddChild(listContainer, optBtn);
                handle.AddOptionPtr(UIRuntime.GetPtr(optBtn));
            }

            _panel?.RegisterDropdownHandle(handle);
            _currentX += width;
            return handle;
        }







        // ── Remaining space ───────────────────────────────────────────────
        public float RemainingWidth => _totalWidth - _currentX;
        public float Height => _height;

        // ── AddLabel ──────────────────────────────────────────────────────
        public UILabelHandle AddLabel(string text, float width,Color? color = null)
        {
            var lbl = Activator.CreateInstance(UIRuntime.LabelType);
            var s = UIRuntime.GetStyle(lbl);
            S.Position(s, "Absolute");
            S.Left(s, _currentX); S.Top(s, 0f);
            S.Width(s, width); S.Height(s, _height);
            S.Color(s, color ?? Color.white);
            S.Font(s);
            UIRuntime.LabelType.GetProperty("text").SetValue(lbl, text);
            UIRuntime.AddChild(UIRuntime.WrapVE(_containerPtr), lbl);

            _currentX += width;
            return new UILabelHandle(UIRuntime.GetPtr(lbl));
        }

        // ── AddButton ─────────────────────────────────────────────────────
        public UIButtonHandle AddButton(string label, float width,
                                         Action onClick,
                                         Color? bgColor = null)
        {
            Color bg = bgColor ?? new Color(0.18f, 0.28f, 0.48f, 1f);
            var btn = Activator.CreateInstance(UIRuntime.ButtonType);
            var s = UIRuntime.GetStyle(btn);
            S.Position(s, "Absolute");
            S.Left(s, _currentX); S.Top(s, 0f);
            S.Width(s, width); S.Height(s, _height);
            S.BgColor(s, bg);
            S.Color(s, Color.white);
            S.Font(s);
            S.TextAlign(s, TextAnchor.MiddleCenter);
            S.Padding(s, 0f);
            UIRuntime.ButtonType.GetProperty("text").SetValue(btn, label);

            if (onClick != null)
                WireClick(btn, onClick);

            Color hover = new Color(Mathf.Min(bg.r + 0.12f, 1f), Mathf.Min(bg.g + 0.12f, 1f), Mathf.Min(bg.b + 0.12f, 1f), bg.a);
            Color press = new Color(bg.r * 0.70f, bg.g * 0.70f, bg.b * 0.70f, bg.a);
            WireHoverPress(btn, bg, hover, press);

            UIRuntime.AddChild(UIRuntime.WrapVE(_containerPtr), btn);
            _currentX += width;
            return new UIButtonHandle(UIRuntime.GetPtr(btn));
        }

        // ── AddToggle ─────────────────────────────────────────────────────
        public UIToggleHandle AddToggle(float width,
                                         bool initial = false,
                                         Action<bool> onChange = null)
        {
            var handle = new UIToggleHandle(initial, onChange);
            Color bg = initial
                ? new Color(0.18f, 0.58f, 0.28f, 1f)
                : new Color(0.50f, 0.15f, 0.15f, 1f);

            var btn = Activator.CreateInstance(UIRuntime.ButtonType);
            var s = UIRuntime.GetStyle(btn);
            S.Position(s, "Absolute");
            S.Left(s, _currentX); S.Top(s, 0f);
            S.Width(s, width); S.Height(s, _height);
            S.BgColor(s, bg);
            S.Color(s, Color.white);
            S.Font(s);
            S.TextAlign(s, TextAnchor.MiddleCenter);
            S.Padding(s, 0f);
            UIRuntime.ButtonType.GetProperty("text").SetValue(btn, initial ? "ON" : "OFF");

            WireClick(btn, () => handle.Toggle());
            UIRuntime.AddChild(UIRuntime.WrapVE(_containerPtr), btn);
            handle.Init(UIRuntime.GetPtr(btn));

            _currentX += width;
            return handle;
        }

        // ── AddProgressBar ────────────────────────────────────────────────
        public UIProgressBarHandle AddProgressBar(float width,
                                                   float initial = 0f,
                                                   Color? fillColor = null)
        {
            Color fc = fillColor ?? new Color(0.20f, 0.65f, 0.30f, 1f);
            float clamped = Mathf.Clamp01(initial);

            // track
            var track = UIRuntime.NewVE();
            var ts = UIRuntime.GetStyle(track);
            S.Position(ts, "Absolute");
            S.Left(ts, _currentX); S.Top(ts, (_height - 10f) * 0.5f);
            S.Width(ts, width); S.Height(ts, 10f);
            S.BgColor(ts, new Color(0.15f, 0.15f, 0.18f, 1f));
            S.Overflow(ts, "Hidden");
            UIRuntime.AddChild(UIRuntime.WrapVE(_containerPtr), track);

            // fill
            var fill = UIRuntime.NewVE();
            var fs = UIRuntime.GetStyle(fill);
            S.Position(fs, "Absolute");
            S.Left(fs, 0f); S.Top(fs, 0f);
            S.Width(fs, width * clamped); S.Height(fs, 10f);
            S.BgColor(fs, fc);
            UIRuntime.AddChild(track, fill);

            _currentX += width;
            return new UIProgressBarHandle(UIRuntime.GetPtr(fill), UIRuntime.GetPtr(track), IntPtr.Zero, IntPtr.Zero, width, clamped, fc);
        }

        // ── AddSeparator (vertical) ───────────────────────────────────────
        public void AddSeparator(float separatorWidth = 1f, Color? color = null)
        {
            var sep = UIRuntime.NewVE();
            var s = UIRuntime.GetStyle(sep);
            S.Position(s, "Absolute");
            S.Left(s, _currentX); S.Top(s, 2f);
            S.Width(s, separatorWidth); S.Height(s, _height - 4f);
            S.BgColor(s, color ?? new Color(0.30f, 0.30f, 0.40f, 0.8f));
            UIRuntime.AddChild(UIRuntime.WrapVE(_containerPtr), sep);
            _currentX += separatorWidth;
        }

        // ── AddSpace ──────────────────────────────────────────────────────
        public void AddSpace(float pixels = 8f) => _currentX += pixels;

        // ── Raw VE access ─────────────────────────────────────────────────
        /// <summary>
        /// Add a custom-built VisualElement directly into the row.
        /// You are responsible for setting its Left/Top/Width/Height.
        /// </summary>
        public void AddRaw(object ve, float consumedWidth)
        {
            var s = UIRuntime.GetStyle(ve);
            S.Position(s, "Absolute");
            S.Left(s, _currentX);
            S.Top(s, 0f);
            UIRuntime.AddChild(UIRuntime.WrapVE(_containerPtr), ve);
            _currentX += consumedWidth;
        }

        // ── Internal helpers ──────────────────────────────────────────────
        private static void WireClick(object btn, Action onClick)
        {
            var clickable = UIRuntime.ButtonType.GetProperty("clickable").GetValue(btn);
            var il2Action = Il2CppInterop.Runtime.DelegateSupport
                                .ConvertDelegate<Il2CppSystem.Action>(onClick);
            UIRuntime.ClickableType.GetMethod("add_clicked")
                .Invoke(clickable, new object[] { il2Action });
        }

        private static void WireHoverPress(object ve, Color normal, Color hover, Color press)
        {
            try
            {
                var ue = UIRuntime.UEAsm;
                var trickle = ue.GetType("UnityEngine.UIElements.TrickleDown");
                var td = Enum.Parse(trickle, "TrickleDown");
                var regBase = UIRuntime.VisualElementType.GetMethods()
                    .First(m => m.Name == "RegisterCallback"
                             && m.IsGenericMethod
                             && m.GetParameters().Length == 2);

                var enterReg = regBase.MakeGenericMethod(typeof(UnityEngine.UIElements.PointerEnterEvent));
                Action<UnityEngine.UIElements.PointerEnterEvent> enterH =
                    _ => S.BgColor(UIRuntime.GetStyle(ve), hover);
                enterReg.Invoke(ve, new object[] {
            Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerEnterEvent>>(enterH), td });

                var leaveReg = regBase.MakeGenericMethod(typeof(UnityEngine.UIElements.PointerLeaveEvent));
                Action<UnityEngine.UIElements.PointerLeaveEvent> leaveH =
                    _ => S.BgColor(UIRuntime.GetStyle(ve), normal);
                leaveReg.Invoke(ve, new object[] {
            Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerLeaveEvent>>(leaveH), td });

                var downReg = regBase.MakeGenericMethod(typeof(UnityEngine.UIElements.PointerDownEvent));
                Action<UnityEngine.UIElements.PointerDownEvent> downH =
                    _ => S.BgColor(UIRuntime.GetStyle(ve), press);
                downReg.Invoke(ve, new object[] {
            Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerDownEvent>>(downH), td });

                var upReg = regBase.MakeGenericMethod(typeof(UnityEngine.UIElements.PointerUpEvent));
                Action<UnityEngine.UIElements.PointerUpEvent> upH =
                    _ => S.BgColor(UIRuntime.GetStyle(ve), hover);
                upReg.Invoke(ve, new object[] {
            Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.PointerUpEvent>>(upH), td });
            }
            catch { }
        }
    }
}