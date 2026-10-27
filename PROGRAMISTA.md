# PROGRAMISTA: notatki przekazania (PAY WITH PIXELS, 1-Bit Jam 15)

Stan na 2026-10-27. Plik dla kolejnej sesji programisty; GDD.md i TASKS.md prowadzi designer.

## Rola i zasady pracy
- Zespół: **designer** (*.md, TASKS.md, GDD.md; buduje WebGL i pushuje na itch.io), **grafik** (Resources art, szablon WebGL, Marketing), **Kosmik** (człowiek, testuje i decyduje). Unity 6000.3.27f1, URP, WebGL, tylko PC (mobile anulowane).
- Programista pracuje w worktree `.claude/worktrees/keen-booth-501416`, branch `claude/prog-1bit`. Ścieżki programisty: `Assets/_Project/Scripts`, `Shaders`, `Scenes`, `Prefabs`, `Editor`, `Tools`, `Plugins`, `Packages/manifest.json`.
- Zmiany do `1-bit-jam-15`: commit, `git merge 1-bit-jam-15` w worktree, potem `git merge --ff-only <hash>` w głównym checkoucie. Agenci nie pushują; build i push na itch robi designer.
- Commity kończyć: `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`.
- Komunikacja: `PYTHONIOENCODING=utf-8 PYTHONUTF8=1 comms.bat listen` (stały nasłuch, Monitor wygasa po 30 min), do jednej osoby `comms.bat send --to designer "..."`. Po każdym bloku prac raport prywatny do designera.
- Nie ruszać FAZY 13 (nowe strefy): GDD v6 pisze designer.

## Zrobione (zmergowane do 1-bit-jam-15)
- **FAZA 9:** klasa `Save` (localStorage przez `Assets/_Project/Plugins/WebGL/PwpStorage.jslib`, zapas PlayerPrefs, migracja starych danych), EXPORT/IMPORT SAVE CODE w menu pauzy.
- **FAZA 10:** 13 zysków warunkowych (drzewka Dron / Dash / Krytyk, DEBT WEAPON, TIME REWIND, PRICE LOCK), ceny SILENCE (przerywany), INPUT DELAY, INERTIA, PRICEY SHOP; 10d: waga cen 1-3 (zniżka kosztu zysku 0/15/30%, nagroda za sprzedaż x0.8/1/1.3); 10e: wzorce bossów (Toll Keeper ściana z przerwą, Banker LOAN CALL i obrotowa przerwa, Auctioneer 4 etapy z liniami); 10f: Poborca z szansą dług/MaxDebt.
- **FAZA 11:** `Core/Online.cs` (Unity Leaderboards, tablica `pay_with_pixels_score`, anonimowy login, nazwa gracza, kolejka wyniku offline, timeouty bez `Task.Delay`), nakładka TOP 10 w `GameUI` (klawisz L, zmiana nazwy N). Środowisko domyślne (Leaderboards i Authentication nie mają środowisk w panelu).
- **FAZA 12:** bot ze strategią `human` (65% monet) i dashem (`Player.TryDash`), log zakupów; raporty przed/po wysłane designerowi.
- **Poprawki:** Swarm (clamp prędkości, max 2.7), Strażnik złagodzony (łuk tarczy 0.7, obrót 0.7 rad/s, HP 4+d/4, efekt bloku i podpowiedź), pasek HP dla wrogów z >=8 HP (bez Strażnika), HP strefy 3 x1.2.

## Otwarte
- Wyniki serii botów po 10f (`%TEMP%\jam_bot_*_v6.csv`) nie zostały zaraportowane designerowi. Wcześniej (v5, z dashem): careful 7%, human 7% wygranych.
- Kosmik testuje tablicę wyników na itch (build 86a7a6f6). Przy komunikacie OFFLINE: sprawdzić ID tablicy, anonimowe Authentication, `cloudProjectId` w `ProjectSettings`, konsolę przeglądarki.
- Decyzja designera: liczby balansu zostają, czekamy na grę Kosmika. Cele bezwzględne (careful 30-50% wygranych) niespełnione; bot jest dolną granicą trudności.

## Testy i boty
- Smoke test (kompilacja i pełny run): `powershell -NoProfile -File Tools/SmokeTest/run.ps1`, oczekiwane `exit 0`; log `%TEMP%\jam_smoketest\unity.log`. Zmienne: `SMOKE_WEAPON` 0/1/2, `SMOKE_ALL=1` (wszystkie zyski na maksimum).
- Boty: `Tools/SmokeTest/balance.ps1 -Strategy careful|human|random|nobuy|buyall -Runs N -Tag _x [-Root <repo>]`, CSV `%TEMP%\jam_bot_<strategia><tag>.csv` i `.buys.csv`, log `%TEMP%\jam_bot_<strategia><tag>\unity.log`. Analiza: `python Tools/SmokeTest/analyze.py [--buys] pliki.csv`.
- Boty kopiują repo do `%TEMP%`, więc edycja kodu w trakcie jest bezpieczna; 3-4 boty równolegle działają. Wersja "przed": `git worktree add --detach %TEMP%\jam_base <commit>` i `-Root`.
- Czekanie na boty: filtrować `Get-CimInstance Win32_Process -Filter "Name='Unity.exe'"` po `CommandLine -match 'testFilter'` (nie po samej nazwie, bo Unity użytkownika też jest `Unity.exe`).
- Boty i testy w batchmode mają wyłączoną sieć (`Online` nie inicjuje się przy `Application.isBatchMode`).

## Pułapki
- Długie heredoki Pythona w Bash potrafią się wywalić ("unexpected EOF"): zapisać skrypt do `Tools/SmokeTest/_patchN.py`, uruchomić i usunąć.
- Pliki mają mieszane CRLF/LF; skrypty patchujące czytać z `newline=''`.
- `AreaBlast.Spawn(parent, pos, delay, radius, damage)`: delay przed radius.
- Nowe pakiety UPM wpisywać do `Packages/manifest.json` (wersje z packages.unity.com). `InitializationOptions.SetProjectId` nie istnieje; użyć `SetOption("com.unity.services.core.cloud-project-id", id)`.
- PowerShell: alias `R` to Invoke-History; w Bash `sleep` jest zablokowany (używać Monitor).
- Komunikaty comms wymagają `PYTHONIOENCODING=utf-8 PYTHONUTF8=1`.
