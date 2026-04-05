using System;
using UnityEngine;
using System.Collections.Generic;

namespace CMS2026UITKFramework
{
    // ── Label ─────────────────────────────────────────────────────────────────
    public class UILabelHandle
    {
        private readonly IntPtr _ptr;
        internal UILabelHandle(IntPtr ptr) { _ptr = ptr; }

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

        public void SetVisible(bool visible)
        {
            if (_ptr == IntPtr.Zero) return;
            var lbl = Activator.CreateInstance(UIRuntime.LabelType, new object[] { _ptr });
            S.Display(UIRuntime.GetStyle(lbl), visible);
        }
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

        public void SetVisible(bool visible)
        {
            if (_ptr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType, new object[] { _ptr });
            S.Display(UIRuntime.GetStyle(btn), visible);
        }
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

        private string FormatValue(float v)
            => (_step < 1f) ? v.ToString("F1") : ((int)v).ToString();


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

        public void SetVisible(bool visible)
        {
            if (_ptr == IntPtr.Zero) return;
            var tf = Activator.CreateInstance(UIRuntime.TextFieldType, new object[] { _ptr });
            S.Display(UIRuntime.GetStyle(tf), visible);
        }
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

        // Pointers
        private readonly IntPtr _headerBtnPtr;
        private readonly IntPtr _listContainerPtr;
        private readonly List<IntPtr> _optionPtrs = new();

        private Action<int> _onChange;

        internal UIDropdownHandle(IntPtr headerBtnPtr, IntPtr listContainerPtr,
                                   string[] options, int selected,
                                   Action<int> onChange)
        {
            _headerBtnPtr = headerBtnPtr;
            _listContainerPtr = listContainerPtr;
            _options = options;
            _selected = selected;
            _onChange = onChange;
        }

        internal void AddOptionPtr(IntPtr ptr) => _optionPtrs.Add(ptr);

        public int SelectedIndex => _selected;
        public string SelectedValue => (_options != null && _selected >= 0 && _selected < _options.Length)
                                        ? _options[_selected] : "";

        public void SetSelected(int index)
        {
            if (_options == null || index < 0 || index >= _options.Length) return;
            _selected = index;
            RefreshHeader();
            _onChange?.Invoke(_selected);
        }

        public void SetOptions(string[] options, int selectedIndex = 0)
        {
            _options = options ?? Array.Empty<string>();
            _selected = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, _options.Length - 1));

            // Rebuild option buttons text
            for (int i = 0; i < _optionPtrs.Count && i < _options.Length; i++)
            {
                var btn = Activator.CreateInstance(UIRuntime.ButtonType,
                              new object[] { _optionPtrs[i] });
                UIRuntime.ButtonType.GetProperty("text").SetValue(btn, _options[i]);
            }
            RefreshHeader();
        }

        public void SetVisible(bool visible)
        {
            if (_headerBtnPtr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType,
                          new object[] { _headerBtnPtr });
            S.Display(UIRuntime.GetStyle(btn), visible);
            if (!visible) SetOpen(false);
        }

        // Called by header button click
        internal void Toggle() => SetOpen(!_open);

        internal void Select(int index)
        {
            SetSelected(index);
            SetOpen(false);
        }

        private void SetOpen(bool open)
        {
            _open = open;
            if (_listContainerPtr == IntPtr.Zero) return;
            var container = Activator.CreateInstance(UIRuntime.VisualElementType,
                                new object[] { _listContainerPtr });
            S.Display(UIRuntime.GetStyle(container), open);
            RefreshHeader();
        }

        private void RefreshHeader()
        {
            if (_headerBtnPtr == IntPtr.Zero) return;
            var btn = Activator.CreateInstance(UIRuntime.ButtonType,
                          new object[] { _headerBtnPtr });
            string arrow = _open ? "▲" : "▼";
            string label = SelectedValue;
            UIRuntime.ButtonType.GetProperty("text")
                .SetValue(btn, $"{label}  {arrow}");
        }
    }
}