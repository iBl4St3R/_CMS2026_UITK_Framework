using CMS2026UITKFramework;
using Il2CppMono;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: MelonInfo(typeof(CMS2026UITKFramework.FrameworkPlugin),
    "_CMS2026_UITK_Framework", "0.1.0", "Blaster")]
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
        public const string Version = "0.1.0";
        internal static MelonLogger.Instance Log => Melon<FrameworkPlugin>.Logger;

        private static GameObject _runtimeHost;

        // ── Rejestr aktywnych paneli — UIKitUpdater tickuje je co klatkę ──
        internal static readonly List<UIPanel> ActivePanels = new();

        public override void OnInitializeMelon()
        {
            Log.Msg($"[UIKit] v{Version} initializing...");
            bool ok = UIRuntime.TryResolveTypes();
            Log.Msg(ok
                ? "[UIKit] UIToolkit types resolved OK"
                : "[UIKit] UIToolkit unavailable — standby");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (_runtimeHost != null) return;

            _runtimeHost = new GameObject("_CMS2026UIKit_Host");
            UnityEngine.Object.DontDestroyOnLoad(_runtimeHost);
            _runtimeHost.AddComponent<UIKitUpdater>();

            Log.Msg($"[UIKit] Host created (scene: {sceneName})");

            TryRegisterInConsole();  // ← DODAJ
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

                Log.Msg("[UIKit] Registered in SimpleConsole mod list.");
            }
            catch (Exception ex)
            {
                Log.Warning($"[UIKit] ConsoleAPI registration failed: {ex.Message}");
            }
        }
    }
}