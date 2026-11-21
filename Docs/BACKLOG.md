# Backlog

> Otwarte znaleziska, ograniczenia i pomysły. Zrobione przenoś do `Docs/History/backlog-done.md` (z datą). Źródło początkowe: README (Known Limitations), praca nad GlideRail 0.2.0 i przegląd kodu (2026-11-20). **Wszystko poniżej to informacje, nie zadania:** realizacja tylko na polecenie użytkownika.

## Znane ograniczenia (z README)

1. UI frameworka renderuje się pod natywnym Canvasem gry na ekranach ładowania.
3. `SetSize` na Label/Button nie przelicza układu elementów niżej (pozycjonowanie absolutne).

## Znaleziska

4. **Licznik `CursorManager` rozjeżdża się** (opis w `Docs/Design/contract.md`): panel rodzi się widoczny bez `Request()`. Poprawka w frameworku wymaga koordynacji z GlideRail (kompensuje) oraz NGH, OXL, BlastCoreOS (zdarzenia). Opcja bez łamania: nowy parametr `Build(sortOrder, startHidden: false)` albo flaga globalna, stare zachowanie domyślnie.
5. **Dropdown w rzędzie zawsze otwiera się w dół.** Pomysł: automatyczny kierunek w górę, gdy brak miejsca pod spodem (zmiana zachowania: opisać w Release).
7. **Brak schowka w API.** GlideRail kopiuje przez `GUIUtility.systemCopyBuffer` (refleksja). Kandydat na helper.
9. **Czcionka nie ma emoji:** w GlideRail 🎬 💾 📂 rysowały się jako □. Dopisane do `BasicAPI.md` (Known Limitations) w 0.4.0.
10. **`UnityEngine.ScaleMode` jest w `IMGUIModule`**, którego nie ma w referencjach REPL konsoli ani w `GlideRail/Scripts/_check.sh`. Skrypt REPL wołający `SetScaleMode` musi iść refleksją (wzór: `Samples/try_api040.cs`).
13. **GlideRail może przejść na `UIPanel.IsTextInputFocused`** (0.4.0) zamiast własnej kopii logiki w `GlideUi.cs`. Tylko w repo GlideRail, przy jego następnej wersji.
11. **`SetDefaultFont` po `Build`** nie zmienia już istniejących elementów (tytuł, wcześniejsze `Add…`). Jeśli BlastCoreOS będzie potrzebował zmiany w locie: osobna metoda przechodząca po drzewie.
12. **Harness REPL dla frameworka:** `GlideRail/Scripts/_check.sh` kompiluje z DLL z `Mods/`, więc nowego API nie sprawdzi przed buildem w VS. Na 0.4.0 użyta tymczasowa kopia w scratchpadzie z podmienioną ścieżką; warto mieć `Scripts/_check.sh` we frameworku (wymaga `<Compile Remove="Scripts/**" />` w csproj, decyzja użytkownika).

## Pytania do użytkownika (zanim agent coś zmieni)

- Czy framework ma zostać w 100% wstecznie zgodny (tylko dodawanie API), czy planowana jest wersja łamiąca (np. 1.0) z listą zmian dla moderów?
- Czy poprawiamy licznik `CursorManager` (pkt 4) i kto aktualizuje mody, które go kompensują?
- Czy przykłady w `Samples/` są aktualne dla 2027?
