[potwierdzam wszystko, mozesz wszystko naraz zrobic. co do metody i innych rzeczy trzymamy stare kompatybilnosci by stare mody dzialaly jezeli chcemy cos dodac to dodajemy obok tego co juz dziala by nie wywalic innych modow.]

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
- Nazwy członków Unity 6 (IL2CPP) sprawdzone w interop DLL gry (`cms2027assembly` ma tylko Assembly-CSharp), narzędziem w scratchpadzie.
- Po zakończeniu: odpowiedź do `blastcoreos-df` z listą sygnatur i wersją.

### Decyzje użytkownika (2026-11-21)

Wszystko naraz, wersja 0.4.0, z `SetDefaultFont`. Stare zachowanie zostaje, nowe rzeczy obok.

## Wynik

Pliki:
- `UIRuntime.cs`: typy Length/Flex/Wrap/Background* w osobnym `ResolveLayoutTypes` (nie blokuje startu), `SetBackgroundSprite`, `FindFont` (cache), wewn. `MakeFontDefinition`, `IsTextInputFocusedIn`.
- `UIElements.cs`: `BgScaleMode` działa; nowe `S.Font(s, Font)`, flex, marginesy, procenty, min/max.
- `UIHandles.cs`: `UIImageHandle.SetSprite`.
- `UIPanelBuilder.cs`: `IsTextInputFocused`, `SetDefaultFont`, wewn. `ApplyFont`; stare `S.Font(x)` → `ApplyFont(x)` (bez fontu panelu to samo co wcześniej).
- `UIRowBuilder.cs`: `S.Font(x)` → `UIPanel.ApplyFont(_panel, x)`.
- `FrameworkPlugin.cs`: wersja 0.4.0 (`MelonInfo` + `Version`).
- `README.md`, `BasicAPI.md`, `ProAPI.md`: wersja 0.4.0, nowe API, usunięte ograniczenie SetScaleMode.
- `Samples/try_api040.cs`: skrypt REPL testu G1–G5 (sprawdzony kompilacją względem nowej DLL).
- `Docs/Release.md`, `Docs/playtestGoals.md`, `Docs/BACKLOG.md`.

Publiczne API: 23 nowe członki, 0 usuniętych/zmienionych (diff DLL). Zmiana zachowania tylko `BgScaleMode`/`SetScaleMode` (nikt z `contract.md` nie woła). Lista w `Docs/Release.md`.

Goale: G1–G5. `_build_check.sh`: OK.
