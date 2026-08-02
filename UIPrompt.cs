using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CMS2026UITKFramework
{
    public enum PromptPriority { Low = 0, Normal = 1, Warning = 2, Critical = 3, Top = 4 }

    public enum PromptMode { Info, Warning, Error, Texture, KeyIcon }

    public class PromptRequest
    {
        public string Id = Guid.NewGuid().ToString("N").Substring(0, 8);
        public string Text = "";
        public PromptMode Mode = PromptMode.Info;
        public PromptPriority Priority = PromptPriority.Normal;
        public float Duration = 3f;
        public Texture2D Texture;
        public KeyCode Key = KeyCode.None;
        public bool Completed;
        internal bool Interrupted;
    }

    /// <summary>
    /// Publiczne API frameworka.
    /// UIPrompt.Show("Zapisano!", PromptMode.Info);
    /// UIPrompt.Warning("Coś wymaga sprawdzenia.");
    /// UIPrompt.Error("Błąd krytyczny!", PromptPriority.Top);
    /// UIPrompt.KeyIcon(KeyCode.F5, "Szybki zapis");
    /// UIPrompt.ShowTextureFromResources("Nowy wynik!", "achievement.png");
    /// </summary>
    public static class UIPrompt
    {
        public static string Show(string text, PromptMode mode = PromptMode.Info, PromptPriority priority = PromptPriority.Normal, float duration = 3f)
        {
            var r = new PromptRequest { Text = text, Mode = mode, Priority = priority, Duration = duration };
            UIPromptManager.Enqueue(r);
            return r.Id;
        }

        public static string Warning(string text, PromptPriority priority = PromptPriority.Warning, float duration = 3.5f)
            => Show(text, PromptMode.Warning, priority, duration);

        public static string Error(string text, PromptPriority priority = PromptPriority.Critical, float duration = 4f)
            => Show(text, PromptMode.Error, priority, duration);

        public static string ShowTexture(string text, Texture2D texture, PromptPriority priority = PromptPriority.Normal, float duration = 4f)
        {
            var r = new PromptRequest { Text = text, Mode = PromptMode.Texture, Priority = priority, Duration = duration, Texture = texture };
            UIPromptManager.Enqueue(r);
            return r.Id;
        }

        public static string ShowTextureFromResources(string text, string fileName, PromptPriority priority = PromptPriority.Normal, float duration = 4f)
            => ShowTexture(text, UIPromptManager.LoadResourceTexture(fileName), priority, duration);

        public static string KeyIcon(KeyCode key, string text = null, PromptPriority priority = PromptPriority.Normal, float duration = 3f)
        {
            var r = new PromptRequest { Text = text ?? "", Mode = PromptMode.KeyIcon, Priority = priority, Duration = duration, Key = key };
            UIPromptManager.Enqueue(r);
            return r.Id;
        }

        public static void Cancel(string id) => UIPromptManager.CancelById(id);
        public static void ClearQueue() => UIPromptManager.ClearAll();
        public static bool IsBusy => UIPromptManager.IsBusy;
    }

    public static class UIPromptManager
    {
        private const int SortOrder = 50000; // ponad konsolą (48500) i panelami (9999)
        private const float TopMarginPct = 0.05f;
        private const float InDuration = 0.30f;
        private const float OutDuration = 0.25f;
        private const float AnimSpeedGuard = 0.0001f;

        private enum State { Idle, In, Hold, Out }

        private static GameObject _go;
        private static object _panelSettings;
        private static bool _built;

        private static IntPtr _cardPtr;
        private static IntPtr _barFillPtr;
        private static IntPtr _timeLblPtr;

        private static readonly List<PromptRequest> _queue = new();
        private static PromptRequest _current;
        private static State _state = State.Idle;
        private static float _t;
        private static float _holdTimeLeft;
        private static float _cardW, _cardH, _targetY, _startY;

        private static readonly Dictionary<string, Texture2D> _texCache = new();
        private static string _resourcesDir;

        internal static bool IsBusy => _current != null || _queue.Count > 0;

        // ── Public queue ops ─────────────────────────────────────────────────
        internal static void Enqueue(PromptRequest r)
        {
            EnsureBuilt();
            if (!UIRuntime.IsAvailable) return;

            if (_current != null && r.Priority > _current.Priority && _state == State.Hold)
            {
                _current.Interrupted = true;
                _current.Duration = Mathf.Max(0.6f, _holdTimeLeft); // pamiętamy resztę czasu
                _state = State.Out;
                _t = 0f;
            }

            InsertByPriority(r);
        }

        internal static void CancelById(string id)
        {
            _queue.RemoveAll(r => r.Id == id);
            if (_current != null && _current.Id == id && _state == State.Hold)
            {
                _current.Interrupted = false;
                _state = State.Out;
                _t = 0f;
            }
        }

        internal static void ClearAll()
        {
            _queue.Clear();
            if (_current != null && _state == State.Hold)
            {
                _current.Interrupted = false;
                _state = State.Out;
                _t = 0f;
            }
        }

        private static void InsertByPriority(PromptRequest r)
        {
            int insertAt = _queue.FindIndex(q => q.Priority < r.Priority);
            if (insertAt < 0) _queue.Add(r);
            else _queue.Insert(insertAt, r);
        }

        public static Texture2D LoadResourceTexture(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            if (_texCache.TryGetValue(fileName, out var cached)) return cached;

            try
            {
                _resourcesDir ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mods", "_CMS2026_UITK_Framework", "resources");
                string path = Path.Combine(_resourcesDir, fileName);
                if (!File.Exists(path)) { FrameworkPlugin.Log.Warning($"[UIPrompt] Nie znaleziono tekstury: {path}"); return null; }


                byte[] bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                var il2Bytes = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>(bytes.Length);
                for (int i = 0; i < bytes.Length; i++) il2Bytes[i] = bytes[i];

                var icType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                    .FirstOrDefault(t => t.FullName == "UnityEngine.ImageConversion");
                if (icType == null) { FrameworkPlugin.Log.Warning("[UIPrompt] Brak typu ImageConversion"); return null; }

                var loadImg = icType.GetMethods().FirstOrDefault(m => m.Name == "LoadImage" && m.GetParameters().Length == 2);
                if (loadImg == null) return null;

                bool ok = (bool)loadImg.Invoke(null, new object[] { tex, il2Bytes });
                if (!ok) return null;

                _texCache[fileName] = tex;
                return tex;
            }
            catch (Exception ex)
            {
                FrameworkPlugin.Log.Warning($"[UIPrompt] LoadResourceTexture: {ex.Message}");
                return null;
            }
        }

        // ── Build ────────────────────────────────────────────────────────────
        private static void EnsureBuilt()
        {
            if (_built) return;
            if (!UIRuntime.IsAvailable) return;
            _built = true;

            _panelSettings = UIRuntime.CreatePanelSettings();
            UIRuntime.PanelSettingsType.GetProperty("sortingOrder").SetValue(_panelSettings, SortOrder);

            _go = new GameObject("_CMS2026UIKit_PromptHost");
            UnityEngine.Object.DontDestroyOnLoad(_go);

            var docType = UIRuntime.UIDocumentType;
            var docRaw = _go.AddComponent(Il2CppInterop.Runtime.Il2CppType.From(docType));
            var docWrap = Activator.CreateInstance(docType, new object[] { ((Component)docRaw).Pointer });
            docType.GetProperty("panelSettings").SetValue(docWrap, _panelSettings);

            var root = docType.GetProperty("rootVisualElement").GetValue(docWrap);
            Ignore(root);

            var card = UIRuntime.NewVE();
            Ignore(card);
            var cs = UIRuntime.GetStyle(card);
            S.Position(cs, "Absolute");
            S.Overflow(cs, "Hidden");
            S.BgColor(cs, new Color(0.07f, 0.07f, 0.10f, 0.95f));
            S.BorderRadius(cs, 10f);
            S.Opacity(cs, 0f);
            S.Display(cs, false);
            UIRuntime.AddChild(root, card);
            _cardPtr = UIRuntime.GetPtr(card);
        }

        private static void Ignore(object ve)
        {
            try
            {
                var pmType = UIRuntime.UEAsm.GetType("UnityEngine.UIElements.PickingMode");
                if (pmType != null)
                    UIRuntime.VisualElementType.GetProperty("pickingMode")?.SetValue(ve, Enum.Parse(pmType, "Ignore"));
            }
            catch { }
        }

        // ── Tick — wołane co klatkę z UIKitUpdater ──────────────────────────
        internal static void Tick(float dt)
        {
            if (!_built || !UIRuntime.IsAvailable) return;

            switch (_state)
            {
                case State.Idle:
                    if (_current == null && _queue.Count > 0)
                    {
                        _current = _queue[0];
                        _queue.RemoveAt(0);
                        Activate(_current);
                        _state = State.In;
                        _t = 0f;
                    }
                    break;

                case State.In:
                    _t += dt / InDuration;
                    ApplyInFrame(Mathf.Clamp01(_t));
                    if (_t >= 1f) { _state = State.Hold; _holdTimeLeft = _current.Duration; }
                    break;

                case State.Hold:
                    _holdTimeLeft -= dt;
                    UpdateCountdown(_holdTimeLeft, _current.Duration);
                    if (_holdTimeLeft <= 0f) { _current.Interrupted = false; _state = State.Out; _t = 0f; }
                    break;

                case State.Out:
                    _t += dt / OutDuration;
                    ApplyOutFrame(Mathf.Clamp01(_t));
                    if (_t >= 1f) { FinishCurrent(); _state = State.Idle; }
                    break;
            }
        }

        private static void FinishCurrent()
        {
            var c = _current;
            _current = null;
            if (c == null) return;

            if (c.Interrupted) { c.Interrupted = false; InsertByPriority(c); }
            else c.Completed = true;

            S.Display(UIRuntime.GetStyle(UIRuntime.WrapVE(_cardPtr)), false);
        }

        // ── Aktywacja treści karty pod nowy request ─────────────────────────
        private static void Activate(PromptRequest r)
        {
            var card = UIRuntime.WrapVE(_cardPtr);
            UIRuntime.VisualElementType.GetMethod("Clear")?.Invoke(card, null);

            float scale = Mathf.Clamp(Screen.height / 1080f, 0.75f, 1.85f);
            float pad = 14f * scale;
            float iconSize = 42f * scale;      // było 30f — +40%
            Color accent = ModeColor(r.Mode);

            _cardW = (r.Mode == PromptMode.Texture ? 560f : 480f) * scale;

            var accentVe = UIRuntime.NewVE();
            Ignore(accentVe);
            var accs = UIRuntime.GetStyle(accentVe);
            S.Position(accs, "Absolute");
            S.Left(accs, 0f); S.Top(accs, 0f);
            S.Width(accs, 5f * scale);
            S.BgColor(accs, accent);
            UIRuntime.AddChild(card, accentVe);

            float leftIconX = pad + 8f * scale;
            float iconBoxSize = iconSize * 1.3f;
            float textX = leftIconX + iconBoxSize + pad;
            float textW = _cardW - textX - pad;

            if (r.Mode == PromptMode.Texture && r.Texture != null)
            {
                var img = UIRuntime.NewVE();
                Ignore(img);
                var ims = UIRuntime.GetStyle(img);
                S.Position(ims, "Absolute");
                S.Left(ims, leftIconX); S.Top(ims, pad);
                S.Width(ims, iconSize * 1.6f); S.Height(ims, iconSize * 1.6f);
                S.BorderRadius(ims, 6f);
                UIRuntime.AddChild(card, img);
                UIRuntime.SetBackgroundImage(img, r.Texture);
                textX = leftIconX + iconSize * 1.6f + pad;
                textW = _cardW - textX - pad;
            }
            else if (r.Mode == PromptMode.KeyIcon)
            {
                string keyText = KeyLabel(r.Key);
                float capW = EstimateKeyWidth(keyText, scale, iconBoxSize);
                float capH = iconBoxSize;

                // Jeśli etykieta klawisza jest szeroka (np. "CapsLock"), poszerz całą kartę
                if (leftIconX + capW + pad * 2f > _cardW * 0.55f)
                    _cardW = Mathf.Max(_cardW, leftIconX + capW + pad + 260f * scale);

                var cap = UIRuntime.NewVE();
                Ignore(cap);
                var caps = UIRuntime.GetStyle(cap);
                S.Position(caps, "Absolute");
                S.Left(caps, leftIconX); S.Top(caps, pad);
                S.Width(caps, capW); S.Height(caps, capH);
                S.BgColor(caps, new Color(accent.r * 0.22f, accent.g * 0.22f, accent.b * 0.28f, 1f));
                S.BorderRadius(caps, 6f * scale);
                S.BorderColor(caps, accent);
                S.BorderWidth(caps, 2f * scale);
                UIRuntime.AddChild(card, cap);

                var keyLbl = Activator.CreateInstance(UIRuntime.LabelType);
                Ignore(keyLbl);
                var kls = UIRuntime.GetStyle(keyLbl);
                S.Position(kls, "Absolute");
                S.Left(kls, 0f); S.Top(kls, 0f);
                S.Width(kls, capW); S.Height(kls, capH);
                S.Color(kls, Color.white); S.Font(kls); S.TextAlign(kls, TextAnchor.MiddleCenter);
                S.FontSize(kls, Mathf.RoundToInt(16f * scale));
                S.FontBold(kls, true);
                S.Padding(kls, 4f * scale);
                UIRuntime.LabelType.GetProperty("text").SetValue(keyLbl, keyText);
                UIRuntime.AddChild(cap, keyLbl);

                textX = leftIconX + capW + pad;
                textW = _cardW - textX - pad;
            }
            else
            {
                var iconVe = UIRuntime.NewVE();
                Ignore(iconVe);
                var ivs = UIRuntime.GetStyle(iconVe);
                S.Position(ivs, "Absolute");
                S.Left(ivs, leftIconX); S.Top(ivs, pad);
                S.Width(ivs, iconBoxSize); S.Height(ivs, iconBoxSize);
                UIRuntime.AddChild(card, iconVe);

                var iconTex = LoadResourceTexture(IconFileFor(r.Mode));
                if (iconTex != null)
                {
                    UIRuntime.SetBackgroundImage(iconVe, iconTex);
                    S.BgTint(ivs, accent);
                }
                else
                {
                    S.BgColor(ivs, accent);
                    S.BorderRadius(ivs, iconBoxSize * 0.5f);
                }
            }

            var txt = Activator.CreateInstance(UIRuntime.LabelType);
            Ignore(txt);
            var txs = UIRuntime.GetStyle(txt);
            S.Position(txs, "Absolute");
            S.Left(txs, textX); S.Top(txs, pad);
            S.Width(txs, textW);
            S.Color(txs, Color.white); S.Font(txs);
            S.FontSize(txs, Mathf.RoundToInt(18f * scale));   // było 13f
            S.WhiteSpaceNormal(txs);
            UIRuntime.LabelType.GetProperty("text").SetValue(txt, r.Text);
            UIRuntime.AddChild(card, txt);

            float charsPerLine = Mathf.Max(1f, textW / (9.2f * scale));   // było 6.6f
            int lines = Mathf.Max(1, Mathf.CeilToInt(r.Text.Length / charsPerLine));
            float textH = lines * 25f * scale;   // było 18f
            S.Height(txs, textH);

            float contentH = pad + Mathf.Max(iconBoxSize, textH) + pad;

            var track = UIRuntime.NewVE();
            Ignore(track);
            var trs = UIRuntime.GetStyle(track);
            S.Position(trs, "Absolute");
            S.Left(trs, 0f); S.Top(trs, contentH);
            S.Width(trs, _cardW); S.Height(trs, 3f * scale);
            S.BgColor(trs, new Color(1f, 1f, 1f, 0.10f));
            UIRuntime.AddChild(card, track);

            var fill = UIRuntime.NewVE();
            Ignore(fill);
            var fs = UIRuntime.GetStyle(fill);
            S.Position(fs, "Absolute");
            S.Left(fs, 0f); S.Top(fs, 0f);
            S.Width(fs, _cardW); S.Height(fs, 3f * scale);
            S.BgColor(fs, accent);
            UIRuntime.AddChild(track, fill);
            _barFillPtr = UIRuntime.GetPtr(fill);

            var timeLbl = Activator.CreateInstance(UIRuntime.LabelType);
            Ignore(timeLbl);
            var tls = UIRuntime.GetStyle(timeLbl);
            S.Position(tls, "Absolute");
            S.Left(tls, _cardW - 38f * scale); S.Top(tls, 4f * scale);
            S.Width(tls, 34f * scale); S.Height(tls, 20f * scale);
            S.Color(tls, new Color(1f, 1f, 1f, 0.55f)); S.Font(tls);
            S.FontSize(tls, Mathf.RoundToInt(14f * scale));   // było 10f
            S.TextAlign(tls, TextAnchor.MiddleRight);
            UIRuntime.AddChild(card, timeLbl);
            _timeLblPtr = UIRuntime.GetPtr(timeLbl);

            _cardH = contentH + 3f * scale;
            S.Height(accs, _cardH);
            var cardStyle = UIRuntime.GetStyle(card);
            S.Width(cardStyle, _cardW);
            S.Height(cardStyle, _cardH);

            _targetY = Screen.height * TopMarginPct;
            _startY = -_cardH - 20f;
            S.Left(cardStyle, (Screen.width - _cardW) * 0.5f);

            S.Display(cardStyle, true);
            ApplyInFrame(0f);
        }

        private static void ApplyInFrame(float t)
        {
            float e = 1f - Mathf.Pow(1f - t, 3f);
            var s = UIRuntime.GetStyle(UIRuntime.WrapVE(_cardPtr));
            S.Top(s, Mathf.Lerp(_startY, _targetY, e));
            S.Opacity(s, t);
            float sc = Mathf.Lerp(0.85f, 1f, e);
            S.Scale(s, sc, sc);
        }

        private static void ApplyOutFrame(float t)
        {
            float e = t * t;
            var s = UIRuntime.GetStyle(UIRuntime.WrapVE(_cardPtr));
            S.Top(s, Mathf.Lerp(_targetY, _targetY - 14f, e));
            S.Opacity(s, 1f - t);
            float sc = Mathf.Lerp(1f, 0.85f, e);
            S.Scale(s, sc, sc);
        }

        private static void UpdateCountdown(float left, float total)
        {
            float ratio = total > AnimSpeedGuard ? Mathf.Clamp01(left / total) : 0f;
            if (_barFillPtr != IntPtr.Zero) S.Width(UIRuntime.GetStyle(UIRuntime.WrapVE(_barFillPtr)), _cardW * ratio);
            if (_timeLblPtr != IntPtr.Zero)
            {
                var lbl = Activator.CreateInstance(UIRuntime.LabelType, new object[] { _timeLblPtr });
                UIRuntime.LabelType.GetProperty("text").SetValue(lbl, Mathf.CeilToInt(Mathf.Max(0f, left)) + "s");
            }
        }

        private static Color ModeColor(PromptMode m) => m switch
        {
            PromptMode.Error => new Color(0.85f, 0.25f, 0.25f, 1f),
            PromptMode.Warning => new Color(0.90f, 0.65f, 0.15f, 1f),
            PromptMode.Texture => new Color(0.35f, 0.55f, 0.90f, 1f),
            PromptMode.KeyIcon => new Color(0.45f, 0.70f, 0.95f, 1f),
            _ => new Color(0.35f, 0.65f, 0.95f, 1f),
        };

        private static string IconFileFor(PromptMode m) => m switch
        {
            PromptMode.Warning => "icon_warning.png",
            PromptMode.Error => "icon_error.png",
            _ => "icon_info.png",
        };

        private static string KeyLabel(KeyCode k)
        {
            switch (k)
            {
                case KeyCode.Return: case KeyCode.KeypadEnter: return "Enter";
                case KeyCode.Escape: return "Esc";
                case KeyCode.LeftControl: case KeyCode.RightControl: return "Ctrl";
                case KeyCode.LeftShift: case KeyCode.RightShift: return "Shift";
                case KeyCode.LeftAlt: case KeyCode.RightAlt: return "Alt";
                case KeyCode.Space: return "Space";
                case KeyCode.UpArrow: return "↑";
                case KeyCode.DownArrow: return "↓";
                case KeyCode.LeftArrow: return "←";
                case KeyCode.RightArrow: return "→";
                case KeyCode.Tab: return "Tab";
            }
            string s = k.ToString();
            if (s.StartsWith("Alpha")) return s.Substring(5);
            if (s.StartsWith("Keypad")) return "N" + s.Substring(6);
            return s.Length > 4 ? s.Substring(0, 4) : s;
        }

        private static float EstimateKeyWidth(string label, float scale, float squareSize)
        {
            float charW = 11f * scale;
            float estimated = label.Length * charW + 16f * scale;
            return Mathf.Max(squareSize, estimated);
        }
    }
}