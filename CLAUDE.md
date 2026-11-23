# _CMS2026_UITK_Framework: instrukcja dla agenta

Biblioteka UI dla modów **Car Mechanic Simulator 2027** (Demo; Unity 6000.3, **IL2CPP**, MelonLoader 0.7.2, net6). Daje modom panele UI Toolkit (Unity 6) przez IL2CPP: fluent API (`FrameworkAPI.CreatePanel` → `AddLabel/AddButton/AddRow/…`), surowe `VisualElement` przez wskaźniki, style `S.*`, prompty, pasek skrótów, znaczniki w świecie. Ładuje się przed innymi modami (`MelonPriority(-100)`). Wersja **0.3.1**, publiczna na Nexusie (MIT), `MelonGame` już na 2027.

**To biblioteka, na której stoją inne mody.** Każda zmiana publicznego API może zepsuć mody użytkownika i obce mody z Nexusa. Zasady kompatybilności niżej są w tym repo najważniejsze.

- **Z użytkownikiem rozmawiaj po polsku.** Krótko i konkretnie.
- **Dokumentacja dla moderów (`README.md`, `BasicAPI.md`, `ProAPI.md`) i logi po angielsku.** Komentarze w kodzie w języku danego pliku. Dokumenty w `Docs/` po polsku.
- **Nie zgaduj.** IL2CPP: typy UI Toolkit są wołane przez refleksję (`UIRuntime`). Nazwy członków Unity sprawdzaj w kodzie albo skryptem REPL.
- **Nazwa "CMS2026" zostaje.** Assembly `_CMS2026_UITK_Framework`, namespace `CMS2026UITKFramework`, nazwa melona i DLL są kontraktem z modami (referencje i refleksja po nazwie). Nie zmieniać.

## Gdzie co jest

| Plik | KB | Co robi |
|---|---|---|
| `FrameworkPlugin.cs` | 10 | `MelonMod`, `Version`, `ActivePanels`, log, rejestracja w konsoli (komendy `ui_list`, `ui_resize`). |
| `FrameworkAPI.cs` | 2 | Publiczny punkt wejścia: `IsReady`, `Version`, `CreatePanel`, `GetPanel`, `DestroyPanel`. |
| `UIPanelBuilder.cs` | **71** | `UIPanel`: budowa panelu (ekran i world space), tytuł, przewijanie, przeciąganie, wszystkie `Add…`, kontenery (`AddButtonToContainer`, `AddLabelToContainer`, `AddOverlayToPanel`, `AddToRoot`), `WireClick`, `WireHover`, widoczność, `ClearFocus`. Czytaj zakresami. |
| `UIRowBuilder.cs` | 19 | `UIRowBuilder`: rzędy wielokolumnowe (`AddLabel/AddButton/AddToggle/AddProgressBar/AddDropdown/AddRaw`). |
| `UIHandles.cs` | 34 | Uchwyty elementów (`UILabelHandle`, `UIButtonHandle`, `UIToggleHandle`, `UISliderHandle`, `UITextInputHandle`, `UIColorPickerHandle`, `UIImageHandle`, `UIProgressBarHandle`, `UIDropdownHandle`). |
| `UIRuntime.cs` | 16 | Refleksja na UI Toolkit: typy (`VisualElementType`, `LabelType`, `ButtonType`, `TextFieldType`…), `NewVE`, `WrapVE`, `GetStyle`, `GetPtr`, `AddChild`, tła z tekstur. |
| `UIElements.cs` | 9 | `S`: setery stylów (pozycja, rozmiar, kolory, czcionka, ramki, `Display`, cień, outline). |
| `CursorManager.cs` | 4 | Licznik żądań kursora (`Request/Release`, zdarzenia `OnCursorShow/OnCursorHide`). |
| `UIPrompt.cs` | 22 | Kolejka promptów (`Show`, `Warning`, `Error`, `ShowTexture`, `KeyIcon`). |
| `UIShortcutBar.cs` | 15 | Pasek skrótów klawiszowych (`Add/Update/Remove/Show/Hide`). |
| `WorldMarker.cs` | 13 | Znaczniki w świecie 3D (`Show/Hide/ClearAll`). |
| `UIKitUpdater.cs` | 1 | Tick paneli co klatkę. |
| `BasicAPI.md`, `ProAPI.md`, `README.md` | | Dokumentacja dla moderów (wersje w nagłówkach bywają starsze niż kod). |
| `Samples/` | | Przykłady (wyłączone z kompilacji w csproj); `Samples/REPL_NO_REF/*.cs` to skrypty REPL bez referencji do frameworka. |
| `Docs/` | | Wiedza dla agenta: `Docs/Design/contract.md` (kto czego używa), `Docs/BACKLOG.md`, `Docs/ZADANIE.md`, `Docs/Release.md`, `Docs/playtestGoals.md`, `Docs/History/`. |
| `Scripts/_build_check.sh` | | Kompilacja w `%TEMP%` bez kopiowania DLL do gry. **Uwaga:** csproj jest w roocie i nie wyklucza `Scripts/`, więc plik `.cs` wrzucony do `Scripts/` trafi do DLL. Skrypty REPL trzymaj w `Samples/` (wykluczone) albo najpierw dodaj `<Compile Remove="Scripts/**" />` (zmiana csproj = za zgodą użytkownika). |

**Nie czytaj i nie zmieniaj:** `bin/`, `obj/`, `.vs/`, `.git/`, `*.sln`, obrazów (`*.png`). `_CMS2026_UITK_Framework.csproj` tylko, gdy zadanie tego wymaga.

### Wiedza wspólna (tylko odczyt, w repo GlideRail)

- `C:\Users\usmat\source\repos\GlideRail\Docs\Research\Ecosystem.md`: gra, loader, mody użytkownika i ich stan w 2027.
- `C:\Users\usmat\source\repos\GlideRail\Docs\Research\Repl.md`: skrypty REPL konsoli.

### Poza repo (tylko odczyt)

`C:\Users\usmat\source\repos\cms2027assembly\` (zrzut gry), repo modów korzystających z frameworka (`GlideRail`, `NPCGarageHelper`, `TheBucket`, `CMS2026_OXL`, `BlastCoreOS`) do sprawdzania wpływu zmian, folder gry `C:\Program Files (x86)\Steam\steamapps\common\Car Mechanic Simulator 2026 Demo\` (`Mods/`, `MelonLoader/Latest.log`).

## Twarde zasady (kompatybilność)

1. **Publiczne API jest kontraktem.** Nie usuwaj i nie zmieniaj sygnatur publicznych typów i członków (realnie używane: `Docs/Design/contract.md`). Nowe parametry tylko opcjonalne na końcu. Zamiast usuwać: `[Obsolete]` + nowa metoda.
2. **Zmiana zachowania istniejącej metody** (licznik `CursorManager`, kierunek dropdownu, kolejność warstw…) = sprawdź mody z `contract.md`, które ją wołają, i opisz wpływ w `Docs/Release.md` przed wydaniem. Mody kompensują dziś niektóre zachowania (GlideRail robi `CursorManager.Request()` po `Build`); poprawka po stronie frameworka bez koordynacji zepsułaby licznik w tych modach.
3. **Nazwa assembly, namespace, `MelonInfo` i `MelonPriority(-100)` bez zmian.**
4. **`MelonGame` 2027** (obie nazwy) zostaje.
5. **Unity API tylko na głównym wątku.** Refleksja UI Toolkit w `UIRuntime`; nowe typy dodawaj tam.
6. **Wersja w jednym miejscu** (`FrameworkPlugin.Version` + `MelonInfo`), zgodna z README i Nexusem.
7. **Dokumentacja razem z API:** nowa publiczna metoda = wpis w `BasicAPI.md` albo `ProAPI.md` w tej samej partii.

## Zasady kodu

- Nowe wywołania i sygnatury w jednej linii; istniejącego formatowania nie przerabiaj. Target jak w csproj (net6.0).
- Zmiany w rozmowie: zmieniona metoda w całości; kilka linii z 2 liniami kontekstu; >2 miejsca = cały plik. Przy pracy agentowej edytuj pliki i wypisz zmiany.
- Pełne ścieżki plików. Logi przez log frameworka, po angielsku.
- Bez myślników w zdaniach w README i opisie Nexusa (zdradzają tekst AI).

## Przebieg pracy

1. Zadanie w `Docs/ZADANIE.md` (komenda `/zadanie`); pierwsza linia to słowa użytkownika. **Bez zadania nic nie implementuj.**
2. Przed oddaniem: `bash Scripts/_build_check.sh`. Potem użytkownik buduje w VS (target `CopyToMods` kopiuje DLL do `Mods`, gra musi być zamknięta).
3. Test: skrypt REPL albo mod korzystający z frameworka; `Latest.log` grepem po `[_CMS2026_UITK_Framework]`. Zmiana zachowania = goal w `Docs/playtestGoals.md`.
4. Po zmianie API lub zachowania: wpis w `Docs/Release.md` (wpływ na mody z `contract.md`).
5. **Git:** nie commituj bez prośby. W repo bywają niezacommitowane zmiany użytkownika: nie nadpisuj ich i nie cofaj.
6. Bez artefaktów. Brak dostępu do pliku = zapytaj.

## Stan projektu

Wersja **0.3.1**, działa w 2027 (log 2026-11-20: ładuje się, panele GlideRail 0.2.0 działają). Port na 2027 (`MelonGame` 2027, wersja 0.3.0 → 0.3.1) zacommitowany w `e78a32a` (2026-11-21); log z 2026-11-21 potwierdza `v0.3.1` na 2027 Demo. Znane ograniczenia i pomysły: `Docs/BACKLOG.md`.

**2026-11-23: wydanie 0.4.0** (API dla BlastCoreOS + poprawka ikon promptów). Szczegóły i otwarte sprawy: `Docs/Release.md` (wpis 0.4.0), goal G6 w `Docs/playtestGoals.md`. Pełny changelog: `CHANGELOG.md`; opis Nexusa (BBCode): `Docs/NexusOpis.md`. Paczka = DLL + folder `_CMS2026_UITK_Framework/resources` z ikonami.
