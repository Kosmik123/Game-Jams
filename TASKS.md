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
