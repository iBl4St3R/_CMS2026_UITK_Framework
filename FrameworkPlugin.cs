using CMS2026UITKFramework;
using Il2CppMono;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;

[assembly: MelonInfo(typeof(CMS2026UITKFramework.FrameworkPlugin),
    "_CMS2026_UITK_Framework", "0.2.1", "Blaster")]
[assembly: MelonGame("Red Dot Games", "Car Mechanic Simulator 2026 Demo")]
[assembly: MelonGame("Red Dot Games", "Car Mechanic Simulator 2026")]
[assembly: MelonPriority(-100)]  // wysoki priorytet = ładuje się wcześniej


//_CMS2026_UITK_Framework /
//├── FrameworkPlugin.cs        ← entry point MelonMod
//├── UIRuntime.cs              ← globalny kontekst, typy UI, inicjalizacja refleksji
//├── UIPanelBuilder.cs         ← tworzenie paneli
//├── UIElements.cs             ← Label, Button, Toggle — builderowy API
//└── FrameworkAPI.cs           ← publiczne API dla innych modów


namespace CMS2026UITKFramework
{
    public class FrameworkPlugin : MelonMod
    {
        public const string Version = "0.2.1";
        internal static MelonLogger.Instance Log => Melon<FrameworkPlugin>.Logger;

        private static GameObject _runtimeHost;

        // ── Rejestr aktywnych paneli — UIKitUpdater tickuje je co klatkę ──
        internal static readonly List<UIPanel> ActivePanels = new();

        public override void OnInitializeMelon()
        {
            FrameworkLog.Msg($"[UIKit] v{Version} initializing...");
            bool ok = UIRuntime.TryResolveTypes();
            FrameworkLog.Msg(ok
                ? "[UIKit] UIToolkit types resolved OK"
                : "[UIKit] UIToolkit unavailable — standby");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (_runtimeHost != null) return;

            _runtimeHost = new GameObject("_CMS2026UIKit_Host");
            UnityEngine.Object.DontDestroyOnLoad(_runtimeHost);
            _runtimeHost.AddComponent<UIKitUpdater>();

            FrameworkLog.Msg($"[UIKit] Host created (scene: {sceneName})");

            TryRegisterInConsole();
            RegisterFrameworkCommands();
        }

        private static void TryRegisterInConsole()
        {
            try
            {
                var apiType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                    .FirstOrDefault(t => t.FullName == "CMS2026SimpleConsole.ConsoleAPI");

                if (apiType == null) return;   // konsola nie jest załadowana — nic się nie dzieje

                apiType.GetMethod("RegisterMod")?.Invoke(null, new object[]
                {
            "_CMS2026_UITK_Framework",          // assembly name
            "_CMS2026 UITK Framework",          // display name
            "Blaster",                          // author
            "UI Toolkit panel framework for CMS2026 mods",  // description
            "https://github.com/iBl4St3R/_CMS2026_UITK_Framework",  // GitHub
            null,                               // Nexus (brak)
            null                                // version — null = auto z MelonInfo
                });

                FrameworkLog.Msg("[UIKit] Registered in SimpleConsole mod list.");
            }
            catch (Exception ex)
            {
                Log.Warning($"[UIKit] ConsoleAPI registration failed: {ex.Message}");
            }
        }

        private static void RegisterFrameworkCommands()
        {
            try
            {
                var apiType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                    .FirstOrDefault(t => t.FullName == "CMS2026SimpleConsole.ConsoleAPI");

                if (apiType == null) return;

                var regMethod = apiType.GetMethod("RegisterCommand");

                // 1. KOMENDA: ui_list - Pokazuje aktywne panele i ich indeksy
                regMethod?.Invoke(null, new object[] {
            "ui_list",
            "Lists all active UI Framework panels",
            (Action<string[]>)(args => {
                var panels = FrameworkPlugin.ActivePanels;
                FrameworkLog.Msg($"--- Active Panels ({panels.Count}) ---");
                for (int i = 0; i < panels.Count; i++) {
                    var p = panels[i];
                    if (p != null) FrameworkLog.Msg($"[{i}] \"{p.Title}\"");
                }
            })
        });

                // 2. KOMENDA: ui_resize <indeks/tytuł> <szerokość> <wysokość>
                regMethod?.Invoke(null, new object[] {
            "ui_resize",
            "Resizes a panel: ui_resize <name/index> <w> <h>",
            (Action<string[]>)(args => {
                // Diagnostyka, abyś widział w konsoli co dokładnie odbiera plugin
                FrameworkLog.Msg($"[UIKit] Debug args: count={args.Length}, values='{string.Join("|", args)}'");

                if (args.Length < 3) {
                    Log.Warning("Usage: ui_resize <name/index> <width> <height>");
                    return;
                }

                try {
                    // Pobieramy wymiary z dwóch ostatnich argumentów
                    string strW = args[args.Length - 2];
                    string strH = args[args.Length - 1];
                    
                    // Ustalamy cel (nazwa lub indeks). 
                    // Jeśli args[0] to nazwa komendy, bierzemy args[1]. Jeśli nie, bierzemy args[0].
                    string target = (args[0].ToLower() == "ui_resize") ? args[1] : args[0];

                    float newW = float.Parse(strW, System.Globalization.CultureInfo.InvariantCulture);
                    float newH = float.Parse(strH, System.Globalization.CultureInfo.InvariantCulture);

                    UIPanel panel = null;
                    if (int.TryParse(target, out int idx) && idx >= 0 && idx < FrameworkPlugin.ActivePanels.Count)
                        panel = FrameworkPlugin.ActivePanels[idx];
                    else
                        panel = FrameworkAPI.GetPanel(target);

                    if (panel != null) {
                        panel.SetSize(newW, newH);
                         FrameworkLog.Msg($"[UIKit] Resized '{panel.Title}' to {newW}x{newH}");
                    } else {
                        Log.Warning($"[UIKit] Panel '{target}' not found. Use 'ui_list' to check names.");
                    }
                }
                catch (Exception ex) {
                    Log.Error($"[UIKit] ui_resize error: {ex.Message}");
                }
            })
        });
            }
            catch (Exception ex)
            {
                Log.Warning($"[UIKit] Failed to register console commands: {ex.Message}");
            }
        }



    }


    internal static class FrameworkLog
    {
        // Bezpośredni logger — nie przez Melon<T>, działa od pierwszej linii OnInitializeMelon
        private static readonly MelonLogger.Instance _log =
            new MelonLogger.Instance("_CMS2026_UITK_Framework");

        private static System.Reflection.MethodInfo _consolePrint;
        private static bool _resolved;

        private static void TryResolve()
        {
            if (_resolved) return;
            _resolved = true;
            try
            {
                var t = System.AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); }
                                      catch { return System.Type.EmptyTypes; } })
                    .FirstOrDefault(x => x.FullName == "CMS2026SimpleConsole.ConsoleAPI");

                // spróbuj Print(string, string) — source tag
                _consolePrint = t?.GetMethod("Print",
                    new[] { typeof(string), typeof(string) });

                // fallback: Print(string) bez source
                if (_consolePrint == null)
                    _consolePrint = t?.GetMethod("Print",
                        new[] { typeof(string) });
            }
            catch { }
        }

        public static void Msg(string msg)
        {
            try
            {
                TryResolve();
                if (_consolePrint != null)
                {
                    var pars = _consolePrint.GetParameters();
                    if (pars.Length == 2)
                        _consolePrint.Invoke(null, new object[] { msg, "UIKit" });
                    else
                        _consolePrint.Invoke(null, new object[] { msg });
                    return;
                }
            }
            catch { }
            _log.Msg(msg);
        }

        public static void Warn(string msg)  => _log.Warning(msg);
        public static void Error(string msg) => _log.Error(msg);
    }
}