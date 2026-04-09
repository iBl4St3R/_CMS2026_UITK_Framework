using System;

namespace CMS2026UITKFramework
{
    /// <summary>
    /// Central cursor manager for the entire framework.
    ///
    /// Operates on a reference counting principle:
    ///   • Request() — a panel/mod signals that it needs the cursor
    ///   • Release() — a panel/mod signals that it no longer needs it
    ///
    /// The cursor is shown when the counter is > 0 and hidden when it drops to 0.
    /// Conflicts between mods are impossible — each mod manages only its own requests.
    ///
    /// Integration with game logic is handled via delegates:
    ///   CursorManager.OnCursorShow += () => { /* GameMode.SetCurrentMode(UI) */ };
    ///   CursorManager.OnCursorHide += () => { /* GameMode.SetCurrentMode(Garage) */ };
    /// </summary>
    public static class CursorManager
    {
        private static int _requestCount = 0;
        private static readonly object _lock = new object();

        /// <summary>Triggered when the counter transitions from 0 to 1 (the first panel requests the cursor).</summary>
        public static event Action OnCursorShow;

        /// <summary>Triggered when the counter drops to 0 (the last panel released the cursor).</summary>
        public static event Action OnCursorHide;

        /// <summary>True when at least one panel/mod is requesting a visible cursor.</summary>
        public static bool IsCursorActive
        {
            get { lock (_lock) return _requestCount > 0; }
        }

        /// <summary>
        /// Informs the framework that a panel/mod requires the cursor to be shown.
        /// Safe to call multiple times — every call must have a corresponding Release().
        /// </summary>
        public static void Request()
        {
            lock (_lock)
            {
                _requestCount++;
                if (_requestCount == 1)          // 0→1: first requester
                    FireShow();
            }
        }

        /// <summary>
        /// Releases a cursor request.
        /// When all requests are released, the cursor is hidden.
        /// </summary>
        public static void Release()
        {
            lock (_lock)
            {
                if (_requestCount <= 0)
                {
                    FrameworkPlugin.Log.Warning(
                        "[CursorManager] Release() called with count already 0 — ignored");
                    return;
                }
                _requestCount--;
                if (_requestCount == 0)          // N→0: last one released
                    FireHide();
            }
        }

        /// <summary>
        /// Emergency force hide and counter reset.
        /// Use only during scene unloading or in exceptional circumstances.
        /// </summary>
        public static void ForceHide()
        {
            lock (_lock)
            {
                if (_requestCount > 0)
                {
                    _requestCount = 0;
                    FireHide();
                }
            }
        }

        // ── Internal ────────────────────────────────────────────────────

        private static void FireShow()
        {
            try { OnCursorShow?.Invoke(); }
            catch (Exception ex)
            {
                FrameworkPlugin.Log.Error($"[CursorManager] OnCursorShow error: {ex.Message}");
            }
        }

        private static void FireHide()
        {
            try { OnCursorHide?.Invoke(); }
            catch (Exception ex)
            {
                FrameworkPlugin.Log.Error($"[CursorManager] OnCursorHide error: {ex.Message}");
            }
        }
    }
}