# DESIGNER_MEMORY — przekazanie sesji designera (stan: 27.10.2026, ok. 18:40)

Gra "Pay With Pixels" (temat Everything Has a Price), 1-bit Jam 15, branch `1-bit-jam-15`, Unity 6000.3.27f1, WebGL, tylko PC (mobile anulowane). Pliki sterujące: `GDD.md` (rozdział STAN AKTUALNY ma pierwszeństwo), `TASKS.md` (FAZY 9-13, 10b-10f dopisane), `JAM_RULES.md`.

## Role i komunikacja
- Ja = designer (nazwa na comms `designer`, plik `.chat-name`). Agenci: `programista`, `grafik` (własne worktree, lokalne branche, mergują do `1-bit-jam-15`). Człowiek: `Kosmik`.
- Komenda `comms` nie jest w PATH: `python /c/Users/Michas/source/Comms-Agents-Chat/comms.py <read|send|wait|status>`; na Windows ustaw `PYTHONUTF8=1` (inaczej błąd Unicode).
- Do jednej osoby: `send --to NAZWA "tekst"`; publicznie tylko decyzje dla wszystkich.
- Nasłuch: `comms.py wait --mode direct --timeout 1800` jako zadanie w tle (run_in_background). Kończy się timeoutem (kod 2) po braku wiadomości, trzeba WZNAWIAĆ po każdym zakończeniu. Nie uruchamiaj z `&`/disown (odłącza się i nie powiadamia).
- Liczby balansu wolno mi zmieniać (zgoda Kosmika), kod zleca się programiście przez TASKS.md + comms.

## Build i deploy (wolno bez pytania, zgoda Kosmika)
1. Unity na głównym projekcie musi być ZAMKNIĘTE (sprawdź procesy Unity.exe z `Game Jams`, bez `Temp`; boty programisty w Temp nie przeszkadzają).
2. `git lfs checkout`, potem `Unity.exe -batchmode -nographics -quit -noaudio -projectPath "C:/Projekty gier/Unity/Game Jams" -logFile <log> -executeMethod PayWithPixels.Build.WebGLBuilder.BuildWebGL` (w logu: "WebGL build: Succeeded").
3. `butler push Builds/WebGL kosmik123/pay-with-pixels:html5 --userversion <hash>-<opis>`. Strona: https://kosmik123.itch.io/pay-with-pixels.
4. Ostatni deploy: `86a7a6f6-leaderboard` (44 MB) — tablica wyników, bossowie 10e, Poborca od długu 10f, INERCJA, SILENCE przerywany, waga cen, poprawieni Strażnicy.

## Zrobione (w buildzie)
3 strefy x (5 fal + boss), 8 wrogów + Poborca, 29 zysków (drzewka Dron/Dash/Krytyk, Dług jako broń, Rewers czasu, Blokada ceny), ceny zmysłów (w tym Cisza, Opóźnienie wejścia, Drogi sklep, Inercja), dług, kontrakty, REDEEM, fabuła, Ascension, trwałe save'y (localStorage + kod zapisu), tablica wyników (klasa Online, board `pay_with_pixels_score`, anonimowe logowanie, projekt podłączony: cloudProjectId 31bceb74-5233-4801-bb98-8aa6618d16f7, org kosmik123).

## Decyzje designerskie z tej sesji
- SILENCE nie wycisza na stałe: losowe krótkie ciszy (przerwa 6-12 s, cisza 1.5+0.5*stack s), jak FLICKER/INVERT.
- INERCJA: opóźniony start i hamowanie (0.10/0.22/0.40 s), dash poza inercją, z INPUT DELAY bierze się max startu.
- FAZA 10d: waga cen 1-3 (zniżka 0/-15/-30% na koszt zysku, nagroda za sprzedaż zmysłu x0.8/1/1.3), bez nowego UI poza etykietą zniżki.
- FAZA 10e: bossowie zmieniają WZORZEC przy progach HP (Toll Keeper ściana z przerwą od 50%, Banker LOAN CALL, Auctioneer 4 etapy). Strefy 4+: min. 4 wzorce, 3 fazy.
- FAZA 10f: szansa Poborcy = dług/MaxDebt(20), min. 10%, od 2. fali strefy, max 1 naraz, gwarancja w ostatniej fali przy długu >= 10; BEZ nowego UI (Kosmik: za dużo informacji).
- Strażnik: tarcza blokuje pociski z przodu, trzeba okrążać; złagodzony (obrót 0.7 rad/s, łuk 0.7, HP 4+d/4), grafik dał łuk tarczy i FX bloku.
- Wyniki botów (FAZA 12): careful 6% wygranych (cel 30-50% niespełniony, bot nie dashuje/nie unika), human 0%, średnia fala ok. 10.6; główna ściana to wejście do Z3 (fala 13). Decyzja: liczby zostają do uwag z gry Kosmika; następne dźwignie: HP/waga Oblężnika i Spawnera, x1.1 HP strefy 3.

## W kolejce
1. Kosmik testuje tablicę wyników na itch (L na tytule, wynik po runie, F12 przy "OFFLINE"). Jeśli błąd logowania: sprawdzić Authentication w Unity Dashboard; błąd przekazać programiście.
2. Uwagi z gry Kosmika -> ewentualny dalszy balans (Z3, Strażnik, Poborca 10f).
3. FAZA 13: nowe strefy 4+ (GDD v6 napiszę ja, po balansie), nowi bossowie, wrogowie, palety.
4. Grafik: ewentualny 9-slice tła areny (dyskusja z Kosmikiem, mała korzyść), ikony do nowych rzeczy na prośbę programisty.
5. Niezacommitowane zmiany Kosmika (URP, ProjectSettings poza cloudProjectId, arena_bg.png.meta, bg page old.png, page_bg.pdn) — sprząta Kosmik.

## Uwagi
- Panel przeglądarki dławi FPS i gra dźwięk w tle: zamykaj kartę po teście.
- Pamięć sesji (poza repo): `C:\Users\Michas\.claude\projects\C--Projekty-gier-Unity-Game-Jams\memory\` (plik `one-bit-jam-state.md`).
