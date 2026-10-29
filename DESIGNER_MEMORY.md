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
4. Deploy `86a7a6f6-leaderboard` (44 MB; tablica wyników, 10e, 10f, INERCJA, SILENCE przerywany, waga cen, Strażnicy) miał błędy tablicy, więc ZOSTAŁ COFNIĘTY: aktualnie na itch jest `74555e24-10e-rollback` (stan sprzed tablicy, bez 10f; Poborca po staremu w ostatniej fali). Rollback robi się przez worktree: `git worktree add --detach <Temp>/jam_rollback <commit>`, build tamtędy, `butler push`, potem `git worktree remove --force` (główne repo ma niezacommitowane zmiany Kosmika).

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
1. NAPRAWA TABLICY WYNIKÓW (kod jest w `1-bit-jam-15`, Kosmik: "naprawimy kiedyś indziej"): zebrać błędy z konsoli (F12, "OFFLINE"), sprawdzić Authentication w Unity Dashboard i środowisko, przekazać programiście; potem zbudować aktualne `1-bit-jam-15` i wrzucić na itch.
2. Uwagi z gry Kosmika -> ewentualny dalszy balans (Z3, Strażnik, Poborca 10f).
3. FAZA 13: nowe strefy 4+ (GDD v6 napiszę ja, po balansie), nowi bossowie, wrogowie, palety.
4. Grafik: ewentualny 9-slice tła areny (dyskusja z Kosmikiem, mała korzyść), ikony do nowych rzeczy na prośbę programisty.
5. Niezacommitowane zmiany Kosmika (URP, ProjectSettings poza cloudProjectId, arena_bg.png.meta, bg page old.png, page_bg.pdn) — sprząta Kosmik.

## Uwagi
- Panel przeglądarki dławi FPS i gra dźwięk w tle: zamykaj kartę po teście.
- Pamięć sesji (poza repo): `C:\Users\Michas\.claude\projects\C--Projekty-gier-Unity-Game-Jams\memory\` (plik `one-bit-jam-state.md`).

## Aktualizacja 29.10.2026
- Poborca: szansa od długu (10f), ucieczka po kradzieży, zwrot zysku po zabiciu, +12% HP na spotkanie, spawn po losowej liczbie wrogów; zabicie nie zmniejsza długu ani nie daje monet (decyzja Kosmika). Szczegóły w GDD.md (STAN AKTUALNY).
- Toast: ramka z wypełnieniem poza areną; w sklepie wiersz długu jest w tytule, pas y 0.82-0.90 wolny. Tablica wyników działa na itch (naprawy: wyjątki WebGL, klawisz L w tej samej klatce, kolumny nagłówka). Aktualny deploy: `b3d37833-toast-box`.
- Kontrakty: 14 (6 dodanych), tymczasowe ceny cofają się generycznie. Skrypt buildu czeka na zamknięcie Unity na głównym projekcie.

## Stan na koniec sesji 29.10.2026
- **Na itch:** `f7d72470-economy` (44 MB). Wszystko z tej sesji jest zacommitowane na `1-bit-jam-15`. Jeśli Kosmik ma Unity otwarte na głównym projekcie, batch build się nie uda: skrypt buildu ma czekać na zamknięcie (pętla po `Unity.exe` bez `Temp`).
- **Ekonomia (f7d72470):** mnożnik kosztu zysku wg strefy: fale 1-6 x1.3, 7-12 x1.5, 13-18 x1.7; bonus +25% monet w strefach 2-3 USUNIĘTY. Powód: dochód rósł szybciej niż ceny (careful: dochód/fala 15 -> 52 -> 94, koszt zakupu 10 -> 29 -> 46), Kosmik mógł kupić 2-3 rzeczy na sklep. Po zmianie careful: dochód Z2 52 -> 37, Z3 94 -> 75; zakupy z dochodu jednej fali Z2 1.8 -> 1.3.
- **Boty (tools/SmokeTest):** `powershell -File tools\SmokeTestalance.ps1 -Strategy careful|human|random|nobuy -Runs N -Tag _x`, wynik CSV w `%TEMP%\jam_bot_<strategia><tag>.csv`, analiza `python -I tools/SmokeTest/analyze.py pliki.csv` (+ `--buys`). Boty liczą w kopiach w Temp (kopiują repo razem z niezacommitowanymi zmianami!). careful = spłaca dług, kupuje 1 rzecz/sklep, punktacja zysków i kar cen; human = jak careful, ale 65% monet i celność x1.5 gorsza. Bot nie dashuje i nie unika pocisków, więc nie mierzy trudności bossów dla człowieka. Próby 8-12 runów = szum około +-15 pkt.
- **Wyniki po zmianie ekonomii (t2):** careful 8% wygranych, średnia fala 12.8; human 8%, 11.4; zgony głównie Z2 fale 3-5 i pierwsza fala Z3. Żaden bot nie zginął na Bankerze ani na Toll Keeperze. Żaden nie pokonał Auctioneera.
- **Otwarte problemy:** (1) Kosmik ginie w strefie 2 ("w banku") albo na bossie 1 (Toll Keeper), NIE na Bankerze; przyczyna zgonu na bossie 1 nieznana (boty go przechodzą w 16-24 s). (2) ściana Z2 fale 3-5 i wejście do Z3 (HP 12-45% po Z2) nie jest problemem ekonomii. (3) Banker/Auctioneer niezmierzone botem.
- **Zmiany UI/kodu tej sesji:** toast z ramką poza areną, wolny pas w sklepie (info o długu w tytule); tablica wyników (naprawy: wyjątki WebGL = `ExplicitlyThrownExceptionsOnly`, klawisz L w tej samej klatce, nagłówki kolumn); Poborca (ucieczka, zwrot zysku, +12% HP, brak monet i brak redukcji długu); Strażnik z osobną tarczą 8-kierunkową; 14 kontraktów; nowy kontrakt zdradza mały druk w toaście po podpisaniu (zostawione celowo na prośbę Kosmika).
- **Procedury:** Kosmik edytuje to samo drzewo i czasem stage'uje pliki: przed commitem sprawdź `git status`/`git diff --cached`, nie commituj jego WIP; commituj tylko własne pliki. Serwer comms (127.0.0.1:8765) był wyłączony: w tej sesji ja robiłem też robotę programisty i grafika (zgoda Kosmika). Brak PIL: obrazki edytuj przez PowerShell `System.Drawing`.
- **Do zrobienia:** zebrać od Kosmika, co zabija go na bossie 1 i w Z2; ewentualnie dodać log fazy bossa przy zgonie; GDD v6 i FAZA 13 (strefy 4+) po uspokojeniu balansu; test botami po każdej zmianie ekonomii/wrogów.
