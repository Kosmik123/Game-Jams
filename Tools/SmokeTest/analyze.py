"""Podsumowanie telemetrii bota: python analyze.py plik1.csv plik2.csv ..."""
import csv
import statistics
import sys
from collections import defaultdict


def main(paths):
    rows = []
    for p in paths:
        with open(p, newline='') as f:
            rows += list(csv.DictReader(f))

    by_strategy = defaultdict(list)
    for r in rows:
        by_strategy[r['strategy']].append(r)

    for strat, rs in sorted(by_strategy.items()):
        ends = [r for r in rs if r['event'] in ('death', 'win', 'timeout')]
        n = len(ends)
        if n == 0:
            continue
        wins = [r for r in ends if r['event'] == 'win']
        deaths = [r for r in ends if r['event'] == 'death']
        print(f"== {strat}: {n} runow, wygrane {len(wins)} ({100 * len(wins) / n:.0f}%), zgony {len(deaths)}, timeouty {n - len(wins) - len(deaths)}")
        if wins:
            print(f"   czas wygranej (s): mediana {statistics.median(float(r['simTime']) for r in wins):.0f}, min {min(float(r['simTime']) for r in wins):.0f}")
        waves = [int(r['wave']) for r in ends]
        print(f"   fala konca: srednia {statistics.mean(waves):.1f}, mediana {statistics.median(waves):.0f}, rozklad {sorted(waves)}")
        print(f"   srednie ceny (stacki) na koniec: {statistics.mean(int(r['priceStacks']) for r in ends):.1f}, "
              f"poziomy zyskow: {statistics.mean(int(r['gainLevels']) for r in ends):.1f}, dlug na koniec: {statistics.mean(int(r['debt']) for r in ends):.0f}")

        # gdzie umieraja
        where = defaultdict(int)
        for r in deaths:
            where[f"Z{r['zone']}-{r['waveInZone']}"] += 1
        print(f"   zgony wg fali: {dict(sorted(where.items()))}")

        # czasy fal
        wave_end = [r for r in rs if r['event'] == 'wave_end']
        per_wave = defaultdict(list)
        for r in wave_end:
            per_wave[(int(r['zone']), int(r['waveInZone']))].append(float(r['simTime']))
        print("   sredni czas (s) od startu runu do konca fali:", {f"Z{z}-{w}": round(statistics.mean(v)) for (z, w), v in sorted(per_wave.items())})
        coins = defaultdict(list)
        for r in wave_end:
            coins[(int(r['zone']), int(r['waveInZone']))].append(int(r['coins']))
        print("   srednie monety w sklepie:", {f"Z{z}-{w}": round(statistics.mean(v)) for (z, w), v in sorted(coins.items())})


def buys_report(paths):
    """Zyski: ile razy wybrane, w ilu runach i jaki % takich runow zakonczyl sie wygrana (python analyze.py --buys a.csv b.csv; plik *.buys.csv obok)."""
    import os
    runs_win = {}
    buys = []
    for p in paths:
        with open(p, newline='') as f:
            for r in csv.DictReader(f):
                if r['event'] in ('death', 'win', 'timeout'):
                    runs_win[(r['strategy'], r['run'], p)] = r['event'] == 'win'
        bp = p.replace('.csv', '.buys.csv')
        if os.path.exists(bp):
            with open(bp, newline='') as f:
                for row in csv.reader(f):
                    buys.append((row[0], row[1], p, row[2], row[3], int(row[4]), int(row[5])))
    gains = defaultdict(lambda: [0, set()])
    prices = defaultdict(lambda: [0, set()])
    for strat, run, p, gain, price, wave, cost in buys:
        gains[gain][0] += 1; gains[gain][1].add((strat, run, p))
        prices[price][0] += 1; prices[price][1].add((strat, run, p))
    base = sum(runs_win.values()) / max(1, len(runs_win))
    print(f"\n== zakupy (wszystkie runy: {len(runs_win)}, wygrane {100 * base:.0f}%) ==")
    for title, d in (("ZYSKI", gains), ("CENY", prices)):
        print(f"-- {title}: nazwa, zakupy, runy z tym, % wygranych w runach z tym")
        for name, (cnt, rs) in sorted(d.items(), key=lambda kv: -kv[1][0]):
            w = sum(1 for k in rs if runs_win.get(k)) / max(1, len(rs))
            print(f"   {name:18} {cnt:4}  {len(rs):3}  {100 * w:3.0f}%")


if __name__ == '__main__':
    args = sys.argv[1:]
    if args and args[0] == '--buys':
        main(args[1:]); buys_report(args[1:])
    else:
        main(args)
