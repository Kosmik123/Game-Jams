# GDD — 0h Game Jam 2026 („Stack City”)

Temat jamu: **stack**. Jam trwa 1 godzinę (powtórzona godzina przy zmianie czasu, 25.10.2026).
Plik jest źródłem prawdy o decyzjach projektowych — każdy agent czyta go przed pracą i dopisuje nowe decyzje.
Sekcje oznaczone **[DECYZJA]** są ustalone, **[PROPOZYCJA]** czekają na akceptację autora.

## Platforma i testowanie [DECYZJA]
- Gra **pionowa (portrait)**, mobile-first; testy wyłącznie przez build WebGL → itch.io (https://kosmik123.itch.io/0h-game-jam-2026) → przeglądarka telefonu.
- Autor nie ma dostępu do edytora Unity w trakcie jamu → **cała scena budowana w kodzie** (bootstrap `[RuntimeInitializeOnLoadMethod]`), bez edycji scen/prefabów.
- Grafika na razie generowana w kodzie (kwadraty z ramką, kolor = typ piętra).

## Rdzeń rozgrywki [DECYZJA]
- Na dole ekranu jest **platforma (podstawa wieżowca)**. Porusza się tylko w osi **X**: tam gdzie palec (lub kursor myszy), tam platforma. W osi Y stoi w miejscu.
- Z górnej krawędzi ekranu **spadają piętra** — kwadraty różnych typów (różne grafiki/kolory).
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
- **HUD** u góry ekranu (`TowerHud.cs`, legacy `UI.Text`): wysokość, pieniądze i dochód, mieszkańcy/praca, energia, szczęście, stabilność.
- Wynik: wysokość + pieniądze; ewentualnie cele wysokości jako poziomy (otwarte).

## Kod
- `Assets/_Project/Scripts/Tower/` (namespace `ZeroHour.Tower`): `TowerGame` (bootstrap, kamera, spawner), `TowerController` (platforma, piętra, stabilność), `FallingFloor`, `FloorType`, `CityStats`, `TowerHud`.
- `Assets/_Project/Scripts/Controls/`: `GameControls` (akcja Move), `MobileJoystick` (wyłączony flagą `isEnabled`).
- Build/deploy: `.\Scripts\build.ps1 -Targets WebGL`, potem `.\Scripts\deploy.ps1 -SkipBuild -Targets WebGL` (w trakcie jamu bez pytania). Edytor Unity musi być zamknięty; budujemy na 6000.3.27f1.
