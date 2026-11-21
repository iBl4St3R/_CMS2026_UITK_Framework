[ tu wpisz swoimi słowami, co robimy. do tego czasu agent niczego nie implementuje ]

## Zlecenie od BlastCoreOS (agent `blastcoreos-df`, 2026-11-21)

Źródło: wiadomość od agenta BlastCoreOS (overhaul UI, `C:\Users\usmat\source\repos\BlastCoreOS\Docs\Design\ui.md` sekcja 8). **Czeka na potwierdzenie użytkownika w pierwszej linii.** Tylko dodawanie API, 100% wstecznie zgodne (BlastCoreOS ma twardą referencję do DLL).

Priorytet: F2, F3, F1, F4, F5. Po każdym kawałku REPL albo goal do sprawdzenia w grze.

- **F2. Sprite jako tło:** `UIRuntime.SetBackgroundSprite(object ve, Sprite sprite)` (`Background.FromSprite`) + `UIImageHandle.SetSprite(Sprite sprite)`. Ikony z atlasów gry (`sprite.texture` dałoby cały atlas).
- **F3. Wybór fontu:** przeciążenie `S.Font(object style, Font font)` (`FontDefinition.FromFont`) + `UIRuntime.FindFont(string name)` (po `name` w `Resources.FindObjectsOfTypeAll<Font>()`, null gdy brak) + opcjonalnie `UIPanel.SetDefaultFont(Font)` dla nowych elementów. Cel: `Rubik-VariableFont_wght`. `S.Font(style)` bez zmian.
- **F1. Naprawa `S.BgScaleMode` / `UIImageHandle.SetScaleMode`** (BACKLOG pkt 2): `backgroundSize` + `backgroundPositionX/Y` center + `backgroundRepeat` no-repeat przez refleksję. Tryby: ScaleToFit (contain), ScaleAndCrop (cover), StretchToFill. Sygnatury bez zmian. Zmiana zachowania (dziś nie działa): grep 2026-11-21: żaden mod z `contract.md` nie woła `SetScaleMode`/`BgScaleMode`, wpływ zerowy; opisać w `Release.md`.
- **F4. Flex i procenty w `S`:** `FlexDirection(style, bool row)`, `FlexGrow`, `FlexShrink`, `FlexWrap(style, bool)`, `JustifyContent(style, string)` (FlexStart/Center/FlexEnd/SpaceBetween/SpaceAround), `AlignItems(style, string)` (FlexStart/Center/FlexEnd/Stretch), `Margin` + `MarginLeft/Right/Top/Bottom`, `WidthPercent`, `HeightPercent`, `MinWidth`, `MaxWidth`, `MaxHeight`.
- **F5. `public bool UIPanel.IsTextInputFocused`** (BACKLOG pkt 6; wzór: refleksja w GlideRail, `focusController.focusedElement` + nazwa typu IL2CPP).

### Plan (do akceptacji)

- Pliki: `UIRuntime.cs` (typy `Background`, `FontDefinition`, `BackgroundSize`/`BackgroundPosition`/`BackgroundRepeat`, `Length` %), `UIElements.cs` (nowe `S.*`), `UIHandles.cs` (`SetSprite`, `SetScaleMode`), `UIPanelBuilder.cs` (`SetDefaultFont`, `IsTextInputFocused`), `ProAPI.md`/`BasicAPI.md`, `Docs/Release.md`, `Docs/playtestGoals.md`, `Docs/BACKLOG.md`.
- Nazwy członków Unity 6 (IL2CPP) sprawdzam w `cms2027assembly` przed kodem.
- Po zakończeniu: odpowiedź do `blastcoreos-df` z listą sygnatur i wersją.

### Pytania do użytkownika

1. Potwierdzasz zlecenie (wpisz je w pierwszej linii)?
2. Wersja po zmianie: **0.4.0** (nowe API) czy 0.3.2?
3. `UIPanel.SetDefaultFont` też robimy (opcjonalne w zleceniu)? BlastCoreOS woli tak (setki elementów przez AddLabel/AddButton); minimum to `S.Font(style, Font)` + `UIRuntime.FindFont`. BlastCoreOS popiera też 0.4.0.

## Wynik

(pusto)
