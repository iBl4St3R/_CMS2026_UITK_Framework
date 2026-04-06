# _CMS2026_UITK_Framework

# ⚠️ IMPORTANT FOR PLAYERS
<font color="red">**THIS IS NOT A STANDALONE GAME MOD.**</font> It is a developer tool and a library required by other mods to display their user interfaces. Only install this if another mod requires it or if you are a mod developer.

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

## 📂 Samples & Examples
If you want to see the framework in action or learn how to implement specific features, check the **[Samples folder](https://github.com/iBl4St3R/_CMS2026_UITK_Framework/tree/main/Samples)** in the repository. It contains ready-to-use code snippets for various UI layouts.

---

## 🛠 Installation
1. Copy `_CMS2026_UITK_Framework.dll` into your `Mods/` folder.
2. The framework loads automatically with `MelonPriority(-100)` — ensuring it initializes before other mods.

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
```

## ✨ Features

| Element | Description |
|---|---|
| `AddLabel` | Static or dynamic text |
| `AddHeader` | Section title with separator |
| `AddButton` | Clickable button with auto hover/press |
| `AddToggle` | ON/OFF switch with callback |
| `AddSlider` | Value slider with +/- buttons and drag |
| `AddProgressBar` | Horizontal bar 0–1 |
| `AddTextInput` | Single-line text field |
| `AddColorPicker` | RGB picker with channel sliders |
| `AddDropdown` | Expandable option list |
| `AddImage` | Texture display with tint support |
| `AddSeparator` | Horizontal divider for visual grouping |
| `AddSpace` | Empty vertical gap for layout spacing |
| `AddRow` | Horizontal row container for multi-column layouts |

Panel capabilities:
* Draggable title bar and mouse wheel scrolling.
* Sort order control for z-layering between multiple panels.
* Per-frame update callbacks for animations and live data tracking.
* Advanced styling (rounded corners, borders, opacity) via Pro API.

## 📚 Documentation
Detailed documentation is available for both beginners and advanced users:
* 📖 Basic API Documentation – Recommended for most modders. Covers elements, callbacks, and basic panel setup.
* 🛡️ Pro API Documentation – For advanced usage, including raw pointers, S{} styles, and custom Visual Elements.

## ⚠️ Known Limitations
* Framework UI renders below the game's native Canvas during scene loading screens due to Unity architecture.
* `SetScaleMode` on images is currently pending due to Unity 6 API changes.
* `SetSize` on Labels/Buttons uses absolute positioning and does not trigger element reflow.

## 📂 File Structure
_CMS2026_UITK_Framework/
├── FrameworkPlugin.cs   — MelonMod entry point
├── FrameworkAPI.cs      — Public API
├── UIRuntime.cs         — IL2CPP type resolution and helpers
├── UIElements.cs        — S{} style helper class
├── UIPanelBuilder.cs    — UIPanel and all AddXxx methods
├── UIHandles.cs         — Handle classes for dynamic updates
└── UIKitUpdater.cs      — MonoBehaviour frame ticker

## 📄 License
<<<<<<< HEAD
This framework is released under the MIT License.

