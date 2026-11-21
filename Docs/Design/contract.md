# Kontrakt publicznego API: kto czego używa

> Stan 2026-11-20, z grepa po repo modów użytkownika. Obce mody z Nexusa mogą używać więcej, dlatego zasada z `CLAUDE.md`: **żadnych zmian sygnatur publicznych**. Ta lista mówi, co na pewno zepsuje mody użytkownika. Odświeżaj grepem przed każdą zmianą API.

## Użycie w modach użytkownika

| Element | GlideRail | NPCGarageHelper | TheBucket | CMS2026_OXL | BlastCoreOS |
|---|---|---|---|---|---|
| `FrameworkAPI.IsReady` | ✔ | ✔ | ✔ | ✔ | ✔ |
| `FrameworkAPI.CreatePanel` | | | | ✔ | |
| `FrameworkAPI.DestroyPanel` | | ✔ | ✔ | | |
| `UIPanel.Create` + `Build` | ✔ | ✔ | ✔ | ✔ | ✔ |
| `UIPanel.BuildWorldSpace` | | | | | ✔ |
| `AddTitleButton` | ✔ | ✔ | ✔ | ✔ | |
| `AddRow` + `UIRowBuilder.*`, `AddLabel/AddButton/AddTextInput/AddSeparator/AddSpace`, uchwyty `UI*Handle` | ✔ | ✔ | ✔ | ✔ | ✔ |
| `AddButtonToContainer`, `AddLabelToContainer`, `AddOverlayToPanel` | ✔ | | | ✔ | ✔ |
| `WireClick`, `WireHover` | | | | ✔ | ✔ |
| `SetUpdateCallback` | ✔ | | | ✔ | ✔ |
| `ClearFocus`, `GetRootVE` | ✔ | | | | ✔ |
| `UIRuntime.NewVE/WrapVE/GetStyle/GetPtr/AddChild/VisualElementType` | ✔ | | | ✔ | ✔ |
| `UIRuntime.LabelType/ButtonType/TextFieldType/UEAsm`, `SetBackgroundImage` | | | | ✔ | ✔ |
| `UIRuntime.SetBackgroundRenderTexture` | | | | | ✔ |
| `S.*` (Position, Left, Top, Width, Height, BgColor, Color, FontSize, TextAlign, Overflow, Border*) | ✔ | ✔ (Border*, BgColor) | ✔ (Border*, BgColor) | ✔ (+Display, Font, Padding, WhiteSpaceNormal) | ✔ (+Bottom, Right, Scale, FontBold, TextOutline*, TextShadow) |
| `CursorManager.Request/Release` | ✔ (Request po Build) | | | | ✔ |
| `CursorManager.OnCursorShow/OnCursorHide` | | ✔ | | ✔ | ✔ |
| `UIPrompt.Show` | | | | | ✔ |
| `UIShortcutBar.Add/Remove/Show/Hide` | | | | | ✔ |

Refleksja po nazwie (bez twardej referencji): przykłady w `BasicAPI.md` (`GetType("CMS2026UITKFramework.FrameworkAPI")`), więc **pełne nazwy typów też są kontraktem**.

## Zachowania, na których mody polegają

- **`CursorManager`:** panel po `Build()` jest widoczny (`_visible = true`), ale `Build` nie woła `Request()`; `SetVisible(false)` i `Destroy()` na widocznym panelu wołają `Release()`. Skutek: ostrzeżenie `Release() called with count already 0` i rozjechany licznik. GlideRail 0.2.0 kompensuje (`Request()` zaraz po `Build`). NGH, OXL i BlastCoreOS reagują na zdarzenia `OnCursorShow/OnCursorHide` (przełączają `GameMode`), więc zmiana licznika wpływa na tryb gry w tych modach.
- **Dropdown w rzędzie** otwiera listę w dół, w roocie panelu: przy panelach przy dolnej krawędzi ekranu lista wychodzi poza ekran (GlideRail dlatego nie używa dropdownów).
- **Każdy panel ma własne `PanelSettings`** (osobny `IPanel` i `focusController`): fokus jest per panel, nie globalny (`ClearFocus` czyści tylko swój panel).
- **Kontenery:** `AddOverlayToPanel` dodaje do elementu panelu (współrzędne względem panelu z paskiem tytułu, `TitleH = 24`), wiersze `AddRow` są w obszarze treści (`Pad = 6`, start pod tytułem).
