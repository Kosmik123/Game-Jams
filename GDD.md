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
