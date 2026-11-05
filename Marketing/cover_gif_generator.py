import os, sys
from PIL import Image

SP = r"C:\Projekty gier\Unity\Game Jams\.claude\worktrees\game-jam-graphics-fb7ebc\Assets\_Project\Resources\Sprites"
OUT = sys.argv[1]
W, H = 315, 250          # logiczne piksele; GIF = x2 = 630x500
FPS = 20
CYCLE = 3.2              # jak w menu: klatka 0 spokojna, 2 i 1 to glitch
NCYC = 3
T = CYCLE * NCYC         # 9.6 s, pętla bez szwu
NF = int(T * FPS)
# jasne tło (kontrast ze stroną itch), ciemny tusz; paleta zmienia się przy glitchu
PALS = [((255, 176, 0), (26, 10, 0)),
        ((255, 94, 156), (43, 10, 61)),
        ((244, 241, 222), (20, 18, 26))]


def mask(name):
    im = Image.open(os.path.join(SP, name + ".png")).convert("RGBA")
    a = im.split()[3].point(lambda v: 255 if v > 0 else 0)
    return a


def scaled(m, k):
    return m.resize((m.width * k, m.height * k), Image.NEAREST)


logo = [mask("title_logo_%d" % i) for i in range(3)]
tag = mask("title_tagline")
player = mask("player")
bullet = scaled(mask("bullet_player"), 2)
chaser = [mask("enemy_chaser_walk_0"), mask("enemy_chaser_walk_1")]
tank = [mask("enemy_tank_walk_0"), mask("enemy_tank_walk_1")]
coll = [mask("enemy_collector_walk_0"), mask("enemy_collector_walk_1")]

LOOP = 2                  # ile razy postać obiega pas w czasie T
MARGIN = 56
LEN = W + 2 * MARGIN      # długość pętli pasa (poza ekranem wchodzi/wychodzi)
SPEED = LEN * LOOP / T


def lane_x(t, phase, direction):
    p = (phase + SPEED * t) % LEN
    return (p - MARGIN) if direction > 0 else (W + MARGIN - p)


def sprite_at(canvas, m, cx, cy, direction, k=2):
    s = scaled(m, k)
    s = s.rotate(-90 if direction > 0 else 90, expand=True)
    canvas.paste(255, (int(cx - s.width / 2), int(cy - s.height / 2)), s)


def frame_logo(t):
    c = t % CYCLE
    if c > 2.9:
        return 1
    if c > 2.78:
        return 2
    if 1.30 < c < 1.38:      # dodatkowy krótki glitch dla efektu
        return 2
    if 1.38 <= c < 1.52:
        return 1
    return 0


def pal_at(t):
    # zmiana palety w momencie glitchu (koniec cyklu)
    n = int((t + 0.30) // CYCLE) % NCYC
    return PALS[n]


frames = []
for i in range(NF):
    t = i / FPS
    m = Image.new("L", (W, H), 0)
    # linie pasów (kropkowane)
    for y in (58, 192):
        for x in range(0, W, 4):
            m.putpixel((x, y), 255)
    # tytuł (główny element) + tagline
    m.paste(255, ((W - 282) // 2, 64), logo[frame_logo(t)])
    m.paste(255, ((W - 262) // 2, 162), tag)
    fr = int(t / 0.15) % 2
    # górny pas, w prawo: gracz ucieka, chaser goni, gracz strzela
    py = 30
    px = lane_x(t, 140, 1)
    for k in range(-6, 24):
        tf = k * 0.6
        if tf > t + 0.001 and k >= 0:
            continue
        # pozycja gracza w chwili wystrzału (okresowo)
        sx = lane_x(tf, 140, 1) + 18
        bx = sx + 150 * (t - tf)
        if 0 <= t - tf < 2.6 and -4 < bx < W + 4:
            m.paste(255, (int(bx) - 4, py - 4), bullet)
    sprite_at(m, player, px, py + (1 if fr else 0), 1)
    sprite_at(m, chaser[fr], lane_x(t, 60, 1), py, 1)
    # dolny pas, w lewo: tank prowadzi, Collector goni
    by = 218
    sprite_at(m, tank[fr], lane_x(t, 120, -1), by, -1)
    sprite_at(m, coll[fr], lane_x(t, 20, -1), by, -1)
    # kolor
    bg, ink = pal_at(t)
    big = m.resize((W * 2, H * 2), Image.NEAREST)
    img = Image.new("RGB", big.size, bg)
    img.paste(Image.new("RGB", big.size, ink), (0, 0), big)
    frames.append(img.quantize(colors=2, dither=Image.NONE))

frames[0].save(OUT, save_all=True, append_images=frames[1:], duration=int(1000 / FPS),
               loop=0, optimize=True, disposal=1)
print(OUT, len(frames), os.path.getsize(OUT) // 1024, "KB")
