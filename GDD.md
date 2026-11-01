# GDD — 0h Game Jam 2026 („Stack City”)

Temat jamu: **stack**. Jam trwa 1 godzinę (powtórzona godzina przy zmianie czasu, 25.10.2026).
Plik jest źródłem prawdy o decyzjach projektowych — każdy agent czyta go przed pracą i dopisuje nowe decyzje.
Sekcje oznaczone **[DECYZJA]** są ustalone, **[PROPOZYCJA]** czekają na akceptację autora.

## Wersje gry: jam / post-jam [DECYZJA, 25.10.2026]
- Gra startuje od **sceny wyboru wersji** (`VersionSelect`, scena nr 0): przyciski **„Jam version”** i **„Post-jam version”**. Jeden build WebGL na itch.io zawiera obie wersje.
- **Jam version** = rozgrywka jak w buildzie z jamu (`39a94ec`), ale z rundą **40 s** jak w poprawce `bbd8628` (decyzja autora 25.10, 18:10), stała prędkość/interwał, bez PERFECT i samouczka. **Zamrożona na zawsze** — żadnych zmian rozgrywki ani UI, także poprawek błędów; cały rozwój tylko w post-jam. Wyjątek (decyzja autora 25.10, 21:15): przycisk „Menu” na ekranie końca.
- **Post-jam version** = dalszy rozwój: runda 40 s, PERFECT!, rosnąca trudność, samouczek i kolejne funkcje.
- Różnice między wersjami żyją w `TowerRuleset` (`JamRuleset`, `PostJamRuleset`); wspólny kod nie sprawdza wersji „ifami” rozsianymi po logice.
- Poniższe sekcje opisują wersję post-jam, chyba że zaznaczono inaczej.

## Platforma i testowanie [DECYZJA]
- **[DECYZJA 25.10]** Okno WebGL **540×720** (było 600×960 — za duże na typowy ekran). Dotyczy obu wersji.
- Gra **pionowa (portrait)**, mobile-first; testy wyłącznie przez build WebGL → itch.io (https://kosmik123.itch.io/0h-game-jam-2026) → przeglądarka telefonu.
- Autor nie ma dostępu do edytora Unity w trakcie jamu → **cała scena budowana w kodzie** (bootstrap `[RuntimeInitializeOnLoadMethod]`), bez edycji scen/prefabów.
- Grafika pięter: pixel art 32×32 (x4 → 128×128 PNG) w `Assets/_Project/Resources/Floors/<FloorType.Name>.png`, ładowany przez `Resources.Load<Sprite>`. Generuje je `Tools/floor_sprites.py` (czysty Python; grafikę zmienia designer). Wspólny styl: boczne filary w ciemniejszym odcieniu, szara płyta stropowa na dole (piętra ładnie się stykają), dominujący kolor = kolor typu w HUD. Platforma i tło nadal generowane w kodzie.

## Rdzeń rozgrywki [DECYZJA]
- **[DECYZJA post-jam, 25.10]** Zakres ruchu liczony dla **dachu** (tam spadają piętra): dach zawsze sięga obu krawędzi `playWidth`, platforma może wyjechać poza ekran (i tak zwykle jest pod ekranem). Przy myszy pod kursorem jest dach.
- **[DECYZJA post-jam, 25.10, zmienione 18:40]** Sterowanie: **dotyk** — przeciąganie względne (platforma przesuwa się o tyle, o ile palec, 1:1, w granicach `playWidth`); **mysz (PC)** — X platformy = X kursora, bez klikania (jak w jamie). Opis poniżej dotyczy wersji jamowej.
- Na dole ekranu jest **platforma (podstawa wieżowca)**. Porusza się tylko w osi **X**: tam gdzie palec (lub kursor myszy), tam platforma. W osi Y stoi w miejscu. **Bez dotyku platforma zostaje w miejscu** (nie wraca do krawędzi).
- Z górnej krawędzi ekranu **spadają piętra na zmianę**: pojedynczo co ~0,7 s, każde w innym miejscu X niż poprzednie (odstęp ≥ 1,5 szerokości piętra) i innego typu niż poprzednie. W powietrzu są naraz 2–3 piętra na różnych wysokościach; gracz wybiera, które złapać — to główna decyzja w grze. (Wcześniejsze „fale” 3 pięter naraz odrzucone — wyglądały jak ściana.)
- **Rosnąca trudność**: z wysokością wieży piętra spadają szybciej i częściej — `fallSpeed = min(3 + 0,15 × Height, 8)`, `spawnInterval = max(0,7 − 0,01 × Height, 0,4)`.
- **PERFECT!**: idealne lądowanie (przesunięcie < 12% szerokości) pokazuje unoszący się napis „PERFECT!” nad piętrem (oprócz uspokojenia wieży).
- **Samouczek**: na starcie każdej rundy napis „Move finger to catch floors” na środku; znika po pierwszym złapanym piętrze albo po 5 s.
- Piętro, które spadnie na szczyt wieży (przetnie jego wysokość nad wieżą), dokleja się do budynku z zachowaniem przesunięcia w X. Chybione piętro spada dalej i znika.
- **[DECYZJA post-jam, 25.10]** Kamera stoi w miejscu, a wieża zjeżdża w dół po każdym piętrze (szczyt na ~40% ekranu). Powód: ruch kamery w górę sprawiał, że spadające piętra wyglądały na chwilowo szybsze (zgłoszenie graczy). Poniższy opis dotyczy wersji jamowej.
- **Kamera** jedzie w górę razem z budynkiem: szczyt wieży trzyma się na ~40% wysokości ekranu, widać tylko kilka górnych pięter, a nad budynkiem zostaje wolna przestrzeń na sterowanie i decyzje gracza.

## Statystyki [DECYZJA — zaimplementowane]
Budynek jako „miasto w pionie”, każde piętro wpływa na statystyki (wartości w `FloorType.cs`):
| Typ | Kolor | Mieszkańcy | Miejsca pracy | Energia | Szczęście |
|---|---|---|---|---|---|
| Residential | pomarańczowy | +10 | 0 | −1 | −1 (tłok) |
| Office | niebieski | 0 | +10 | −1 | 0 |
| Power | żółty | 0 | 0 | +4 | −2 (smog) |
| Park | zielony | 0 | 0 | 0 | +3 |

- **Pieniądze**: dochód = min(mieszkańcy, miejsca pracy) × 0.2 $/s, tylko przy zasilaniu (`CityStats.cs`).
- **Energia < 0** → BLACKOUT: piętra (poza elektrowniami) ciemnieją, dochód = 0.
- **Szczęście < 0** → każdy punkt poniżej zera wyprowadza 5 mieszkańców.
- **Stabilność** (`TowerController.cs`): niestabilność += (przesunięcie/szerokość)² × 120 przy każdym piętrze; idealne trafienie (< 12% szerokości) uspokaja o 15. Przy 100% górne 3 piętra się zawalają, niestabilność spada do 40. Wieża kołysze się wizualnie proporcjonalnie do niestabilności.
- **[DECYZJA post-jam, 25.10]** Rewizja ekonomii (jam zostaje jak wyżej):
  - Biura nie zmieniają populacji; zamieniają zatrudnionych mieszkańców na pieniądze — `min(Pop, Jobs)` to liczba pracujących.
  - Zasilanie proporcjonalne: `PowerRatio = min(1, produkcja / zużycie)`, dochód × `PowerRatio`, ciemnieją górne piętra. Koniec skoku „jedno piętro za dużo → dochód 0”, który przy starcie (pierwsze piętro bez elektrowni) był regułą.
  - Szczęście > 0 daje +5% dochodu za punkt (maks. +50%), więc park ma sens także powyżej zera. Park +4 (było +3).
  - Wzorcowy „blok”: 2 mieszkalne + 2 biura + 1 elektrownia + 1 park = energia 0, szczęście 0, 4 $/s.
- **[DECYZJA post-jam, 25.10]** Kołysanie: wychylenie szczytu maleje z wysokością (`1 / (1 + H/15)`), wygina się głównie góra (`((i+1)/H)^1,5`), wysoka wieża kołysze się wolniej (`ω = 3/√(1 + H/15)`). Tylko efekt wizualny.
- **[DECYZJA post-jam, 25.10]** Rekord (`PlayerPrefs`, „NEW BEST!”), combo za serię PERFECT (mnożnik dochodu x2–x5, zerowany nieidealnym lądowaniem) i juice (wstrząs przy zawaleniu, „+$N” z wieży co sekundę, sprężyste lądowanie).
- **[DECYZJA post-jam, 25.10]** Piętra 4:3: szerokość 1 j., wysokość 0,75 j., grafiki 32×24 (×4 = 128×96) w `Resources/FloorsWide/`, generowane `Tools/floor_sprites.py --wide`. Kwadraty 32×32 w `Resources/Floors/` zostają dla wersji jamowej.
- **[DECYZJA post-jam, 26.10]** Dach niżej: szczyt wieży trzyma się na 25% wysokości ekranu (było 40%), żeby było więcej wolnego nieba do manewrowania. Jam bez zmian (40%).
- **[DECYZJA post-jam, 26.10]** Ekran końca: wieżowiec od platformy po dach wypełnia praktycznie cały ekran (margines ~3% z góry i z dołu), wyniki to wyśrodkowany tekst z obrysem na wierzchu, bez ciemnej zasłony; panel statystyk i timer znikają. Zastępuje układ „wieża w dolnych 2/3”.
- **[DECYZJA post-jam, 26.10]** Złote piętro: szansa 5% na spawn (od Height 3, jedno naraz, gwarancja po 25 spawnach), spada ×1,4 szybciej, pulsuje. Po złapaniu: +3 s, niestabilność −25, +$25, popup „GOLD!”. Bez statystyk i synergii, liczy się do Height. Liczby do strojenia po testach.
- **[DECYZJA post-jam, 26.10]** Tło zależne od Height: dzień (0) → zachód (10) → zmierzch (20) → noc (30) → głęboka noc (40+), gradient góra/dół, od 20 do 32 pojawiają się mrugające gwiazdy. Jam: stały kolor nieba.
- **[DECYZJA post-jam, 26.10]** Trudniej: premia za piętro `1,0 × max(0,3; 1 − H/40)` s (było 1,5), kara za zawalenie −4 s (było −3). Reszta czasu bez zmian; liczby do strojenia po rundach testowych.
- **[OTWARTE, 26.10]** Grafika złotego piętra jest podobna do elektrowni (obie żółte). Na razie zostaje; do zmiany, jeśli testy graczy pokażą mylenie obu w locie (opcje: więcej bieli/szampana w Gold albo odsunięcie Power od żółci razem z kolorem Energy w HUD).
- **[DECYZJA post-jam, 26.10]** Progi tła ×2,5 (zmiana po decyzji autora): dzień 0, zachód 25, zmierzch 50, noc 75, głęboka noc 100+; gwiazdy od 50 do 80. Tabela wysokości w opisie tła powyżej jest przestarzała.
- **[DECYZJA post-jam, 26.10]** Trudniej (robione przez menedżera, bez programisty): (1) **kara za pominięte piętro** — każde zwykłe piętro, które spadnie poza dolny brzeg ekranu, odejmuje 0,5 s (`MissedFloorPenalty`; złote jest bez kary; szczątki z zawalenia nie liczą się); (2) **combo bez limitu** — N-te idealne lądowanie z rzędu = combo N (od 2), dźwięk `perfect_N` zatrzymuje się na 5; (3) **mnożnik dochodu z combo** = `1 + 0,1 × (combo − 1)`: combo 2 → ×1,1, 3 → ×1,2, 4 → ×1,3 itd. (było ×combo, maks. ×5). HUD pokazuje „COMBO N (x1.2)”.
- **[DECYZJA post-jam, 26.10]** Nieregularne spawny: odstęp między piętrami = `SpawnInterval(Height)` × losowo od 0,5 do 1,5 (rozrzut ±50%, ta sama średnia, minimum 0,15 s). Jam: stałe odstępy.
- Cele wysokości jako poziomy — otwarte, na razie nie robimy.

## Koniec gry: bankructwo [DECYZJA 31.10.2026 — do implementacji, wersja post-jam]
Zastępuje timer i „Score = Height × 10 + Money”. Jam bez zmian.
- **Wynik = maksymalna osiągnięta wysokość.** Remis rozstrzyga **łączna suma zarobków** w rundzie. Rekord (`PlayerPrefs`) liczy wysokość, potem sumę. Kolaps nie obniża wyniku (liczy się maksimum).
- **Przegrana = bankructwo.** Saldo < 0 przez **8 s z rzędu** (odliczanie na HUD, reset po powrocie na plus) → game over. Game over także przy 0 pięter (po kolapsie). Zegar, premie/kary czasowe (piętro, perfect, kolaps, pominięte piętro) i combo znikają.
- **Saldo:** start **100 $**, limit bazowy **100 $**. Każdy **Bank** zwiększa limit o **150 $ × q**. Nadwyżka ponad limit przepada. Pieniądze się nie sumują do wyniku (wynik to wysokość), służą jako bufor.
- **Utrzymanie: 1 $/s za każde piętro** (banki też), liniowe — brak zaprojektowanego sufitu wysokości. **Pierwsze 8 pięter bez utrzymania** (inaczej 16–24% dobrych graczy bankrutuje przed 40. piętrem z samego pecha w losowaniu).
- **Ekonomia, jednostka = 10 ludzi (stare wartości ÷10)** [DECYZJA 31.10]: dom +1 ludność, biuro +1 miejsce pracy. Energia bez zmian (dom −1, biuro −1, elektrownia +4). **Zadowolenie: dom 0, biuro −1, elektrownia −2, park +4** (dodatnie efekty × `q`); dawniej „dom −1, biuro 0” — symulacja pokazała, że zamiana nic nie zmienia dla zbalansowanych wież (domów ≈ biur). `r = min(1, 4·ΣqP / (R+O))`. Zadowolenie < 0 odejmuje 0,5 ludności na punkt: `ludność = max(0, ΣqR + 0,5·zadowolenie)` (dawne „5 mieszkańców na punkt”). `pracownicy = min(ΣqO·r, ludność·r)`. **Dochód = `k` × pracownicy**; `k` do ustalenia (patrz wyniki symulacji; 9 $/s na pracownika, czyli biuro z domem i zasilaniem = 9 $/s, obniża próg `q*` do ok. 0,5–0,6). Zadowolenie > 0: +5% dochodu za punkt, maks. +50% [PROPOZYCJA — obniża `q*` o 0,1–0,15]. HUD pokazuje zadowolenie jako liczbę (jak dotąd). Bank bez statystyk poza pojemnością.
- **Jakość ułożenia `q` (0,5–1)** [ZROBIONE]: perfect (okno zależne od wysokości) = 1,0, w przeciwnym razie `q` maleje liniowo od 1 do `minQuality` (0,5) przy przesunięciu równym szerokości piętra. `q` mnoży efekty dodatnie piętra (ludność, praca, produkcja energii, zadowolenie parku, pojemność banku), nie zużycie energii, smog ani tłok, i nie utrzymanie. Gorzej ułożone piętro jest ciemniejsze. Próg opłacalności `q*` zależy od `k` (symulacja).
- **Tempo bez limitu** [ZROBIONE 1.11, pierwiastkowe]: od **maksymalnej** osiągniętej wysokości `H`: prędkość spadania `3 + 0,8·√H` (bez sufitu), odstęp między piętrami `0,7 / (1 + 0,1·√H)` (nie krócej niż 0,15 s), niestabilność z lądowania × `1 + √H/8`, okno perfect ÷ `1 + √H/10`. Wszystko w `PostJamConfig`. To tempo, a nie wzór, wyznacza granicę: gdy dokładność gracza spadnie poniżej `q*`, wieża zaczyna tracić pieniądze.
- **Złoty blok = zastrzyk gotówki:** po złapaniu natychmiast **+60 $** (ucięte limitem salda), niestabilność −25 jak dotąd, bez utrzymania, rzadki jak dotąd. Bez +3 s.
- **Synergie sąsiadów** [OTWARTE]: zostają jak dotąd (dom+park +1 zadowolenia, dom+elektrownia −1); bonus dojazdu (+0,3 $/s) przeliczyć na nową skalę dochodu. Symulacja ich nie obejmuje.
- **Banki**: zwykły typ piętra, losowany tak samo często jak inne (już w grze, bez mechaniki).
- **HUD:** saldo/limit, bilans na sekundę (dochód − utrzymanie), „zapas: N s”, odliczanie bankructwa, wysokość; wskaźnik dokładności vs. wymagana (`q*`) [PROPOZYCJA].

**Wyniki symulacji** (`Tools/econ_sim.py`: gracz zachłanny wybiera najlepsze z 3 pięter, 1 piętro na 1,5 s, miks losowy, 5 typów):
- Przy stałej jakości jest **klif**: średnie `q` powyżej progu → wieża żyje bez końca, poniżej → bankructwo ok. 18–23. piętra. Najniższe przeżywające `q` (wersja ze zmienionym zadowoleniem, gracz zachłanny): bez bonusu za zadowolenie `k` 0,5 → 0,85; 0,6 → 0,75; 0,7 → 0,7; 0,8 → 0,65; 0,9 → 0,6 (na człowieka; ×10 = $/s na pracownika). Z bonusem +5%/pkt próg spada o 0,1–0,15. Cel `q* ≈ 0,75`: **`k` = 0,6 (6 $/s na pracownika) bez bonusu** albo `k` = 0,5 z bonusem. Dom/biuro niosące zadowolenie: bez różnicy.
- Gdy jakość maleje z wysokością (to robi rosnące tempo), klif znika: wieża pada na wysokości zależnej od umiejętności (rzędu 200–600 pięter przy `k = 0,8–1,0`), czyli bez zaprojektowanego sufitu.
- Start: bez darmowych pięter 16–24% dobrych graczy pada przed 40. piętrem; przy 8 darmowych ≈ 2–5%.
- Liczby to punkt wyjścia, strojenie po graniu na telefonie.

## Runda i wynik [DECYZJA]
- Runda trwa **40 s** w post-jam (skrócone z 90 s — było za długo); wersja jamowa też 40 s (decyzja autora 25.10). Po czasie spawn się zatrzymuje i pojawia się ekran końca: Height, Money, **Score = Height × 10 + Money**, napis „Tap to play again”.
- Tap/klik restartuje grę bez przeładowania sceny (bootstrap `AfterSceneLoad` nie odpala się ponownie po `LoadScene`).

- **[DECYZJA post-jam, 25.10 — wstępna, do strojenia]** Czas rundy zależny od gry: start 25 s, +1,5 s × max(0,3; 1 − H/40) za piętro, +1 s za PERFECT, −3 s za zawalenie, limit 30 s. Dobry gracz gra dłużej; rosnąca trudność i malejąca premia w końcu go doganiają.

- **[DECYZJA post-jam, 25.10]** Synergie sąsiednich pięter (pary bezpośrednio nad sobą): Mieszkalne+Park +1 Joy, Mieszkalne+Elektrownia −1 Joy, Mieszkalne+Biuro +0,3 $/s. Każda nowa para pokazuje popup przy styku — gracz uczy się zasad z gry.

- **[DECYZJA post-jam, 25.10]** Dźwięk: proceduralne 8-bit SFX i pętla chiptune (C–Am–F–G, 120 BPM, 16 s) z `Tools/sound_gen.py` → `Resources/Audio/`. Na razie wersja generowana; w razie potrzeby podmienimy na ręcznie robione assety. Wersja jamowa bez dźwięku. Przycisk wyciszenia zapamiętywany.

## HUD [DECYZJA]
- Legacy `UI.Text` (`TowerHud.cs`), referencja 1080×1920, czcionka ≥ 60.
- Lewy górny róg: jedna statystyka na linijkę — Height, Money (+dochód/s), Pop, Jobs, Energy, Joy, Stability. Prawy górny róg: pozostały czas rundy.
- Kolory etykiet = kolory typów pięter: Pop pomarańczowy, Jobs niebieski, Energy żółty, Joy zielony. Ujemne wartości i BLACKOUT na czerwono. Dopuszczalne półprzezroczyste tło pod tekstem.

## Proces pracy (role)
- **Designer/menadżer** (agent Claude #1): pisze taski w `TASKS.md` i decyzje w tym pliku, rozwiązuje konflikty w `TASKS.md`.
- **Programista** (agent Claude #2): wykonuje taski z `TASKS.md` w kolejności, odhacza `- [x]`, pytania pisze pod taskiem jako `  > ❓ ...`.
- Po każdym tasku: build WebGL → test lokalny (np. `python -m http.server` w `Builds/webgl`, konsola bez wyjątków) → commit → deploy. Buildy/deploy robi jeden agent naraz (Unity blokuje projekt).

## Kod
- `Assets/_Project/Scripts/Tower/` (namespace `ZeroHour.Tower`): `TowerGame` (bootstrap, kamera, spawner), `TowerController` (platforma, piętra, stabilność), `FallingFloor`, `FloorType`, `CityStats`, `TowerHud`.
- `Assets/_Project/Scripts/Controls/`: `GameControls` (akcja Move), `MobileJoystick` (wyłączony flagą `isEnabled`).
- Build/deploy: `.\Scripts\build.ps1 -Targets WebGL`, potem `.\Scripts\deploy.ps1 -SkipBuild -Targets WebGL` (w trakcie jamu bez pytania). Edytor Unity musi być zamknięty; budujemy na 6000.3.27f1.
