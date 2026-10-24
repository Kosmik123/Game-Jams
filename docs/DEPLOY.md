# Deploy na itch.io

## Typowy przebieg
```powershell
git status                                # czysto? wszystko zacommitowane?
git tag v0.1.0
.\Scripts\deploy.ps1 -DryRun              # próba
.\Scripts\deploy.ps1                      # build Windows + WebGL i push
```
Wersja na itch.io = tag gita (`git describe --tags --always`) albo wartość `-Version`.

Tylko jedna platforma: `.\Scripts\deploy.ps1 -Targets WebGL`.
Wysłanie gotowego buildu bez przebudowy: `-SkipBuild`.

## Kanały
| Platforma | Kanał | Uwagi |
|---|---|---|
| Windows | `windows` | itch.io sam rozpoznaje platformę po nazwie kanału |
| Linux | `linux` | j.w. |
| WebGL | `webgl` | **raz** zaznacz na stronie gry (Edit game > Uploads): "This file will be played in the browser". Ustaw rozmiar okna |

## WebGL na itch.io
- `BuildScript` wyłącza kompresję WebGL. Kompresja Gzip/Brotli wymaga poprawnych nagłówków i bywa problematyczna na itch.io. Włączaj ją dopiero, gdy podstawowy build działa i rozmiar naprawdę przeszkadza.
- `index.html` musi leżeć w katalogu głównym wysyłanego folderu (`Builds/webgl/`). Tak jest domyślnie.
- Wyłączony Decompression Fallback jest OK przy braku kompresji.

## Rozwiązywanie problemów
| Objaw | Co sprawdzić |
|---|---|
| "Projekt jest otwarty w edytorze" | Zamknij edytor Unity dla tego projektu |
| Nie znaleziono Unity.exe | Wersja z `ProjectVersion.txt` nie jest zainstalowana, albo ustaw `$UnityExe` w `config.ps1` |
| Build kończy się kodem 1 | `Logs\build-<Platforma>.log`, szukaj `error CS` |
| Kod 2 | Brak włączonych scen w Build Settings |
| "target not supported" / brak platformy | Doinstaluj moduł Build Support w Unity Hub |
| `butler` nie znaleziony | Folder z butlerem nie jest w `PATH` (otwórz nowy terminal) |
| Błąd autoryzacji butlera | `butler login` ponownie, albo zmienna `BUTLER_API_KEY` (klucz nie może trafić do repo) |
| WebGL nie uruchamia się w przeglądarce | Kanał nieoznaczony jako "played in the browser", brak `index.html` w rootcie, albo włączona kompresja |
