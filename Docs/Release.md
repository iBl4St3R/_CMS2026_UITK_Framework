# Publikacja (Nexus i GitHub)

> Strona Nexusa: Car Mechanic Simulator 2027 → UITK_Framework (publiczna, MIT). GitHub: https://github.com/iBl4St3R/_CMS2026_UITK_Framework. Uwaga ze strony: po wydaniu pełnej gry framework przestanie wspierać demo.

## Historia wydań

**0.3.1** (port na 2027 w kodzie: `MelonGame` 2027; niezacommitowane 2026-11-20). Wcześniejsze wersje: changelog na Nexusie (agent go nie widział; uzupełnić od użytkownika).

## Checklista wydania

1. Wersja w `MelonInfo` = `FrameworkPlugin.Version` = README = Nexus (+ nagłówki `BasicAPI.md`/`ProAPI.md`, jeśli zmieniało się API).
2. `MelonGame` 2027 (obie nazwy), `MelonPriority(-100)`, nazwa assembly bez zmian.
3. `bash Scripts/_build_check.sh` bez błędów, build Release; w paczce tylko `_CMS2026_UITK_Framework.dll`.
4. **Wpływ na mody:** każda zmiana API lub zachowania sprawdzona względem `Docs/Design/contract.md`; mody użytkownika przebudowane i sprawdzone, jeśli trzeba.
5. Brak tymczasowych flag (lista niżej).
6. README i opis: bez myślników w zdaniach, changelog dla moderów (co nowe, co `[Obsolete]`).

## Sprawdzić przed wydaniem (tymczasowe rzeczy w kodzie)

(pusto)
