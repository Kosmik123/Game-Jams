# GDD — 0h Game Jam 2026 („Stack City”)

Temat jamu: **stack**. Jam trwa 1 godzinę (powtórzona godzina przy zmianie czasu, 25.10.2026).
Plik jest źródłem prawdy o decyzjach projektowych — każdy agent czyta go przed pracą i dopisuje nowe decyzje.
Sekcje oznaczone **[DECYZJA]** są ustalone, **[PROPOZYCJA]** czekają na akceptację autora.

## Platforma i testowanie [DECYZJA]
- Gra **pionowa (portrait)**, mobile-first; testy wyłącznie przez build WebGL → itch.io (https://kosmik123.itch.io/0h-game-jam-2026) → przeglądarka telefonu.
- Autor nie ma dostępu do edytora Unity w trakcie jamu → **cała scena budowana w kodzie** (bootstrap `[RuntimeInitializeOnLoadMethod]`), bez edycji scen/prefabów.
- Grafika na razie generowana w kodzie (kwadraty z ramką, kolor = typ piętra).

## Rdzeń rozgrywki [DECYZJA]
- Na dole ekranu jest **platforma (podstawa wieżowca)**. Porusza się tylko w osi **X**: tam gdzie palec (lub kursor myszy), tam platforma. W osi Y stoi w miejscu. **Bez dotyku platforma zostaje w miejscu** (nie wraca do krawędzi).
- Z górnej krawędzi ekranu **spadają piętra falami**: 2–3 piętra naraz, na tej samej wysokości, w różnych miejscach X (odstęp ≥ 1,5 szerokości piętra), najlepiej różnych typów; fala co ~1,8 s. Gracz wybiera, które piętro złapać — to główna decyzja w grze.
- Piętro, które spadnie na szczyt wieży (przetnie jego wysokość nad wieżą), dokleja się do budynku z zachowaniem przesunięcia w X. Chybione piętro spada dalej i znika.
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
- Cele wysokości jako poziomy — otwarte, na razie nie robimy.

## Runda i wynik [DECYZJA]
- Runda trwa **90 s**. Po czasie spawn się zatrzymuje i pojawia się ekran końca: Height, Money, **Score = Height × 10 + Money**, napis „Tap to play again”.
- Tap/klik restartuje grę bez przeładowania sceny (bootstrap `AfterSceneLoad` nie odpala się ponownie po `LoadScene`).

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
