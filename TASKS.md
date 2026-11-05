# TASKS — Evolution jam

Opis gry w `GDD.md`. Kolejność taska = kolejność robienia. Każdy etap kończy się czymś, w co da się zagrać. Status: `[ ]` do zrobienia, `[~]` zrobione częściowo, `[x]` zrobione (kod napisany i zacommitowany; testy w Unity po stronie Kosmika).

## Etap 1 — Struktura stworzenia (najważniejsze)
- [x] **T1.1 Enumy i genom.** `ModuleType` (None, Brain, Flagellum, Spikes, Armor), `Direction` (4 strony), struktura wpisu genomu `(pozycja, typ, kierunek)`, serializowalny genom (lista wpisów). *Gotowe, gdy:* da się utworzyć genom w kodzie i go skopiować.
- [~] **T1.2 Config.** Tabela per `ModuleType` w jednym ScriptableObject: HP, koszt w DNA, siła (wić), kolor/sprite. Osobno: koszt bloku ciała. *Gotowe, gdy:* zmiana liczby w assecie zmienia grę bez zmian w kodzie. *Zrobione:* `CreatureConfig` z kosztami DNA i startowym DNA. *Brakuje:* HP, siła wici, kolory (kolory są jeszcze w kodzie edytora).
- [ ] **T1.3 Budowa obiektów z genomu.** `switch` po enumie tworzy obiekty stworzenia z blokami. Każdy blok ma stan (aktualne HP). *Gotowe, gdy:* stworzenie z genomu widać na scenie, bloki mają różne wyglądy dla różnych typów.
- [x] **T1.4 Walidacja genomu.** Spójna struktura (każdy blok połączony), dokładnie jeden mózg, brak nakładających się pozycji. *Gotowe, gdy:* niepoprawny genom jest odrzucany z komunikatem.
- [x] **T1.5 Zapis zatwierdzonej formy.** Aktualna forma gracza trzymana jako genom, po śmierci odtwarzana. *Gotowe, gdy:* jest funkcja zapisu i odtworzenia formy. *Zrobione:* `PlayerProfile` (forma i DNA w pamięci, zapis przyciskiem Confirm). Odtworzenie po śmierci wymaga T4.6.

## Etap 2 — Edytor stworzenia
- [~] **T2.1 Scena Editor.** Siatka, podgląd stworzenia, przełączanie `Action` ↔ `Editor`. *Zrobione:* scena Genome Editor z nieskończoną siatką, start jako losowe tetromino z mózgiem. *Brakuje:* przełączanie scen.
- [x] **T2.2 Dostawianie bloków.** Postawienie bloku ciała tylko przy istniejącym bloku. Usuwanie bloku (nie mózgu, nie takiego, który rozspójni strukturę).
- [x] **T2.3 Moduły.** Wybór typu modułu dla bloku, obracanie (kierunek), wymiana i zdejmowanie.
- [x] **T2.4 Koszt w DNA.** Pokazany stan DNA, ceny, blokada zakupu bez środków, zatwierdzenie formy.
- [ ] **T2.5 Kolejność odblokowywania.** Na jam: ciało, potem wić, kolce, pancerz (proste odblokowanie według gry).

## Etap 3 — Ruch i kamera
- [x] **T3.1 Ruch środka masy.** WASD, bazowa mała prędkość stworzenia bez wici. *(Na razie na kapsule; prędkości w `MovementStats`.)*
- [~] **T3.2 Obrót do kursora.** Stworzenie patrzy w stronę myszy, kierunki bloków obracają się razem z nim. *(Obrót działa na kapsule; bloki dojdą z T1.3.)*
- [ ] **T3.3 Wici.** Ciąg zależny od kierunku wici względem kierunku WASD. Wpływ położenia wici na obrót (do balansu).
- [~] **T3.4 Kamera i arena.** Kamera za graczem, ograniczona arena (zupa). *Zrobione:* kamera podąża za graczem, tło to tymczasowa szachownica. Arena zastąpiona planem nieskończonego świata (T3.5).
- [ ] **T3.5 Nieskończony świat.** Proceduralnie generowany poza granicami ekranu (chunki wokół gracza), na początek rozsiane komórki-jedzenie.

## Etap 4 — Walka i zjadanie
- [ ] **T4.1 Kolizje bloków.** Kolizja pojedynczych bloków między stworzeniami.
- [ ] **T4.2 Kolce.** Zadają obrażenia tylko od swojej strony, tylko przy kontakcie.
- [ ] **T4.3 Pancerz.** Chroni tylko od swojej strony (więcej HP bloku).
- [ ] **T4.4 Zniszczenie bloków.** Blok przy HP 0 odpada. Odcięte od mózgu bloki też odpadają. Śmierć stworzenia przy zniszczeniu mózgu.
- [ ] **T4.5 Jedzenie.** Odpadłe bloki i zabite stworzenia zostawiają kawałki do zjedzenia przez dotknięcie. Dają DNA. Mały drop dla komórek-jedzenia rozsianych po arenie (na start).
- [ ] **T4.6 Respawn gracza.** Po śmierci odtwarzana ostatnia zatwierdzona forma, DNA zostaje.

## Etap 5 — NPC i ewolucja
- [ ] **T5.1 Spawn populacji.** Stały rozmiar populacji (15-30), startowe genomy (gołe komórki z prostymi wariantami).
- [ ] **T5.2 AI NPC.** Proste zachowanie: szukaj jedzenia, goń słabszych, uciekaj przed silniejszymi (poziom: proste progi, nie planowanie).
- [ ] **T5.3 Wynik starcia.** Śledzenie interakcji NPC z graczem: wygrana / remis (lub brak interakcji) / porażka.
- [ ] **T5.4 Potomstwo.** Losowanie rodzica z wagami (duża / umiarkowana / mała szansa), kopia genomu z mutacjami (dodanie, usunięcie, zmiana typu, obrót, przesunięcie).
- [ ] **T5.5 Uzupełnianie populacji.** Potomstwo zastępuje wyginiętych, populacja zostaje stała.

## Etap 6 — Balans i polish
- [ ] **T6.1 Balans liczb.** HP (pancerz dużo, reszta równo), ceny DNA, siła wici, drop DNA, tempo mutacji.
- [ ] **T6.2 Czytelność.** Odróżnialne kolory/wzory bloków, wyraźny kierunek modułu, HUD z DNA.
- [ ] **T6.3 Menu i tytuł.** Ekran startowy, nazwa gry.
- [ ] **T6.4 Dźwięk** (łatwo ciąć).

## Etap 7 — Build
- [ ] **T7.1 Build WebGL** i testy na docelowej przeglądarce.
- [ ] **T7.2 Publikacja** (tylko po decyzji Kosmika).

## Zmiany względem pierwotnego planu
- Start jako losowe tetromino (4 bloki, mózg w środku), a nie goła komórka. GDD do aktualizacji.
- Świat eksploracji nieskończony i proceduralny, nie ograniczona arena.
- Kod według `BIPOLAR_RULES.md` (namespace `MicrojamEvolution.*`).

## Ciąć w pierwszej kolejności
Drzewko rozwoju, osiągnięcia, dźwięk, moduły aktywne, pysk jako osobny moduł, kombo wici w szeregu, osobny moduł zwrotności.

## Otwarte
- Liczby balansu (patrz GDD "Do ustalenia"), tytuł gry.
