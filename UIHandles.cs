using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CMS2026UITKFramework
{
    // ── Label ─────────────────────────────────────────────────────────────────
    public class UILabelHandle
    {
        private readonly IntPtr _ptr;
        public UILabelHandle(IntPtr ptr) { _ptr = ptr; }

        public void SetText(string text)
        {
            if (_ptr == IntPtr.Zero) return;
            var lbl = Activator.CreateInstance(UIRuntime.LabelType, new object[] { _ptr });
            UIRuntime.LabelType.GetProperty("text").SetValue(lbl, text);
        }

        public void SetColor(Color color)
        {
            if (_ptr == IntPtr.Zero) return;
            var lbl = Activator.CreateInstance(UIRuntime.LabelType, new object[] { _ptr });
            S.Color(UIRuntime.GetStyle(lbl), color);
        }

        public void SetFontSize(int px)
        {
            if (_ptr == IntPtr.Zero) return;
            var lbl = Activator.CreateInstance(UIRuntime.LabelType, new object[] { _ptr });
            S.FontSize(UIRuntime.GetStyle(lbl), px);
        }

        public void SetSize(float w, float h)
        {
            if (_ptr == IntPtr.Zero) return;
            var lbl = Activator.CreateInstance(UIRuntime.LabelType, new object[] { _ptr });
            var s = UIRuntime.GetStyle(lbl);
            S.Width(s, w);
            S.Height(s, h);
        }

        public void SetVisible(bool visible)
        {
            if (_ptr == IntPtr.Zero) return;
            var lbl = Activator.CreateInstance(UIRuntime.LabelType, new object[] { _ptr });
            S.Display(UIRuntime.GetStyle(lbl), visible);
        }

        public void SetBorderColor(Color color)
        {
            if (_ptr == IntPtr.Zero) return;
            var lbl = Activator.CreateInstance(UIRuntime.LabelType, new object[] { _ptr });
            S.BorderColor(UIRuntime.GetStyle(lbl), color);
        }

        public void SetBorderRadius(float radius)
        {
            if (_ptr == IntPtr.Zero) return;
            var lbl = Activator.CreateInstance(UIRuntime.LabelType, new object[] { _ptr });
            S.BorderRadius(UIRuntime.GetStyle(lbl), radius);
        }

        public void SetBorderWidth(float width)
        {
            if (_ptr == IntPtr.Zero) return;
            var lbl = Activator.CreateInstance(UIRuntime.LabelType, new object[] { _ptr });
            S.BorderWidth(UIRuntime.GetStyle(lbl), width);
        }

        public void SetTint(UnityEngine.Color color)
        {
            if (_ptr == IntPtr.Zero) return;
            var ve = Activator.CreateInstance(UIRuntime.VisualElementType, new object[] { _ptr });
            S.BgTint(UIRuntime.GetStyle(ve), color);
        }

        public void SetScaleMode(UnityEngine.ScaleMode mode)
        {
            if (_ptr == IntPtr.Zero) return;
            var ve = Activator.CreateInstance(UIRuntime.VisualElementType, new object[] { _ptr });
            S.BgScaleMode(UIRuntime.GetStyle(ve), mode);
        }

        public IntPtr GetRawPtr() => _ptr;
    }

    // ── Button ────────────────────────────────────────────────────────────────
    public class UIButtonHandle
    {
        private readonly IntPtr _ptr;
        internal UIButtonHandle(IntPtr ptr) { _ptr = ptr; }

        public void SetText(string text)
        {
            if (_ptr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _ptr });
            UIRuntime.ButtonType.GetProperty("text").SetValue(btn, text);
        }

        public void SetBgColor(Color color)
        {
            if (_ptr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _ptr });
            S.BgColor(UIRuntime.GetStyle(btn), color);
        }

        

        public void SetTextColor(Color color)
        {
            if (_ptr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _ptr });
            S.Color(UIRuntime.GetStyle(btn), color);
        }

        public void SetFontSize(int px)
        {
            if (_ptr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _ptr });
            S.FontSize(UIRuntime.GetStyle(btn), px);
        }

        public void SetSize(float w, float h)
        {
            if (_ptr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _ptr });
            var s = UIRuntime.GetStyle(btn);
            S.Width(s, w);
            S.Height(s, h);
        }

        public void SetBorderColor(Color color)
        {
            if (_ptr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _ptr });
            S.BorderColor(UIRuntime.GetStyle(btn), color);
        }

        public void SetBorderRadius(float radius)
        {
            if (_ptr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _ptr });
            S.BorderRadius(UIRuntime.GetStyle(btn), radius);
        }

        public void SetBorderWidth(float width)
        {
            if (_ptr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _ptr });
            S.BorderWidth(UIRuntime.GetStyle(btn), width);
        }

        public void SetVisible(bool visible)
        {
            if (_ptr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _ptr });
            S.Display(UIRuntime.GetStyle(btn), visible);
        }

        public IntPtr GetRawPtr() => _ptr; 
    }

    // ── Toggle ────────────────────────────────────────────────────────────────
    public class UIToggleHandle
    {
        private bool _state;
        private IntPtr _btnPtr;
        private Action<bool> _onChange;

        internal UIToggleHandle(bool initial, Action<bool> onChange)
        {
            _state = initial;
            _onChange = onChange;
        }

        // Called by UIPanelBuilder after button is created
        internal void Init(IntPtr btnPtr) { _btnPtr = btnPtr; }

        public bool Value => _state;

        public void SetValue(bool value)
        {
            _state = value;
            Refresh();
        }

        // Called from button click closure
        internal void Toggle()
        {
            _state = !_state;
            Refresh();
            _onChange?.Invoke(_state);
        }

        private void Refresh()
        {
            if (_btnPtr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType,
                          new object[] { _btnPtr });
            UIRuntime.ButtonType.GetProperty("text")
                .SetValue(btn, _state ? "ON" : "OFF");
            S.BgColor(UIRuntime.GetStyle(btn), _state
                ? new Color(0.18f, 0.58f, 0.28f, 1f)
                : new Color(0.50f, 0.15f, 0.15f, 1f));
            S.Color(UIRuntime.GetStyle(btn), Color.white);
        }

        public void SetVisible(bool visible)
        {
            if (_btnPtr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType,
                          new object[] { _btnPtr });
            S.Display(UIRuntime.GetStyle(btn), visible);
        }

        public IntPtr GetRawPtr() => _btnPtr; 
    }

    // ── Slider ────────────────────────────────────────────────────────────────
    public class UISliderHandle
    {
        private float _value;
        private readonly float _min, _max, _step, _trackW;
        private readonly IntPtr _fillPtr;
        private readonly IntPtr _valueLblPtr;
        private Action<float> _onChange;

        internal UISliderHandle(IntPtr fillPtr, IntPtr valueLblPtr,
                                float min, float max, float initial,
                                float step, float trackW,
                                Action<float> onChange)
        {
            _fillPtr = fillPtr;
            _valueLblPtr = valueLblPtr;
            _min = min;
            _max = max;
            _value = Mathf.Clamp(initial, min, max);
            _step = step;
            _trackW = trackW;
            _onChange = onChange;
        }

        public float Value => _value;

        public void SetValue(float value)
        {
            _value = Mathf.Clamp(value, _min, _max);
            Refresh();
        }

        // Called from +/- button closures
        internal void Step(float delta)
        {
            float next = Mathf.Clamp(_value + delta, _min, _max);
            // Snap to step grid
            _value = Mathf.Round(next / _step) * _step;
            _value = Mathf.Clamp(_value, _min, _max);
            Refresh();
            _onChange?.Invoke(_value);
        }

        private void Refresh()
        {
            float t = (_max > _min) ? (_value - _min) / (_max - _min) : 0f;

            if (_fillPtr != IntPtr.Zero)
            {
                var fill = Activator.CreateInstance(UIRuntime.VisualElementType,
                               new object[] { _fillPtr });
                S.Width(UIRuntime.GetStyle(fill), _trackW * t);
            }

            if (_valueLblPtr != IntPtr.Zero)
            {
                var lbl = Activator.CreateInstance(UIRuntime.LabelType,
                              new object[] { _valueLblPtr });
                UIRuntime.LabelType.GetProperty("text")
                    .SetValue(lbl, FormatValue(_value));
            }
        }

        public void SeekToPosition(float localX)
        {
            float t = Mathf.Clamp01(localX / _trackW);
            float raw = _min + t * (_max - _min);
            _value = Mathf.Round(raw / _step) * _step;
            _value = Mathf.Clamp(_value, _min, _max);
            Refresh();
            _onChange?.Invoke(_value);
        }

        private string FormatValue(float v)
            => (_step < 1f) ? v.ToString("F1") : ((int)v).ToString();

        public IntPtr GetFillPtr() => _fillPtr;
        public IntPtr GetValueLblPtr() => _valueLblPtr;

    }


    // ── TextInput ─────────────────────────────────────────────────────────────
    public class UITextInputHandle
    {
        private readonly IntPtr _ptr;
        internal UITextInputHandle(IntPtr ptr) { _ptr = ptr; }

        public string GetValue()
        {
            if (_ptr == IntPtr.Zero) return "";
            var tf = Activator.CreateInstance(UIRuntime.TextFieldType, new object[] { _ptr });
            return (string)UIRuntime.TextFieldType.GetProperty("value").GetValue(tf) ?? "";
        }

        public void SetValue(string value)
        {
            if (_ptr == IntPtr.Zero) return;
            var tf = Activator.CreateInstance(UIRuntime.TextFieldType, new object[] { _ptr });
            UIRuntime.TextFieldType.GetProperty("value").SetValue(tf, value ?? "");
        }

        public void SetPlaceholder(string text)
        {
            if (_ptr == IntPtr.Zero) return;
            try
            {
                var tf = Activator.CreateInstance(UIRuntime.TextFieldType, new object[] { _ptr });
                var edit = UIRuntime.TextFieldType.GetProperty("textEdition")?.GetValue(tf);
                edit?.GetType().GetProperty("placeholder")?.SetValue(edit, text);
            }
            catch { }
        }

        /// <summary>
        /// Simulates placeholder: gray hint when empty/unfocused,
        /// clears on focus, restores on blur if empty.
        /// </summary>
        public void SetFakePlaceholder(string placeholder, Color phColor, Color activeColor)
        {
            if (_ptr == IntPtr.Zero) return;
            var tf = Activator.CreateInstance(UIRuntime.TextFieldType, new object[] { _ptr });
            UIRuntime.TextFieldType.GetProperty("value").SetValue(tf, placeholder);
            S.Color(UIRuntime.GetStyle(tf), phColor);
            try
            {
                var ue = UIRuntime.UEAsm;
                var trickle = ue.GetType("UnityEngine.UIElements.TrickleDown");
                var td = Enum.Parse(trickle, "TrickleDown");
                var focusT = ue.GetType("UnityEngine.UIElements.FocusEvent");
                var blurT = ue.GetType("UnityEngine.UIElements.BlurEvent");
                var regBase = UIRuntime.VisualElementType.GetMethods()
                                 .First(m => m.Name == "RegisterCallback"
                                          && m.IsGenericMethod
                                          && m.GetParameters().Length == 2);

                // Focus → clear placeholder
                var focusReg = regBase.MakeGenericMethod(focusT);
                Action<UnityEngine.UIElements.FocusEvent> focusH = _ =>
                {
                    var t2 = Activator.CreateInstance(UIRuntime.TextFieldType, new object[] { _ptr });
                    if (((string)UIRuntime.TextFieldType.GetProperty("value").GetValue(t2) ?? "") == placeholder)
                    {
                        UIRuntime.TextFieldType.GetProperty("value").SetValue(t2, "");
                        S.Color(UIRuntime.GetStyle(t2), activeColor);
                    }
                };
                focusReg.Invoke(tf, new object[] {
                    Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.FocusEvent>>(focusH), td });

                // Blur → restore if empty
                var blurReg = regBase.MakeGenericMethod(blurT);
                Action<UnityEngine.UIElements.BlurEvent> blurH = _ =>
                {
                    var t2 = Activator.CreateInstance(UIRuntime.TextFieldType, new object[] { _ptr });
                    if (string.IsNullOrEmpty((string)UIRuntime.TextFieldType.GetProperty("value").GetValue(t2) ?? ""))
                    {
                        UIRuntime.TextFieldType.GetProperty("value").SetValue(t2, placeholder);
                        S.Color(UIRuntime.GetStyle(t2), phColor);
                    }
                };
                blurReg.Invoke(tf, new object[] {
                    Il2CppInterop.Runtime.DelegateSupport.ConvertDelegate<UnityEngine.UIElements.EventCallback<UnityEngine.UIElements.BlurEvent>>(blurH), td });
            }
            catch (Exception ex) { FrameworkPlugin.Log.Warning("[FakePH] " + ex.Message); }
        }


        public void SetVisible(bool visible)
        {
            if (_ptr == IntPtr.Zero) return;
            var tf = Activator.CreateInstance(UIRuntime.TextFieldType, new object[] { _ptr });
            S.Display(UIRuntime.GetStyle(tf), visible);
        }

        public IntPtr GetRawPtr() => _ptr;
    }

    // ── ColorPicker ───────────────────────────────────────────────────────────
    public class UIColorPickerHandle
    {
        private Color _value;
        private readonly IntPtr _previewPtr;
        private readonly IntPtr[] _fillPtrs;
        private readonly IntPtr[] _valuePtrs;
        private readonly float _trackW;
        private Action<Color> _onChange;

        internal UIColorPickerHandle(Color initial,
            IntPtr previewPtr, IntPtr[] fillPtrs, IntPtr[] valuePtrs,
            float trackW, Action<Color> onChange)
        {
            _value = initial;
            _previewPtr = previewPtr;
            _fillPtrs = fillPtrs;
            _valuePtrs = valuePtrs;
            _trackW = trackW;
            _onChange = onChange;
        }

        public Color GetValue() => _value;

        public void SetValue(Color color) { _value = color; Refresh(); }

        internal void StepChannel(int channel, float delta)
        {
            float[] ch = { _value.r, _value.g, _value.b };
            ch[channel] = Mathf.Clamp01(ch[channel] + delta);
            _value = new Color(ch[0], ch[1], ch[2], _value.a);
            Refresh();
            _onChange?.Invoke(_value);
        }

        private void Refresh()
        {
            float[] ch = { _value.r, _value.g, _value.b };

            if (_previewPtr != IntPtr.Zero)
            {
                var prev = Activator.CreateInstance(UIRuntime.VisualElementType, new object[] { _previewPtr });
                S.BgColor(UIRuntime.GetStyle(prev), _value);
            }
            for (int i = 0; i < 3; i++)
            {
                if (_fillPtrs[i] != IntPtr.Zero)
                {
                    var fill = Activator.CreateInstance(UIRuntime.VisualElementType, new object[] { _fillPtrs[i] });
                    S.Width(UIRuntime.GetStyle(fill), _trackW * ch[i]);
                }
                if (_valuePtrs[i] != IntPtr.Zero)
                {
                    var lbl = Activator.CreateInstance(UIRuntime.LabelType, new object[] { _valuePtrs[i] });
                    UIRuntime.LabelType.GetProperty("text")
                        .SetValue(lbl, Mathf.RoundToInt(ch[i] * 255f).ToString());
                }
            }
        }

        public void SetVisible(bool visible)
        {
            if (_previewPtr == IntPtr.Zero) return;
            var ve = Activator.CreateInstance(UIRuntime.VisualElementType, new object[] { _previewPtr });
            S.Display(UIRuntime.GetStyle(ve), visible);
        }

        public void SeekChannel(int channel, float localX)
        {
            float[] ch = { _value.r, _value.g, _value.b };
            ch[channel] = Mathf.Clamp01(localX / _trackW);
            _value = new Color(ch[0], ch[1], ch[2], _value.a);
            Refresh();
            _onChange?.Invoke(_value);
        }


        public IntPtr GetPreviewPtr() => _previewPtr;     
        public IntPtr GetFillPtr(int channel) => _fillPtrs[channel]; 
        public IntPtr GetValueLblPtr(int channel) => _valuePtrs[channel];
    }


    // ── Image ─────────────────────────────────────────────────────────────────
    public class UIImageHandle
    {
        private readonly IntPtr _ptr;
        internal UIImageHandle(IntPtr ptr) { _ptr = ptr; }

        public void SetTexture(Texture2D tex)
        {
            if (_ptr == IntPtr.Zero) return;
            var ve = Activator.CreateInstance(UIRuntime.VisualElementType, new object[] { _ptr });
            UIRuntime.SetBackgroundImage(ve, tex);
        }

        public void SetSize(float width, float height)
        {
            if (_ptr == IntPtr.Zero) return;
            var ve = Activator.CreateInstance(UIRuntime.VisualElementType, new object[] { _ptr });
            var s = UIRuntime.GetStyle(ve);
            S.Width(s, width);
            S.Height(s, height);
        }

        public void SetVisible(bool visible)
        {
            if (_ptr == IntPtr.Zero) return;
            var ve = Activator.CreateInstance(UIRuntime.VisualElementType, new object[] { _ptr });
            S.Display(UIRuntime.GetStyle(ve), visible);
        }


        public void SetTint(UnityEngine.Color color)
        {
            if (_ptr == IntPtr.Zero) return;
            var ve = Activator.CreateInstance(UIRuntime.VisualElementType, new object[] { _ptr });
            S.BgTint(UIRuntime.GetStyle(ve), color);
        }

        public void SetScaleMode(UnityEngine.ScaleMode mode)
        {
            if (_ptr == IntPtr.Zero) return;
            var ve = Activator.CreateInstance(UIRuntime.VisualElementType, new object[] { _ptr });
            S.BgScaleMode(UIRuntime.GetStyle(ve), mode);
        }

        public IntPtr GetRawPtr() => _ptr;  
    }

    // ── ProgressBar ───────────────────────────────────────────────────────────
    public class UIProgressBarHandle
    {
        private float _value;           // 0–1
        private readonly IntPtr _fillPtr;
        private readonly IntPtr _labelPtr;
        private readonly float _trackW;
        private Color _fillColor;

        internal UIProgressBarHandle(IntPtr fillPtr, IntPtr labelPtr,
                                      float trackW, float initial, Color fillColor)
        {
            _fillPtr = fillPtr;
            _labelPtr = labelPtr;
            _trackW = trackW;
            _fillColor = fillColor;
            _value = Mathf.Clamp01(initial);
        }

        /// <summary>value in 0–1 range</summary>
        public void SetValue(float value)
        {
            _value = Mathf.Clamp01(value);
            Refresh();
        }

        public float Value => _value;

        public void SetColor(Color color)
        {
            _fillColor = color;
            Refresh();
        }

        public void SetVisible(bool visible)
        {
            if (_fillPtr == IntPtr.Zero) return;
            // We hide fill; label hides itself when no ptr
            var ve = Activator.CreateInstance(UIRuntime.VisualElementType,
                         new object[] { _fillPtr });
            S.Display(UIRuntime.GetStyle(ve), visible);
        }

        public IntPtr GetFillPtr() => _fillPtr;
        public IntPtr GetLabelPtr() => _labelPtr;

        private void Refresh()
        {
            if (_fillPtr != IntPtr.Zero)
            {
                var fill = Activator.CreateInstance(UIRuntime.VisualElementType,
                               new object[] { _fillPtr });
                S.Width(UIRuntime.GetStyle(fill), _trackW * _value);
                S.BgColor(UIRuntime.GetStyle(fill), _fillColor);
            }
            if (_labelPtr != IntPtr.Zero)
            {
                var lbl = Activator.CreateInstance(UIRuntime.LabelType,
                              new object[] { _labelPtr });
                UIRuntime.LabelType.GetProperty("text")
                    .SetValue(lbl, Mathf.RoundToInt(_value * 100f) + "%");
            }
        }
    }

    // ── Dropdown ──────────────────────────────────────────────────────────────
    public class UIDropdownHandle
    {
        private string[] _options;
        private int _selected;
        private bool _open = false;
        private int _hoveredIndex = -1;

        private readonly IntPtr _headerBtnPtr;
        private readonly IntPtr _listContainerPtr;
        private readonly List<IntPtr> _optionPtrs = new();

        private Action<int> _onChange;
        private float _listTopInPanel;
        private Func<float> _getScrollY;

        internal UIDropdownHandle(IntPtr headerBtnPtr, IntPtr listContainerPtr,
                                   string[] options, int selected,
                                   Action<int> onChange,
                                   float listTopInPanel,
                                   Func<float> getScrollY)
        {
            _headerBtnPtr = headerBtnPtr;
            _listContainerPtr = listContainerPtr;
            _options = options ?? Array.Empty<string>();
            _selected = selected;
            _onChange = onChange;
            _listTopInPanel = listTopInPanel;
            _getScrollY = getScrollY;
        }

        public int SelectedIndex => _selected;
        public string SelectedValue => (_options != null && _selected >= 0 && _selected < _options.Length)
                                        ? _options[_selected] : "";

        internal void AddOptionPtr(IntPtr ptr) => _optionPtrs.Add(ptr);

        internal void Toggle() => SetOpen(!_open);
        internal void ForceClose() { if (_open) SetOpen(false); }

        internal void OnHoverEnter(int index) { _hoveredIndex = index; RefreshOptionHighlights(); }
        internal void OnHoverLeave(int index) { if (_hoveredIndex == index) _hoveredIndex = -1; RefreshOptionHighlights(); }

        internal void Select(int index) { SetSelected(index); SetOpen(false); }

        public void SetSelected(int index)
        {
            if (_options == null || index < 0 || index >= _options.Length) return;
            _selected = index;
            RefreshHeader();
            RefreshOptionHighlights();
            _onChange?.Invoke(_selected);
        }

        public void SetOptions(string[] options, int selectedIndex = 0)
        {
            _options = options ?? Array.Empty<string>();
            _selected = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, _options.Length - 1));
            for (int i = 0; i < _optionPtrs.Count && i < _options.Length; i++)
            {
                var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _optionPtrs[i] });
                UIRuntime.ButtonType.GetProperty("text").SetValue(btn, _options[i]);
            }
            RefreshHeader();
            RefreshOptionHighlights();
        }

        public void SetVisible(bool visible)
        {
            if (_headerBtnPtr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _headerBtnPtr });
            S.Display(UIRuntime.GetStyle(btn), visible);
            if (!visible) SetOpen(false);
        }

        private void SetOpen(bool open)
        {
            _open = open;
            if (_listContainerPtr == IntPtr.Zero) return;
            var container = Activator.CreateInstance(UIRuntime.VisualElementType,
                                new object[] { _listContainerPtr });
            if (open)
            {
                float top = _listTopInPanel - (_getScrollY?.Invoke() ?? 0f);
                S.Top(UIRuntime.GetStyle(container), top);
                RefreshOptionHighlights();
            }
            S.Display(UIRuntime.GetStyle(container), open);
            RefreshHeader();
        }

        private void RefreshHeader()
        {
            if (_headerBtnPtr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _headerBtnPtr });
            UIRuntime.ButtonType.GetProperty("text")
                .SetValue(btn, $"{SelectedValue}  {(_open ? "▲" : "▼")}");
        }

        private void RefreshOptionHighlights()
        {
            for (int i = 0; i < _optionPtrs.Count; i++)
            {
                if (_optionPtrs[i] == IntPtr.Zero) continue;
                var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _optionPtrs[i] });
                Color bg = i == _hoveredIndex ? new Color(0.28f, 0.48f, 0.72f, 1f) :
                           i == _selected ? new Color(0.20f, 0.35f, 0.55f, 1f) :
                                                new Color(0.10f, 0.14f, 0.22f, 1f);
                S.BgColor(UIRuntime.GetStyle(btn), bg);
            }
        }

        /// <summary>
        /// Zwraca true jeśli dropdown jest otwarty i kursor uitY jest nad jego listą.
        /// panelY — pozycja Y górnej krawędzi panelu w przestrzeni UI.
        /// </summary>
        internal bool IsOpenAndContains(float uitY, float panelY)
        {
            if (!_open) return false;
            float listTop = panelY + _listTopInPanel - (_getScrollY?.Invoke() ?? 0f);
            float listBottom = listTop + (_optionPtrs.Count * 24f);
            return uitY >= listTop && uitY <= listBottom;
        }

    }
}