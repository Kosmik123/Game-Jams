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
- [ ] **6. [post-jam] Balans ekonomii (`CityStats` per ruleset).** Wersja jamowa liczy jak dotąd; post-jam dostaje nową ekonomię (np. `TowerRuleset.CreateEconomy()` albo wirtualne metody — Twój wybór). Liczby jako stałe/pola w `PostJamRuleset`, łatwe do strojenia.
  - **Zasilanie proporcjonalne zamiast BLACKOUT 0/1.** `supply` = suma dodatnich `Energy`, `demand` = suma ujemnych (wartość bezwzględna). `PowerRatio = demand == 0 ? 1 : min(1, supply / demand)`. Dochód × `PowerRatio`. Wizualnie ciemnieją piętra **od góry**: z pięter zużywających prąd zasilone jest `floor(PowerRatio × liczba_konsumentów)` najniższych, reszta ciemna; elektrownie zawsze jasne. HUD: `Energy +2` jak dotąd, a przy niedoborze `Power 75%` na czerwono zamiast „BLACKOUT”.
  - **Szczęście dodatnie coś daje.** Dziś parki są bezużyteczne powyżej zera. Nowe: `Happiness > 0` → mnożnik dochodu `1 + 0,05 × Happiness`, maks. `×1,5`. Ujemne bez zmian (−5 mieszkańców za punkt).
  - **Liczby pięter (post-jam):** Residential `+10 pop, 0 jobs, −1 energy, −1 joy`; Office `0, +10, −1, 0`; Power `0, 0, +4, −2`; **Park `0, 0, 0, +4`** (było +3 — przepis „2 mieszkalne + 2 biura + 1 elektrownia + 1 park” ma wychodzić na zero, a nie na −1).
  - Biura **nie** zmieniają populacji — dochód = `min(Pop, Jobs) × 0,2 $/s` (zatrudnieni mieszkańcy) zostaje.
  - Sprawdzenie (EditMode test na `CityStats` z listą typów, bez scen): 2R+2O+1P+1Park → Energy 0, Joy 0, Pop 20, dochód 4 $/s; 3R+1O, bez elektrowni → PowerRatio 0, dochód 0; 4R+2O+1P → PowerRatio 4/6, Joy −6 → Pop 10; 1R+1O+1P+2Park → Joy +5 → dochód 2 × 1,25 = 2,5 $/s.
- [ ] **7. [post-jam] Spokojniejsze kołysanie wysokiej wieży.** Dziś wychylenie piętra `i` = `sin(3t) × I × 0,06 × (i+1)`, przycięte do 0,6 → przy 10+ piętrach cała górna część kiwa się o 0,6 szerokości. Nowe (tylko wizualne, logika niestabilności bez zmian):
  - Wychylenie szczytu: `topAmplitude = I × maxTopWobble × 1 / (1 + Height / heightDamping)`, `maxTopWobble = 0,45`, `heightDamping = 15` (10 pięter → 0,27; 30 → 0,15; 60 → 0,09 przy pełnej niestabilności). `I` = niestabilność 0..1.
  - Kształt: piętro `i` dostaje `topAmplitude × ((i+1) / Height)^1,5` — dół stoi, wygina się góra; brak przycinania na sztywno.
  - Częstotliwość maleje z wysokością (wysokie budynki kołyszą się wolniej): `ω = 3 / sqrt(1 + Height / 15)`. Licz fazę przyrostowo (`phase += ω × dt`), żeby zmiana ω nie powodowała skoków.
  - Wygładzanie: wyświetlana niestabilność dąży do rzeczywistej (`Mathf.MoveTowards` / lerp ~2/s), bez szarpnięcia po złapaniu piętra.
  - Parametry w `PostJamRuleset`; jam zostaje przy starym wzorze.

## Zrobione (jam, do `39a94ec` na itch.io)

- [x] Input na telefonie (platforma zostaje w miejscu bez dotyku).
- [x] Fale pięter → zastąpione pojedynczymi piętrami co 0,7 s na zmianę.
- [x] Koniec rundy, ekran wyniku, restart bez przeładowania sceny.
- [x] Czytelny HUD na telefonie.
- [x] Grafiki pięter (pixel art z `Tools/floor_sprites.py`).
- [x] Deploy na itch.io.
