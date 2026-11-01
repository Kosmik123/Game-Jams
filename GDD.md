# GDD — roboczy tytuł: PRIMORDIAL SOUP (do zmiany)

Jam: temat **Evolution**, dodatek **You start with nothing**. Termin bardzo krótki, skala małego jamu. Status: **DRAFT v1** (do uwag Kosmika).

## Pitch
Top-down gra 2D o ewolucji w zupie pierwotnej. Zaczynasz jako goła komórka, zjadasz inne stworzenia, zbierasz ich DNA i w edytorze w stylu Spore dobudowujesz sobie z pikselowych bloków ciało, kolce, pancerz i napęd. Wokół ewoluują inne istoty, i to Twoje starcia decydują, które z nich się rozmnażają.

Fantazja gracza: "Zaczynałem jako nic, a teraz to ja jestem szczytem łańcucha pokarmowego."

## Pętla
1. **Tryb akcji**: pływasz po zupie, walczysz, uciekasz, zjadasz pokonanych, zbierasz DNA.
2. Gdy masz dość DNA na wybraną część, wchodzisz do trybu edycji i **dobudowujesz** ją do stworzenia, zatwierdzając nową formę.
3. Wracasz do akcji z nową formą. Inne istoty w tym czasie ewoluują wg wyników starć z Tobą.

Dwie sceny: `Action` i `Editor`. Brak przegranej (patrz "Śmierć i koniec").

## Start with nothing
Stworzenie startowe nie ma żadnej części ciała poza mózgiem. Wszystko inne trzeba zdobyć, kupując za DNA.

## Edytor stworzenia
Stworzenie to siatka pikselowych kwadratów (jak pixel art albo "Minecraft 2D"). Bloki łączą się w jedną spójną strukturę.

### Rodzaje bloków
| Blok | Rola | Uwagi |
|---|---|---|
| **Ciało** | zwiększa powierzchnię stworzenia, nośnik dla modułów | ma własne HP |
| **Mózg** (moduł główny) | konieczny element, jest jeden | uszkodzony do zera = **natychmiastowa śmierć stworzenia**. Opłaca się go chować w środku ciała |
| **Napęd** | przyspiesza ruch | prędkość = liczba boków napędu stykających się ze światem zewnętrznym ("światłem") |
| **Kolce** (broń pasywna) | ranią przy kontakcie | tylko od strony, na której są zamontowane. Napęd + kolce z przodu = taran |
| **Pancerz** | chroni | tylko od strony, na której jest zamontowany |

Moduł (mózg, napęd, kolce, pancerz) siedzi na bloku ciała. Każdy blok i moduł ma własne HP (pancerz ma go dużo, reszta tyle samo; dokładne liczby z balansu). Po zniszczeniu **odpada**.

Później dojdą moduły aktywne (np. strzał) i ewentualnie pysk (patrz "Tryb akcji"). O sposobie ataku decyduje moduł: kolce działają samoczynnie przy kontakcie, moduły aktywne mają własne sterowanie.

### Zasady budowania
- Blok można dostawić tylko do boku istniejącego bloku (struktura musi być spójna).
- Odpadnięcie bloku odrywa też wszystko, co przez niego było połączone z mózgiem. **Odcięte bloki odpadają** i leżą na mapie jako jedzenie (może je zjeść każdy).
- Znaczenie ma i dobór, i **położenie** elementów (np. kolce z przodu, pancerz osłaniający mózg, napęd z odkrytymi bokami).
- Zwrotność: wstępnie wynika z napędu. Możliwy osobny moduł w przyszłości.
- **Koszt w DNA:** zbudowanie bloku ciała (fundamentu pod moduł) kosztuje DNA. Moduł (napęd, kolce, pancerz) też kosztuje DNA, osobno. Ceny w config.
- Stworzenie ma w danej chwili **formę** (zatwierdzony projekt). Po śmierci wracasz do ostatniej zatwierdzonej formy (patrz "Śmierć i koniec").

### Zakres jamu
Trzy podstawowe moduły ponad mózgiem i ciałem: **napęd, kolce, pancerz**. Kolejne moduły tylko jeśli zostanie czas.

## Tryb akcji
- Świat 2D top-down, zupa pierwotna, jedna ograniczona arena (bez poziomów).
- Sterowanie (web, klawiatura+mysz): ruch WASD lub w stronę myszy. Atak zależy od modułu: kolce są pasywne, moduły aktywne (później) mają własne klawisze.
- Walka: kolce ranią bloki, które dotykają od strony swojego montażu. Pancerz po właściwej stronie przejmuje trafienia. Zniszczony blok odpada.
- Zjadanie: **pokonany = zginął i został pożarty**. Zjadanie to dotknięcie dowolnym blokiem ciała kawałka pozostawionego przez zabite stworzenie albo oderwanego bloku. Daje DNA.
- Możliwa też ucieczka. Zjadanie nie jest przymusowe.
- Pyski jako osobny moduł (tylko one jedzą) to kandydat na pierwszy dodatek po jamie.

## Ewolucja istot (NPC)
Ewolucja NPC odbywa się **wyłącznie przez mutacje potomstwa**. Nie mają etapów ani drzewka. Populacja NPC o stałym rozmiarze (wstępnie 15-30 stworzeń na mapie). Każde stworzenie to **genom**: lista bloków (pozycja + typ). Ten sam format służy edytorowi gracza.

### Wynik starcia z graczem
| Wynik | Definicja | Szansa na potomstwo |
|---|---|---|
| **Wygrana NPC** | gracz zginął | duża |
| **Remis / brak interakcji** | obie strony przeżyły albo obie zginęły, ucieczka | umiarkowana |
| **Porażka NPC** | NPC zginął i został pożarty | mała |

### Rozmnażanie i mutacje
- Potomek dziedziczy genom rodzica z **mutacjami**: dodanie bloku, usunięcie bloku, zmiana typu modułu, przesunięcie modułu.
- Potomstwo pojawia się na mapie w miejsce wyginiętych, żeby populacja pozostała stała.
- Skutek: z czasem NPC adaptują się do stylu gry gracza (np. pancerz od strony, z której zwykle atakujesz).

## DNA i postęp
- Źródło DNA: zjadanie komórek i stworzeń. Im większe i bardziej rozbudowane stworzenie, tym więcej DNA.
- **DNA się gromadzi** i nie przepada przy śmierci.
- **DNA jest walutą**: płacisz nim w edytorze za bloki ciała i za moduły. Nie ma progu ewolucji, rozwijasz się, kiedy Cię na coś stać.
- Docelowo (po jamie): **drzewko rozwoju** wydawane DNA. Odblokowuje nowe części ciała i ulepszenia istniejących.
- Na jam: prosta kolejność odblokowywania (ciało, potem napęd, kolce, pancerz).

## Śmierć i koniec
- **Brak przegranej.** Po śmierci **respawn jako poprzednia forma** (ostatnia zatwierdzona w edytorze). Zebrane DNA zostaje.
- Śmierć gracza jest **wynikiem starcia** dla NPC (patrz tabela wyżej).
- Ewentualna wygrana: odblokowanie wszystkich części ciała i osiągnięć.

## Wygląd
- Wszystkie postacie pikselowe, z kwadratowych bloków. Zupa pierwotna: prosty, czytelny, kontrastowy styl.
- Bloki różnych typów muszą być od razu odróżnialne kolorem lub wzorem.

## Zakres jamu (MVP)
1. Ruch gracza w zupie, kamera.
2. Blok ciała, mózg, 3 moduły (napęd, kolce, pancerz), HP bloków, odpadanie.
3. Edytor (siatka, dostawianie bloków, koszt w DNA).
4. Walka i zjadanie, drop DNA.
5. Populacja NPC z genomem, wyniki starć, potomstwo z mutacjami.
6. Respawn gracza, HUD z DNA.

**Ciąć w pierwszej kolejności:** drzewko rozwoju, osiągnięcia, dźwięk, dodatkowe moduły, zwrotność, rozbudowa grafiki.

## Do ustalenia
1. Liczby z balansu: HP bloków, ceny w DNA, rozmiar populacji, tempo mutacji.
2. Tytuł gry.
