"""Generuje motywy bossow (boss1..3.wav) do Assets/_Project/Resources/Music.

Czysty Python, bez zaleznosci. Uzycie: python -I tools/MusicGen/make_boss_music.py [katalog_wyjsciowy]

Kazdy motyw to 16 taktow w petli (ogon dzwieku zawija sie na poczatek, wiec petla jest bez szwu):
  boss1 TOLL KEEPER - marsz a-moll, dzwon myta (E6) co 4 takty, "szlaban" jako sztywne akordy.
  boss2 BANKER      - klawesynowe arpeggio d-moll, chlodna melodia, brzdek monet na koncu fraz.
  boss3 AUCTIONEER  - gwaltowna paplanina licytatora: wysokosc rosnie z kazdym taktem (kolejne oferty), mlotek na koncu frazy.
"""
import math
import os
import random
import struct
import sys
import wave

RATE = 22050
NOTE = {"C": 0, "D": 2, "E": 4, "F": 5, "G": 7, "A": 9, "B": 11}


def midi(name):
    n = NOTE[name[0]]
    i = 1
    while name[i] in "#b":
        n += 1 if name[i] == "#" else -1
        i += 1
    return 12 * (int(name[i:]) + 1) + n


def hz(m):
    return 440.0 * 2 ** ((m - 69) / 12.0)


class Track:
    def __init__(self, bars, step):
        self.step = step
        self.n = int(bars * 16 * step * RATE)
        self.buf = [0.0] * self.n
        self.rng = random.Random(7)

    def add(self, start_s, samples):
        s0 = int(start_s * RATE)
        n = self.n
        buf = self.buf
        for i, v in enumerate(samples):
            buf[(s0 + i) % n] += v   # zawijanie ogona = petla bez szwu

    # ---- glosy ----
    def tone(self, start, freq, dur, vol, duty=0.5, decay=6.0, wave="sq", vib=0.0, release=0.015):
        total = int((dur + release) * RATE)
        out = [0.0] * total
        phase = 0.0
        for i in range(total):
            t = i / RATE
            f = freq * (1.0 + vib * math.sin(2 * math.pi * 5.5 * t)) if vib else freq
            phase += f / RATE
            p = phase % 1.0
            if wave == "sq":
                v = 1.0 if p < duty else -1.0
            elif wave == "tri":
                v = 4.0 * abs(p - 0.5) - 1.0
            else:
                v = math.sin(2 * math.pi * p)
            env = min(1.0, t / 0.003) * math.exp(-decay * t)
            if t > dur:
                env *= max(0.0, 1.0 - (t - dur) / release)
            out[i] = v * vol * env
        self.add(start, out)

    def kick(self, start, vol=0.55):
        n = int(0.16 * RATE)
        out = []
        phase = 0.0
        for i in range(n):
            t = i / RATE
            f = 45 + 110 * math.exp(-t * 40)
            phase += f / RATE
            out.append(math.sin(2 * math.pi * phase) * vol * math.exp(-t * 22))
        self.add(start, out)

    def snare(self, start, vol=0.30, tone=190.0):
        n = int(0.14 * RATE)
        out = []
        for i in range(n):
            t = i / RATE
            out.append((self.rng.uniform(-1, 1) * math.exp(-t * 32) * 0.8 + math.sin(2 * math.pi * tone * t) * math.exp(-t * 45) * 0.5) * vol)
        self.add(start, out)

    def hat(self, start, vol=0.10, decay=130.0):
        n = int(0.05 * RATE)
        out = []
        prev = 0.0
        for i in range(n):
            t = i / RATE
            r = self.rng.uniform(-1, 1)
            out.append((r - prev) * 0.5 * vol * math.exp(-t * decay))   # roznica kolejnych probek = szum "jasny"
            prev = r
        self.add(start, out)

    def bell(self, start, freq, vol=0.16, decay=2.4, length=2.4):
        n = int(length * RATE)
        out = []
        for i in range(n):
            t = i / RATE
            v = math.sin(2 * math.pi * freq * t) + 0.6 * math.sin(2 * math.pi * freq * 2.76 * t) + 0.3 * math.sin(2 * math.pi * freq * 5.4 * t)
            out.append(v * vol * min(1.0, t / 0.002) * math.exp(-decay * t))
        self.add(start, out)

    def gavel(self, start, vol=0.7):
        n = int(0.28 * RATE)
        out = []
        phase = 0.0
        for i in range(n):
            t = i / RATE
            f = 60 + 70 * math.exp(-t * 55)
            phase += f / RATE
            v = math.sin(2 * math.pi * phase) * math.exp(-t * 16)
            v += self.rng.uniform(-1, 1) * math.exp(-t * 90) * 0.9   # trzask drewna
            out.append(v * vol)
        self.add(start, out)

    # ---- zapis ----
    def save(self, path):
        peak = max(1e-6, max(abs(v) for v in self.buf))
        gain = 0.88 / peak
        data = struct.pack("<%dh" % self.n, *[int(max(-1.0, min(1.0, v * gain)) * 32767) for v in self.buf])
        with wave.open(path, "wb") as w:
            w.setnchannels(1)
            w.setsampwidth(2)
            w.setframerate(RATE)
            w.writeframes(data)
        print("%s: %.1f s, peak %.2f" % (os.path.basename(path), self.n / RATE, peak))


def melody(track, bars, start_bar, **kw):
    """bars: lista napisow po 16 tokenow: nuta, '-' (przedluz), '.' (pauza)."""
    for b, text in enumerate(bars):
        toks = text.split()
        assert len(toks) == 16, (b, len(toks), text)
        i = 0
        while i < 16:
            t = toks[i]
            if t in ".-":
                i += 1
                continue
            ln = 1
            while i + ln < 16 and toks[i + ln] == "-":
                ln += 1
            track.tone((start_bar + b) * 16 * track.step + i * track.step, hz(midi(t)), ln * track.step * 0.92, **kw)
            i += ln


# ---------------------------------------------------------------- BOSS 1: TOLL KEEPER
def toll_keeper():
    tr = Track(16, 60.0 / 140 / 4)
    S = tr.step
    A = [
        "A4 - . A4 C5 - . C5 E5 - - . E5 - D5 -",
        "C5 - . C5 E5 - . E5 A5 - - - G5 - E5 -",
        "F5 - . F5 A5 - . A5 C6 - - . A5 - G5 -",
        "E5 - . E5 G#5 - . G#5 B5 - - - . . E5 E5",
        "A5 - . A5 G5 - E5 - C5 - E5 - A5 - - -",
        "G5 - E5 - D5 - C5 - D5 - E5 - . . E5 E5",
        "F5 - A5 - C6 - A5 - F5 - A5 - G5 - F5 -",
        "E5 - - . G#5 - - . B5 - - - E6 - - -",
    ]
    B = [
        "E5 . E5 . G5 . E5 . C6 . G5 . E5 . G5 .",
        "D5 . D5 . G5 . D5 . B5 . G5 . D5 . G5 .",
        "C5 . C5 . F5 . C5 . A5 . F5 . C5 . F5 .",
        "B4 . B4 . E5 . B4 . G#5 . E5 . B4 . G#5 .",
    ]
    lead = A + B + A[:4]
    roots = [45, 45, 41, 40, 45, 45, 41, 40, 48, 43, 41, 40, 45, 45, 41, 40]
    melody(tr, lead[:8], 0, vol=0.20, duty=0.25, decay=3.0)
    melody(tr, lead[8:12], 8, vol=0.20, duty=0.5, decay=3.0)
    melody(tr, lead[12:], 12, vol=0.20, duty=0.25, decay=3.0)
    for b, r in enumerate(roots):
        t0 = b * 16 * S
        for s in range(16):
            ts = t0 + s * S
            # bas: osemki, na przemian prym i oktawa
            if s % 2 == 0:
                tr.tone(ts, hz(r + (12 if s % 4 == 2 else 0)), S * 1.6, 0.30, wave="tri", decay=5.0)
            if s % 8 in (0,):
                tr.kick(ts)
            if s % 8 == 6:
                tr.kick(ts, 0.40)
            if s % 8 == 4:
                tr.snare(ts)
            if s % 2 == 0:
                tr.hat(ts, 0.07 if s % 4 else 0.11)
            # sztywne akordy "szlabanu" na 'i' drugiej i czwartej cwiartki
            if s in (6, 14):
                for iv in (0, 7, 12):
                    tr.tone(ts, hz(r + 24 + iv), S * 1.5, 0.07, duty=0.5, decay=8.0)
        if b % 4 == 0:
            tr.bell(t0, hz(midi("E6")))   # dzwon myta
        if b % 4 == 2:
            tr.bell(t0, hz(midi("A5")), vol=0.10)
    return tr


# ---------------------------------------------------------------- BOSS 2: BANKER
def banker():
    tr = Track(16, 60.0 / 150 / 4)
    S = tr.step
    theme = [
        "A5 - - - D6 - - - C6 - Bb5 - A5 - - .",
        "Bb5 - - - G5 - - - A5 - Bb5 - C6 - - .",
        "C#6 - - - E6 - - - D6 - C#6 - A5 - - .",
        "D6 - - - . . A5 D6 F6 - E6 D6 - - . .",
        "D6 - - - F6 - - - E6 - D6 - Bb5 - - .",
        "D6 - - - Bb5 - - - C6 - D6 - Eb6 - - .",
        "E6 - - - A6 - - - G6 - F6 - E6 - D6 C#6",
        "D6 - - - - - - - . . . . . . . .",
    ]
    prog = [(50, "m"), (43, "m"), (45, "M"), (50, "m"), (46, "M"), (43, "m"), (45, "M"), (50, "m")]
    iv = {"m": (0, 3, 7), "M": (0, 4, 7)}
    melody(tr, theme, 0, vol=0.17, duty=0.5, decay=1.6, vib=0.006)
    melody(tr, theme, 8, vol=0.17, duty=0.5, decay=1.6, vib=0.006)   # drugi obieg: ta sama fraza, ale z padem i dodatkowym kopnieciem
    for b in range(16):
        r, q = prog[b % 8]
        t0 = b * 16 * S
        a, c, d = iv[q]
        arp = [r + 12 + a, r + 12 + c, r + 12 + d, r + 24 + a]
        order = [0, 1, 2, 3, 2, 1, 2, 3]
        for s in range(16):
            ts = t0 + s * S
            tr.tone(ts, hz(arp[order[s % 8]] + 12), S * 0.9, 0.085, duty=0.125, decay=16.0)   # klawesyn
            if s % 4 == 0:
                walk = (0, 7, 12, 7)[s // 4]
                tr.tone(ts, hz(r + walk - 12), S * 3.6, 0.30, wave="tri", decay=3.0)
            if s % 8 == 0:
                tr.kick(ts, 0.45)
            if s % 8 == 4:
                tr.snare(ts, 0.20, 260.0)
            tr.hat(ts, 0.06 if s % 2 else 0.09, 160.0)
        if b >= 8:
            if b % 2 == 1:
                tr.kick(t0 + 14 * S, 0.38)
            tr.tone(t0, hz(r + 24), S * 8, 0.05, duty=0.5, decay=1.2)   # chlodny pad na drugim obiegu
        if b % 4 == 3:
            # brzdek monet: dwa wysokie "ping" na koncu frazy
            tr.bell(t0 + 12 * S, hz(midi("B6")), vol=0.10, decay=7.0, length=0.6)
            tr.bell(t0 + 13 * S, hz(midi("E7")), vol=0.10, decay=7.0, length=0.6)
    return tr


# ---------------------------------------------------------------- BOSS 3: AUCTIONEER
def auctioneer():
    tr = Track(16, 60.0 / 170 / 4)
    S = tr.step
    pat = [0, 0, 12, 0, 0, 0, 7, 0, 0, 12, 0, 0, 7, 0, 0, 0]       # paplanina licytatora (polprzesuniecia nuty bazowej)
    pat2 = [0, None, 0, 12, 0, 7, 0, None, 0, 12, 0, 7, 0, 0, 12, None]
    bases = [76, 77, 79, 81,   79, 81, 83, 84,   76, 77, 79, 81,   81, 83, 84, 88]   # kolejne, rosnace oferty
    roots = [40, 41, 43, 45,   43, 45, 47, 48,   40, 41, 43, 45,   45, 47, 48, 52]
    for b in range(16):
        t0 = b * 16 * S
        base = bases[b]
        r = roots[b]
        p = pat if b % 2 == 0 else pat2
        last = b % 4 == 3
        for s in range(16):
            ts = t0 + s * S
            off = p[s]
            if off is not None and not (last and s >= 12):
                tr.tone(ts, hz(base + off), S * 0.6, 0.15, duty=0.25, decay=14.0)
            # bas: pulsujace osemki z chromatycznym "szarpnieciem"
            if s % 2 == 0 and not (last and s >= 12):
                tr.tone(ts, hz(r + (12 if s % 8 == 6 else 0)), S * 1.5, 0.28, wave="tri", decay=6.0)
            if s % 4 == 0 and not (last and s >= 12):
                tr.kick(ts, 0.5)
            if s % 8 == 4:
                tr.snare(ts, 0.28)
            tr.hat(ts, 0.07 if s % 2 else 0.10)
        if b >= 8:
            # druga polowa: kontrmelodia "mosiadzu" w interwale kwarty
            for s in range(0, 16, 4):
                if not (last and s >= 12):
                    tr.tone(t0 + s * S, hz(base - 5), S * 3.2, 0.09, duty=0.5, decay=2.5)
        if last:
            # "SOLD!" - uderzenie mlotka, krotka pauza i akord
            tr.gavel(t0 + 12 * S)
            tr.gavel(t0 + 14 * S, 0.5)
            for iv_ in (0, 3, 7, 12):
                tr.tone(t0 + 14 * S, hz(base - 12 + iv_), S * 2.0, 0.07, duty=0.5, decay=4.0)
    return tr


if __name__ == "__main__":
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "_Project", "Resources", "Music")
    os.makedirs(out, exist_ok=True)
    for name, fn in (("boss1", toll_keeper), ("boss2", banker), ("boss3", auctioneer)):
        fn().save(os.path.join(out, name + ".wav"))
