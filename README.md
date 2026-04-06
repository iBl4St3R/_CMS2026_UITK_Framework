# _CMS2026_UITK_Framework

# ⚠️ IMPORTANT FOR PLAYERS
**THIS IS NOT A STANDALONE GAME MOD.** It is a developer tool and a library required by other mods to display their user interfaces. Only install this if another mod requires it or if you are a mod developer.

---

# _CMS2026_UITK_Framework
A UI framework for **Car Mechanic Simulator 2026** mods.  
Interfaces with Unity 6's UI Toolkit via MelonLoader and IL2CPP, giving mod developers a clean, fluent API to build in-game panels.

**Version:** 0.1.0  
**Author:** Blaster  
**License:** MIT  
**Game:** Car Mechanic Simulator 2026 / Demo  
**MelonLoader:** v0.7.2+

---

## 🛠 Installation
1. Copy `_CMS2026_UITK_Framework.dll` into your `Mods/` folder.
2. The framework loads automatically with `MelonPriority(-100)` — ensuring it initializes before other mods.

---

## 📂 Samples & Examples
If you want to see the framework in action or learn how to implement specific features, check the **[Samples folder](https://github.com/iBl4St3R/_CMS2026_UITK_Framework/tree/main/Samples)** in the repository. It contains ready-to-use code snippets for various UI layouts.

---

## 🚀 Quick Start
```csharp
using CMS2026UITKFramework;

public class MyMod : MelonMod
{
    public override void OnSceneWasLoaded(int idx, string name)
    {
        if (!FrameworkAPI.IsReady) return;

        var panel = FrameworkAPI.CreatePanel("My Mod", 40, 40, 300, 400);

        panel.AddHeader("Controls");
        panel.AddButton("Do something", () => MelonLogger.Msg("clicked!"));
        panel.AddToggle("God Mode", false, v => SetGodMode(v));
        panel.AddSlider("Speed", 1f, 100f, 10f, v => SetSpeed(v));

        panel.SetScrollbarVisible(true);
        panel.SetDragWhenScrollable(true);
    }
}
