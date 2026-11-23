# _CMS2026_UITK_Framework

![Version](https://img.shields.io/badge/version-0.4.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![MelonLoader](https://img.shields.io/badge/MelonLoader-v0.7.2+-red)
![Game](https://img.shields.io/badge/game-CMS2027-orange)

> ⚠️ **THIS IS NOT A STANDALONE GAME MOD.** It is a developer tool and a library required by other mods to display their user interfaces. Only install this if another mod requires it or if you are a mod developer.

A UI framework for **Car Mechanic Simulator 2027** mods (the name CMS2026 in files and namespaces stays for compatibility).  
Interfaces with Unity 6's UI Toolkit via MelonLoader and IL2CPP, giving mod developers a clean, fluent API to build in-game panels.

---

## 📂 Samples & Examples
If you want to see the framework in action or learn how to implement specific features, check the **[Samples folder](https://github.com/iBl4St3R/_CMS2026_UITK_Framework/tree/main/Samples)** in the repository. It contains ready-to-use code snippets for various UI layouts.

---

## 🛠 Installation
1. Copy `_CMS2026_UITK_Framework.dll` and the `_CMS2026_UITK_Framework` folder (prompt icons in `resources/`) into your `Mods/` folder.
2. The framework loads automatically with `MelonPriority(-100)`, so it initializes before other mods.

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

---

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

---

## 📚 Documentation
Detailed documentation is available for both beginners and advanced users:
* 📖 [Basic API Documentation](https://github.com/iBl4St3R/_CMS2026_UITK_Framework/blob/main/BasicAPI.md): Recommended for most modders. Covers elements, callbacks, and basic panel setup.
* 🛡️ [Pro API Documentation](https://github.com/iBl4St3R/_CMS2026_UITK_Framework/blob/main/ProAPI.md): For advanced usage, including raw pointers, S{} styles, and custom Visual Elements.

---

## 📝 Changelog

**0.4.0**
Only new API was added. Nothing was removed or renamed, so mods built for 0.3.1 keep working.
* Sprites as backgrounds: `UIRuntime.SetBackgroundSprite(ve, sprite)` and `UIImageHandle.SetSprite(sprite)`. Only the sprite rect is drawn, so icons from game atlases work.
* Fonts: `UIRuntime.FindFont(name)`, new overload `S.Font(style, Font)` and `UIPanel.SetDefaultFont(font)` for elements added after the call. `S.Font(style)` works as before.
* Layout helpers in `S`: `FlexDirection`, `FlexGrow`, `FlexShrink`, `FlexWrap`, `JustifyContent`, `AlignItems`, `Margin` (plus Left/Right/Top/Bottom), `WidthPercent`, `HeightPercent`, `MinWidth`, `MaxWidth`, `MaxHeight`.
* `UIPanel.IsTextInputFocused` tells you when a text field in the panel has focus, so your hotkeys can ignore typing.
* Fixed: `S.BgScaleMode` and `SetScaleMode` did nothing before. They now work (ScaleToFit = contain, ScaleAndCrop = cover, StretchToFill = fill). If your mod called them, check that your images still look right.
* Fixed: prompt icons (info, warning, error) turned into plain colored circles after loading a save. Icon textures are now kept across scene changes.
* Reflection users: `S.Font` now has two overloads, so look it up with explicit parameter types.

Full history of all versions: [CHANGELOG.md](https://github.com/iBl4St3R/_CMS2026_UITK_Framework/blob/main/CHANGELOG.md).

---

## ⚠️ Known Limitations
* Framework UI renders below the game's native Canvas during scene loading screens due to Unity architecture.
* `SetSize` on Labels/Buttons uses absolute positioning and does not trigger element reflow.
* The default font has no emoji glyphs, so emoji render as empty boxes.

---

## 📄 License
This framework is released under the [MIT License](https://github.com/iBl4St3R/_CMS2026_UITK_Framework/blob/main/LICENSE).