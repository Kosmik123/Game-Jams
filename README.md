# Zestaw startowy: Unity + lokalny build + deploy na itch.io

Wszystko działa lokalnie na Windowsie: Unity w trybie `-batchmode` buduje grę, `butler` wysyła ją na itch.io.
Nie potrzeba GitHub Actions ani licencji Unity w CI.

## Zawartość
| Plik | Do czego |
|---|---|
| `Assets/Editor/BuildScript.cs` | Metody buildu wywoływane z CLI (Windows, Linux, WebGL) |
| `Scripts/build.ps1` | Build wybranych platform do `Builds/` |
| `Scripts/test.ps1` | Testy EditMode / PlayMode z CLI |
| `Scripts/deploy.ps1` | Build + `butler push` na itch.io (`-DryRun` do prób) |
| `Scripts/common.ps1` | Wspólne funkcje (wykrywanie Unity, logi) |
| `Scripts/config.example.ps1` | Szablon lokalnej konfiguracji (`config.ps1` jest w `.gitignore`) |
| `CLAUDE.md` | Zasady pracy dla Claude Code w tym projekcie |
| `.gitignore`, `.gitattributes` | Ustawienia gita dla Unity (LFS opcjonalnie) |
| `docs/SETUP.md` | Jednorazowa konfiguracja krok po kroku |
| `docs/DEPLOY.md` | Codzienny deploy, WebGL na itch.io, rozwiązywanie problemów |

## Szybki start
1. Skopiuj **zawartość** tego folderu do katalogu głównego projektu Unity (obok `Assets/` i `ProjectSettings/`).
2. Przejdź przez `docs/SETUP.md`.
3. `.\Scripts\build.ps1 -Targets Windows`, a potem `.\Scripts\deploy.ps1 -DryRun`.
