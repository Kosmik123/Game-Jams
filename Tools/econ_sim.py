"""Economy sim for the bankruptcy model (GDD, section "Koniec gry: bankructwo"): greedy/random player, quality m, income per worker k.
Run: python Tools/econ_sim.py   (needs no packages; tune k, upkeep, free floors, caps and see how long a tower survives)."""
import random, statistics

TYPES = ['R', 'O', 'P', 'Pk', 'B']

def stats(sums, nR_O, P, k):
    """sums: effective (quality-weighted) counts per type; nRO: raw count of consumers; P: raw plants."""
    pop = 10 * sums['R']
    jobs = 10 * sums['O']
    demand = nR_O
    supply = 4 * sums['P']
    r = 1.0 if demand == 0 else min(1.0, supply / demand)
    slots = max(0.0, 5 * sums['R'] + 20 * sums['Pk'] - 10 * P)
    workers = min(jobs * r, pop * r, slots)
    return k * workers, workers

def run(k, m, policy, seed, free=8, upkeep=1.0, start=100.0, base_cap=100.0, bank_cap=150.0,
        step=1.5, grace=8.0, max_floors=400, noise=0.08):
    rng = random.Random(seed)
    cnt = {t: 0 for t in TYPES}
    eff = {t: 0.0 for t in TYPES}
    balance, neg_time, floors = start, 0.0, 0
    while floors < max_floors:
        options = [rng.choice(TYPES) for _ in range(3)]
        cap = base_cap + bank_cap * (eff['B'])
        def marginal(t, q):
            c2 = dict(cnt); e2 = dict(eff)
            c2[t] += 1; e2[t] += q
            inc, _ = stats(e2, c2['R'] + c2['O'], c2['P'], k)
            inc0, _ = stats(eff, cnt['R'] + cnt['O'], cnt['P'], k)
            return inc - inc0
        mm = m(floors) if callable(m) else m
        q = min(1.0, max(0.5, rng.gauss(mm, noise)))
        if policy == 'random':
            t = options[0]
        else:
            cur_inc, _ = stats(eff, cnt['R'] + cnt['O'], cnt['P'], k)
            net = cur_inc - upkeep * max(0, floors - free)
            def score(t):
                v = marginal(t, q)
                if t == 'B':
                    # a bank is worth its cap only when money would otherwise be wasted, or when the buffer is thin
                    need = 0.0
                    if balance > 0.7 * cap: need = 0.8
                    if net < 0 and balance < 60: need = 0.3
                    return need
                return v
            t = max(options, key=score)
        cnt[t] += 1; eff[t] += q; floors += 1
        inc, _ = stats(eff, cnt['R'] + cnt['O'], cnt['P'], k)
        cap = base_cap + bank_cap * eff['B']
        balance = min(cap, balance + (inc - upkeep * max(0, floors - free)) * step)
        if balance < 0:
            neg_time += step
            if neg_time >= grace:
                return floors, cnt
        else:
            neg_time = 0.0
    return floors, cnt

def survive(k, m, policy, n=60):
    res = [run(k, m, policy, s)[0] for s in range(n)]
    return statistics.median(res), min(res), max(res)

if __name__ == '__main__':
    ks = [0.4, 0.5, 0.6, 0.7, 0.8, 1.0]
    ms = [1.0, 0.9, 0.8, 0.7, 0.6, 0.5]
    for policy in ('greedy', 'random'):
        print('policy', policy, ' median floors survived (cap 400); rows = quality m, cols = k')
        print('m/k   ' + '  '.join(f'{k:>6}' for k in ks))
        for m in ms:
            print(f'{m:<5} ' + '  '.join(f'{survive(k, m, policy)[0]:>6.0f}' for k in ks))
        print()
