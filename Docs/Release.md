# Publikacja (Nexus i GitHub)

> Strona Nexusa: Car Mechanic Simulator 2027 → UITK_Framework (publiczna, MIT). GitHub: https://github.com/iBl4St3R/_CMS2026_UITK_Framework. Uwaga ze strony: po wydaniu pełnej gry framework przestanie wspierać demo.

## Historia wydań

**0.4.0** (niewydane, 2026-11-21; zlecenie BlastCoreOS F1–F5). Tylko dodawanie API. Porównanie publicznego API starej (`Mods/`, 0.3.1) i nowej DLL: 0 usuniętych lub zmienionych, 23 nowe.

- Nowe: `UIRuntime.SetBackgroundSprite`, `UIRuntime.FindFont`, `UIImageHandle.SetSprite`, `UIPanel.SetDefaultFont`, `UIPanel.IsTextInputFocused`, `S.Font(style, Font)`, `S.FlexDirection/FlexGrow/FlexShrink/FlexWrap/JustifyContent/AlignItems`, `S.Margin` (+ Left/Right/Top/Bottom), `S.WidthPercent/HeightPercent`, `S.MinWidth/MaxWidth/MaxHeight`.
- **Zmiana zachowania:** `S.BgScaleMode` i `SetScaleMode` (obraz, etykieta) wcześniej nic nie robiły, teraz ustawiają `backgroundSize`, `backgroundPositionX/Y` (center) i `backgroundRepeat` (no-repeat). Wpływ: grep 2026-11-21 w modach z `contract.md`: nikt ich nie woła, więc zero.
- **Refleksja:** `S.Font` ma teraz dwa przeciążenia, więc `GetMethod("Font")` bez typów rzuci `AmbiguousMatchException`. Grep 2026-11-21: żaden mod tak nie woła. Opisane w `ProAPI.md`.
- Panele bez `SetDefaultFont` zachowują się jak dotąd: wywołania `S.Font(s)` w `UIPanel`/`UIRowBuilder` idą przez `ApplyFont`, który bez fontu panelu woła to samo `S.Font(s)`.
- Nowe typy UI Toolkit rozwiązywane w osobnym `try` (`ResolveLayoutTypes`): brak typu wyłącza tylko nowe helpery, nie cały framework.
- Wersja 0.4.0: `MelonInfo`, `FrameworkPlugin.Version`, badge w README (był 0.2.3), nagłówki `BasicAPI.md`/`ProAPI.md`. Nexus: zaktualizować przy wydaniu.
- Test w grze 2026-11-21: G1–G5 OK. BlastCoreOS zaczyna używać nowego API (dodać do `contract.md`, gdy wejdzie do jego kodu).

**0.3.1** (port na 2027 w kodzie: `MelonGame` 2027; commit `e78a32a`). Wcześniejsze wersje: changelog na Nexusie (agent go nie widział; uzupełnić od użytkownika).

## Checklista wydania

1. Wersja w `MelonInfo` = `FrameworkPlugin.Version` = README = Nexus (+ nagłówki `BasicAPI.md`/`ProAPI.md`, jeśli zmieniało się API).
2. `MelonGame` 2027 (obie nazwy), `MelonPriority(-100)`, nazwa assembly bez zmian.
3. `bash Scripts/_build_check.sh` bez błędów, build Release; w paczce tylko `_CMS2026_UITK_Framework.dll`.
4. **Wpływ na mody:** każda zmiana API lub zachowania sprawdzona względem `Docs/Design/contract.md`; mody użytkownika przebudowane i sprawdzone, jeśli trzeba.
5. Brak tymczasowych flag (lista niżej).
6. README i opis: bez myślników w zdaniach, changelog dla moderów (co nowe, co `[Obsolete]`).

## Sprawdzić przed wydaniem (tymczasowe rzeczy w kodzie)

(pusto)
