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
