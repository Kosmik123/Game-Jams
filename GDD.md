# GDD — roboczy tytuł: PAY WITH PIXELS

Jam: 1-bit Jam, temat **Everything Has a Price**, 2 dni. Zasady w `JAM_RULES.md`. Status: **DRAFT v1** (do uwag zespołu).

## Pitch
Top-down arena roguelite w 2 kolorach. Po każdej fali odwiedzasz sklep, w którym **każda zdolność ma cenę, ale walutą nie są tylko monety: płacisz swoimi zmysłami**. Im silniejszy jesteś, tym gorzej gra wygląda i się czyta (mgła, pikselizacja, migotanie palety). Cena jest dosłownie widoczna na ekranie. Możesz też **sprzedać** zmysł za monety.

Fantazja gracza: "Jestem potężny, ale ledwo widzę, co się dzieje."

## Pętla
1. Fala wrogów na jednym ekranie (arena), ok. 30-60 s.
2. Wrogowie zostawiają monety (białe kropki).
3. Sklep: 3 oferty. Każda = **zysk + cena (efekt wizualny/sterowania trwający do końca runu)**. Można zapłacić monetami za zysk, a cenę przyjmuje się zawsze. Dodatkowo ofert "Sprzedaj": przyjmij cenę, dostań monety.
4. Następna fala, trudniejsza. Po fali 8 boss. Zgon = koniec runu, wynik = fala.

## Sterowanie (web, klawiatura+mysz)
WASD ruch, mysz celowanie, LPM strzał, Spacja dash (odblokowany ze sklepu). Mobile poza zakresem.

## Wygląd i 2 kolory
- Dwa kolory na całym ekranie. Domyślnie tło ciemne, obiekty jasne.
- Końcowy shader wymusza 2 kolory (próg luminancji) i obsługuje ceny (poniżej).
- Paleta zmienia się co kilka fal (kilka par kolorów). Inwersja = efekt ceny.
- Styl: proste sylwetki, pixel art 16x16 / 32x32, bez półprzezroczystości.

## Ceny (efekty globalne, w shaderze/kamerze/kodzie)
| Cena | Efekt | Implementacja |
|---|---|---|
| Mgła | widać tylko okrąg wokół gracza (promień maleje z każdą kupioną ceną) | maska w shaderze |
| Pikselizacja | rozdzielczość renderu spada (czytelność gorsza) | render texture o mniejszej rozdzielczości |
| Migotanie | co ~8 s paleta odwraca się na 1.5 s | inwersja w shaderze |
| Ociężałość | celowanie z opóźnieniem/wygładzeniem | kod |
| Ciężkie nogi | wolniejszy ruch | kod |
| Głuchota | brak dźwięku (tylko flavor, tanie) | kod |

## Zyski (oferty)
Szybsze strzały, +max HP, dash, strzały przebijające, magnes monet, strzał 3-promieniowy, leczenie, tarcza. Każdy zysk dostaje przypisaną cenę losowo (lub z puli), z różną "kwotą" (monety + liczba stacków ceny).

## Wrogowie (MVP)
1. Goniec: biegnie na gracza.
2. Strzelec: trzyma dystans, strzela.
3. Tank: wolny, dużo HP.
4. Boss (fala 8): wzorce pocisków.

## Zakres
- **MVP (dzień 1):** ruch, strzał, 3 wrogowie, fale, monety, sklep z 5 zyskami i 3 cenami, shader 2 kolorów, ekran śmierci.
- **Dzień 2:** pozostałe ceny, dash, boss, zmiany palet, dźwięk, polish, tytuł/menu, build WebGL na itch.io.
- **Ciąć w pierwszej kolejności:** boss, "Sprzedaj", dźwięk.

## Odpowiedzialności
- designer: GDD, TASKS, balans, decyzje, build/deploy.
- programista: kod gry (Unity, repo ma szkielet w `Assets/_Project`, kamera w `Scripts/Camera Fit`).
- grafik: sprity, tło, UI, paleta; assety w `Assets/_Project/Graphics`.

---

# GDD v2 — rozszerzenie (MVP gotowe po ~30 min, mamy 2 dni, więc celujemy w pełną grę)

Status v1: MVP działa (WebGL, tytuł, fale, sklep, 3-4 ceny, boss fali 8, dźwięk). Teraz **głębia, treść, czytelność i szlif**. Nie zmieniamy rdzenia, tylko go pogłębiamy.

## Filary v2
1. **Cena jest widoczna i odczuwalna** (zmysły), ale **też ma konsekwencje w systemach** (dług, synergie).
2. **Decyzje w sklepie mają być trudne**: zysk vs cena vs dług, z synergiami i pułapkami.
3. **Run ma strukturę i narrację**: 3 strefy, każda z własnym wyglądem, wrogami i bossem.
4. **Gra jest czytelna mimo kar** (czytelny gracz, pociski wrogów, UI nigdy zasłonięte).

## Struktura runu
- 3 strefy po 5 fal + boss strefy (łącznie 15 fal + 3 bossy). Wygrana = pokonanie bossa strefy 3.
- Każda strefa: własna para palet (zmienia się w obrębie strefy co 2 fale), własny zestaw wrogów, własne przeszkody na arenie (ściany, kolce), nowy boss.
- Przed każdą strefą krótki ekran "ZONE N" z jednym zdaniem tematu (np. "THE TOLL ROAD").

## Dług (nowa mechanika centralna)
- Zamiast tylko "masz za mało monet": **kredyt**. W sklepie można kupić zysk na dług (brak monet).
- Dług rośnie o 20% (zaokrąglony w górę) po każdej fali (odsetki).
- Dług > 0 po fali 4 w strefie: pojawia się **Poborca** (Collector), mini-boss który ściga gracza i po dotknięciu **zabiera zysk** (usuwa losowy zdobyty zysk, zostawia cenę). Zabicie Poborcy spłaca dług.
- Dług widoczny w HUD jako licznik z ikoną.

## Zyski (docelowo ~16)
Obecne: szybsze strzały, +max HP, strzał potrójny, leczenie, przebijanie, dash, magnet, tarcza. Dodać:
- Rykoszet (pociski odbijają się od ścian), Wybuch (zabici wrogowie wybuchają), Wampiryzm (zabójstwa leczą co N), Spowolnienie (aura), Pet-dron (towarzysz strzela), Krytyk (szansa x3), Minowanie (dash zostawia miny), Zamrożenie czasu (jednorazowo na falę).
Każdy zysk ma 3 poziomy (kolejny zakup ulepsza).

## Ceny (docelowo ~9 + modyfikatory)
Obecne: Mgła, Pikselizacja, Migotanie, Ociężałość (sluggish aim), Ciężkie nogi. Dodać:
- **Odwrócone sterowanie** (krótko co jakiś czas), **Cisza** (brak dźwięku, tylko dla tych co mają dźwięk), **Zbliżenie** (kamera ciaśniej: gorzej widać wrogów), **Kruchość** (-1 max HP), **Drogie życie** (leczenie kosztuje 2x), **Głód** (monety znikają szybciej z ziemi).
- Ostrzeżenie: Migotanie może być męczące, dodać opcję "Reduce flicker" w menu (domyślnie wyłączona, mocno wyeksponowana).

## Synergie i pułapki (decyzje)
- Pary zysk+cena z bonusem: np. **Mgła + Krytyk** (kryt x5 w ciemności), **Pikselizacja + Pet-dron** (dron rośnie), **Ciężkie nogi + Tarcza** (tarcza dłuższa).
- Oferta sklepu pokazuje podpowiedź "SYNERGY" gdy para pasuje do tego co masz.
- **Reroll** oferty za monety (koszt rośnie), **Haggle**: za moneta+ryzyko zamień cenę oferty (zła cena na inną).

## Wrogowie (docelowo ~9 + 3 bossy)
Obecni: Goniec, Strzelec, Tank. Dodać: Skaczek (dash na gracza), Oblężnik (pocisk obszarowy z ostrzeżeniem), Rój (małe, grupami), Strażnik (tarcza z przodu), Spawner (rodzi gońców), Poborca (zobacz Dług). Bossy: strefa 1 (obecny, "The Toll Keeper"), strefa 2 (wielofazowy, "The Banker"), strefa 3 ("The Auctioneer": sprzedaje twoje zyski podczas walki).

## Meta (między runami)
- Ekran końca runu: statystyki, rekord (localStorage/PlayerPrefs), **odblokowania**: nowe bronie startowe (pistolet, strzelba, laser), nowe zyski w puli (odblokowywane osiągnięciami "pokonaj bossa z mgłą x3", "ukończ run z długiem 0" itp.).
- 8-10 osiągnięć, widocznych na ekranie startowym jako ikony 1-bit.

## Feeling / polish
- Hit-stop 30-60 ms, lekki screenshake (do wyłączenia), błysk trafienia (inwersja koloru sprite'a), cząsteczki śmierci (odłamki), animacje UI sklepu (karty wjeżdżają), liczby obrażeń 5x7.
- Tutorial: 3 krótkie podpowiedzi w pierwszej fali + objaśnienie sklepu przy pierwszej wizycie.
- Pauza (Esc) z ustawieniami: głośność, reduce flicker, screenshake.
- Muzyka: 3 pętle (po jednej na strefę), proceduralne.
- Balans: docelowy czas runu 12-15 min. Każda strefa ~4-5 min.

## Harmonogram (2 dni)
- Dzień 1 (reszta): dług+Poborca, 3 nowe wrogowie, ceny rozszerzone, strefa 1 dopracowana, balans po testach.
- Dzień 2: strefy 2-3, bossy 2-3, meta/odblokowania, osiągnięcia, tutorial, pauza/ustawienia, polish, QA, build WebGL + deploy na itch.io.
- Ostatnie 3-4 godziny: tylko poprawki błędów, balans i wypolerowanie. Zero nowych funkcji.

---

# GDD v3 — fabuła i Kontrakty (decyzja designera po uwadze Kosmika)

## Fabuła (lekka)
- Gracz to Dłużnik: wszedł na Drogę Opłat, żeby spłacić dług, którego nie pamięta. Każdy boss to etap "rozliczenia": Toll Keeper (myto), Banker (kredyt), Auctioneer (licytuje Ciebie).
- Intro: 3-4 kadry 1-bit na tytule (po pierwszym uruchomieniu, pomijalne klawiszem).
- Przed każdym bossem 1 linijka dialogu w ramce. Po Auctioneerze zakończenie: "SOLD." i ekran z łączną ceną runu (suma stacków cen, długu, sprzedanych zysków).
- Zakończenie złe (zgon): "PAID IN FULL" z liczbą fal.

## Kontrakty (nowa mechanika)
- Przed falą (w sklepie, jako czwarta opcja co 2. sklep) można podpisać kontrakt: nagroda teraz, **mały druk** ujawniany po 2 falach.
- Przykłady: "+30 monet teraz / po 2 falach tracisz 1 losowy zysk"; "+1 max HP / po 2 falach dług +10"; "podwójne monety przez 2 fale / po nich Mgła x2 do końca strefy"; "leczenie do pełna / następny boss +20% HP".
- Mały druk jest ukryty do czasu, ale kontrakty z tego samego typu są znane z doświadczenia (UI pokazuje "???"). Kontrakt można odrzucić.
- Osiągnięcie: "READ THE FINE PRINT" (podpisz 0 kontraktów w runie), "SIGNED AND SEALED" (podpisz 5).

## Harmonogram (48 h)
- Do 24. godziny: rdzeń i treść zamknięte (fabuła, Kontrakty, balans, czytelność).
- 24-40: testy ręczne, balans, bugi, polish, strona gry.
- 40-48: bufor, build finalny, publikacja (tylko gdy Kosmik założy stronę).

---

# GDD v3.1 — Ulgi (REDEEM) i łagodniejsze ceny (feedback testerów)
- Ceny były zbyt dotkliwe: każda oferta = monety + trwała wada na cały run. Zmiany:
- **REDEEM**: w co 3. sklepie jedna oferta to "zdejmij 1 stack wybranej ceny" (koszt 8 + 4 za każdy posiadany stack tej ceny). Gracz wybiera cenę do zdjęcia.
- **Nieliniowe stacki**: 1. stack każdej ceny słaby (np. mgła -15% promienia), 2.-3. coraz mocniejsze.
- **Zabicie Poborcy**: dług spłacony + zdejmuje 1 stack losowej ceny.
- **Siła ceny dopasowana do zysku**: proste zyski (leczenie, tarcza) dostają łagodną cenę, potężne (dash, wybuch, kryt) mocniejszą.
- Cel balansu: bot 'careful' 30-50% wygranych, ludzki tester ma czuć wybór, a nie karę z góry.
