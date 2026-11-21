# Backlog

> Otwarte znaleziska, ograniczenia i pomysły. Zrobione przenoś do `Docs/History/backlog-done.md` (z datą). Źródło początkowe: README (Known Limitations), praca nad GlideRail 0.2.0 i przegląd kodu (2026-11-20). **Wszystko poniżej to informacje, nie zadania:** realizacja tylko na polecenie użytkownika.

## Znane ograniczenia (z README)

1. UI frameworka renderuje się pod natywnym Canvasem gry na ekranach ładowania.
2. `SetScaleMode` na obrazach i etykietach nie działa (Unity 6 wymaga struktury `backgroundSize`).
3. `SetSize` na Label/Button nie przelicza układu elementów niżej (pozycjonowanie absolutne).

## Znaleziska

4. **Licznik `CursorManager` rozjeżdża się** (opis w `Docs/Design/contract.md`): panel rodzi się widoczny bez `Request()`. Poprawka w frameworku wymaga koordynacji z GlideRail (kompensuje) oraz NGH, OXL, BlastCoreOS (zdarzenia). Opcja bez łamania: nowy parametr `Build(sortOrder, startHidden: false)` albo flaga globalna, stare zachowanie domyślnie.
5. **Dropdown w rzędzie zawsze otwiera się w dół.** Pomysł: automatyczny kierunek w górę, gdy brak miejsca pod spodem (zmiana zachowania: opisać w Release).
6. **Brak API "czy pole tekstowe ma fokus".** GlideRail sprawdza to sam refleksją (`focusController.focusedElement` + nazwa typu IL2CPP), żeby skróty klawiszowe nie odpalały się przy pisaniu. Kandydat na publiczne `UIPanel.IsTextInputFocused`.
7. **Brak schowka w API.** GlideRail kopiuje przez `GUIUtility.systemCopyBuffer` (refleksja). Kandydat na helper.
8. **Wersje w nagłówkach `BasicAPI.md` (0.2.3) i `ProAPI.md` (0.1.0)** starsze niż kod (0.3.1).
9. **Czcionka nie ma emoji:** w GlideRail 🎬 💾 📂 rysowały się jako □. Warto dopisać do dokumentacji dla moderów.

## Pytania do użytkownika (zanim agent coś zmieni)

- Czy framework ma zostać w 100% wstecznie zgodny (tylko dodawanie API), czy planowana jest wersja łamiąca (np. 1.0) z listą zmian dla moderów?
- Czy poprawiamy licznik `CursorManager` (pkt 4) i kto aktualizuje mody, które go kompensują?
- Czy przykłady w `Samples/` są aktualne dla 2027?
