# Taski

Dopisuj zadania jako `- [ ] opis`. Claude obserwuje ten plik, wykonuje nowe zadania i odhacza je jako `- [x]`.

## Do zrobienia

Kolejność = priorytet. Szczegóły zasad gry: `GDD.md`. Po każdym tasku: build WebGL, uruchom lokalnie (konsola bez wyjątków), commit, deploy.

- [x] **Napraw input na telefonie (PRIORYTET).** Gdy palec nie dotyka ekranu, platforma ucieka do lewej krawędzi: `TowerController.TryGetPointerWorldX` spada wtedy na `Mouse.current`, którego pozycja na telefonie to (0,0). Ma być: bez dotyku platforma **zostaje tam, gdzie była**; z dotykiem idzie za palcem; na komputerze dalej idzie za myszą. Np. ignoruj mysz, gdy `Application.isMobilePlatform` albo gdy był już jakikolwiek dotyk. Sprawdź: puszczenie palca nie przesuwa platformy.
- [x] **Wybór pięter — spadają falami.** Zamiast pojedynczych pięter co 1,2 s spawnuj **falę 2–3 pięter naraz**, na tej samej wysokości, w różnych miejscach X (odstęp min. 1,5 szerokości piętra, w granicach `playWidth`), w miarę możliwości różnych typów. Fala co ok. 1,8 s. Gracz realnie złapie jedno piętro z fali — to jego decyzja. Parametry (liczba w fali, odstęp, interwał) jako `[SerializeField]` w `TowerGame`.
- [x] **Koniec gry i restart.** Runda trwa **90 s**; licznik czasu widoczny w HUD (prawy górny róg). Po czasie: spawn się zatrzymuje, pokaż ekran końca z „Height”, „Money” i „Score = Height × 10 + Money” oraz napis „Tap to play again”. Tap/klik restartuje grę. Uwaga: bootstrap działa przez `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`, który **nie odpali się ponownie** po `SceneManager.LoadScene` — restart przez zniszczenie i ponowne utworzenie obiektów gry (albo reset stanu), nie przez przeładowanie sceny.
- [x] **Czytelny HUD na telefonie.** Czcionka min. 60 (przy referencji 1080×1920), **jedna statystyka na linijkę**, wyrównanie do lewej w lewym górnym rogu; timer w prawym górnym. Etykiety w kolorach typów pięter: Pop — pomarańczowy, Jobs — niebieski, Energy — żółty, Joy — zielony; wartości ujemne/BLACKOUT na czerwono. HUD nie może zasłaniać spadających pięter bardziej niż konieczne (półprzezroczyste tło pod tekstem jest OK).
- [ ] **Deploy na itch.io.** Na itch.io wciąż jest stara wersja `9f103222` — żaden z powyższych tasków nie został wysłany. Zacommituj wszystko, zbuduj WebGL, sprawdź lokalnie (konsola bez wyjątków), potem `.\Scripts\deploy.ps1 -SkipBuild -Targets WebGL`. Sprawdź `butler status kosmik123/0h-game-jam-2026:webgl`, że wersja = ostatni commit.

**⏸ PAUZA po deployu.** Autor testuje grę na telefonie. Nie zaczynaj kolejnych tasków, dopóki nie pojawią się nowe poniżej tej linii.

