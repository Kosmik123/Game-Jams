# GDD — roboczy tytuł: PRIMORDIAL SOUP (do zmiany)

Jam: temat **Evolution**, dodatek **You start with nothing**. Termin bardzo krótki, skala małego jamu. Status: **DRAFT v1** (do uwag Kosmika).

## Pitch
Top-down gra 2D o ewolucji w zupie pierwotnej. Zaczynasz jako goła komórka, zjadasz inne stworzenia, zbierasz ich DNA i w edytorze w stylu Spore dobudowujesz sobie z pikselowych bloków ciało, kolce, pancerz i wici. Wokół ewoluują inne istoty, i to Twoje starcia decydują, które z nich się rozmnażają.

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
| **Wić** (napęd) | popycha stworzenie | w kierunku zapisanym w bloku (kierunek ciągu). Szczegóły w "Ruch" |
| **Kolce** (broń pasywna) | ranią przy kontakcie | tylko od strony, na której są zamontowane. Wić + kolce z przodu = taran |
| **Pancerz** | chroni | tylko od strony, na której jest zamontowany |

Moduł (mózg, wić, kolce, pancerz) siedzi na bloku ciała. Każdy blok i moduł ma własne HP (pancerz ma go dużo, reszta tyle samo; dokładne liczby z balansu). Po zniszczeniu **odpada**.

Później dojdą moduły aktywne (np. strzał) i ewentualnie pysk (patrz "Tryb akcji"). O sposobie ataku decyduje moduł: kolce działają samoczynnie przy kontakcie, moduły aktywne mają własne sterowanie.

### Struktura danych
- **Genom** to lista wpisów `(pozycja na siatce, ModuleType, Direction)`. `ModuleType`: `None` (zwykłe ciało), `Brain`, `Flagellum`, `Spikes`, `Armor`. `Direction`: góra / prawo / dół / lewo (ma znaczenie dla wici, kolców i pancerza).
- Z genomu (przez `switch` po enumie) buduje się w trakcie gry **obiekty stworzenia**. Każdy blok ma własny **stan** (aktualne HP, ewentualnie cooldown), trzymany osobno od genomu, który pozostaje niezmienny i dziedziczny.
- Ten sam format opisuje stworzenie gracza, NPC, potomstwo i zapis zatwierdzonej formy. Mutacje to proste zmiany wpisów.
- Wartości (HP, koszt w DNA, siła ciągu) leżą w tabeli configu per `ModuleType`, nie w kodzie.
- Moduł działa na **jednej stronie** bloku (wskazanej przez `Direction`). W edytorze moduł obraca się, żeby ustawić kierunek.
- Mózg jest zawsze dokładnie jeden. Edytor nie pozwala go usunąć ani dodać drugiego.

### Zasady budowania
- Blok można dostawić tylko do boku istniejącego bloku (struktura musi być spójna).
- Odpadnięcie bloku odrywa też wszystko, co przez niego było połączone z mózgiem. **Odcięte bloki odpadają** i leżą na mapie jako jedzenie (może je zjeść każdy).
- Znaczenie ma i dobór, i **położenie** elementów (np. kolce z przodu, pancerz osłaniający mózg, wici z tyłu i po bokach).
- Zwrotność: wstępnie stała. Możliwy osobny moduł w przyszłości.
- **Koszt w DNA:** zbudowanie bloku ciała (fundamentu pod moduł) kosztuje DNA. Moduł (wić, kolce, pancerz) też kosztuje DNA, osobno. Ceny w config.
- Stworzenie ma w danej chwili **formę** (zatwierdzony projekt). Po śmierci wracasz do ostatniej zatwierdzonej formy (patrz "Śmierć i koniec").

### Zakres jamu
Trzy podstawowe moduły ponad mózgiem i ciałem: **wić, kolce, pancerz**. Kolejne moduły tylko jeśli zostanie czas.

## Tryb akcji
- Świat 2D top-down, zupa pierwotna, jedna ograniczona arena (bez poziomów).
- Sterowanie (web, klawiatura+mysz): **WASD** porusza stworzeniem, **kursor** wyznacza, gdzie stworzenie patrzy (obraca się w jego stronę). Można więc iść bokiem albo tyłem. Atak zależy od modułu: kolce są pasywne, moduły aktywne (później) mają własne klawisze.
- Walka: kolce ranią bloki, które dotykają od strony swojego montażu. Pancerz po właściwej stronie przejmuje trafienia. Zniszczony blok odpada.
- Zjadanie: **pokonany = zginął i został pożarty**. Zjadanie to dotknięcie dowolnym blokiem ciała kawałka pozostawionego przez zabite stworzenie albo oderwanego bloku. Daje DNA.
- Możliwa też ucieczka. Zjadanie nie jest przymusowe.
- Pyski jako osobny moduł (tylko one jedzą) to kandydat na pierwszy dodatek po jamie.

## Ruch
- Ruch liczymy dla **środka masy** stworzenia (jeden ruch całości, nie fizyka każdego bloku).
- Stworzenie bez wici ma **bardzo małą** prędkość bazową, żeby start "z niczym" dało się ruszyć. Wici dają dużo prędkości.
- Wić pcha w swoim kierunku (obróconym razem ze stworzeniem). Działa, gdy kierunek ciągu zgadza się z kierunkiem WASD. Stworzenie z wiciami tylko z tyłu rusza do przodu, a bokiem już słabo.
- Położenie wici względem środka masy wpływa na tempo obrotu w stronę kursora (wici na obrzeżach pomagają obracać). Wstępnie do rozważenia przy balansie.
- Po jamie: wici ułożone w szereg w jednym kierunku dają bonus kombo.

## Ewolucja istot (NPC)
Ewolucja NPC odbywa się **wyłącznie przez mutacje potomstwa**. Nie mają etapów ani drzewka. Populacja NPC o stałym rozmiarze (wstępnie 15-30 stworzeń na mapie). Każde stworzenie to **genom**: lista bloków (pozycja + typ + kierunek). Ten sam format służy edytorowi gracza.

### Wynik starcia z graczem
| Wynik | Definicja | Szansa na potomstwo |
|---|---|---|
| **Wygrana NPC** | gracz zginął | duża |
| **Remis / brak interakcji** | obie strony przeżyły albo obie zginęły, ucieczka | umiarkowana |
| **Porażka NPC** | NPC zginął i został pożarty | mała |

### Rozmnażanie i mutacje
- Potomek dziedziczy genom rodzica z **mutacjami**: dodanie bloku, usunięcie bloku, zmiana typu modułu, obrót modułu, przesunięcie modułu.
- Potomstwo pojawia się na mapie w miejsce wyginiętych, żeby populacja pozostała stała.
- Skutek: z czasem NPC adaptują się do stylu gry gracza (np. pancerz od strony, z której zwykle atakujesz).

## DNA i postęp
- Źródło DNA: zjadanie komórek i stworzeń. Im większe i bardziej rozbudowane stworzenie, tym więcej DNA.
- **DNA się gromadzi** i nie przepada przy śmierci.
- **DNA jest walutą**: płacisz nim w edytorze za bloki ciała i za moduły. Nie ma progu ewolucji, rozwijasz się, kiedy Cię na coś stać.
- Docelowo (po jamie): **drzewko rozwoju** wydawane DNA. Odblokowuje nowe części ciała i ulepszenia istniejących.
- Na jam: prosta kolejność odblokowywania (ciało, potem wić, kolce, pancerz).

## Śmierć i koniec
- **Brak przegranej.** Po śmierci **respawn jako poprzednia forma** (ostatnia zatwierdzona w edytorze). Zebrane DNA zostaje.
- Śmierć gracza jest **wynikiem starcia** dla NPC (patrz tabela wyżej).
- Ewentualna wygrana: odblokowanie wszystkich części ciała i osiągnięć.

## Wygląd
- Wszystkie postacie pikselowe, z kwadratowych bloków. Zupa pierwotna: prosty, czytelny, kontrastowy styl.
- Bloki różnych typów muszą być od razu odróżnialne kolorem lub wzorem.

## Zakres jamu (MVP)
1. Ruch gracza w zupie, kamera.
2. Blok ciała, mózg, 3 moduły (wić, kolce, pancerz), HP bloków, odpadanie.
3. Edytor (siatka, dostawianie bloków, koszt w DNA).
4. Walka i zjadanie, drop DNA.
5. Populacja NPC z genomem, wyniki starć, potomstwo z mutacjami.
6. Respawn gracza, HUD z DNA.

**Ciąć w pierwszej kolejności:** drzewko rozwoju, osiągnięcia, dźwięk, dodatkowe moduły, zwrotność, rozbudowa grafiki.

## Do ustalenia
1. Liczby z balansu: HP bloków (pancerz dużo, reszta tyle samo), ceny w DNA, siła wici, rozmiar populacji, tempo mutacji.
2. Tytuł gry.
