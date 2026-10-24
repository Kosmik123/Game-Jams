# Konfiguracja jednorazowa

## 1. Unity
- W Unity Hub zainstaluj edytor w wersji projektu wraz z modułami: **Windows Build Support (Mono/IL2CPP)** i **WebGL Build Support** (oraz Linux, jeśli potrzebny). Bez modułu build danej platformy się nie uda.
- Edit > Project Settings > Editor:
  - **Version Control Mode:** Visible Meta Files
  - **Asset Serialization Mode:** Force Text
- File > Build Settings: dodaj i włącz sceny, które mają wejść do gry. Bez nich `BuildScript` zakończy się kodem 2.
- Ustaw Player Settings: Company Name, Product Name (z niego powstaje nazwa `.exe`), wersję.

## 2. Pliki startowe
Skopiuj zawartość `unity-starter/` do katalogu głównego projektu. Skopiuj `Scripts/config.example.ps1` jako `Scripts/config.ps1` i wpisz `$ItchTarget` (`uzytkownik/gra`).

## 3. Git
```powershell
git init
git add .
git commit -m "Initial commit"
```
- **Backup:** sam lokalny git nie chroni przed awarią dysku. Ustaw prywatne repo na GitHubie (`git remote add origin ...; git push -u origin main`) albo bare repo na drugim dysku (`git init --bare D:\backup\gra.git`).
- **LFS:** tylko przy zdalnym repo i dużych assetach. Wtedy `git lfs install`, odkomentuj reguły w `.gitattributes` **przed** pierwszym commitem assetów (albo `git lfs migrate import --include="*.png,*.psd,..."` później).

## 4. Butler
1. Pobierz butler dla Windows 64-bit ze strony `itch.io/docs/butler/installing.html`.
2. Rozpakuj do `C:\Tools\butler\` i dodaj ten folder do `PATH` (Ustawienia > Zmienne środowiskowe). Otwórz nowy terminal.
3. `butler -V` (sprawdzenie) i `butler login` (jednorazowo, otworzy przeglądarkę).
4. Aktualizacja: `butler upgrade`.

## 5. Strona gry na itch.io
- Utwórz projekt (Dashboard > Create new project). Może być w trybie Draft.
- Adres `https://UZYTKOWNIK.itch.io/GRA` daje `$ItchTarget = "UZYTKOWNIK/GRA"`.
- Upload nie wymaga wcześniejszego tworzenia plików na stronie; kanały powstają przy pierwszym `butler push`.

## 6. Pierwszy test
```powershell
.\Scripts\build.ps1 -Targets Windows     # edytor Unity musi być zamknięty
.\Scripts\deploy.ps1 -DryRun -SkipBuild -Targets Windows
```
Jeśli `--dry-run` nie jest rozpoznane przez Twoją wersję butlera, sprawdź `butler push --help` i usuń flagę z `deploy.ps1`.
