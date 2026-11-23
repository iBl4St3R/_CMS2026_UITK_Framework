# Publikacja (Nexus i GitHub)

> Strona Nexusa: Car Mechanic Simulator 2027 → UITK_Framework (publiczna, MIT). GitHub: https://github.com/iBl4St3R/_CMS2026_UITK_Framework. Uwaga ze strony: po wydaniu pełnej gry framework przestanie wspierać demo.

## Historia wydań

**0.4.0** (gotowe do wydania 2026-11-23, kod z 2026-11-21; zlecenie BlastCoreOS F1–F5; Nexus i GitHub release: do zrobienia przez użytkownika). Tylko dodawanie API. Porównanie publicznego API starej (`Mods/`, 0.3.1) i nowej DLL: 0 usuniętych lub zmienionych, 23 nowe.

- Nowe: `UIRuntime.SetBackgroundSprite`, `UIRuntime.FindFont`, `UIImageHandle.SetSprite`, `UIPanel.SetDefaultFont`, `UIPanel.IsTextInputFocused`, `S.Font(style, Font)`, `S.FlexDirection/FlexGrow/FlexShrink/FlexWrap/JustifyContent/AlignItems`, `S.Margin` (+ Left/Right/Top/Bottom), `S.WidthPercent/HeightPercent`, `S.MinWidth/MaxWidth/MaxHeight`.
- **Zmiana zachowania:** `S.BgScaleMode` i `SetScaleMode` (obraz, etykieta) wcześniej nic nie robiły, teraz ustawiają `backgroundSize`, `backgroundPositionX/Y` (center) i `backgroundRepeat` (no-repeat). Wpływ: grep 2026-11-21: nikt ich nie wołał. Grep 2026-11-23: woła tylko BlastCoreOS (`S.BgScaleMode` 20×), pisany już pod nowe zachowanie, więc wpływ zero. Obce mody, które wołały to "na zapas", zobaczą teraz contain/cover zamiast domyślnego rozciągania.
- **Refleksja:** `S.Font` ma teraz dwa przeciążenia, więc `GetMethod("Font")` bez typów rzuci `AmbiguousMatchException`. Grep 2026-11-21 i 2026-11-23: żaden mod tak nie woła (BlastCoreOS woła `S.Font(style, font)` twardą referencją). Opisane w `ProAPI.md`.
- Panele bez `SetDefaultFont` zachowują się jak dotąd: wywołania `S.Font(s)` w `UIPanel`/`UIRowBuilder` idą przez `ApplyFont`, który bez fontu panelu woła to samo `S.Font(s)`.
- Nowe typy UI Toolkit rozwiązywane w osobnym `try` (`ResolveLayoutTypes`): brak typu wyłącza tylko nowe helpery, nie cały framework.
- Wersja 0.4.0: `MelonInfo`, `FrameworkPlugin.Version`, badge w README (był 0.2.3), nagłówki `BasicAPI.md`/`ProAPI.md`. Nexus: zaktualizować przy wydaniu.
- Test w grze 2026-11-21: G1–G5 OK.
- **Zależność:** BlastCoreOS (grep 2026-11-23) używa `SetBackgroundSprite`, `FindFont`, `SetDefaultFont`, `S.Font(style, font)`, `S.BgScaleMode`, flex, marginesów i `HeightPercent`; wpisane do `contract.md`. Ma twardą referencję do DLL, więc na 0.3.1 nie wstanie (`MissingMethodException`). Wydać framework 0.4.0 przed BlastCoreOS albo razem z nim, a na stronie BlastCoreOS podać wymaganie `UITK Framework >= 0.4.0`. Pozostałe mody (GlideRail, NGH, TheBucket, OXL) nie używają nic z 0.4.0 i działają na obu wersjach.
- **Poprawka (2026-11-23): ikony promptów jako pełne kółka.** `UIPromptManager.LoadResourceTexture` trzymał `Texture2D` w cache bez `hideFlags`; po zmianie sceny Unity ją niszczył, `iconTex != null` dawało false i rysował się zastępczy kolorowy krąg. Teraz `HideFlags.HideAndDontSave` + ponowne wczytanie martwej tekstury z cache + ostrzeżenie przy nieudanym `LoadImage`. API bez zmian; naprawia prompty BlastCoreOS, konsoli i `ShowTextureFromResources`. Goal G6.
- README: sekcja Changelog z 0.4.0 (2026-11-23); ten sam tekst do opisu/changelogu na Nexusie.

**0.3.1** (port na 2027 w kodzie: `MelonGame` 2027; commit `e78a32a`). Pełny changelog wszystkich wersji (0.1.0–0.4.0, od użytkownika 2026-11-23): `CHANGELOG.md` w roocie, po angielsku; README i opis Nexusa (`Docs/NexusOpis.md`, BBCode) do niego linkują. Nowa wersja = wpis w `CHANGELOG.md` + sekcja "What's new" w `Docs/NexusOpis.md`.

## Checklista wydania

1. Wersja w `MelonInfo` = `FrameworkPlugin.Version` = README = Nexus (+ nagłówki `BasicAPI.md`/`ProAPI.md`, jeśli zmieniało się API).
2. `MelonGame` 2027 (obie nazwy), `MelonPriority(-100)`, nazwa assembly bez zmian.
3. `bash Scripts/_build_check.sh` bez błędów, build Release; w paczce `_CMS2026_UITK_Framework.dll` **oraz folder `_CMS2026_UITK_Framework/resources/` z `icon_info.png`, `icon_warning.png`, `icon_error.png`** (`UIPrompt` czyta je z `Mods/_CMS2026_UITK_Framework/resources`; bez nich prompty mają pełne kółka zamiast ikon). Ikon nie ma w repo: źródło to folder `Mods` gry.
4. **Wpływ na mody:** każda zmiana API lub zachowania sprawdzona względem `Docs/Design/contract.md`; mody użytkownika przebudowane i sprawdzone, jeśli trzeba.
5. Brak tymczasowych flag (lista niżej).
6. README i opis: bez myślników w zdaniach, changelog dla moderów (co nowe, co `[Obsolete]`).

## Sprawdzić przed wydaniem (tymczasowe rzeczy w kodzie)

(pusto)
