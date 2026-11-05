import os, sys, math
from PIL import Image, ImageDraw, ImageFont

R = r"C:\Projekty gier\Unity\Game Jams\.claude\worktrees\game-jam-graphics-fb7ebc\Assets\_Project\Resources"
OUT = sys.argv[1]
W, H = 480, 270
FONT = ImageFont.truetype(os.path.join(R, "Fonts", "Pixel57.ttf"), 8)
_cache = {}


def spr(name):
    if name not in _cache:
        im = Image.open(os.path.join(R, "Sprites", name + ".png")).convert("RGBA")
        _cache[name] = im.split()[3].point(lambda v: 255 if v > 0 else 0)
    return _cache[name]


ROT = {"up": 0, "right": -90, "left": 90, "down": 180}


def face(dx, dy):
    if abs(dx) >= abs(dy):
        return "right" if dx > 0 else "left"
    return "down" if dy > 0 else "up"


class Scene:
    def __init__(self, bg):
        self.m = Image.new("L", (W, H), 0)
        self.m.paste(255, (0, 0), spr(bg))

    def put(self, name, cx, cy, d="up"):
        s = spr(name)
        if ROT[d]:
            s = s.rotate(ROT[d], expand=True)
        self.m.paste(255, (int(round(cx - s.width / 2)), int(round(cy - s.height / 2))), s)

    def toward(self, name, cx, cy, tx, ty):
        self.put(name, cx, cy, face(tx - cx, ty - cy))

    def line(self, name, x0, y0, x1, y1, n, d=None):
        for i in range(n):
            t = i / max(1, n - 1)
            self.put(name, x0 + (x1 - x0) * t, y0 + (y1 - y0) * t, d or face(x1 - x0, y1 - y0))

    def hud(self, hp, zone, coins):
        dr = ImageDraw.Draw(self.m)
        dr.fontmode = "1"
        dr.text((4, 3), "HP %s" % hp, font=FONT, fill=255)
        dr.text((40, 3), "ZONE %s" % zone, font=FONT, fill=255)
        dr.text((138, 3), "COINS %s" % coins, font=FONT, fill=255)

    def save(self, name, dark, light):
        big = self.m.resize((W * 3, H * 3), Image.NEAREST)
        img = Image.new("RGB", big.size, dark)
        img.paste(Image.new("RGB", big.size, light), (0, 0), big)
        img.save(os.path.join(OUT, name))
        print(name, img.size)


# ---------------- ujęcie 1: strefa 1, biała paleta ----------------
s = Scene("arena_bg")
px, py = 150, 178                       # gracz poza środkiem (lewy dół)
s.put("player", px, py, "right")
s.put("fx_muzzle_0", px + 12, py, "right")
for bx in (178, 202, 228, 252):          # seria pocisków gracza
    s.put("bullet_player", bx, py, "right")
s.put("bullet_player", 196, py - 14, "right")   # drugi tor (podwójny strzał)
s.put("bullet_player", 222, py - 15, "right")
# wybuch trafionego chasera + odłamki
s.put("fx_explosion_2", 282, py, "up")
for (sx, sy, k) in ((266, 164, 0), (270, 190, 1), (300, 168, 2), (296, 188, 3), (284, 150, 4), (310, 178, 1)):
    s.put("shard_%d" % k, sx, sy)
# wrogowie
s.toward("enemy_chaser_walk_0", 352, 150, px, py)
s.toward("enemy_chaser_walk_1", 330, 214, px, py)
s.toward("enemy_chaser_walk_0", 236, 96, px, py)
s.toward("enemy_chaser_walk_1", 90, 100, px, py)
for (cx, cy, k) in ((300, 62, 0), (432, 128, 1), (186, 56, 1), (262, 240, 0), (40, 150, 0), (330, 100, 1), (210, 134, 0), (96, 200, 1)):
    s.toward("enemy_chaser_walk_%d" % k, cx, cy, px, py)
s.toward("enemy_tank_walk_0", 410, 196, px, py)
s.toward("enemy_tank_walk_1", 120, 52, px, py)
s.toward("enemy_shooter_walk_0", 236, 236, px, py)
s.put("fx_explosion_0", 338, 152, "up")
for (sx, sy, k) in ((326, 144, 2), (350, 160, 4), (346, 142, 0)):
    s.put("shard_%d" % k, sx, sy)
s.toward("enemy_shooter_walk_1", 400, 78, px, py)
s.toward("enemy_shooter_walk_0", 60, 226, px, py)
# błyskawice wrogów lecą w stronę gracza
def bolt(sx, sy, k, frac):
    x = sx + (px - sx) * frac
    y = sy + (py - sy) * frac
    s.put("bullet_enemy", x, y, face(px - sx, py - sy))
for f in (0.22, 0.40, 0.58):
    bolt(400, 78, 0, f)
for f in (0.25, 0.5):
    bolt(60, 226, 0, f)
for f in (0.3, 0.6):
    bolt(236, 236, 0, f)
# monety
for (cx, cy) in ((205, 214), (190, 236), (222, 228), (318, 118), (290, 232), (120, 148), (452, 176), (260, 120), (372, 232), (88, 176), (410, 40)):
    s.put("coin", cx, cy)
s.hud("4/6", "1-3", "17")
s.save("screen_6_zone1_action.png", (20, 18, 26), (244, 241, 222))

# ---------------- ujęcie 2: strefa 2, zielona paleta ----------------
s = Scene("arena_bg_zone2")
for x0, y0, n in ((120, 80, 3), (312, 80, 3), (120, 175, 3), (312, 175, 3), (232, 127, 2)):
    for i in range(n):
        s.put("tile_wall", x0 + 8 + i * 16, y0 + 8)
px, py = 372, 206                       # gracz w prawym dolnym rogu
s.put("player", px, py, "up")
s.put("fx_muzzle_1", px, py - 12, "up")
for by in (180, 156, 132):
    s.put("bullet_player", px, by, "up")
s.put("bullet_player", px - 12, 168, "up")
s.put("bullet_player", px + 12, 150, "up")
s.put("drone", px - 22, py - 6)
s.put("drone", px + 24, py + 4)
# Strażnik z tarczą od strony gracza, blokuje pociski
s.put("enemy_guard_walk_0", px, 96, "down")
s.put("fx_block_1", px, 114, "up")
s.put("fx_block_0", px - 10, 112, "up")
# rój
for (sx, sy) in ((230, 150), (246, 162), (222, 172), (252, 184), (212, 196), (238, 204), (262, 172), (206, 160)):
    s.put("enemy_swarm_walk_0" if (sx + sy) % 2 else "enemy_swarm_walk_1", sx, sy)
# oblężnik z ostrzeżeniem ataku obszarowego
s.put("enemy_siege_walk_0", 100, 130, "up")
s.put("fx_shockwave_0", 300, 224, "up")
s.put("telegraph_ring", 300, 224, "up")
# skaczek z smugą
s.put("enemy_dasher_walk_0", 280, 56, "down")
s.put("fx_dash_1", 280, 38, "down")
s.put("fx_dash_2", 280, 24, "down")
# spawner i poborca
s.put("enemy_spawner_walk_0", 60, 214, "up")
s.put("enemy_chaser_walk_0", 96, 226, "right")
s.put("enemy_collector_walk_0", 430, 60, "left")
# eksplozja trafionego roju + odłamki
s.put("fx_explosion_1", 180, 170, "up")
for (sx, sy, k) in ((170, 160, 0), (190, 184, 2), (176, 190, 4), (196, 160, 1)):
    s.put("shard_%d" % k, sx, sy)
# pociski wrogów
s.line("bullet_enemy", 100, 118, 340, 196, 3, "right")
for (bx, by) in ((100, 150), (70, 190)):
    s.put("bullet_enemy", bx, by, "down")
# miny i monety
for (mx, my) in ((152, 232), (180, 238), (410, 120)):
    s.put("mine", mx, my)
for (cx, cy) in ((330, 120), (350, 232), (440, 200), (200, 100), (60, 150), (270, 240)):
    s.put("coin", cx, cy)
s.hud("5/6", "2-2", "34")
s.save("screen_7_green_action.png", (15, 56, 15), (155, 188, 15))
