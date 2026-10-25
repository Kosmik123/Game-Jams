# Taski

Dopisuj zadania jako `- [ ] opis`. Claude obserwuje ten plik, wykonuje nowe zadania i odhacza je jako `- [x]`.

> 📨 Programista: zaproponowałem osobny plik do rozmów między nami — **`COMMS.md`** (jest tam pierwsza wiadomość do Ciebie). Tę notkę możesz usunąć, gdy przeczytasz.

## Do zrobienia

Kolejność = priorytet. Szczegóły zasad gry: `GDD.md`. Po każdym tasku: build WebGL, uruchom lokalnie (konsola bez wyjątków), commit, deploy.

- [x] **Napraw input na telefonie (PRIORYTET).** Gdy palec nie dotyka ekranu, platforma ucieka do lewej krawędzi: `TowerController.TryGetPointerWorldX` spada wtedy na `Mouse.current`, którego pozycja na telefonie to (0,0). Ma być: bez dotyku platforma **zostaje tam, gdzie była**; z dotykiem idzie za palcem; na komputerze dalej idzie za myszą. Np. ignoruj mysz, gdy `Application.isMobilePlatform` albo gdy był już jakikolwiek dotyk. Sprawdź: puszczenie palca nie przesuwa platformy.
- [x] **Wybór pięter — spadają falami.** Zamiast pojedynczych pięter co 1,2 s spawnuj **falę 2–3 pięter naraz**, na tej samej wysokości, w różnych miejscach X (odstęp min. 1,5 szerokości piętra, w granicach `playWidth`), w miarę możliwości różnych typów. Fala co ok. 1,8 s. Gracz realnie złapie jedno piętro z fali — to jego decyzja. Parametry (liczba w fali, odstęp, interwał) jako `[SerializeField]` w `TowerGame`.
- [x] **Koniec gry i restart.** Runda trwa **90 s**; licznik czasu widoczny w HUD (prawy górny róg). Po czasie: spawn się zatrzymuje, pokaż ekran końca z „Height”, „Money” i „Score = Height × 10 + Money” oraz napis „Tap to play again”. Tap/klik restartuje grę. Uwaga: bootstrap działa przez `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`, który **nie odpali się ponownie** po `SceneManager.LoadScene` — restart przez zniszczenie i ponowne utworzenie obiektów gry (albo reset stanu), nie przez przeładowanie sceny.
- [x] **Czytelny HUD na telefonie.** Czcionka min. 60 (przy referencji 1080×1920), **jedna statystyka na linijkę**, wyrównanie do lewej w lewym górnym rogu; timer w prawym górnym. Etykiety w kolorach typów pięter: Pop — pomarańczowy, Jobs — niebieski, Energy — żółty, Joy — zielony; wartości ujemne/BLACKOUT na czerwono. HUD nie może zasłaniać spadających pięter bardziej niż konieczne (półprzezroczyste tło pod tekstem jest OK).
- [x] **Deploy na itch.io — zrobił designer** (wersja `42900e59`). Zasada od teraz: **builda i deploy robi designer**; programista tylko commituje. Jeśli programista chce sprawdzić kompilację buildem, najpierw upewnij się, że projekt nie jest zablokowany (`Temp\UnityLockfile`).

## Kolejka

- [x] **Grafiki pięter.** Gotowe sprite'y (pixel art 32×32 przeskalowany do 128×128) leżą w `Assets/_Project/Resources/Floors/<FloorType.Name>.png` (Residential, Office, Power, Park). W `FallingFloor.Init` ładuj `Resources.Load<Sprite>($"Floors/{type.Name}")` (fallback: obecny kwadrat, gdy null), `drawMode = Simple`, `color = Color.white`, skala tak, by sprite miał `size × size` jednostek (`size / sprite.bounds.size.x`). `SetPowered`: zasilone = `Color.white`, bez prądu = ciemny tint (np. `0.45, 0.45, 0.5`), elektrownie zawsze białe. `FloorType.Color` zostaje dla HUD. Generator grafik: `Tools/floor_sprites.py` (zmiany grafiki robi designer).
  > ✅ Designer: import poprawiony w `.meta` (commit `bf83a4ce`) — **Point**, **Single** (było Multiple, przez co `Resources.Load<Sprite>` mógł zwracać null/`_0`), bez kompresji. Nie ustawiaj `filterMode` w kodzie. Designer buduje i wysyła teraz wersję z grafikami — nie odpalaj buildów.
- [x] **Piętra spadają na zmianę, nie seriami.** Teraz fala 2–3 pięter spawnuje się w tej samej chwili i leci równo. Zmiana: spawnuj **pojedyncze piętra po kolei**, co `spawnInterval` ≈ **0,7 s**, każde w innym miejscu X niż poprzednie (odstęp od poprzedniego ≥ `waveSpacing` = 1,5 szerokości piętra), typ inny niż poprzedni. Efekt: w powietrzu są 2–3 piętra na różnych wysokościach, gracz dalej wybiera, ale nie ma „ściany” trzech naraz. Usuń/wyłącz parametry fali (`min/maxFloorsPerWave`), jeśli niepotrzebne.
- [ ] **Napis „PERFECT!”.** Gdy piętro wyląduje idealnie (warunek `perfectOffset` w `TowerController.Attach`, ten sam, który uspokaja wieżę), nad tym piętrem pojawia się napis **„PERFECT!”** (biały z obrysem, czcionka ~70 przy referencji 1080×1920), unosi się o ~1 jednostkę i znika w ~0,8 s. Napis podąża za światem (pozycja piętra przeliczona na ekran albo world-space canvas), nie przykleja się do HUD.
- [ ] **Rosnąca trudność.** Z każdym piętrem w wieży nowe piętra spadają szybciej i częściej: `fallSpeed = min(3 + 0,15 × Height, 8)` j/s, `spawnInterval = max(0,7 − 0,01 × Height, 0,4)` s. Dotyczy nowo spawnowanych pięter. Współczynniki jako `[SerializeField]` w `TowerGame`.
- [ ] **Samouczek na starcie.** Na starcie rundy na środku ekranu napis **„Move finger to catch floors”** (biały z obrysem, ~64). Znika płynnie (fade ~0,5 s) po złapaniu pierwszego piętra albo po 5 s — co pierwsze. Pokazuje się też po restarcie.

