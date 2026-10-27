# Rola GRAFIK (notatka dla nowej sesji)

**W nowej sesji jestem GRAFIKIEM** (artysta) w projekcie "PAY WITH PIXELS" (1-Bit Jam 15, temat "Everything Has a Price", dokładnie 2 kolory). Notatka jam-roles (ja = designer) dotyczy innego jamu, 0h; tutaj współpracuję z agentami **designer** i **programista** oraz człowiekiem Kosmikiem przez `comms`. Zob. one-bit-jam-state, one-bit-jam-itch-deploy.

**Co robię:** rysuję sprity/ikony/UI/ekrany fabuły/czcionkę/muzykę/materiały na stronę itch.io (cover, baner, screeny, GIF), na prośby designera (TASKS.md sekcje "Grafik" i prywatne wiadomości) i z własnej inicjatywy, gdy brakuje zadań. Nie programuję i nie ruszam ProjectSettings.

**Dostarczanie assetów**
- Pliki trafiają do `Assets/_Project/Resources/{Sprites,Icons,UI,Story,Fonts,Music}` i `Marketing/`. Generuję je skryptami Pythona (PIL) w scratchpadzie, z deterministycznymi .meta (guid z md5).
- Merge tylko własnych ścieżek: commit w worktree → `git merge 1-bit-jam-15` (z `GIT_LFS_SKIP_SMUDGE=1`) → w głównym checkoucie `git merge --ff-only <branch grafika>` → **`git lfs checkout <ścieżki>`**, bo png/gif są w LFS i po merge zostają wskaźnikami. Nigdy nie pushuję (push robi designer).
- Po każdej dostawie wysyłam designerowi prywatną wiadomość (`comms send --to designer`) z listą nazw plików (comms-private-messages).

**Konwencje**
- Sprity białe na przezroczystym, shader mapuje paletę; sprity patrzą w GÓRĘ (Art.FaceRotation, kod obraca, snap 90°). Gdy designer pisze "przód po prawej", przypomnieć, że przód to góra.
- Render 480x270 (było 320x180), PPU 16, Point. Ikony 16x16 (`icon_gain_*` zyski, `icon_<nazwa>` ceny), medale 24x24.
- Mobile/touch ANULOWANE przez Kosmika (assety touch zostają nieużywane).

**Praca na żywo**
- Nasłuch: Monitor z `PYTHONIOENCODING=utf-8 PYTHONUTF8=1 comms.bat listen 2>&1`, timeout 1800000, po wygaśnięciu (30 min) uruchomić ponownie. Gdy designer ogłasza przerwę: nic nie zaczynać, tylko nasłuch.
- Screenshoty/GIF z buildu: lokalny `python -m http.server 8099` w `Builds/webgl` + odbiornik POST dataURL, przeglądarka z `?debug=1` (F1 god, F3 następna fala, F4 następna strefa), viewport 960x568 daje canvas 960x540 (2x, czysty downsample do 480x270). Klawisze syntetyczne działają w grze (poza startem Space na tytule, tam prawdziwy klawisz). Zawsze wyciszać M, zob. mute-audio-in-browser-tests.

**Otwarte na koniec sesji:** propozycja Kosmika, by tło areny było 9-slice (odpowiedź wysłana na comms, brak decyzji designera/programisty); GDD v6 (nowe strefy 4+, bossowie, wrogowie) od designera po FAZIE 12; ewentualne prośby programisty o sprity (np. dla drzewek zysków).

**Why:** proces ukształtował się przez jam; pominięcie `git lfs checkout` dawało "uszkodzone" pliki (banner itch) i błędy merge.
**How to apply:** na starcie sesji w tym projekcie przyjąć rolę grafika, uruchomić nasłuch comms i sprawdzić TASKS.md (sekcje "Grafik").
