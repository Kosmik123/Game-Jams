# Taski

Dopisuj zadania jako `- [ ] opis`. Claude obserwuje ten plik, wykonuje nowe zadania i odhacza je jako `- [x]`.
Rozmowy, pytania i protokół buildów (`[BUILD START]`/`[BUILD END]`): `COMMS.md`. Zasady gry: `GDD.md`.

## Etap: post-jam (od 25.10.2026, 15:40)

**Worktree (od 25.10, 19:00):**
- Programista: `C:\Users\Kosmik\Unity Projects\Game Jams-dev`, gałąź `0h-jam-2026-dev` — tam kod, testy, commity, własne buildy (własne `Library/`, własny lock Unity — buildy mogą iść równolegle).
- Menedżer: `C:\Users\Kosmik\Unity Projects\Game Jams`, gałąź `0h-jam-2026` — dokumenty, merge `0h-jam-2026-dev`, build do deployu, deploy.
- `TASKS.md`, `COMMS.md`, `GDD.md` żyją **tylko w worktree menedżera** — programista czyta/dopisuje je po pełnej ścieżce i **nie commituje** ich (odhaczenia commituje menedżer). W worktree dev te pliki są nieaktualne — nie czytaj ich stamtąd.
- Programista nie merguje do `0h-jam-2026`; menedżer nie commituje do `0h-jam-2026-dev`. Gdy dev potrzebuje zmian z głównej gałęzi: `git merge 0h-jam-2026` w worktree dev.
Po każdym tasku: kompilacja/testy → commit (mały, z opisem). Build WebGL + deploy na itch.io robi **menedżer**. Programista może zrobić lokalny build weryfikacyjny po ogłoszeniu w `COMMS.md`.

### Do zrobienia (kolejność = priorytet)

- [x] **1. Rozdzielenie wersji: jam / post-jam (fundament, bez zmian w rozgrywce).**
  Wersja jamowa ma zostać **dokładnie taka, jaka poszła na itch.io w commicie `39a94ec`** (runda 90 s, bez PERFECT, bez trudności, bez samouczka). Wszystkie nowe funkcje trafiają tylko do post-jam.
  - Wprowadź abstrakcję zestawu zasad, np. `abstract class TowerRuleset` (namespace `ZeroHour.Tower`) z implementacjami `JamRuleset` i `PostJamRuleset`. Ruleset dostarcza parametry (`RoundDuration`, `FallSpeed(height)`, `SpawnInterval(height)`, …) i flagi/hooki funkcji (`ShowPerfectPopup`, `ShowTutorial`, …). Na start obie implementacje dają identyczną rozgrywkę jak teraz, poza: Jam → `roundDuration = 90`, PostJam → `40` (zmiana z `bbd8628`).
  - Wartości, które dziś są `[SerializeField]` w `TowerGame`/`TowerController` i różnią się między wersjami, przenieś do rulesetu. Wspólne mogą zostać.
  - `TowerGame` dostaje ruleset przy tworzeniu (np. `TowerGame.Create(TowerRuleset ruleset)`), nie czyta globali w środku logiki.
  - Jeżeli abstrakcja jakiejś części jest zbyt kosztowna, dopuszczalna jest kopia klasy pod `Jam/` — napisz wtedy w `COMMS.md`, co i dlaczego.
  - Sprawdzenie: oba warianty uruchomione z kodu dają rundę 90 s / 40 s, konsola czysta.

- [x] **2. Scena wyboru wersji (pierwsza scena gry).**
  - Nowa scena `Assets/_Project/Scenes/VersionSelect/VersionSelect.unity` jako **scena nr 0** w Build Settings, `Game.unity` jako nr 1.
  - Scenę i wpis w Build Settings utwórz **skryptem edytorowym** (`Assets/Editor/…`, np. menu `Tools/Create Version Select Scene` + metoda statyczna do odpalenia przez `-executeMethod` w batchmode), nie ręczną edycją YAML. Scena zawiera tylko kamerę; reszta jak dotąd budowana w kodzie.
  - UI (legacy `UI.Text`/`Button`, styl jak HUD, referencja 1080×1920, portrait): tytuł **„Stack City”**, dwa duże przyciski jeden pod drugim: **„Jam version”** i **„Post-jam version”** (czcionka ≥ 70, łatwe do trafienia palcem), pod nimi małym drukiem np. „0h Game Jam 2026”. Tło w kolorze nieba z gry.
  - Klik → zapamiętaj wybór (np. statyczne `GameVersion.Selected`) → `SceneManager.LoadScene("Game")` → `TowerGame` startuje z odpowiednim rulesetem.
  - Bootstrap: obecne `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` odpala się tylko raz, przy pierwszej scenie — zastąp go reakcją na `SceneManager.sceneLoaded` (po nazwie sceny: `VersionSelect` → menu, `Game` → gra). Odpalenie `Game` bezpośrednio z edytora ma działać (domyślnie post-jam).
  - Na ekranie końca rundy, obok „Tap to play again”, mały przycisk/napis **„Menu”** (np. w rogu) wracający do wyboru wersji. **Tylko w post-jam** — wersji jamowej nie zmieniamy w ogóle (powrót z niej: przeładowanie strony).
  - Sprawdzenie: build WebGL, start → menu → każda wersja działa (przeładowanie strony między nimi) → w post-jam „Menu” wraca do wyboru wersji; konsola bez wyjątków.

- [x] **3. [post-jam] Napis „PERFECT!”.** Gdy piętro wyląduje idealnie (warunek `perfectOffset` w `TowerController.Attach`, ten sam, który uspokaja wieżę), nad tym piętrem pojawia się napis **„PERFECT!”** (biały z obrysem, czcionka ~70 przy referencji 1080×1920), unosi się o ~1 jednostkę i znika w ~0,8 s. Napis podąża za światem (pozycja piętra przeliczona na ekran albo world-space canvas), nie przykleja się do HUD. Tylko w post-jam.
- [x] **4. [post-jam] Rosnąca trudność.** Z każdym piętrem w wieży nowe piętra spadają szybciej i częściej: `fallSpeed = min(3 + 0,15 × Height, 8)` j/s, `spawnInterval = max(0,7 − 0,01 × Height, 0,4)` s. Dotyczy nowo spawnowanych pięter. Współczynniki w `PostJamRuleset`; jam zostaje na stałych 3 j/s i 0,7 s.
- [x] **5. [post-jam] Samouczek na starcie.** Na starcie rundy na środku ekranu napis **„Move finger to catch floors”** (biały z obrysem, ~64). Znika płynnie (fade ~0,5 s) po złapaniu pierwszego piętra albo po 5 s — co pierwsze. Pokazuje się też po restarcie. Tylko w post-jam.
- [x] **6. [post-jam] Balans ekonomii (`CityStats` per ruleset).** Wersja jamowa liczy jak dotąd; post-jam dostaje nową ekonomię (np. `TowerRuleset.CreateEconomy()` albo wirtualne metody — Twój wybór). Liczby jako stałe/pola w `PostJamRuleset`, łatwe do strojenia.
  - **Zasilanie proporcjonalne zamiast BLACKOUT 0/1.** `supply` = suma dodatnich `Energy`, `demand` = suma ujemnych (wartość bezwzględna). `PowerRatio = demand == 0 ? 1 : min(1, supply / demand)`. Dochód × `PowerRatio`. Wizualnie ciemnieją piętra **od góry**: z pięter zużywających prąd zasilone jest `floor(PowerRatio × liczba_konsumentów)` najniższych, reszta ciemna; elektrownie zawsze jasne. HUD: `Energy +2` jak dotąd, a przy niedoborze `Power 75%` na czerwono zamiast „BLACKOUT”.
  - **Szczęście dodatnie coś daje.** Dziś parki są bezużyteczne powyżej zera. Nowe: `Happiness > 0` → mnożnik dochodu `1 + 0,05 × Happiness`, maks. `×1,5`. Ujemne bez zmian (−5 mieszkańców za punkt).
  - **Liczby pięter (post-jam):** Residential `+10 pop, 0 jobs, −1 energy, −1 joy`; Office `0, +10, −1, 0`; Power `0, 0, +4, −2`; **Park `0, 0, 0, +4`** (było +3 — przepis „2 mieszkalne + 2 biura + 1 elektrownia + 1 park” ma wychodzić na zero, a nie na −1).
  - Biura **nie** zmieniają populacji — dochód = `min(Pop, Jobs) × 0,2 $/s` (zatrudnieni mieszkańcy) zostaje.
  - Sprawdzenie (EditMode test na `CityStats` z listą typów, bez scen): 2R+2O+1P+1Park → Energy 0, Joy 0, Pop 20, dochód 4 $/s; 3R+1O, bez elektrowni → PowerRatio 0, dochód 0; 4R+2O+1P → PowerRatio 4/6, Joy −6 → Pop 10; 1R+1O+1P+2Park → Joy +5 → dochód 2 × 1,25 = 2,5 $/s.
- [x] **7. [post-jam] Spokojniejsze kołysanie wysokiej wieży.** Dziś wychylenie piętra `i` = `sin(3t) × I × 0,06 × (i+1)`, przycięte do 0,6 → przy 10+ piętrach cała górna część kiwa się o 0,6 szerokości. Nowe (tylko wizualne, logika niestabilności bez zmian):
  - Wychylenie szczytu: `topAmplitude = I × maxTopWobble × 1 / (1 + Height / heightDamping)`, `maxTopWobble = 0,45`, `heightDamping = 15` (10 pięter → 0,27; 30 → 0,15; 60 → 0,09 przy pełnej niestabilności). `I` = niestabilność 0..1.
  - Kształt: piętro `i` dostaje `topAmplitude × ((i+1) / Height)^1,5` — dół stoi, wygina się góra; brak przycinania na sztywno.
  - Częstotliwość maleje z wysokością (wysokie budynki kołyszą się wolniej): `ω = 3 / sqrt(1 + Height / 15)`. Licz fazę przyrostowo (`phase += ω × dt`), żeby zmiana ω nie powodowała skoków.
  - Wygładzanie: wyświetlana niestabilność dąży do rzeczywistej (`Mathf.MoveTowards` / lerp ~2/s), bez szarpnięcia po złapaniu piętra.
  - Parametry w `PostJamRuleset`; jam zostaje przy starym wzorze.
- [x] **8. [post-jam] Kamera stoi, wieża zjeżdża w dół (zgłoszenie graczy).** Po złapaniu piętra kamera rusza w górę (`TowerGame` ~l. 180, lerp do `targetY`), przez co spadające piętra przez chwilę **wyglądają, jakby przyspieszały**. Odwrotnie: kamera stoi na `startCameraY` przez całą rundę, a **cała wieża (platforma + piętra) płynnie zjeżdża w dół**, tak żeby szczyt trzymał się na `towerTopScreenFraction` (40%) wysokości ekranu.
  - `BaseY` staje się dynamiczne: cel `baseY = min(startBaseY, startBaseY − (TowerTopLocal − dozwolona wysokość))`, dążenie do celu jak teraz kamera (wykładniczo, `cameraFollowSpeed` → np. `towerSinkSpeed`). `TowerController.Update` dalej ustawia `position.y = game.BaseY`.
  - Spadające piętra, spawn (`TopY + floorSize`) i tło są w stałym układzie ekranu — ich ruch na ekranie ma być **jednostajny** niezależnie od tego, co robi wieża.
  - Po zawaleniu (`CollapseTop`) wieża płynnie podjeżdża do góry, ale nie wyżej niż `startBaseY`. Zawalone piętra spadają jak dotąd.
  - Restart rundy: `baseY = startBaseY`.
  - Sprawdzenie: piętro spadające obok wieży w chwili łapania innego piętra ma stałą prędkość na ekranie (np. log `screenY` co klatkę — różnice równe); konsola czysta.
  - Tylko post-jam (jam zostaje z ruchomą kamerą, w rulesecie np. `bool FixedCamera`).
- [x] **9. [post-jam] Sterowanie względne (przeciąganie).** Dziś platforma skacze pod palec (`TowerController.TryGetPointerWorldX` → `position.x = pointerX`). Nowe: platforma przesuwa się **o tyle, o ile przesunął się palec**, niezależnie od tego, gdzie palec leży na ekranie.
  - Co klatkę, gdy palec jest wciśnięty: `deltaX = worldX(teraz) − worldX(poprzednia klatka)`, `x = clamp(x + deltaX × dragSensitivity, −playWidth/2, playWidth/2)`. `dragSensitivity = 1` (1:1 w jednostkach świata), w `PostJamRuleset`.
  - Przyrost per klatka (nie „pozycja startowa + przesunięcie”), żeby po dociśnięciu do krawędzi ruch w drugą stronę działał od razu, bez martwej strefy.
  - Przyłożenie palca **nie przesuwa** platformy (pierwsza klatka dotyku tylko zapamiętuje pozycję). Puszczenie — platforma stoi. Zmiana palca / nowy dotyk = nowy punkt odniesienia, bez skoku.
  - Komputer: to samo przy **wciśniętym lewym przycisku myszy** (przeciąganie). Ruch myszy bez przycisku nic nie robi. Na telefonie mysz dalej ignorowana (logika `hasSeenTouch`/`isMobilePlatform` zostaje).
  - Wersja jamowa bez zmian (absolutne podążanie za palcem/myszą). Np. w rulesecie `bool RelativeControl` albo osobna strategia sterowania.
  - Sprawdzenie: przyłożenie palca w dowolnym miejscu nie rusza platformy; przesunięcie palca o 1 j. przesuwa platformę o 1 j.; przy krawędzi platforma się zatrzymuje i od razu reaguje na ruch w drugą stronę.

- [x] **10. [post-jam] Najlepszy wynik.** Po rundzie zapisz rekord w `PlayerPrefs` (klucz np. `BestScore_PostJam`, `PlayerPrefs.Save()` od razu — w WebGL to trafia do IndexedDB). Ekran końca: pod „Score” linijka **„Best: N”**; gdy rekord pobity — **„NEW BEST!”** na żółto, lekko pulsujący (skala 1↔1,1, ~2 Hz). Pierwsza rozegrana runda też jest rekordem. Sprawdzenie: dwie rundy, rekord przeżywa restart i przeładowanie strony (w buildzie WebGL).
- [x] **14. ⚡ PRIORYTET (zrób zaraz po 10, przed 11) — rozmiar okna WebGL 540×720.** Strona gry jest za duża (600×960 nie mieści się w typowym oknie, ucina górę HUD).
  - `PlayerSettings.defaultWebScreenWidth/Height` = **540 × 720** — ustaw skryptem edytorowym w batchmode (np. metoda w `Assets/Editor/`), nie ręczną edycją `ProjectSettings.asset`. Zacommituj tylko te dwie wartości (w `ProjectSettings.asset` leży niezacommitowana obca zmiana batchingu — nie dołączaj jej, chyba że nie da się rozdzielić; wtedy napisz).
  - To ustawienie strony, nie rozgrywki — dotyczy obu wersji. **Sprawdź wersję jamową** tak samo jak post-jam: proporcje 3:4 zamiast ~10:16, więc kamera (`FitCamera`) pokaże szerszy, niższy obszar. Ma się mieścić cała szerokość gry (`playWidth`), szczyt wieży na 40%, spadające piętra widoczne od góry.
  - HUD (`CanvasScaler` 1080×1920): przy 540×720 sprawdź, czy statystyki (lewy górny), timer, ekran końca, PERFECT, samouczek i menu wyboru wersji się mieszczą i nie nachodzą na siebie. Jeśli trzeba, ustaw `matchWidthOrHeight` tak, żeby UI skalowało się sensownie (np. 1 = wg wysokości) — napisz w COMMS, co wybrałeś.
  - Sprawdzenie: lokalny build WebGL w oknie ~800×800 — cała gra widoczna bez przewijania, konsola czysta. (Rozmiar osadzenia na stronie itch.io ustawia autor w panelu gry — nie Twoja działka.)
- [x] **15. ⚡ PRIORYTET (po 14, przed 11) — sterowanie: telefon przeciąga, PC podąża za myszą.** Zmiana taska 9 w post-jam:
  - **Dotyk** (telefon/tablet): bez zmian — przeciąganie względne (`RelativeDrag`).
  - **Mysz** (PC): powrót do starego zachowania — **X platformy = X kursora**, bez wciskania przycisku (jak w wersji jamowej). Przeciąganie z LPM znika.
  - Wybór per wejście, nie per platforma: dotyk ekranu → ścieżka względna; ruch myszy → ścieżka absolutna. Zachowaj ochronę z jamu: na telefonie mysz ignorowana (`hasSeenTouch`/`isMobilePlatform`), żeby platforma nie uciekała do (0,0).
  - Kod absolutny weź wspólny z jamem (nie kopiuj, jeśli się da); jam zachowuje się 1:1 jak dotąd.
  - Zaktualizuj `RelativeDragTests`/Play Mode: mysz bez przycisku → platforma pod kursorem; dotyk → przeciąganie jak w tasku 9.

- [x] **18. ⚡ PRIORYTET (zaraz po 16) — w zakresie ruchu wygrywa dach, nie ekran.** Poprawka taska 17 (decyzja autora): platforma i tak zwykle jest pod ekranem (kamera stoi, wieża zjeżdża — widać kilka górnych pięter), więc **usuń ograniczenie platformy do widocznej szerokości ekranu**. Zakres ruchu = tylko warunek dachu: `platformX ∈ [−playWidth/2 − topOffset, playWidth/2 − topOffset]`, dach zawsze dosięga obu krawędzi `playWidth` niezależnie od wychylenia. Mysz: dach pod kursorem (bez zmian). Zaktualizuj `PlatformRangeTests` (np. offset ±20 → zakres przesunięty o −20, bez przycinania do ekranu). Jam bez zmian.

- [x] **19. [post-jam] Na koniec rundy kamera pokazuje cały wieżowiec.** Po „TIME'S UP!” kamera płynnie (~1,5 s, ease in-out) oddala się i przesuwa tak, żeby w kadrze był **cały budynek — od platformy po dach** — z marginesem ~10%. Satysfakcja z tego, co się zbudowało.
  - Cel: `orthographicSize` = max(obecny, połowa wysokości wieży z marginesem, rozmiar mieszczący szerokość wieży z wychyleniem przy bieżących proporcjach ekranu); pozycja kamery tak, żeby **wieża zajmowała dolne ~2/3 ekranu**, a górna 1/3 była na tekst ekranu końca. Tekst ekranu końca (TIME'S UP!, Height, Money, Score, Best, NEW BEST!) przesuń do górnej części, żeby nie zasłaniał wieży; tło panelu bardziej przezroczyste (~0,35) albo tylko pod tekstem.
  - Na czas ujęcia kołysanie wygasa do zera (wieża „stoi dumnie”). Spadające jeszcze piętra mogą spaść.
  - „Tap to play again” / „Menu” działają dopiero po zakończeniu animacji (`restartDelay` ≥ czas animacji). Restart: kamera wraca do pozycji i rozmiaru startowego natychmiast.
  - Dla bardzo wysokich wież (np. 60 pięter) piętra robią się małe — OK, to cel. Sprawdź, że nic się nie rozjeżdża przy 0, 1 i ~40 piętrach.
  - Jam bez zmian.
- [x] **20. [post-jam] PERFECT wyśrodkowuje piętro.** Gdy lądowanie liczy się jako PERFECT (offset < `perfectOffset`), piętro zostaje **ustawione dokładnie nad poprzednim** (`TowerPosition.x` = X poprzedniego piętra; dla pierwszego piętra = środek platformy). Logicznie od razu (wpływa na `topOffset`, następne lądowania, niestabilność = jak przy offsecie 0). Wizualnie: krótki dosuw ~0,08 s zamiast skoku (może się nakładać ze sprężystym lądowaniem z taska 12). Napis PERFECT nad wyśrodkowanym piętrem. Test EditMode: lądowanie z offsetem 0,1 × szerokość → pozycja = poprzednie piętro; offset 0,2 → bez zmiany. Jam bez zmian.

- [x] **21. [post-jam] Czas rundy zależny od gry (wersja wstępna).** Zamiast stałych 40 s: **start 25 s**, zegar odlicza w dół, a gracz dokłada czas grą:
  - złapane piętro: `+1,5 × max(0,3; 1 − Height/40)` s (Height = wysokość **po** złapaniu; premia maleje z wysokością, min. 0,45 s),
  - PERFECT: dodatkowo **+1 s** (bez spadku z wysokością),
  - zawalenie (`CollapseTop`): **−3 s** (nie poniżej 0 — przy 0 koniec rundy jak dziś),
  - **limit 30 s** — zegar nigdy nie pokazuje więcej (nadwyżka przepada).
  - Każda zmiana czasu pokazuje się przy timerze w prawym górnym rogu: **„+1.5s”** na zielono / **„−3s”** na czerwono, unosi się i znika w ~0,6 s. Timer < 5 s świeci na czerwono.
  - Wszystkie liczby jako stałe w `PostJamRuleset` (np. `StartTime`, `MaxTime`, `TimePerFloor`, `TimeFalloffHeight`, `MinTimeFactor`, `PerfectTimeBonus`, `CollapseTimePenalty`) — będziemy je stroić po testach.
  - Jam bez zmian (stałe 40 s, bez bonusów).
  - Test EditMode na funkcji premii: H=1 → 1,46 s; H=20 → 0,75 s; H=40 → 0,45 s; PERFECT +1; kara −3 z obcięciem do 0; limit 30.

- [x] **11. [post-jam] Combo za idealne lądowania.** Licznik `PerfectStreak`: +1 przy PERFECT, zero przy nieidealnym lądowaniu (zawalenie też zeruje). **Mnożnik dochodu** = `streak ≥ 2 ? min(streak, 5) : 1` (x2…x5), działa razem z premią za szczęście (mnożą się). Napis PERFECT przy serii ≥ 2: **„PERFECT! x3”**. HUD: przy linijce Money, gdy mnożnik > 1, dopisek **„COMBO x3”** w kolorze żółtym. Stałe w `PostJamCityStats`/rulesecie. EditMode test: seria 3 PERFECT → x3, potem zwykłe lądowanie → x1; maks. x5.
- [x] **12. [post-jam] Juice.**
  - **Wstrząs ekranu przy zawaleniu:** kamera drga wokół swojej stałej pozycji ~0,3 s, amplituda ~0,15 j., wygasająca. Po wstrząsie kamera wraca dokładnie na miejsce (kamera w post-jam stoi — task 8).
  - **„+$” z wieży:** co 1 s, gdy dochód > 0, nad szczytem wieży wylatuje mały napis **„+$N”** (N = zarobione w tej sekundzie, zaokrąglone; ~48 px, zielony z obrysem), unosi się ~0,8 j. i znika w ~0,7 s. Można użyć mechaniki z `PerfectPopup`.
  - **Sprężyste lądowanie:** złapane piętro na chwilę się spłaszcza (skala ×1,15 w X, ×0,85 w Y) i wraca do 1 w ~0,15 s. Tylko wizualnie — nie zmieniaj pozycji kolizyjnych ani wysokości wieży.
- [x] **13. [post-jam] Piętra 4:3 (poziomy prostokąt).** Nowe grafiki są gotowe: `Assets/_Project/Resources/FloorsWide/<Name>.png`, **128×96** (pixel art 32×24 ×4, generator `Tools/floor_sprites.py --wide`). Kwadratowe `Resources/Floors/` zostają dla jamu — nie ruszać.
  - Wymiary w post-jam: **szerokość 1 j. (jak dotąd), wysokość 0,75 j.** Trafianie zostaje tak samo trudne; wieża rośnie wolniej na ekranie.
  - `FallingFloor.Size` rozbij na `Width` i `Height` (wszędzie, gdzie jest użyte: dół piętra przy łapaniu, `localTop += Height`, warunek X `< Width`, `perfectOffset` i niestabilność względem `Width`, spawn `TopY + Height`, znikanie pod ekranem, kołysanie, zawalanie, PERFECT/„+$” nad piętrem). Skala sprite'a: X = `Width / bounds.x`, Y = `Height / bounds.y`. Fallback (brak sprite'a) — prostokąt `Width × Height`.
  - Ruleset: `FloorHeight` (Jam 1, PostJam 0,75) i folder grafik (`Floors` / `FloorsWide`). Jam ma wyglądać i działać 1:1 jak dotąd.
  - **Import grafik:** nowe PNG dostaną domyślne ustawienia (rozmyte, skompresowane). Dodaj `Assets/Editor/FloorSpriteImporter.cs` (`AssetPostprocessor.OnPreprocessTexture`) dla ścieżek `Resources/Floors/` i `Resources/FloorsWide/`: Sprite, **Single**, **Point**, bez kompresji, bez mipmap, PPU 100. Po dodaniu wymuś reimport folderu `FloorsWide` (batchmode `AssetDatabase.ImportAsset(..., ForceUpdate)`), sprawdź w `.meta`: `spriteMode: 1`, `filterMode: 0`, `textureCompression: 0`, i zacommituj PNG + `.meta`. Nie zgaduj GUID-ów. Kwadratowe `.meta` nie mogą się zmienić (sprawdź `git diff`).
  - Sprawdzenie: testy przechodzą (poprawione pod Width/Height); Play Mode post-jam: piętro 4:3 ląduje równo na poprzednim (brak szpar/nakładania), PERFECT działa; jam: kwadraty jak dotąd.

- [x] **16. [post-jam] Etykieta „Pop” → „Popul”.** W HUD (`TowerHud`, linijka populacji) post-jam pokazuje **„Popul”** zamiast „Pop”. Jam bez zmian (np. etykieta z rulesetu albo flaga). Sprawdź, czy dłuższa etykieta mieści się w panelu HUD przy 540×720.
- [x] **17. [post-jam] Zakres ruchu liczony dla dachu, nie dla platformy.** Gdy wieża jest krzywa, szczyt (`TowerController.TopX`) jest przesunięty względem platformy o `topOffset = TopX − platformX`. Dziś przycinamy X **platformy** do `±playWidth/2`, więc dach raz nie sięga brzegu (nie da się złapać pięter przy krawędzi), a raz wyjeżdża poza ekran.
  - Nowe: przycinaj tak, żeby **dach** był w `±playWidth/2` (tam spawnują się piętra): `platformX ∈ [−playWidth/2 − topOffset, playWidth/2 − topOffset]`.
  - Dodatkowo platforma nie może całkiem zniknąć z ekranu: `platformX` w granicach widocznej szerokości kamery minus pół szerokości platformy. Gdy oba warunki się wykluczają (bardzo krzywa wieża), wygrywa warunek ekranu.
  - **Mysz (PC, task 15):** pod kursorem ma być **dach**, nie platforma (`platformX = mouseX − topOffset`, potem przycięcie) — gracz celuje tym, na co spadają piętra.
  - **Dotyk (przeciąganie):** ruch względny bez zmian, zmienia się tylko przycięcie.
  - `topOffset` licz z pozycji logicznych pięter (`TowerPosition`), nie z wizualnego kołysania. Gdy wieża pusta → `topOffset = 0`.
  - Jam bez zmian.
  - Sprawdzenie (EditMode na funkcji przycinania): offset +1 → zakres platformy [−4, 2] przy `playWidth = 6`; offset 0 → jak dotąd; skrajny offset → platforma na krawędzi ekranu. Play Mode: krzywa wieża, dach dosięga obu krawędzi `playWidth`.

- [x] **22. [JAM — wyjątek od zamrożenia, decyzja autora] Przycisk „Menu” na ekranie końca wersji jamowej.** `JamRuleset.ShowMenuButton = true` — ten sam przycisk co w post-jam (lewy dolny róg ekranu końca), wraca do wyboru wersji. Nic poza tym w jamie się nie zmienia (rozgrywka, układ ekranu końca, HUD — 1:1). Sprawdź w buildzie: jam → koniec rundy → „Menu” → menu wyboru wersji; tap poza przyciskiem dalej restartuje.
- [x] **23. [post-jam] Synergie sąsiednich pięter.** Kolejność pięter w wieży zaczyna mieć znaczenie. Sąsiedzi = piętra bezpośrednio nad sobą (`Tower.Floors[i]` i `[i+1]`). Każda **para sąsiadów** liczona raz:
  - **Mieszkalne + Park** → **+1 Joy** („widok na park”),
  - **Mieszkalne + Elektrownia** → **−1 Joy** („smog pod oknem”; dodatkowo do −2 elektrowni),
  - **Mieszkalne + Biuro** → **+0,3 $/s** dochodu bazowego („krótki dojazd”; mnożony przez PowerRatio, szczęście i combo jak reszta dochodu).
  - Pozostałe pary: brak efektu.
  - Liczone w `PostJamCityStats.Recalculate` z listy pięter w kolejności od dołu; stałe (`ParkNeighborJoy`, `PowerNeighborJoy`, `CommuteIncome`) w `PostJamCityStats`. Zawalenie usuwa pary razem z piętrami (i tak przeliczane co klatkę).
  - **Feedback (ważne — gracz musi się tego nauczyć z gry):** gdy złapane piętro tworzy parę z piętrem pod spodem, nad stykiem pojawia się `WorldPopup`: **„+1 Joy”** zielony, **„−1 Joy”** czerwony, **„+$”** żółty (ok. 40 px, krótko, nie koliduje z PERFECT — np. lekko z boku/niżej). 
  - HUD bez nowych linijek (efekty widać w Joy i dochodzie).
  - Jam bez zmian.
  - EditMode testy: R,Park → Joy −1+4+1 = +4; R,P → energia +3, Joy −1−2−1 = −4; R,O → dochód (min(10,10)×0,2 + 0,3) × mnożniki przy pełnym zasilaniu; Park,R,Park → +2 Joy z synergii; kolejność R,O,R,O vs R,R,O,O → różna liczba par R–O (3 vs 1).

- [ ] **24. [post-jam] Dźwięk i muzyka.** Pliki gotowe: `Assets/_Project/Resources/Audio/*.wav` (mono 16-bit 22050 Hz, generator `Tools/sound_gen.py` — zmiany brzmienia robi designer). Bez `.meta` — powstaną przy imporcie.
  - `AudioManager` (tworzony w kodzie jak reszta; jeden na grę, przeżywa zmianę sceny — `DontDestroyOnLoad`), ładuje klipy przez `Resources.Load<AudioClip>("Audio/<nazwa>")`. Pula 4–6 `AudioSource` na SFX + jeden na muzykę (loop).
  - **Zdarzenia → dźwięki (tylko post-jam):**
    - złapane piętro → `land_<residential|office|power|park>` (+ lekka losowa zmiana wysokości ±5%),
    - PERFECT → `perfect_<N>`, N = mnożnik combo 1…5 (pojedynczy PERFECT = 1),
    - zawalenie → `collapse`,
    - „+$” z dachu → `coin` (głośność ~0,4 — gra co sekundę, nie może męczyć),
    - synergia → `synergy_good` (+Joy, +$) / `synergy_bad` (−Joy),
    - premia czasu → `time_bonus`, kara → `time_penalty`,
    - ostatnie 5 s rundy → `tick` raz na pełną sekundę,
    - TIME'S UP → `time_up`; „NEW BEST!” → `new_best` (~0,5 s po `time_up`),
    - kliknięcia w menu wyboru wersji i „Menu” → `ui_click` (to też w menu przed wyborem wersji).
  - **Muzyka:** `music_game` w pętli podczas rundy post-jam, głośność ~0,5; na ekranie końca ściszona do ~0,2; w menu wyboru wersji cisza (na razie).
  - **Wersja jamowa: bez dźwięku** (poza `ui_click` w menu wyboru wersji).
  - **Przycisk wyciszenia** w post-jam: mała ikonka/napis „♪” / „♪ off” w prawym dolnym rogu ekranu gry (nie koliduje z HUD, timerem, „Menu”), przełącza wszystko (SFX + muzyka), stan w `PlayerPrefs` (`Muted`). Tap w przycisk nie może przesunąć platformy ani zrestartować gry.
  - **WebGL:** przeglądarka odblokowuje audio dopiero po interakcji — pierwszy klik w menu wyboru wersji to załatwia; muzyka startuje z rundą, więc problemu nie powinno być. Sprawdź w buildzie, że muzyka gra po wejściu do post-jam.
  - **Import** (`AudioImporter`, np. w `Assets/Editor/`, dla `Resources/Audio/`): SFX — Decompress On Load, PCM/ADPCM, mono; `music_game` — Compressed In Memory (Vorbis, quality ~0,5), Load In Background. Zacommituj `.meta`.
  - Sprawdzenie: build WebGL, wszystkie zdarzenia słychać, mute działa i przeżywa przeładowanie, jam cichy, konsola czysta.

- [ ] **25. ⚡ PRIORYTET (po 24, przed 26) [post-jam] Niższy dach — więcej nieba do manewrowania.** Dziś szczyt wieży trzyma się na `towerTopScreenFraction` = 0,4 wysokości ekranu (od dołu), a od góry ~1/3 ekranu zajmuje HUD (statystyki + timer) i miejsce spawnu pięter. Wolnego nieba między dachem a HUD zostaje za mało, żeby wygodnie wybierać i łapać piętra (zgłoszenie autora).
  - Frakcja przestaje być polem `TowerGame` wspólnym dla obu wersji, a staje się właściwością rulesetu: `TowerRuleset.TowerTopScreenFraction` — **Jam 0,4** (bez zmian), **PostJam 0,25**. Stała w `PostJamRuleset`, łatwa do strojenia.
  - Dotyczy tego samego miejsca co task 8 (`FitCamera`, `allowedTopY`). Nic poza tą liczbą nie zmieniaj w mechanice zjeżdżania wieży.
  - Sprawdź w buildzie (540×720): (a) na starcie platforma i pierwsze piętra są w całości w kadrze, a wieża zaczyna zjeżdżać dopiero gdy dach dojdzie do 25% wysokości; (b) zmierz i podaj w `COMMS.md`, ile % wysokości ekranu zostaje między dachem a dolną krawędzią panelu statystyk (przed i po); (c) PERFECT/„+$”/popupy synergii nadal mieszczą się nad dachem; (d) kołysanie wysokiej wieży nie wypycha dachu pod krawędź; (e) jam wygląda dokładnie jak dotąd.
  - Jeśli po zmianie HUD i tak zasłania spawn pięter, napisz w `COMMS.md` zamiast poprawiać układ HUD na własną rękę.

- [ ] **29. ⚡ PRIORYTET (po 25, przed 26) [post-jam] Trudniej: mniej czasu za piętro, większa kara za zawalenie.** Gra jest za łatwa (zgłoszenie autora): premia 1,5 s za piętro (przy H=1) jest większa niż czas, który gracz potrzebuje na złapanie piętra, więc zegar stoi przy limicie 30 s. Zmiana stałych w `PostJamRuleset` (task 21), bez zmian logiki:
  - `TimePerFloor` 1,5 → **1,0** (premia za piętro `1,0 × max(0,3; 1 − H/40)`: H=1 → 0,975 s, H=10 → 0,75 s, H=20 → 0,5 s, H≥28 → 0,3 s),
  - `CollapsePenalty` 3 → **4** s,
  - reszta bez zmian (start 25 s, limit 30 s, PERFECT +1 s, `MinTimeFactor` 0,3, `TimeFalloffHeight` 40).
  - Zaktualizuj testy EditMode z taska 21: H=1 → 0,975 s; H=20 → 0,5 s; H=40 → 0,3 s; PERFECT dodaje 1; kara −4 z obcięciem do 0; limit 30. Opisy w komentarzach/UI (jeśli gdzieś jest „1.5”/„3”) popraw.
  - Szacunek (do weryfikacji grą): gracz łapiący ~1 piętro/s bez PERFECT traci tempem ok. `H²/80` s, więc runda kończy się przy H ≈ 45 zamiast stać na limicie; każdy PERFECT daje +1 s, więc dobry gracz nadal może grać długo, a zawalenie (−4 s) boli bardziej. **Raport do `COMMS.md`:** w 3 próbnych rundach (jedna ostrożna, jedna z PERFECT-ami, jedna z celowym zawaleniem) podaj czas rundy i końcowe Height. Jeśli runda bez zawaleń nadal dochodzi do limitu 30 s i wisi na nim — napisz, nie stroj sam.
  - Jam bez zmian.

- [ ] **26. [post-jam] Ekran końca: wieżowiec na całe tło, tekst na środku.** Zmiana taska 19 (decyzja autora): zamiast „wieża w dolnych 2/3, tekst u góry” — **cały wieżowiec od platformy po dach zajmuje praktycznie cały ekran jako tło, a wyniki są wyśrodkowanym tekstem na wierzchu.** Tekst nie zasłania budynku w istotny sposób, bo to tylko napisy.
  - `towerShotScreenShare`: 2/3 → **1** (ruleset post-jam; jam nie ma ujęcia, bez zmian). Margines wokół wieży zmniejsz do ~6% (po 3% z góry i z dołu), żeby budynek wypełniał kadr; wzory z `UpdateTowerShot` (`size`, `upperY`, `lowerY`) zostają, tylko z nowym udziałem. Krótka wieża (0–3 piętra): jak dotąd nie przybliżaj kamery poniżej rozmiaru startowego.
  - **Tekst końca** (TIME'S UP!, Height, Money, Score, Best, NEW BEST!, „Tap to play again”): wyśrodkowany w pionie i poziomie (`MiddleCenter`), na wierzchu wieży. Zamiast ciemnej zasłony na cały ekran (dziś alfa 0,65) — **brak ciemnej zasłony na całym ekranie**; czytelność przez gruby czarny obrys/cień tekstu (`Outline`, np. odległość 4–5 px) i ewentualnie bardzo lekkie tło tylko pod blokiem tekstu (alfa ≤ 0,35, wąski prostokąt dopasowany do tekstu).
  - **Reszta HUD podczas ujęcia:** panel statystyk (lewy górny róg) i timer ukryj od momentu „TIME'S UP!” (wyniki są w tekście końca), żeby nie zasłaniały dachu. Przycisk „Menu” (lewy dolny róg) i przycisk ♪ (prawy dolny) zostają i mają zostać klikalne.
  - Reszta jak w tasku 19: ease in-out ~1,5 s, wygaszone kołysanie, „Tap to play again”/„Menu” aktywne po animacji, restart przywraca kamerę natychmiast.
  - Sprawdzenie: koniec rundy przy 0, 1, ~10 i ~40 piętrach — wieża wypełnia kadr (górny i dolny margines ≈ 3%), tekst czytelny na jasnym i ciemnym tle (po tasku 28 tło będzie się zmieniać!), nic się nie rozjeżdża; zrzut ekranu z 10 i ~40 pięter do `COMMS.md`. Jam bez zmian.

- [ ] **27. [post-jam] Złote piętro.** Rzadkie, szybsze, daje bonus. Nagroda za refleks i ryzyko (odciąga gracza od wyboru innego piętra).
  - **Nowy `FloorType` „Gold”** — **nie wchodzi** do `FloorType.GetRandom()` (osobny los w `SpawnNextFloor`). Statystyki: wszystko 0 (nie zużywa prądu, nie zmienia Pop/Jobs/Joy, nie tworzy synergii sąsiadów; liczy się jako piętro do `Height`). Sprite: `Assets/_Project/Resources/FloorsWide/Gold.png` (128×96, dostarczę ja — designer; trafi do głównej gałęzi, zrób `git merge 0h-jam-2026` w dev przed rozpoczęciem). `FloorSpriteImporter` ma objąć nowy plik.
  - **Losowanie:** przy każdym spawnie szansa **5%**, ale: nie wcześniej niż od `Height ≥ 3`, najwyżej jedno złote w powietrzu naraz, oraz **gwarancja** — po 25 spawnach bez złotego następne jest złote (licznik zerowany po spawnie złotego i przy restarcie rundy). Liczby w `PostJamRuleset` (`GoldChance`, `GoldMinHeight`, `GoldPity`). Jam: `GoldChance = 0`.
  - **W powietrzu:** prędkość spadania ×1,4 względem zwykłej (`GoldFallSpeedFactor`); sprite lekko pulsuje jasnością (±15%, ~4 Hz); w chwili pojawienia się dźwięk `gold_spawn` (ciche, krótkie błyśnięcie — dostarczę).
  - **Po złapaniu** (wszystko naraz, wartości w rulesecie): **+3 s czasu** (przez `ChangeTime`, więc z napisem czasu i limitem 30 s), **niestabilność −25** (nie poniżej 0), **jednorazowo +$25** (do Money i do wyniku), popup „GOLD!” w złotym kolorze (jak PERFECT, ~70) nad piętrem, dźwięk `gold` (dostarczę). Złote piętro **może** być PERFECT (combo działa normalnie jak dla innych pięter).
  - Chybione złote znika jak każde inne, bez kary.
  - Złote piętro nie jest „konsumentem” prądu — nie ma wpływu na ciemnienie pięter (`PowerRatio`).
  - EditMode: (a) `GetRandom` nigdy nie zwraca Gold; (b) licznik pity: po 25 spawnach bez złotego następny jest złoty (stały seed/wstrzyknięty generator); (c) jedno złote w powietrzu naraz; (d) po złapaniu: czas +3 (z limitem 30), niestabilność max(0, x − 25), Money +25; (e) wieża R,Gold,Park nie tworzy żadnych par synergii z Gold; jam nigdy nie spawnuje Gold.
  - Play Mode: złote piętro pojawia się, spada szybciej, pulsuje, po złapaniu wszystkie efekty widać; konsola czysta.

- [ ] **28. [post-jam] Tło zmienia się z wysokością wieży.** Dzień → zachód słońca → zmierzch → noc ze gwiazdami. Pokazuje postęp i nagradza wysokie wieże.
  - Zależność od `Height` (liczba pięter), wygładzona (płynne dążenie ~1/s do wartości docelowej, żeby kolor nie skakał przy zawaleniu). Dwukolorowy pionowy gradient (góra / dół ekranu) interpolowany liniowo między punktami:

    | Height | Góra | Dół |
    |---|---|---|
    | 0 | obecny kolor nieba z gry | obecny kolor nieba z gry |
    | 10 | `#4A6FA5` | `#F4A261` (zachód) |
    | 20 | `#2B2D6E` | `#C2557A` (zmierzch) |
    | 30 | `#0A0F2C` | `#1F2A5C` (noc) |
    | 40+ | `#03051A` | `#101A40` (głęboka noc) |

  - **Gwiazdy:** od Height 20 do 32 stopniowo (alfa 0 → 1) pojawiają się białe kwadraciki 2–3 px (styl pixel-art), ~40 sztuk w stałych pozycjach (generator z ustalonym seedem), na stałe w układzie ekranu; delikatne mruganie (alfa ±30%, losowa faza, ~0,5–1,5 Hz). Bez księżyca na razie.
  - Implementacja: tło w układzie ekranu za wszystkim (quad jako dziecko kamery skalowany do widoku albo kamera + canvas) — **musi działać także przy oddaleniu kamery w ujęciu końcowym (task 26)** i przy zmianie rozmiaru okna. Żadnych zasobów graficznych — gradient i gwiazdy generowane w kodzie (tekstura 1×64 / pojedyncze kwadraty).
  - Restart rundy: tło wraca natychmiast do dnia. Na ekranie końca tło zostaje takie, jakie było na wysokości wieży.
  - Zawalenie obniża `Height` → tło płynnie się „rozjaśnia” w tył (OK, zamierzone).
  - **Czytelność:** tekst HUD jest biały z obrysem — sprawdź na każdym stopniu tła (w tym najjaśniejszym punkcie zachodu); napisy popupów (PERFECT, „+$N”, synergie, GOLD!) nie mogą znikać na żadnym tle; jeśli któryś zniknie, dodaj obrys i napisz w `COMMS.md`.
  - EditMode: interpolacja kolorów (Height 0, 5, 10, 25, 40, 80 → oczekiwane wartości policzone ręcznie z tabeli), alfa gwiazd (0 przy H ≤ 20, 0,5 przy H = 26, 1 przy H ≥ 32).
  - Play Mode: screeny tła dla Height 0, 10, 20, 30, 40 do `COMMS.md`; jam bez zmian (stały kolor nieba).

## Zrobione (jam, do `39a94ec` na itch.io)

- [x] Input na telefonie (platforma zostaje w miejscu bez dotyku).
- [x] Fale pięter → zastąpione pojedynczymi piętrami co 0,7 s na zmianę.
- [x] Koniec rundy, ekran wyniku, restart bez przeładowania sceny.
- [x] Czytelny HUD na telefonie.
- [x] Grafiki pięter (pixel art z `Tools/floor_sprites.py`).
- [x] Deploy na itch.io.
