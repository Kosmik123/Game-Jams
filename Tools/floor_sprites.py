"""Pixel-art floor sprites written as PNG with pure Python, upscaled x4.

Square 32x32 set (jam version, Resources/Floors):      python floor_sprites.py <out>
Wide 32x24 (4:3) set (post-jam, Resources/FloorsWide): python floor_sprites.py <out> --wide
"""
import os
import struct
import sys
import zlib

N = 32
SCALE = 4


def hexc(h, a=255):
    h = h.lstrip('#')
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)


class Canvas:
    def __init__(self, fill=(0, 0, 0, 0), w=N, h=N):
        self.w, self.h = w, h
        self.p = [[fill for _ in range(w)] for _ in range(h)]

    def px(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.p[y][x] = c

    def rect(self, x, y, w, h, c):
        for yy in range(y, y + h):
            for xx in range(x, x + w):
                self.px(xx, yy, c)

    def circle(self, cx, cy, r, c):
        for yy in range(self.h):
            for xx in range(self.w):
                if (xx + 0.5 - cx) ** 2 + (yy + 0.5 - cy) ** 2 <= r * r:
                    self.px(xx, yy, c)

    def save(self, path):
        width, height = self.w * SCALE, self.h * SCALE
        raw = bytearray()
        for y in range(height):
            raw.append(0)
            row = self.p[y // SCALE]
            for x in range(width):
                raw.extend(row[x // SCALE])

        def chunk(tag, data):
            return struct.pack('>I', len(data)) + tag + data + struct.pack('>I', zlib.crc32(tag + data) & 0xffffffff)

        png = b'\x89PNG\r\n\x1a\n'
        png += chunk(b'IHDR', struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0))
        png += chunk(b'IDAT', zlib.compress(bytes(raw), 9))
        png += chunk(b'IEND', b'')
        with open(path, 'wb') as f:
            f.write(png)


SLAB = hexc('#4a4a55')
SLAB_LIGHT = hexc('#6b6b78')
GLASS = hexc('#bfe6ff')
GLASS_DARK = hexc('#7fb8e0')
WHITE = hexc('#ffffff')


def frame(c, wall, wall_dark, wall_light):
    """Common floor shell: wall fill, side pillars, bottom slab, top highlight (y=0 is top)."""
    w, h = c.w, c.h
    c.rect(0, 0, w, h, wall)
    c.rect(0, 0, 2, h, wall_dark)
    c.rect(w - 2, 0, 2, h, wall_dark)
    c.rect(0, 0, w, 1, wall_light)
    c.rect(0, h - 3, w, 3, SLAB)
    c.rect(0, h - 3, w, 1, SLAB_LIGHT)


def window(c, x, y, w, h, frame_color):
    c.rect(x - 1, y - 1, w + 2, h + 2, frame_color)
    c.rect(x, y, w, h, GLASS)
    c.rect(x, y + h // 2, w, h - h // 2, GLASS_DARK)
    c.px(x, y, WHITE)
    c.px(x + 1, y, WHITE)
    c.px(x, y + 1, WHITE)


def residential():
    wall, dark, light = hexc('#e88c59'), hexc('#b8613a'), hexc('#ffb48a')
    c = Canvas()
    frame(c, wall, dark, light)
    # brick lines
    for y in range(3, N - 4, 4):
        c.rect(2, y, N - 4, 1, hexc('#d67a4a'))
        off = 2 if (y // 4) % 2 else 6
        for x in range(off, N - 2, 8):
            c.rect(x, y - 3, 1, 3, hexc('#d67a4a'))
    curtain = hexc('#f2e2a0')
    for wx in (5, 19):
        window(c, wx, 7, 8, 10, hexc('#fff1e0'))
        c.rect(wx, 7, 2, 10, curtain)
        c.rect(wx + 6, 7, 2, 10, curtain)
        # balcony rail with flowers
        c.rect(wx - 2, 18, 12, 1, hexc('#5a3a2a'))
        for i in range(wx - 2, wx + 10, 2):
            c.px(i, 19, hexc('#5a3a2a'))
            c.px(i, 20, hexc('#5a3a2a'))
        c.rect(wx - 2, 21, 12, 1, hexc('#5a3a2a'))
        c.px(wx, 17, hexc('#ff4f6d'))
        c.px(wx + 5, 17, hexc('#ffd84f'))
        c.px(wx + 8, 17, hexc('#ff4f6d'))
    return c


def office():
    wall, dark, light = hexc('#3d6f9e'), hexc('#2a4f73'), hexc('#7fb0de')
    c = Canvas()
    frame(c, wall, dark, light)
    # curtain wall: 4 columns x 2 rows of glass panes
    glass_a, glass_b, mullion = hexc('#8fd0ff'), hexc('#5aa7e6'), hexc('#2a4f73')
    c.rect(3, 3, N - 6, N - 7, mullion)
    for col in range(4):
        for row in range(2):
            x, y = 4 + col * 6, 4 + row * 12
            c.rect(x, y, 5, 11, glass_b)
            c.rect(x, y, 5, 5, glass_a)
    # diagonal reflection stripes
    for i in range(14):
        for t in (0, 1):
            x, y = 6 + i + t, 18 - i
            if 4 <= y < 27 and 4 <= x < 28 and c.p[y][x] != mullion:
                c.px(x, y, hexc('#d8f0ff'))
    for i in range(6):
        x, y = 20 + i, 14 - i
        if c.p[y][x] != mullion:
            c.px(x, y, hexc('#d8f0ff'))
    return c


def power():
    wall, dark, light = hexc('#f2c230'), hexc('#c29318'), hexc('#ffe27a')
    c = Canvas()
    frame(c, wall, dark, light)
    # hazard stripes above the slab
    for x in range(2, N - 2):
        c.px(x, N - 5, hexc('#222222') if (x // 2) % 2 else hexc('#ffd000'))
        c.px(x, N - 4, hexc('#222222') if ((x + 1) // 2) % 2 else hexc('#ffd000'))
    # dark panel with lightning bolt
    c.rect(9, 4, 14, 20, hexc('#333340'))
    c.rect(10, 5, 12, 18, hexc('#46465a'))
    bolt = hexc('#ffe23a')
    pts = [(17, 6), (16, 6), (15, 7), (16, 7), (14, 8), (15, 8), (13, 9), (14, 9), (12, 10), (13, 10),
           (12, 11), (13, 11), (14, 11), (15, 11), (16, 11), (17, 11), (18, 11),
           (17, 12), (18, 12), (16, 13), (17, 13), (15, 14), (16, 14), (14, 15), (15, 15),
           (14, 16), (13, 17), (14, 17), (13, 18), (12, 19), (13, 12), (14, 12)]
    # chunky bolt: zigzag strokes
    c.rect(15, 6, 4, 2, bolt)
    c.rect(14, 8, 4, 2, bolt)
    c.rect(13, 10, 4, 2, bolt)
    c.rect(12, 12, 9, 2, bolt)
    c.rect(16, 14, 4, 2, bolt)
    c.rect(15, 16, 4, 2, bolt)
    c.rect(14, 18, 4, 2, bolt)
    c.rect(13, 20, 3, 2, bolt)
    # side vents
    for y in range(6, 22, 3):
        c.rect(3, y, 4, 1, dark)
        c.rect(N - 7, y, 4, 1, dark)
    return c


def park():
    wall, dark, light = hexc('#a8e6ff'), hexc('#4f9a5a'), hexc('#d6f4ff')
    c = Canvas()
    frame(c, wall, dark, light)
    # sky gradient feel inside the glass box
    c.rect(2, 1, N - 4, 8, hexc('#bdecff'))
    # grass
    c.rect(2, N - 8, N - 4, 5, hexc('#57b85f'))
    c.rect(2, N - 8, N - 4, 1, hexc('#7fd685'))
    for x in range(3, N - 3, 3):
        c.px(x, N - 9, hexc('#7fd685'))
    # trees
    for cx, r in ((9, 6), (22, 5)):
        c.rect(cx - 1, 15, 2, N - 8 - 15, hexc('#7a4f2a'))
        c.circle(cx, 12, r, hexc('#2f8f3e'))
        c.circle(cx - 1, 11, r - 2, hexc('#45b052'))
        c.px(cx - 2, 9, hexc('#7fd685'))
    # flowers and bench
    c.px(15, N - 7, hexc('#ff6fa0'))
    c.px(5, N - 6, hexc('#ffd84f'))
    c.px(27, N - 6, hexc('#ff6fa0'))
    c.rect(14, N - 11, 5, 1, hexc('#8a5a32'))
    c.px(14, N - 10, hexc('#5a3a2a'))
    c.px(18, N - 10, hexc('#5a3a2a'))
    return c


# --- Wide 32x24 (4:3) set for the post-jam version. Same palette and shell, layouts squeezed vertically.
WW, WH = 32, 24


def residential_wide():
    wall, dark, light = hexc('#e88c59'), hexc('#b8613a'), hexc('#ffb48a')
    brick, rail = hexc('#d67a4a'), hexc('#5a3a2a')
    c = Canvas(w=WW, h=WH)
    frame(c, wall, dark, light)
    for y in range(3, WH - 4, 4):
        c.rect(2, y, WW - 4, 1, brick)
        off = 2 if (y // 4) % 2 else 6
        for x in range(off, WW - 2, 8):
            c.rect(x, y - 3, 1, 3, brick)
    curtain = hexc('#f2e2a0')
    for wx in (5, 19):
        window(c, wx, 4, 8, 8, hexc('#fff1e0'))
        c.rect(wx, 4, 2, 8, curtain)
        c.rect(wx + 6, 4, 2, 8, curtain)
        # balcony rail with flowers
        c.rect(wx - 2, 14, 12, 1, rail)
        for i in range(wx - 2, wx + 10, 2):
            c.px(i, 15, rail)
            c.px(i, 16, rail)
        c.rect(wx - 2, 17, 12, 1, rail)
        c.px(wx, 13, hexc('#ff4f6d'))
        c.px(wx + 5, 13, hexc('#ffd84f'))
        c.px(wx + 8, 13, hexc('#ff4f6d'))
    return c


def office_wide():
    wall, dark, light = hexc('#3d6f9e'), hexc('#2a4f73'), hexc('#7fb0de')
    c = Canvas(w=WW, h=WH)
    frame(c, wall, dark, light)
    glass_a, glass_b, mullion, shine = hexc('#8fd0ff'), hexc('#5aa7e6'), hexc('#2a4f73'), hexc('#d8f0ff')
    c.rect(3, 3, WW - 6, WH - 7, mullion)
    for col in range(4):
        for row in range(2):
            x, y = 4 + col * 6, 4 + row * 8
            c.rect(x, y, 5, 7, glass_b)
            c.rect(x, y, 5, 3, glass_a)
    for i in range(12):
        for t in (0, 1):
            x, y = 7 + i + t, 18 - i
            if 4 <= y < 19 and 4 <= x < 28 and c.p[y][x] != mullion:
                c.px(x, y, shine)
    for i in range(5):
        x, y = 21 + i, 10 - i
        if c.p[y][x] != mullion:
            c.px(x, y, shine)
    return c


def power_wide():
    wall, dark, light = hexc('#f2c230'), hexc('#c29318'), hexc('#ffe27a')
    c = Canvas(w=WW, h=WH)
    frame(c, wall, dark, light)
    for x in range(2, WW - 2):
        c.px(x, WH - 5, hexc('#222222') if (x // 2) % 2 else hexc('#ffd000'))
        c.px(x, WH - 4, hexc('#222222') if ((x + 1) // 2) % 2 else hexc('#ffd000'))
    c.rect(9, 3, 14, 15, hexc('#333340'))
    c.rect(10, 4, 12, 13, hexc('#46465a'))
    bolt = hexc('#ffe23a')
    c.rect(16, 5, 3, 2, bolt)
    c.rect(14, 7, 4, 2, bolt)
    c.rect(12, 9, 9, 2, bolt)
    c.rect(16, 11, 4, 2, bolt)
    c.rect(14, 13, 4, 2, bolt)
    c.rect(13, 15, 3, 1, bolt)
    for y in range(5, 17, 3):
        c.rect(3, y, 4, 1, dark)
        c.rect(WW - 7, y, 4, 1, dark)
    return c


def park_wide():
    wall, dark, light = hexc('#a8e6ff'), hexc('#4f9a5a'), hexc('#d6f4ff')
    c = Canvas(w=WW, h=WH)
    frame(c, wall, dark, light)
    c.rect(2, 1, WW - 4, 6, hexc('#bdecff'))
    c.rect(2, WH - 8, WW - 4, 5, hexc('#57b85f'))
    c.rect(2, WH - 8, WW - 4, 1, hexc('#7fd685'))
    for x in range(3, WW - 3, 3):
        c.px(x, WH - 9, hexc('#7fd685'))
    for cx, r in ((9, 5), (22, 4)):
        c.rect(cx - 1, 11, 2, WH - 8 - 11, hexc('#7a4f2a'))
        c.circle(cx, 8, r, hexc('#2f8f3e'))
        c.circle(cx - 1, 7, r - 2, hexc('#45b052'))
        c.px(cx - 2, 5, hexc('#7fd685'))
    c.px(15, WH - 7, hexc('#ff6fa0'))
    c.px(5, WH - 6, hexc('#ffd84f'))
    c.px(27, WH - 6, hexc('#ff6fa0'))
    c.rect(14, WH - 11, 5, 1, hexc('#8a5a32'))
    c.px(14, WH - 10, hexc('#5a3a2a'))
    c.px(18, WH - 10, hexc('#5a3a2a'))
    return c


if __name__ == '__main__':
    out = sys.argv[1]
    wide_set = '--wide' in sys.argv[2:]
    os.makedirs(out, exist_ok=True)
    sprites = (('Residential', residential_wide), ('Office', office_wide), ('Power', power_wide), ('Park', park_wide))         if wide_set else (('Residential', residential), ('Office', office), ('Power', power), ('Park', park))
    for name, fn in sprites:
        fn().save(os.path.join(out, name + '.png'))
        print('wrote', name)
