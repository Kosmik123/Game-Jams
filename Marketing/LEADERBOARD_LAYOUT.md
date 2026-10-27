# Tablica wyników: układ ui_leaderboard_bg (480x270)

Tło: `Resources/UI/ui_leaderboard_bg` (pełny ekran 480x270, 2 kolory). Teksty rysuje kod czcionką Pixel57 (5x7).
Współrzędne w pikselach gry, (0,0) = lewy górny róg.

| Element | Obszar |
|---|---|
| Tytuł "GLOBAL TOP 10" (wyśrodkowany) | x 32..447, y 22..36 (ramka y 18..40) |
| Nagłówki kolumn RANK / NAME / SCORE | y 46..56 |
| Kolumna RANK | x 34..79 (ikona rangi 8x8 `icon_rank_1/2/3` dla miejsc 1-3, reszta numer) |
| Kolumna NAME | x 84..335 |
| Kolumna SCORE | x 340..445 (wyrównanie do prawej) |
| Wiersz i (1..10) | y = 59 + 16*(i-1), wysokość 15, tekst przy y + 4 |
| Wiersz gracza ("YOU", pozycja i wynik) | wnętrze x 36..443, y 228..238 |
| Stopka (np. ESC BACK / OFFLINE) | y 246..256 |

Błąd sieci: zamiast wierszy wyśrodkowany napis "OFFLINE" w obszarze y 90..180.
