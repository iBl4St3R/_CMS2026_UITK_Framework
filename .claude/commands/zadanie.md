Przeczytaj Docs/ZADANIE.md i wykonaj opisane tam zadania zgodnie z CLAUDE.md.

- Pierwsza linia pliku w nawiasach kwadratowych `[ … ]` to wiadomość ode mnie (moimi słowami, co robimy). Czytaj ją pierwszą i nigdy jej nie zmieniaj ani nie usuwaj; resztę pliku tworzysz i aktualizujesz Ty.
- Najpierw krótko wypisz plan: co zrobisz i które pliki ruszysz. Jeśli coś jest niejasne albo wymaga mojej decyzji, zapytaj, zanim zaczniesz zmieniać kod.
- Jeśli zadanie odwołuje się do testu, przeczytaj MelonLoader/Latest.log (ścieżka w CLAUDE.md) grepem po słowach z opisu, po prefiksie moda w logu i numerach goali oraz odpowiedzi UA: / OUA: w Docs/playtestGoals.md.
- Gdy zachowania gry nie da się ustalić z sygnatur, napisz skrypt REPL w Samples/ (zasady: C:\Users\usmat\source\repos\GlideRail\Docs\Research\Repl.md), sprawdź go `bash ../GlideRail/Scripts/_check.sh <ścieżka do skryptu>` i daj mi komendę `runfile` do odpalenia.
- Przed oddaniem zmian w kodzie uruchom `bash Scripts/_build_check.sh`.
- Jeśli zmieniasz publiczne API albo zachowanie, sprawdź użycia w modach z Docs/Design/contract.md i dopisz wpływ w Docs/Release.md.
- Do każdej zmiany zachowania dopisz goal w Docs/playtestGoals.md (zasady na górze pliku).
- Na koniec dopisz w Docs/ZADANIE.md sekcję "## Wynik": zmienione pliki (1 linia na plik), nowe albo zmienione publiczne API (z wpływem na mody z Docs/Design/contract.md) i numery nowych goali.
- Zaktualizuj Docs/BACKLOG.md: odhacz zrobione, dopisz nowe znaleziska.
- Po etapie czekaj na "dalej".
