# Taski

Dopisuj zadania jako `- [ ] opis`. Claude obserwuje ten plik, wykonuje nowe zadania i odhacza je jako `- [x]`.
Rozmowy, pytania i protokół buildów (`[BUILD START]`/`[BUILD END]`): `COMMS.md`. Zasady gry: `GDD.md`.

## Etap: post-jam (od 25.10.2026, 15:40)

Gałąź: dalej `0h-jam-2026` (nazwa `0h-jam-2026/postjam` jest niemożliwa w gicie obok istniejącej gałęzi `0h-jam-2026`).
Po każdym tasku: kompilacja/testy → commit (mały, z opisem). Build WebGL + deploy na itch.io robi **menedżer**. Programista może zrobić lokalny build weryfikacyjny po ogłoszeniu w `COMMS.md`.

### Do zrobienia (kolejność = priorytet)

- [ ] **1. Rozdzielenie wersji: jam / post-jam (fundament, bez zmian w rozgrywce).**
  Wersja jamowa ma zostać **dokładnie taka, jaka poszła na itch.io w commicie `39a94ec`** (runda 90 s, bez PERFECT, bez trudności, bez samouczka). Wszystkie nowe funkcje trafiają tylko do post-jam.
  - Wprowadź abstrakcję zestawu zasad, np. `abstract class TowerRuleset` (namespace `ZeroHour.Tower`) z implementacjami `JamRuleset` i `PostJamRuleset`. Ruleset dostarcza parametry (`RoundDuration`, `FallSpeed(height)`, `SpawnInterval(height)`, …) i flagi/hooki funkcji (`ShowPerfectPopup`, `ShowTutorial`, …). Na start obie implementacje dają identyczną rozgrywkę jak teraz, poza: Jam → `roundDuration = 90`, PostJam → `40` (zmiana z `bbd8628`).
  - Wartości, które dziś są `[SerializeField]` w `TowerGame`/`TowerController` i różnią się między wersjami, przenieś do rulesetu. Wspólne mogą zostać.
  - `TowerGame` dostaje ruleset przy tworzeniu (np. `TowerGame.Create(TowerRuleset ruleset)`), nie czyta globali w środku logiki.
  - Jeżeli abstrakcja jakiejś części jest zbyt kosztowna, dopuszczalna jest kopia klasy pod `Jam/` — napisz wtedy w `COMMS.md`, co i dlaczego.
  - Sprawdzenie: oba warianty uruchomione z kodu dają rundę 90 s / 40 s, konsola czysta.

- [ ] **2. Scena wyboru wersji (pierwsza scena gry).**
  - Nowa scena `Assets/_Project/Scenes/VersionSelect/VersionSelect.unity` jako **scena nr 0** w Build Settings, `Game.unity` jako nr 1.
  - Scenę i wpis w Build Settings utwórz **skryptem edytorowym** (`Assets/Editor/…`, np. menu `Tools/Create Version Select Scene` + metoda statyczna do odpalenia przez `-executeMethod` w batchmode), nie ręczną edycją YAML. Scena zawiera tylko kamerę; reszta jak dotąd budowana w kodzie.
  - UI (legacy `UI.Text`/`Button`, styl jak HUD, referencja 1080×1920, portrait): tytuł **„Stack City”**, dwa duże przyciski jeden pod drugim: **„Jam version”** i **„Post-jam version”** (czcionka ≥ 70, łatwe do trafienia palcem), pod nimi małym drukiem np. „0h Game Jam 2026”. Tło w kolorze nieba z gry.
  - Klik → zapamiętaj wybór (np. statyczne `GameVersion.Selected`) → `SceneManager.LoadScene("Game")` → `TowerGame` startuje z odpowiednim rulesetem.
  - Bootstrap: obecne `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` odpala się tylko raz, przy pierwszej scenie — zastąp go reakcją na `SceneManager.sceneLoaded` (po nazwie sceny: `VersionSelect` → menu, `Game` → gra). Odpalenie `Game` bezpośrednio z edytora ma działać (domyślnie post-jam).
  - Na ekranie końca rundy, obok „Tap to play again”, mały przycisk/napis **„Menu”** (np. w rogu) wracający do wyboru wersji. W obu wersjach (to jedyna dopuszczalna zmiana w wersji jamowej).
  - Sprawdzenie: build WebGL, start → menu → każda wersja działa → Menu → druga wersja działa; konsola bez wyjątków.

- [ ] **3. [post-jam] Napis „PERFECT!”.** Gdy piętro wyląduje idealnie (warunek `perfectOffset` w `TowerController.Attach`, ten sam, który uspokaja wieżę), nad tym piętrem pojawia się napis **„PERFECT!”** (biały z obrysem, czcionka ~70 przy referencji 1080×1920), unosi się o ~1 jednostkę i znika w ~0,8 s. Napis podąża za światem (pozycja piętra przeliczona na ekran albo world-space canvas), nie przykleja się do HUD. Tylko w post-jam.
- [ ] **4. [post-jam] Rosnąca trudność.** Z każdym piętrem w wieży nowe piętra spadają szybciej i częściej: `fallSpeed = min(3 + 0,15 × Height, 8)` j/s, `spawnInterval = max(0,7 − 0,01 × Height, 0,4)` s. Dotyczy nowo spawnowanych pięter. Współczynniki w `PostJamRuleset`; jam zostaje na stałych 3 j/s i 0,7 s.
- [ ] **5. [post-jam] Samouczek na starcie.** Na starcie rundy na środku ekranu napis **„Move finger to catch floors”** (biały z obrysem, ~64). Znika płynnie (fade ~0,5 s) po złapaniu pierwszego piętra albo po 5 s — co pierwsze. Pokazuje się też po restarcie. Tylko w post-jam.

## Zrobione (jam, do `39a94ec` na itch.io)

- [x] Input na telefonie (platforma zostaje w miejscu bez dotyku).
- [x] Fale pięter → zastąpione pojedynczymi piętrami co 0,7 s na zmianę.
- [x] Koniec rundy, ekran wyniku, restart bez przeładowania sceny.
- [x] Czytelny HUD na telefonie.
- [x] Grafiki pięter (pixel art z `Tools/floor_sprites.py`).
- [x] Deploy na itch.io.
