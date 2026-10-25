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

## Typy pięter (obecne w kodzie)
| Typ | Kolor |
|---|---|
| Residential (mieszkania) | pomarańczowy |
| Office (biura) | niebieski |
| Power (elektrownia) | żółty |
| Park | zielony |

## Statystyki [PROPOZYCJA — do akceptacji]
Budynek jako „miasto w pionie”, każde piętro wpływa na statystyki:
| Typ | Daje | Kosztuje |
|---|---|---|
| Residential | +Mieszkańcy | −Energia, −Szczęście (tłok) |
| Office | +Pieniądze (wymaga mieszkańców do pracy) | −Energia |
| Power | +Energia | −Szczęście (smog) |
| Park | +Szczęście | nic nie produkuje |

- **Energia < 0** → piętra gasną (nie produkują).
- **Szczęście < 0** → mieszkańcy się wyprowadzają.
- **Stabilność**: przesunięcie piętra względem niższego chwieje wieżą; zbyt duże → zawalenie górnych pięter (ryzyko vs nagroda).
- Wynik: wysokość + pieniądze; ewentualnie cele wysokości jako poziomy.

## Kod
- `Assets/_Project/Scripts/Tower/` (namespace `ZeroHour.Tower`): `TowerGame` (bootstrap, kamera, spawner), `TowerController` (platforma + piętra), `FallingFloor`, `FloorType`.
- `Assets/_Project/Scripts/Controls/`: `GameControls` (akcja Move), `MobileJoystick` (wyłączony flagą `isEnabled`).
- Build/deploy: `.\Scripts\build.ps1 -Targets WebGL`, potem `.\Scripts\deploy.ps1 -SkipBuild -Targets WebGL` (w trakcie jamu bez pytania). Edytor Unity musi być zamknięty; budujemy na 6000.3.27f1.
