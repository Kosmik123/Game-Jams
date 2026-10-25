"""Procedural 8-bit sound effects and music loop for Stack City, written as WAV with pure Python.

Usage: python sound_gen.py <out_dir>
Writes mono 16-bit 22050 Hz WAV files. Output is deterministic (fixed random seed).
"""
import math
import os
import random
import struct
import sys
import wave

RATE = 22050


def note_freq(name):
    """'C4', 'F#5', 'Bb3' -> Hz (A4 = 440)."""
    names = {'C': 0, 'D': 2, 'E': 4, 'F': 5, 'G': 7, 'A': 9, 'B': 11}
    semitone = names[name[0]]
    rest = name[1:]
    if rest[0] == '#':
        semitone += 1
        rest = rest[1:]
    elif rest[0] == 'b':
        semitone -= 1
        rest = rest[1:]
    midi = 12 * (int(rest) + 1) + semitone
    return 440.0 * 2 ** ((midi - 69) / 12)


class Osc:
    """Phase-continuous oscillator: square (with duty), triangle, saw, sine or noise."""

    def __init__(self, shape, duty=0.5, rng=None):
        self.shape = shape
        self.duty = duty
        self.phase = 0.0
        self.rng = rng or random.Random(1)
        self.noise_value = 0.0

    def step(self, freq):
        previous = self.phase
        self.phase = (self.phase + freq / RATE) % 1.0
        p = self.phase
        if self.shape == 'square':
            return 1.0 if p < self.duty else -1.0
        if self.shape == 'triangle':
            return 4.0 * p - 1.0 if p < 0.5 else 3.0 - 4.0 * p
        if self.shape == 'saw':
            return 2.0 * p - 1.0
        if self.shape == 'sine':
            return math.sin(2 * math.pi * p)
        # noise: new random value every period, so freq sets the "pitch" of the noise
        if self.phase < previous:
            self.noise_value = self.rng.uniform(-1.0, 1.0)
        return self.noise_value


def tone(duration, shape, f_start, f_end=None, volume=0.5, duty=0.5,
         attack=0.005, release=None, vibrato=0.0, lowpass=1.0, seed=1):
    """One voice with exponential pitch slide and attack/linear-decay envelope."""
    f_end = f_end or f_start
    release = duration if release is None else release
    n = int(duration * RATE)
    osc = Osc(shape, duty, random.Random(seed))
    out = []
    smooth = 0.0
    for i in range(n):
        t = i / RATE
        k = i / max(1, n - 1)
        freq = f_start * (f_end / f_start) ** k
        if vibrato:
            freq *= 1.0 + 0.02 * math.sin(2 * math.pi * vibrato * t)
        sample = osc.step(freq)
        smooth += lowpass * (sample - smooth)
        env = min(1.0, t / attack) if attack > 0 else 1.0
        tail = duration - t
        if tail < release:
            env *= max(0.0, tail / release)
        out.append(smooth * env * volume)
    return out


def silence(duration):
    return [0.0] * int(duration * RATE)


def concat(*parts):
    out = []
    for part in parts:
        out.extend(part)
    return out


def mix(*parts):
    length = max(len(p) for p in parts)
    out = [0.0] * length
    for part in parts:
        for i, s in enumerate(part):
            out[i] += s
    return out


def save(path, samples):
    peak = max(1e-9, max(abs(s) for s in samples))
    gain = 0.9 / peak if peak > 0.9 else 1.0
    frames = b''.join(struct.pack('<h', int(max(-1.0, min(1.0, s * gain)) * 32767)) for s in samples)
    with wave.open(path, 'wb') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(frames)


def arpeggio(notes, step, shape='square', volume=0.35, duty=0.25, last_hold=0.0):
    parts = []
    for i, name in enumerate(notes):
        length = step + (last_hold if i == len(notes) - 1 else 0.0)
        parts.append(tone(length, shape, note_freq(name), volume=volume, duty=duty, release=length * 0.8))
    return concat(*parts)


# --- Sound effects -------------------------------------------------------------------------

def land(base):
    body = tone(0.12, 'square', base, base * 0.5, volume=0.45, duty=0.5, release=0.1)
    thud = tone(0.06, 'noise', 900, 300, volume=0.3, lowpass=0.35, seed=2)
    return mix(body, thud)


def perfect(level):
    # Rising major arpeggio; each combo level starts two semitones higher.
    roots = ['C5', 'D5', 'E5', 'F#5', 'G#5']
    root = note_freq(roots[level - 1])
    ratios = (1.0, 2 ** (4 / 12), 2 ** (7 / 12), 2.0)
    parts = [tone(0.055 if i < 3 else 0.16, 'square', root * r, volume=0.3, duty=0.25,
                  release=0.04 if i < 3 else 0.14) for i, r in enumerate(ratios)]
    return concat(*parts)


def collapse():
    rumble = tone(0.75, 'noise', 2200, 120, volume=0.5, lowpass=0.25, seed=3, release=0.6)
    fall = tone(0.6, 'square', 320, 45, volume=0.3, duty=0.5, release=0.4)
    return mix(rumble, fall)


def coin():
    return concat(tone(0.05, 'square', note_freq('B5'), volume=0.18, duty=0.25, release=0.02),
                  tone(0.14, 'square', note_freq('E6'), volume=0.18, duty=0.25, release=0.12))


def synergy_good():
    return concat(tone(0.06, 'triangle', note_freq('E5'), volume=0.45),
                  tone(0.12, 'triangle', note_freq('A5'), volume=0.45, release=0.1))


def synergy_bad():
    return tone(0.18, 'square', 420, 190, volume=0.3, duty=0.5, release=0.12)


def time_bonus():
    return tone(0.07, 'triangle', 900, 1600, volume=0.4, release=0.05)


def time_penalty():
    return tone(0.16, 'square', 500, 180, volume=0.3, duty=0.5, release=0.1)


def tick():
    return tone(0.025, 'square', 1500, volume=0.25, duty=0.5, release=0.02)


def time_up():
    return arpeggio(['G4', 'E4', 'C4'], 0.14, duty=0.5, volume=0.35, last_hold=0.25)


def new_best():
    return arpeggio(['C5', 'E5', 'G5', 'C6'], 0.09, volume=0.32, last_hold=0.35)


def ui_click():
    return tone(0.04, 'square', 880, 1100, volume=0.25, duty=0.25, release=0.03)


# --- Music ---------------------------------------------------------------------------------

BPM = 120
EIGHTH = 60.0 / BPM / 2

# I-vi-IV-V in C, 8 bars (each chord twice). Bass plays root/octave eighths.
CHORDS = ['C', 'Am', 'F', 'G', 'C', 'Am', 'F', 'G']
CHORD_ROOT = {'C': 'C2', 'Am': 'A1', 'F': 'F2', 'G': 'G2'}

# Lead melody in eighths; '-' holds the previous note, '.' is a rest. 8 eighths per bar.
MELODY = [
    'E5', '-', 'G5', '-', 'C6', '-', 'G5', 'E5',
    'C5', '-', 'E5', '-', 'A5', '-', '-', '.',
    'F5', '-', 'A5', '-', 'C6', '-', 'A5', 'F5',
    'D5', '-', 'G5', '-', 'B5', '-', '-', '.',
    'E5', 'G5', 'C6', '-', 'B5', 'G5', 'E5', '-',
    'A5', '-', 'G5', 'E5', 'C5', '-', '-', '.',
    'F5', '-', 'E5', '-', 'D5', '-', 'C5', '-',
    'D5', '-', '-', '-', 'G4', '-', '.', '.',
]

# Drums per bar in eighths: k = kick, s = snare, h = hi-hat.
DRUMS = ['k', 'h', 's', 'h', 'k', 'k', 's', 'h']


def place(track, start, samples):
    for i, s in enumerate(samples):
        if start + i < len(track):
            track[start + i] += s


def music_loop():
    step = int(EIGHTH * RATE)
    length = step * len(MELODY)
    track = [0.0] * length

    # Lead: group held notes, short release so the loop wraps without clicks.
    i = 0
    while i < len(MELODY):
        name = MELODY[i]
        hold = 1
        while i + hold < len(MELODY) and MELODY[i + hold] == '-':
            hold += 1
        if name not in ('-', '.'):
            duration = hold * EIGHTH
            place(track, i * step, tone(duration, 'square', note_freq(name), volume=0.13, duty=0.25,
                                        release=min(0.08, duration * 0.5), vibrato=5.0))
        i += hold

    for bar, chord in enumerate(CHORDS):
        root = note_freq(CHORD_ROOT[chord])
        for e in range(8):
            freq = root * (2 if e % 2 else 1)
            place(track, (bar * 8 + e) * step, tone(EIGHTH * 0.9, 'triangle', freq, volume=0.28, release=0.03))
            hit = DRUMS[e]
            if hit == 'k':
                drum = tone(0.09, 'sine', 150, 45, volume=0.45, release=0.07)
            elif hit == 's':
                drum = tone(0.1, 'noise', 4000, 2500, volume=0.18, lowpass=0.6, seed=5 + bar, release=0.08)
            else:
                drum = tone(0.03, 'noise', 9000, volume=0.07, lowpass=0.9, seed=7 + e, release=0.025)
            place(track, (bar * 8 + e) * step, drum)

    # Second pass of the loop with the same material keeps file size small; Unity loops the clip.
    return track


SOUNDS = {
    'land_residential': lambda: land(200),
    'land_office': lambda: land(240),
    'land_power': lambda: land(160),
    'land_park': lambda: land(290),
    'perfect_1': lambda: perfect(1),
    'perfect_2': lambda: perfect(2),
    'perfect_3': lambda: perfect(3),
    'perfect_4': lambda: perfect(4),
    'perfect_5': lambda: perfect(5),
    'collapse': collapse,
    'coin': coin,
    'synergy_good': synergy_good,
    'synergy_bad': synergy_bad,
    'time_bonus': time_bonus,
    'time_penalty': time_penalty,
    'tick': tick,
    'time_up': time_up,
    'new_best': new_best,
    'ui_click': ui_click,
    'music_game': music_loop,
}


if __name__ == '__main__':
    out = sys.argv[1]
    os.makedirs(out, exist_ok=True)
    for name, make in SOUNDS.items():
        samples = make()
        save(os.path.join(out, name + '.wav'), samples)
        print(f'wrote {name} ({len(samples) / RATE:.2f} s)')
