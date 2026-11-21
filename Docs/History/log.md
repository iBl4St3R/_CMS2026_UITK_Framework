# Dziennik odkryć

> Krótkie wpisy z datą: co ustalono, skąd, gdzie zapisano wniosek. Nowe wpisy na końcu.

## 2026-11-20: setup dokumentacji dla agentów

- Struktura dokumentacji na wzór GlideRail (`CLAUDE.md`, `Docs/`, `.claude/`, `Scripts/_build_check.sh`). Kod nietknięty.
- Framework 0.3.1 działa w 2027 (log 2026-11-20; GlideRail 0.2.0 zbudowany na nim, wszystkie testy zaliczone).
- Z pracy nad GlideRail: licznik `CursorManager` rozjeżdża się (panel rodzi się widoczny bez `Request()`), dropdown w rzędzie otwiera się w dół, czcionka bez emoji, fokus per panel (`Docs/Design/contract.md`, `Docs/BACKLOG.md`).
- Grep użycia API w modach użytkownika: `Docs/Design/contract.md`.

## 2026-11-21: framework 0.4.0 (zlecenie BlastCoreOS F1–F5)

- Nazwy typów Unity 6 sprawdzone w interop DLL gry (zrzut `cms2027assembly` ma tylko Assembly-CSharp): `Background.FromSprite`, `BackgroundSize(BackgroundSizeType)`/`(Length, Length)`, `BackgroundPosition(Keyword)`, `BackgroundRepeat(Repeat, Repeat)`, `Length(float, LengthUnit)`, `FlexDirection`, `Wrap`; `IStyle` ma `backgroundSize`, `backgroundPositionX/Y`, `backgroundRepeat`.
- Test w grze (`Samples/try_api040.cs`, G1–G5 OK; zrzut `BlastCoreOS/Docs/Design/Screens/0.4.0 Test.png`): `FromSprite` rysuje sam wycinek atlasu (`cat_icon_engine` 128x128 z 1024x1024); `FontDefinition.FromFont` z `Rubik-VariableFont_wght` działa w UI Toolkit; contain/cover/100% i flex wrap z procentami działają; fokus `TextField` wykrywany po nazwie typu IL2CPP zawierającej "Text".
- Diff publicznego API DLL 0.3.1 → 0.4.0: 0 usuniętych, 23 nowe.
