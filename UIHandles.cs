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
}