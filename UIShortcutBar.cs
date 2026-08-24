using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CMS2026UITKFramework
{
    /// <summary>
    /// Publiczne API paska skrótów klawiszowych.
    /// Osobny UIDocument/panel (jak UIPromptManager), ale zamiast kolejki —
    /// trwała, edytowalna lista chipów [KEY] Label. Wjeżdża animowanie znad
    /// dolnej krawędzi ekranu, tuż pod oryginalnym paskiem akcji gry.
    ///
    /// Użycie:
    ///   UIShortcutBar.Add("F1", "Open Panel");
    ///   UIShortcutBar.Update("F1", "Close Panel");
    ///   UIShortcutBar.Remove("F1");
    ///   UIShortcutBar.Show();
    ///   UIShortcutBar.Hide();
    /// </summary>
    public static class UIShortcutBar
    {
        /// <summary>Dodaje skrót. Zwraca false jeśli klawisz już istnieje na liście (duplikat ignorowany).</summary>
        public static bool Add(string key, string label) => UIShortcutBarManager.AddEntry(key, label);

        /// <summary>Aktualizuje etykietę istniejącego skrótu. Zwraca false jeśli klawisz nie istnieje.</summary>
        public static bool Update(string key, string label) => UIShortcutBarManager.UpdateEntry(key, label);

        /// <summary>Usuwa skrót z listy. Zwraca false jeśli nie znaleziono.</summary>
        public static bool Remove(string key) => UIShortcutBarManager.RemoveEntry(key);

        /// <summary>Jak Remove, ale faktyczne usunięcie chipa nastąpi dopiero gdy pasek się w pełni schowa (po Hide()) — nie od razu. Użyteczne gdy chcesz najpierw schować cały pasek animacją, a dopiero potem wyczyścić klawisze, zamiast pojedynczych chipów znikających "z palca".</summary>
        public static bool RemoveDelayed(string key) => UIShortcutBarManager.RemoveEntryDelayed(key);

        /// <summary>Czyści całą listę skrótów.</summary>
        public static void Clear() => UIShortcutBarManager.ClearEntries();

        /// <summary>Pokazuje pasek (animacja wjazdu z dołu ekranu).</summary>
        public static void Show() => UIShortcutBarManager.ShowBar();

        /// <summary>Ukrywa pasek (animacja zjazdu w dół ekranu).</summary>
        public static void Hide() => UIShortcutBarManager.HideBar();

        public static void Toggle() { if (IsVisible) Hide(); else Show(); }

        /// <summary>True gdy pasek jest widoczny lub w trakcie animacji wjazdu.</summary>
        public static bool IsVisible => UIShortcutBarManager.IsVisibleOrShowing;

        /// <summary>Odstęp paska od dolnej krawędzi ekranu w px (skalowany razem z resztą UI). Domyślnie 6f.</summary>
        public static void SetBottomMargin(float pixels) => UIShortcutBarManager.BottomMargin = pixels;
    }

    internal static class UIShortcutBarManager
    {
        private const int SortOrder = 49000; // pod promptami (50000), nad panelami modów (9999)
        private const float OutDuration = 0.14f; // 2x szybciej niż poprzednie 0.28f — In jest teraz natychmiastowe

        private static readonly List<string> _pendingRemovals = new();

        private enum State { Hidden, Shown, Out }

        internal static float BottomMargin = 6f;

        private class ShortcutEntry
        {
            public string Key;
            public string Label;
            public IntPtr KeyBadgePtr;
            public IntPtr KeyLblPtr;
            public IntPtr LabelLblPtr;
        }

        private static GameObject _go;
        private static object _panelSettings;
        private static bool _built;

        private static IntPtr _rootPtr;
        private static IntPtr _barPtr; // kontener/tło wszystkich chipów

        private static readonly List<ShortcutEntry> _entries = new();

        private static State _state = State.Hidden;
        private static float _t;
        private static float _barW, _barH, _targetY, _hiddenY;

        internal static bool IsVisibleOrShowing => _state == State.Shown;

        // ── Public entry ops ─────────────────────────────────────────────────
        internal static bool AddEntry(string key, string label)
        {
            EnsureBuilt();
            if (!UIRuntime.IsAvailable || string.IsNullOrEmpty(key)) return false;

            var existing = _entries.FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                _pendingRemovals.RemoveAll(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
                return false; // klawisz już istnieje — duplikat ignorujemy (ale kasujemy ewentualny pending removal)
            }

            _entries.Add(new ShortcutEntry { Key = key, Label = label ?? "" });
            RebuildLayout();
            return true;
        }

        internal static bool UpdateEntry(string key, string label)
        {
            if (!UIRuntime.IsAvailable) return false;
            var e = _entries.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            if (e == null) return false;

            e.Label = label ?? "";
            RebuildLayout();
            return true;
        }

        internal static bool RemoveEntry(string key)
        {
            if (!UIRuntime.IsAvailable) return false;
            int idx = _entries.FindIndex(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return false;

            _entries.RemoveAt(idx);
            RebuildLayout();
            return true;
        }

        internal static bool RemoveEntryDelayed(string key)
        {
            if (!UIRuntime.IsAvailable || string.IsNullOrEmpty(key)) return false;
            if (!_entries.Any(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase))) return false;

            if (!_pendingRemovals.Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase)))
                _pendingRemovals.Add(key);
            return true;
        }

        private static void ApplyPendingRemovals()
        {
            if (_pendingRemovals.Count == 0) return;
            foreach (var key in _pendingRemovals)
                _entries.RemoveAll(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));
            _pendingRemovals.Clear();
            RebuildLayout();
        }

        internal static void ClearEntries()
        {
            _entries.Clear();
            if (_built) RebuildLayout();
        }

        // ── Show / Hide ──────────────────────────────────────────────────────
        internal static void ShowBar()
        {
            EnsureBuilt();
            if (!UIRuntime.IsAvailable) return;
            if (_state == State.Shown) return;

            var s = UIRuntime.GetStyle(UIRuntime.WrapVE(_barPtr));
            S.Display(s, true);
            S.Top(s, _targetY);
            S.Opacity(s, 1f);
            _state = State.Shown;
        }

        internal static void HideBar()
        {
            if (!_built || !UIRuntime.IsAvailable) return;
            if (_state == State.Out || _state == State.Hidden) return;

            _state = State.Out;
            _t = 0f;
        }

        // ── Tick — wołane co klatkę z UIKitUpdater ──────────────────────────
        internal static void Tick(float dt)
        {
            if (!_built || !UIRuntime.IsAvailable) return;

            if (_state == State.Out)
            {
                _t += dt / OutDuration;
                ApplyFrame(1f - Mathf.Clamp01(_t));
                if (_t >= 1f)
                {
                    _state = State.Hidden;
                    S.Display(UIRuntime.GetStyle(UIRuntime.WrapVE(_barPtr)), false);
                    ApplyPendingRemovals();
                }
            }
        }

        private static void ApplyFrame(float shownT)
        {
            float e = 1f - Mathf.Pow(1f - shownT, 3f); // ease-out
            var s = UIRuntime.GetStyle(UIRuntime.WrapVE(_barPtr));
            S.Top(s, Mathf.Lerp(_hiddenY, _targetY, e));
            S.Opacity(s, shownT);
        }

        // ── Build ────────────────────────────────────────────────────────────
        private static void EnsureBuilt()
        {
            if (_built) return;
            if (!UIRuntime.IsAvailable) return;
            _built = true;

            _panelSettings = UIRuntime.CreatePanelSettings();
            UIRuntime.PanelSettingsType.GetProperty("sortingOrder").SetValue(_panelSettings, SortOrder);

            _go = new GameObject("_CMS2026UIKit_ShortcutBarHost");
            UnityEngine.Object.DontDestroyOnLoad(_go);

            var docType = UIRuntime.UIDocumentType;
            var docRaw = _go.AddComponent(Il2CppInterop.Runtime.Il2CppType.From(docType));
            var docWrap = Activator.CreateInstance(docType, new object[] { ((Component)docRaw).Pointer });
            docType.GetProperty("panelSettings").SetValue(docWrap, _panelSettings);

            var root = docType.GetProperty("rootVisualElement").GetValue(docWrap);
            _rootPtr = UIRuntime.GetPtr(root);

            var bar = UIRuntime.NewVE();
            var bs = UIRuntime.GetStyle(bar);
            S.Position(bs, "Absolute");
            S.Overflow(bs, "Hidden");
            S.BgColor(bs, new Color(0.07f, 0.07f, 0.10f, 0.90f));
            S.BorderRadius(bs, 8f);
            S.Opacity(bs, 0f);
            S.Display(bs, false);
            UIRuntime.AddChild(root, bar);
            _barPtr = UIRuntime.GetPtr(bar);

            _state = State.Hidden;
        }
        /// <summary>Sort key for a shortcut chip: numeric keys ("1".."9") sort first, ascending; F-keys ("F1".."F12") sort next, ascending by number; everything else (letters, "ESC", "TAB", etc.) keeps its original insertion order after those two groups.</summary>
        private static (int category, int numeric) GetSortKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return (2, 0);
            if (key.All(char.IsDigit) && int.TryParse(key, out int numVal)) return (0, numVal);
            if ((key[0] == 'F' || key[0] == 'f') && key.Length > 1 && key.Substring(1).All(char.IsDigit) && int.TryParse(key.Substring(1), out int fVal)) return (1, fVal);
            return (2, 0);
        }

        // ── Layout ───────────────────────────────────────────────────────────
        // ── Layout ───────────────────────────────────────────────────────────
        private static void RebuildLayout()
        {
            if (!_built || !UIRuntime.IsAvailable) return;

            var bar = UIRuntime.WrapVE(_barPtr);
            UIRuntime.VisualElementType.GetMethod("Clear")?.Invoke(bar, null);

            var orderedEntries = _entries.Select((e, idx) => (e, idx)).OrderBy(x => GetSortKey(x.e.Key).category).ThenBy(x => GetSortKey(x.e.Key).numeric).ThenBy(x => x.idx).Select(x => x.e).ToList();

            float scale = Mathf.Clamp(Screen.height / 1080f, 0.75f, 1.85f);
            float padX = 12f * scale;
            float padY = 8f * scale;
            float chipGap = 16f * scale;
            float rowH = 26f * scale;
            float keyPadX = 8f * scale;
            float keyLblGap = 6f * scale;
            float charW = 8.5f * scale;

            float x = padX;

            foreach (var entry in orderedEntries)
            {
                float keyTextW = Mathf.Max(14f * scale, entry.Key.Length * charW);
                float keyBadgeW = keyTextW + keyPadX * 2f;
                float labelTextW = Mathf.Max(0f, entry.Label.Length * (charW * 0.92f));

                // keycap
                var badge = UIRuntime.NewVE();
                var bds = UIRuntime.GetStyle(badge);
                S.Position(bds, "Absolute");
                S.Left(bds, x); S.Top(bds, padY);
                S.Width(bds, keyBadgeW); S.Height(bds, rowH);
                S.BgColor(bds, new Color(0.90f, 0.90f, 0.87f, 1f));
                S.BorderRadius(bds, 5f * scale);
                S.BorderColor(bds, new Color(0.55f, 0.55f, 0.50f, 1f));
                S.BorderWidth(bds, 1.2f * scale);
                UIRuntime.AddChild(bar, badge);
                entry.KeyBadgePtr = UIRuntime.GetPtr(badge);

                var keyLbl = Activator.CreateInstance(UIRuntime.LabelType);
                var kls = UIRuntime.GetStyle(keyLbl);
                S.Position(kls, "Absolute");
                S.Left(kls, 0f); S.Top(kls, 0f);
                S.Width(kls, keyBadgeW); S.Height(kls, rowH);
                S.Color(kls, new Color(0.20f, 0.20f, 0.18f, 1f)); S.Font(kls);
                S.FontBold(kls, true);
                S.FontSize(kls, Mathf.RoundToInt(13f * scale));
                S.TextAlign(kls, TextAnchor.MiddleCenter);
                UIRuntime.LabelType.GetProperty("text").SetValue(keyLbl, entry.Key);
                UIRuntime.AddChild(badge, keyLbl);
                entry.KeyLblPtr = UIRuntime.GetPtr(keyLbl);

                x += keyBadgeW + keyLblGap;

                // label
                var lbl = Activator.CreateInstance(UIRuntime.LabelType);
                var ls = UIRuntime.GetStyle(lbl);
                S.Position(ls, "Absolute");
                S.Left(ls, x); S.Top(ls, padY);
                S.Width(ls, labelTextW + 2f); S.Height(ls, rowH);
                S.Color(ls, new Color(0.90f, 0.90f, 0.95f, 1f)); S.Font(ls);
                S.FontSize(ls, Mathf.RoundToInt(13f * scale));
                S.TextAlign(ls, TextAnchor.MiddleLeft);
                UIRuntime.LabelType.GetProperty("text").SetValue(lbl, entry.Label);
                UIRuntime.AddChild(bar, lbl);
                entry.LabelLblPtr = UIRuntime.GetPtr(lbl);

                x += labelTextW + chipGap;
            }

            _barW = Mathf.Max(0f, x - chipGap + padX);
            _barH = rowH + padY * 2f;

            var barStyle = UIRuntime.GetStyle(bar);
            S.Width(barStyle, _barW);
            S.Height(barStyle, _barH);
            S.Left(barStyle, (Screen.width - _barW) * 0.5f);

            _targetY = Screen.height - _barH - BottomMargin * scale;
            _hiddenY = Screen.height + 20f;

            if (_state == State.Shown) S.Top(barStyle, _targetY);
            else if (_state == State.Hidden) S.Top(barStyle, _hiddenY);
        }
    }
}