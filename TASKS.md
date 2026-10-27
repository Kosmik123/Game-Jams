# TASKS (dopisuje tylko designer; wykonawca odhacza `- [x]`)

Branch: `1-bit-jam-15`. Plan: `GDD.md`. Zasady jamu: `JAM_RULES.md`. Komunikacja: `comms`.
Projekt: Unity 6000.4.9f1, kod i assety w `Assets/_Project` (Scripts, Graphics, Prefabs, Scenes).

## Programista — dzień 1 (MVP)
- [ ] Zweryfikuj, że pracujesz na branchu `1-bit-jam-15` i widzisz `Assets/_Project` (Unity 6000.4.9f1). Jeśli nie, napisz na comms.
- [ ] Render: kamera gry -> RenderTexture (Point, MSAA off), wyświetlana z jednym materiałem-shaderem: próg luminancji -> 2 kolory z palety (uniformy: kolor tła, kolor obiektów). Wybierz najprostszą drogę (RT albo Fullscreen Pass w URP 17). UI na wierzchu, nie zasłaniane mgłą.
- [ ] Gracz: WASD, celowanie myszą, LPM strzał (pociski), HP.
- [ ] Wrogowie: Goniec i Strzelec (Tank później), HP, śmierć zostawia monetę.
- [ ] Spawner fal (fala N = więcej/silniejsi wrogowie), licznik fali, ekran śmierci z restartem.
- [ ] Monety (zbieranie, licznik w UI).
- [ ] Sklep po fali: 3 losowe oferty (zysk + cena), zakup za monety. Zyski i ceny jako ScriptableObject z listą modyfikatorów (dodawanie = dane, nie kod).
- [ ] Zyski MVP: szybsze strzały, +max HP, strzał 3-promieniowy, leczenie, przebijanie.
- [ ] Ceny MVP: Mgła (promień w shaderze maleje ze stackami), Pikselizacja (rozmiar RT), Ciężkie nogi (wolniejszy ruch).
- [ ] Zasada 2 kolorów: zero półprzezroczystości/AA/cząsteczek z alfą.

## Grafik — dzień 1
- [ ] Sprity (czysta biel na przezroczystym, Point, PPU 16, no compression): gracz, Goniec, Strzelec, pocisk gracza, pocisk wroga, moneta.
- [ ] Font bitmapowy 5x7 + import jako font dla TMP/UI.
- [ ] Ikony cen 16x16: oko (mgła), kratka (pikselizacja), błysk (migotanie), kotwica (ociężałość), but (nogi), ucho (głuchota).
- [ ] Ramka karty oferty (1 px), tło areny (płytki 16x16, bez szarości).

## Dzień 2 (zadania doprecyzuję po MVP)
- [ ] Tank, Boss, dash, ceny Migotanie/Ociężałość/Głuchota, zmiany palet, "Sprzedaj", dźwięk, menu/tytuł, build WebGL.

---
# FAZA 2 (GDD v2) — dodane przez designera

## Programista — blok A (dzień 1, priorytet)
- [ ] Dług: zakup na kredyt w sklepie, odsetki 20% po fali, licznik długu w HUD.
- [ ] Poborca (mini-boss): pojawia się przy długu po fali 4 strefy, ściga gracza, dotyk usuwa losowy zysk, zabicie spłaca dług.
- [ ] Wrogowie: Skaczek, Oblężnik (pocisk obszarowy z ostrzeżeniem), Rój, Strażnik (tarcza z przodu), Spawner.
- [ ] Struktura strefy: 5 fal + boss strefy, ekran "ZONE N" z hasłem, strefy 1-3 (pula wrogów i palety per strefa), przeszkody na arenie (ściany, kolce).
- [ ] Ceny: Zbliżenie kamery, Kruchość (-1 max HP), Drogie życie, Głód, Odwrócone sterowanie (krótkie okna).
- [ ] Zyski do ~16, każdy z 3 poziomami: Rykoszet, Wybuch, Wampiryzm, Spowolnienie, Pet-dron, Krytyk, Miny z dasha, Zamrożenie czasu.
- [ ] Synergie (Mgła+Krytyk, Pikselizacja+Pet-dron, Ciężkie nogi+Tarcza) z podpowiedzią "SYNERGY" na kartach. Reroll i Haggle w sklepie.

## Programista — blok B (dzień 2)
- [ ] Bossy 2 i 3 ("The Banker" wielofazowy, "The Auctioneer" sprzedający zyski w walce).
- [ ] Meta: ekran końca runu ze statystykami, rekord (PlayerPrefs), odblokowania broni startowych (pistolet, strzelba, laser) i zysków, 8-10 osiągnięć.
- [ ] Pauza (Esc): głośność, "Reduce flicker", screenshake on/off. Tutorial (3 podpowiedzi + sklep).
- [ ] Feeling: hit-stop 30-60 ms, screenshake, błysk inwersji sprite'a po trafieniu, liczby obrażeń, animacja wjazdu kart sklepu.
- [ ] Muzyka: 3 pętle (po jednej na strefę).

## Grafik
- [ ] Nowi wrogowie (Skaczek, Oblężnik, Rój, Strażnik, Spawner, Poborca) z 2 klatkami chodu.
- [ ] Bossy 2 i 3 (48x48+, 2 klatki, pociski).
- [ ] Ikony nowych zysków i cen, ikona długu, ikony osiągnięć (10), nowe tła stref 2 i 3, przeszkody areny (ściana, kolce).
- [ ] 3 nowe pary palet (po jednej na strefę) w wiadomości do designera; ekran "ZONE N" i ekran końca runu (ramki).
- [ ] Przegląd czytelności: wrogowie vs gracz vs pociski w jednej palecie.

## Designer
- [ ] Balans po testach, tuning kosztów, czasu runu (cel 12-15 min), build WebGL, QA.

---
# FAZA 3 — dodane przez designera (po bloku A/B)

## Programista
- [ ] Reszta bloku B: tutorial (3 podpowiedzi w fali 1 + wyjaśnienie sklepu), 3 pętle muzyczne (po jednej na strefę).
- [ ] Balans na danych: rozszerz smoke bota o telemetrię (CSV: fala, czas, HP, monety, dług, liczba cen, zgon/wygrana) i uruchom 20+ runów z różnymi strategiami (losowy, ostrożny, 'kupuj wszystko'); cel: wygrana losowego bota 5-15%, ostrożnego 30-50%, runy 12-15 min. Raport do designera z propozycjami liczb.
- [ ] Tryb ASCENSION po pierwszej wygranej: poziomy 1-5, każdy dodaje obowiązkową cenę na starcie lub mocniejszych wrogów, mnożnik wyniku. Zapis w PlayerPrefs, wybór na ekranie tytułowym.
- [ ] Liczby obrażeń/ monet (małe, 5x7, bez AA) jeśli czytelne; inaczej pomiń.
- [ ] Czytelność: ostrzeżenia przed atakami wrogów (telegraf), limit pocisków na ekranie, test z Mgłą x3 + Zbliżeniem (czy da się grać).
- [ ] Dźwięk: wyciszenie po utracie fokusu karty, głośność z pauzy dla muzyki i SFX osobno.

## Grafik
- [ ] Ekran końca runu i 'ZONE N' (ramki), animowane logo na tytule (2-3 klatki), przejścia stref.
- [ ] Efekty: błyski strzału, eksplozja (4-5 klatek), fala dashu, ikony Ascension (1-5).
- [ ] Przegląd czytelności po testach, poprawki sprite'ów (gracz vs wrogowie vs pociski).
- [ ] Okładka i 3 screeny pod stronę gry (630x500 okładka, 1-bit, dwa kolory) oraz animowany GIF/mockup do opisu.

## Designer
- [ ] Testy ręczne całego runu na WebGL, lista bugów do programisty, tuning wg telemetrii, tekst strony gry (opis, sterowanie).

---
# FAZA 4 — fabuła i Kontrakty (designer)

## Programista
- [ ] Kontrakty w sklepie (co 2. sklep, 4. opcja), mały druk po 2 falach, 6-8 kontraktów w danych, UI z "???" i podglądem po podpisaniu, osiągnięcia READ THE FINE PRINT / SIGNED AND SEALED.
- [ ] Fabuła: intro (3-4 kadry, pomijalne), dialogi przed bossami (1 linijka w ramce), zakończenie SOLD z łączną ceną runu, ekran PAID IN FULL przy zgonie.
- [ ] Statystyka końcowa 'Total price paid' (stacki cen + dług + sprzedane zyski).

## Grafik
- [ ] 3-4 kadry intro i kadr zakończenia (1-bit, 320x180, bez półprzezroczystości), ramka dialogu bossa, ikona kontraktu (pergamin z gwiazdką), ikona 'mały druk'.

---
# FAZA 5 — Ulgi i łagodniejsze ceny (designer)
- [ ] REDEEM: w co 3. sklepie oferta 'zdejmij 1 stack ceny' (wybór ceny, koszt 8 + 4*stacks), ikona/label 'REDEEM'.
- [ ] Nieliniowe stacki cen (1. łagodny, 2.-3. mocniejsze) dla wszystkich cen.
- [ ] (ANULOWANE: Poborca nie zdejmuje ceny, zobacz FAZA 6)
- [ ] Siła ceny zależna od mocy zysku (proste zyski łagodniejsze, potężne mocniejsze).
- [ ] Przestaw bota i puść balans ponownie; raport z liczbami (cele: random 5-15%, careful 30-50%).

---
# FAZA 6 — balans: kupowanie musi się opłacać (designer, wysoki priorytet)
- [ ] Wrogowie rosną z falą: co 3 fale +10% HP; strefa 2 dodatkowo +15% HP i prędkości, strefa 3 +30%. Liczba wrogów rośnie ostrzej.
- [ ] Zyski mocniejsze (większy skok na poziom), ceny zmysłów łagodne na 1. stacku (nieliniowo).
- [ ] Poborca: zabicie zmniejsza dług o połowę (zaokrąglenie w dół) + monety; NIE zdejmuje ceny. Poborca groźniejszy (HP, prędkość). Dotyk nadal kradnie zysk.
- [ ] Bot NOBUY (nie kupuje nic) jako strategia kontrolna w balance.ps1. Cele: NOBUY śr. koniec <= Z2, 0-3% wygranych; RANDOM 5-15%; CAREFUL 30-50%.
- [ ] Raport tabela (strategie, % wygranych, mediana fali końca, średni dług na końcu) przed i po zmianach.

---
# FAZA 7 — domknięcie rdzenia (designer, ok. godz. 7-24 jamu)
## Programista
- [ ] Zróżnicowanie broni: pistolet (celny, średnie tempo), shotgun (szeroki stożek, krótki zasięg, wolniejszy), laser (wiązka przebijająca, rozgrzewanie). Każda ma inny 'feeling' i inną synergię z zyskami.
- [ ] Wydajność WebGL: profil przy 60+ wrogach + mgła + pikselizacja; zmniejszenie rozmiaru builda (stripping, kompresja tekstur, usuń nieużywane pakiety).
- [ ] Zapis/reset: przycisk 'RESET PROGRESS' w ustawieniach pauzy.
## Grafik
- [ ] 3 screeny z buildu (po jednym na strefę) i krótki GIF do strony gry; sprawdzenie czytelności na małych oknach.
## Designer
- [ ] Opis strony gry, lista zmian, instrukcje testerów, zebranie uwag z testów zewnętrznych.

---
# FAZA 8 — mobile — ANULOWANE przez Kosmika (nieczytelne). Nie robić.
## Programista
- [ ] Sterowanie dotykowe, włączane po pierwszym dotyku (Input.touchSupported / pierwszy touch): lewa wirtualna gałka = ruch, prawa = celowanie + strzał w kierunku gałki (przytrzymanie = ogień), przycisk DASH i przycisk FREEZE (widoczne tylko gdy zysk jest posiadany), ikona pauzy w rogu.
- [ ] Wszystkie ekrany (tytuł, wybór broni, intro, sklep, kontrakty, REDEEM, pauza, koniec runu) obsługiwane tapem; przyciski min. 24 px wysokości w RT 480x270 dla palca; skróty klawiszowe zostają na PC.
- [ ] Poziomy ekran: komunikat 'ROTATE YOUR PHONE' w pionie, blokada ekranu/ scroll (touch-action: none).
- [ ] Wejście oparte na zdarzeniach dotyku mapowane na współrzędne RT (z uwzględnieniem skalowania canvasu), wiele palców naraz (gałka + gałka + przycisk).
## Grafik
- [ ] Sprity UI dotyku: podstawa i gałka (2 kolory, bez półprzezroczystości, np. okrąg/pierścień), przyciski DASH i FREEZE (ikony), ikona pauzy, ikona 'obróć telefon'.
- [ ] Szablon WebGL: meta viewport, brak zoomu, pełny ekran na telefonie, przycisk FULLSCREEN powiększony.
## Designer
- [ ] Test mobile w emulacji przeglądarki + prośba do Kosmika o test na prawdziwym telefonie przez itch.io.

---
# FAZA 9 — trwałe save'y między buildami (designer, po przerwie)
Problem: PlayerPrefs w WebGL (IndexedDB, klucz zależny od adresu strony) resetuje się przy każdym nowym uploadzie na itch.io.
## Programista
- [ ] Warstwa zapisu: wtyczka .jslib (Assets/_Project/Plugins/WebGL) czytająca/zapisująca localStorage pod stałymi kluczami z prefiksem 'payWithPixels_v1_' (domena itch może być wspólna z innymi grami). Jedna klasa Save zamiast bezpośredniego PlayerPrefs (rekordy, osiągnięcia, bronie, Ascension, ustawienia). W Edytorze i na innych platformach: PlayerPrefs.
- [ ] Migracja: przy starcie, jeśli localStorage pusty, a PlayerPrefs ma dane, przepisz je.
- [ ] Kod zapisu w pauzie: 'EXPORT SAVE' (pokazuje i kopiuje do schowka krótki tekst, np. base64) oraz 'IMPORT SAVE' (wklejenie przez prompt przeglądarki lub klawisz Ctrl+V).
- [ ] Test: zapis -> nowy build wgrany na tę samą stronę -> save zostaje (sprawdzi designer na itch).
