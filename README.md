# _CMS2026_UITK_Framework

A UI framework for **Car Mechanic Simulator 2026** mods.  
Interfaces with Unity 6's UI Toolkit via MelonLoader and IL2CPP,  
giving mod developers a clean, fluent API to build in-game panels.

**Version:** 0.1.0  
**Author:** Blaster  
**License:** MIT  
**Game:** Car Mechanic Simulator 2026 / Demo  
**MelonLoader:** v0.7.2+

---

## Installation

1. Copy `_CMS2026_UITK_Framework.dll` into your `Mods/` folder.
2. The framework loads automatically with `MelonPriority(-100)` — before all other mods.

---

## Quick Start
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

## Features

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
| `AddSeparator` | Horizontal divider |
| `AddSpace` | Empty vertical gap |

**Panel features:**
- Draggable title bar
- Mouse wheel scroll with optional scrollbar
- Sort order control (z-layering between panels)
- Per-frame update callback for animations and live data
- Rounded corners, borders, opacity via Pro API

---

## Documentation

| File | Audience |
|---|---|
| `BasicAPI.md` | All modders — elements, callbacks, panel setup |
| `ProAPI.md` | Advanced — raw pointers, S{} styles, custom VEs |

---

## Known Limitations

- Framework UI renders **below** the game's native Canvas during scene loading screens.  
  This is a Unity architecture limitation — UI Toolkit and Canvas have separate render stacks.
- `SetScaleMode` on images is pending — Unity 6 changed from `unityBackgroundScaleMode` to a struct-based `backgroundSize` API.
- `SetSize` on Label/Button does not reflow elements below (absolute positioning).

---

## File Structure
```
_CMS2026_UITK_Framework/
├── FrameworkPlugin.cs   — MelonMod entry point
├── FrameworkAPI.cs      — Public API
├── UIRuntime.cs         — IL2CPP type resolution and helpers
├── UIElements.cs        — S{} style helper class
├── UIPanelBuilder.cs    — UIPanel and all AddXxx methods
├── UIHandles.cs         — Handle classes for dynamic updates
└── UIKitUpdater.cs      — MonoBehaviour frame ticker
```

---

## Version History

### 0.1.0
- Initial release
- Panel system with scroll, drag, sort order
- Elements: Label, Button, Toggle, Slider, ProgressBar, TextInput, ColorPicker, Dropdown, Image
- Auto hover/press feedback on buttons
- WireHover for labels and custom VEs
- Border, radius, font size styling on Label and Button
- Background image with tint support
- Raw pointer access via GetRawPtr() on all handles
- Public UIRuntime helpers: WrapVE, GetStyle, NewVE, GetPtr