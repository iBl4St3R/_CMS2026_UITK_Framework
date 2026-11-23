[wykonaj wszystkie poprawki do wersji 0.4.0 blast core to byl mod nad ktorym skonczylem pracowac doslownie 5 minut temu teraz pora na aktualizacje frameworka.]

## Zadanie (2026-11-23): domknięcie wydania 0.4.0

- `Docs/Design/contract.md`: BlastCoreOS używa nowego API 0.4.0 (grep 2026-11-23).
- `Docs/Release.md`: poprawić wpływ zmiany `BgScaleMode` (BlastCoreOS woła), zależność BlastCoreOS od 0.4.0, oznaczyć wydanie.
- `README.md`: changelog 0.4.0 dla moderów (bez myślników w zdaniach).
- Build check.

## Wynik


- `Docs/Design/contract.md`: 4 wiersze z API 0.4.0 dla BlastCoreOS, lista nieużywanego jeszcze API, wymaganie BlastCoreOS >= 0.4.0.
- `Docs/Release.md`: 0.4.0 "gotowe do wydania 2026-11-23"; poprawiony wpływ `BgScaleMode` (BlastCoreOS woła 20×); punkt o zależności BlastCoreOS i kolejności wydań.
- `README.md`: sekcja Changelog (0.4.0, 0.3.1), ograniczenie emoji w Known Limitations.
- `BasicAPI.md`: rozklejone dwa punkty w Known Limitations.
- `_build_check.sh`: OK. Kod bez zmian.

## Dodatkowo (2026-11-23): "poprostu zrob poprawke" (ikony promptów jako pełne kółka)

- `UIPrompt.cs` (`LoadResourceTexture`): `tex.hideFlags = HideFlags.HideAndDontSave`, martwa tekstura z cache wczytywana ponownie, ostrzeżenie przy nieudanym `LoadImage`.
- `Docs/Release.md`, `README.md` (changelog), `Docs/playtestGoals.md` (G6). `_build_check.sh`: OK.

## Dodatkowo (2026-11-23): changelog od użytkownika, opis Nexusa

- `CHANGELOG.md` (nowy, root, EN): 0.1.0–0.4.0 z tekstu użytkownika (myślniki zamienione na dwukropki).
- `README.md`: link do CHANGELOG zamiast 0.3.1, gra 2027 (tekst i badge), instalacja z folderem ikon, bez myślników.
- `Docs/NexusOpis.md`: wersja 0.4.0, "What's new in 0.4.0", info dla graczy BlastCoreOS, instalacja z folderem ikon, nowe możliwości, Known Limitations (usunięte SetScaleMode, dodane emoji), bez myślników, badge changelogu → CHANGELOG.md.
- `Docs/Release.md`: gdzie jest changelog; checklista: paczka musi mieć folder `resources` z ikonami.
