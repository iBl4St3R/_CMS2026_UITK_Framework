using System;
using UnityEngine;

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
}