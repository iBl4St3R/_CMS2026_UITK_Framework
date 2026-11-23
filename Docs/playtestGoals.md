# Goale do sprawdzenia w grze

## Zasady

- Jeden goal = jedno lub dwa zdania: co zrobić w grze i co ma się stać.
- `UA:` na końcu = potrzebna odpowiedź użytkownika (wpisuje ją po dwukropku). `OUA:` = wystarczy `MelonLoader/Latest.log` (agent sprawdza grepem; użytkownik może dopisać uwagi).
- Numeracja ciągła od G1, numerów nie używamy ponownie. Nowy goal dostaje "Następny numer" i podbija go.
- Goal działa: usuń go, wniosek do `Docs/Design/` albo `Docs/Research/`. Nie działa: poprawka z nowym goalem albo wpis w `Docs/BACKLOG.md`. Brak danych: zostaje.
- Przed testem: zamknij grę, zbuduj w VS (DLL kopiuje się sam), uruchom grę.

Następny numer: **G7**

G1–G5 z 0.4.0 zaliczone 2026-11-21 (wniosek w `Docs/History/log.md`).

- **G6.** Wczytaj zapis (zmiana sceny), potem wywołaj hint Info lub Warning (np. akcja w BlastCoreOS albo ostrzeżenie konsoli). Ikona ma być konturem „i” lub trójkątem, nie pełnym kółkiem; w logu brak `[UIPrompt]` ostrzeżeń. UA:
