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


if __name__ == '__main__':
    main(sys.argv[1:])
