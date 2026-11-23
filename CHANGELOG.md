# Changelog

## 0.4.0

Only new API was added. Nothing was removed or renamed, so mods built for 0.3.1 keep working.

New Features
* Sprites as backgrounds: `UIRuntime.SetBackgroundSprite(ve, sprite)` and `UIImageHandle.SetSprite(sprite)`. Only the sprite rect is drawn, so icons from game atlases work.
* Fonts: `UIRuntime.FindFont(name)`, new overload `S.Font(style, Font)` and `UIPanel.SetDefaultFont(font)` for elements added after the call. `S.Font(style)` works as before.
* Layout helpers in `S`: `FlexDirection`, `FlexGrow`, `FlexShrink`, `FlexWrap`, `JustifyContent`, `AlignItems`, `Margin` (plus Left/Right/Top/Bottom), `WidthPercent`, `HeightPercent`, `MinWidth`, `MaxWidth`, `MaxHeight`.
* `UIPanel.IsTextInputFocused` tells you when a text field in the panel has focus, so your hotkeys can ignore typing.

Bug Fixes
* `S.BgScaleMode` and `SetScaleMode` did nothing before. They now work (ScaleToFit = contain, ScaleAndCrop = cover, StretchToFill = fill). If your mod called them, check that your images still look right.
* Prompt icons (info, warning, error) turned into plain colored circles after loading a save. Icon textures are now kept across scene changes.

Notes for mod authors
* Reflection users: `S.Font` now has two overloads, so look it up with explicit parameter types.

## 0.3.1

* Updated the game name in MelonLoader so the mod loads correctly under the new title (Car Mechanic Simulator 2027).

## 0.3.0

New Features & Systems
* World Space UI: added support for rendering UI directly onto object meshes in the game world.
* Prompt System: added a system for displaying and managing UI prompts.
* Keybindings UI: added a system for displaying and managing a keybinding shortcuts panel.

Fixes & Improvements
* Engine: minor engine fixes and stability improvements.

## 0.2.2

* API unchanged, fully backwards compatible. Details on GitHub.

## 0.2.1

New Features & Core Changes
* Centralized Cursor Management: introduced a new `CursorManager` to globally handle cursor visibility and game input locking.
* Reference Counting System: replaced individual mod/panel UI blocking logic with a centralized reference counting system. This ensures seamless integration and prevents input conflicts when multiple UI layers or mods are active simultaneously.

## 0.2.0

Breaking: `UIPanel.Build()` is now public and returns `this` for chaining.

New Features
* Title bar buttons: `AddTitleButton()` adds custom buttons to the panel title bar (right aligned, right to left). Call before `Build()`.
* Dropdown API: `AddDropdown()` in `UIRowBuilder` with smart open/close overlay that is not clipped by the panel viewport.
* Fake placeholder: `SetFakePlaceholder()` on text inputs simulates native placeholder behavior (gray hint, clears on focus, restores on blur).
* `SetDraggable(bool)`: enable or disable title bar dragging at runtime.
* Panel resize command: console command to resize a panel at runtime.
* Console logging: framework logs routed to SimpleConsole if present.

Changes
* `UIPanel.Build()` is now public and accepts a `sortOrder` parameter.
* `FrameworkAPI.CreatePanel()` now correctly calls `Build(sortOrder)`.
* `UIRuntime` core types (`VisualElementType`, `LabelType`, etc.) changed from internal to public.
* `UIPanel.AddRow()` passes the panel reference and row Y position to `UIRowBuilder`. Required for dropdown overlay positioning.
* Title bar rebuilt: label and buttons now share a single bar `VisualElement` instead of floating independently.
* `AddOverlayToPanel()`, `GetScrollY()`, `RegisterDropdownHandle()` marked internal for framework only use.

Bug Fixes
* `CreatePanel()` in `FrameworkAPI` no longer silently skips `Build()`.
* Title label no longer overlaps control buttons on narrow panels.

## 0.1.0

* Initial release (beta).
