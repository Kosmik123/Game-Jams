# Zasady projektu (dla Claude Code)

Gra w Unity (wersja: `ProjectSettings/ProjectVersion.txt`), C#. Build i deploy lokalnie, publikacja na itch.io przez butler.
Odpowiadaj po polsku; kod, nazwy klas i zmiennych pisz po angielsku.

## Komendy (PowerShell, z katalogu głównego projektu)
- Build: `.\Scripts\build.ps1 -Targets Windows` (opcje: Windows, Linux, WebGL)
- Testy: `.\Scripts\test.ps1` (EditMode) lub `.\Scripts\test.ps1 -Platform PlayMode`
- Deploy (próba bez wysyłania): `.\Scripts\deploy.ps1 -DryRun`

## Pętla pracy po zmianie kodu C#
1. Uruchom build lub testy.
2. Przy porażce przeczytaj `Logs\build-<Platforma>.log` lub `Logs\test-<tryb>.xml` (szukaj `error CS`).
3. Popraw i powtórz, aż przejdzie.

## Reguły
- Edytor Unity musi być **zamknięty** podczas skryptów (jedna instancja na projekt). Jeśli jest otwarty, poproś użytkownika o zamknięcie.
- **Nie uruchamiaj `deploy.ps1` bez `-DryRun`**, dopóki użytkownik wyraźnie o to nie poprosi (to publikuje grę na itch.io).
- Nie commituj: `Scripts/config.ps1`, `Builds/`, `Library/`, `Logs/`, kluczy API ani haseł.
- Nie wymyślaj GUID-ów i nie edytuj ręcznie plików `.meta`, scen ani prefabów (YAML), chyba że to konieczne i uzgodnione. Nowe pliki w `Assets/` dostaną `.meta` przy następnym otwarciu edytora.
- Nie zmieniaj wersji Unity ani nie dodawaj/nie usuwaj pakietów (`Packages/manifest.json`) bez pytania.
- Kod edytorowy umieszczaj w folderach `Editor/`.
- Commity: małe, z opisem co i dlaczego.

## Struktura
- `Assets/Editor/BuildScript.cs` - punkty wejścia buildów z CLI
- `Scripts/` - skrypty PowerShell (build, test, deploy, config)
- `docs/` - instrukcje (SETUP.md, DEPLOY.md)

## Dokumentacja
Stosuj generalne zasady zapisane w pliku @BIPOLAR_RULES.md.
Przy deployu, problemach z buildem lub WebGL przeczytaj najpierw @docs/DEPLOY.md.
Przy pierwszej konfiguracji środowiska: @docs/SETUP.md.
