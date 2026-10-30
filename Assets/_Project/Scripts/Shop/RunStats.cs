using System;
using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>
    /// Wszystkie modyfikatory biezacego runu. Zyski maja poziomy i sa przeliczane od zera (Recompute),
    /// dzieki czemu Poborca moze zabrac zysk. Ceny to liczniki "stackow".
    /// </summary>
    public class RunStats
    {
        public const int BaseHp = 6;

        // ---- zyski (poziomy) ----
        public readonly Dictionary<string, int> levels = new Dictionary<string, int>();

        // ---- wartosci pochodne od zyskow (ustawia Recompute) ----
        public int maxHp;
        public float fireRateMul;
        public int shotCount;
        public int pierce;
        public int dashLevel;
        public float magnetRange;
        public int shieldMax;
        public int ricochet, explosion, vampire, slowAura, drones, crit, mines, freeze;
        // drzewka zyskow warunkowych
        public int droneDmg, droneRate, droneOrbit, shockwave, doubleDash, dashCrit, critBlast, critCoins, tripleCrit, debtWeapon, rewind, priceLock;

        // ---- ceny (stacki) ----
        public int fogStacks, pixelStacks, flickerStacks, aimLagStacks, legsStacks;
        public readonly Dictionary<string, int> priceCounts = new Dictionary<string, int>();
        public int zoomStacks, fragileStacks, costlyHealStacks, hungerStacks, invertStacks, silenceStacks, inputDelayStacks, priceyShopStacks, inertiaStacks;

        public readonly Weapon weapon;

        public RunStats(Weapon weapon = Weapon.Pistol)
        {
            this.weapon = weapon;
            Recompute();
        }

        // ---- parametry broni ----
        public float BulletSpeed => weapon == Weapon.Laser ? 24f : weapon == Weapon.Shotgun ? 11f : 12f;
        public float BulletLife => weapon == Weapon.Shotgun ? 0.55f : weapon == Weapon.Laser ? 1.4f : 2.5f;   // strzelba ~6 j. zasiegu
        public float SpreadStep => weapon == Weapon.Shotgun ? 7.5f * 4f / Mathf.Max(4, shotCount - 1) : 12f;   // 5 srutow = stozek ok. 30 st.
        public bool LongBullets => weapon == Weapon.Laser;

        public int PriceCount(string name) => priceCounts.TryGetValue(name, out var c) ? c : 0;
        public void AddPrice(string name) => priceCounts[name] = PriceCount(name) + 1;

        public int Level(string gainName) => levels.TryGetValue(gainName, out var l) ? l : 0;
        public bool HasDash => dashLevel > 0;

        public void Recompute()
        {
            maxHp = BaseHp - fragileStacks + bonusMaxHp;
            fireRateMul = weapon == Weapon.Shotgun ? 0.6f : weapon == Weapon.Laser ? 1f : 1f;
            shotCount = weapon == Weapon.Shotgun ? 5 : 1;
            pierce = 0;
            dashLevel = 0;
            magnetRange = 1.8f;
            shieldMax = 0;
            ricochet = explosion = vampire = slowAura = drones = crit = mines = freeze = 0;
            droneDmg = droneRate = droneOrbit = shockwave = doubleDash = dashCrit = critBlast = critCoins = tripleCrit = debtWeapon = rewind = priceLock = 0;

            foreach (var g in Catalog.Gains)
            {
                int lv = Level(g.name);
                for (int i = 0; i < lv; i++) g.apply?.Invoke(this);
            }
            maxHp = Mathf.Max(1, maxHp);
        }

        /// <summary>Poborca: usuwa losowy posiadany zysk (z poziomami). Zwraca jego nazwe albo null.</summary>
        public string StealRandomGain(out int level)
        {
            level = 0;
            var owned = new List<string>();
            foreach (var kv in levels) if (kv.Value > 0) owned.Add(kv.Key);
            if (owned.Count == 0) return null;
            string name = owned[UnityEngine.Random.Range(0, owned.Count)];
            level = levels[name];
            levels.Remove(name);
            Recompute();
            return name;
        }

        /// <summary>Oddaje zabrany zysk z poziomem, jaki gracz mial w chwili kradziezy.</summary>
        public void RestoreGain(string name, int level)
        {
            levels.TryGetValue(name, out int now);
            levels[name] = Mathf.Max(now, level);
            Recompute();
        }

        public int bonusMaxHp;              // kontrakty
        public float tempFireMul = 1f;      // kontrakty (czasowe)
        public float FireInterval => 0.35f / (fireRateMul * tempFireMul);
        public float MoveSpeed => 5f * Curve(legsStacks, 1f, 0.92f, 0.76f, 0.58f);

        /// <summary>Krzywa nieliniowa stackow ceny: indeks = liczba stackow (0..3), dalej wartosc ostatnia.</summary>
        private static float Curve(int n, float s0, float s1, float s2, float s3) => n <= 0 ? s0 : n == 1 ? s1 : n == 2 ? s2 : s3;
        public float CritChance => crit * 0.12f + 0.06f * Mathf.Min(crit, 1) + (weapon == Weapon.Pistol ? 0.08f : 0f);   // pistolet: celny, krytyki
        public float CritMultiplier => (fogStacks > 0 ? 5f : 3f) + 2f * tripleCrit;   // synergia: Mgla + Krytyk
        public float ExplosionRadius => (1.1f + 0.4f * explosion) * (weapon == Weapon.Shotgun ? 1.25f : 1f);   // strzelba: wieksze wybuchy
        public int ExplosionDamage => explosion >= 3 ? 2 : 1;
        public int VampireKills => vampire >= 3 ? 5 : vampire == 2 ? 8 : 12;
        public float SlowAuraRadius => 1.8f + 0.7f * slowAura;
        public float SlowAuraFactor => slowAura >= 3 ? 0.5f : slowAura == 2 ? 0.62f : 0.75f;
        public float DroneInterval => (pixelStacks > 0 ? 0.75f : 1.1f) * Mathf.Pow(0.75f, droneRate);   // synergia: Pikselizacja + Pet-dron
        public int DroneDamage => (pixelStacks > 0 ? 2 : 1) + droneDmg;
        public float FreezeDuration => 1.5f + freeze * 1.0f;
        public float ShieldRegen => legsStacks > 0 ? 10f : 0f;           // synergia: Ciezkie nogi + Tarcza
        /// <summary>Dlug jako bron: +1 obrazen przy jakimkolwiek dlugu, +1 przy dlugu >= 30 (poziom 2).</summary>
        public int DebtDamageBonus(int debt) => debtWeapon <= 0 || debt <= 0 ? 0 : 1 + (debtWeapon >= 2 && debt >= 30 ? 1 : 0);
        public float ShockwaveRadius => 2.2f + 0.6f * shockwave;
        public float InertiaTime => Curve(inertiaStacks, 0f, 0.10f, 0.22f, 0.40f);   // czas rozpedzania; hamowanie 1.3x dluzej
        public float InputDelay => Curve(inputDelayStacks, 0f, 0.15f, 0.3f, 0.5f);   // czas rozpedzania do pelnej predkosci
        public float DashCooldown => dashLevel >= 3 ? 0.7f : dashLevel == 2 ? 0.95f : 1.2f;

        /// <summary>Promien widocznosci w jednostkach swiata; 0 = brak mgly.</summary>
        public float FogRadius => fogStacks <= 0 ? 0f : Mathf.Max(2.2f, Curve(fogStacks, 0f, 9f, 6.2f, 3.8f) - 0.6f * Mathf.Max(0, fogStacks - 3));

        /// <summary>Cena "Migotanie": co 8 s paleta odwraca sie na (1 + 0.5*stack) s.</summary>
        public float FlickerDuration => flickerStacks <= 0 ? 0f : Curve(flickerStacks, 0f, 0.8f, 1.7f, 2.8f);

        /// <summary>Cena "Zblizenie": mnoznik rozmiaru ortograficznego kamery.</summary>
        public float CameraZoom => Curve(zoomStacks, 1f, 0.93f, 0.76f, 0.58f);

        /// <summary>Cena "Glod": monety znikaja po tylu sekundach (0 = nigdy).</summary>
        public float CoinLifetime => hungerStacks <= 0 ? 0f : Curve(hungerStacks, 0f, 10f, 6.5f, 3.5f);
    }

    public class Gain
    {
        public string name, description, icon;
        public int cost;
        public int maxLevel = 3;
        public Action<RunStats> apply;           // wywolywane raz na poziom (kumulatywnie)
        public Action<RunStats, Player> onBuy;   // natychmiastowy efekt przy zakupie
        public Func<RunStats, bool> available;   // dodatkowy warunek puli (zysk warunkowy: pojawia sie 2x czesciej)
        public bool consumable;                  // brak poziomow (np. leczenie)
        public bool heal;                        // koszt rosnie przez cene "Drogie zycie"
    }

    public class Price
    {
        public string name, description, icon;
        public Action<RunStats> apply;
        public Action<RunStats> revert;   // REDEEM: zdejmuje 1 stack
        public Func<RunStats, bool> available;
        public int severity = 1;   // dotkliwosc 1-3: zniznka kosztu zysku 0/15/30% i wyzsza nagroda za sprzedaz
    }

    public class Offer
    {
        public Gain gain;
        public Price price;
        public int cost;
        public int level;   // poziom zysku po zakupie
        public bool sold;   // kupiona w tej wizycie
        public bool free;   // pierwszy zakup w runie
        public bool sell;   // oferta "Sprzedaj": bierzesz cene, dostajesz monety
        public int reward;
        public int stacks = 1;   // ile stackow ceny wchodzi (potezne zyski = 2)
        public int shownCost = -1;   // cena zamrozona w chwili pokazania oferty (-1 = jeszcze nie policzona)
    }

    /// <summary>Katalog ofert. Dodanie zysku lub ceny = jedna pozycja na liscie.</summary>
    public static class Catalog
    {
        public static readonly List<Gain> Gains = new List<Gain>
        {
            new Gain { name = "FASTER SHOTS", icon = "icon_gain_firerate", description = "+50% FIRE RATE", cost = 5,
                apply = s => s.fireRateMul *= 1.5f },
            new Gain { name = "MORE HEALTH", heal = true, icon = "icon_heart", description = "+3 MAX HP AND HEAL 3", cost = 8,
                apply = s => s.maxHp += 3, onBuy = (s, p) => p.Heal(3) },
            new Gain { name = "TRIPLE SHOT", icon = "icon_gain_triple", description = "MORE BULLETS PER SHOT", cost = 8, maxLevel = 2,
                apply = s => s.shotCount += 2 },
            new Gain { name = "HEALING", heal = true, icon = "icon_gain_heal", description = "FULL HP", cost = 3, consumable = true,
                onBuy = (s, p) => p.Heal(99) },
            new Gain { name = "PIERCING", icon = "icon_gain_pierce", description = "BULLETS PIERCE +1 ENEMY", cost = 6,
                apply = s => s.pierce += 1 },
            new Gain { name = "DASH", icon = "icon_gain_dash", description = "SPACE: DASH, SHORTER COOLDOWN", cost = 7,
                apply = s => s.dashLevel += 1 },
            new Gain { name = "MAGNET", icon = "icon_gain_magnet", description = "PULLS COINS FROM FAR AWAY", cost = 4,
                apply = s => s.magnetRange += 2.5f },
            new Gain { name = "RICOCHET", icon = "icon_gain_ricochet", description = "BULLETS BOUNCE OFF WALLS", cost = 6,
                apply = s => s.ricochet += 1 },
            new Gain { name = "EXPLOSION", icon = "icon_gain_explosion", description = "DEAD ENEMIES BLOW UP", cost = 8,
                apply = s => s.explosion += 1 },
            new Gain { name = "VAMPIRISM", icon = "icon_gain_vampire", description = "KILLS HEAL YOU NOW AND THEN", cost = 7,
                apply = s => s.vampire += 1 },
            new Gain { name = "SLOW AURA", icon = "icon_gain_aura", description = "ENEMIES NEAR YOU MOVE SLOWER", cost = 5,
                apply = s => s.slowAura += 1 },
            new Gain { name = "DRONE", icon = "icon_gain_drone", description = "A COMPANION SHOOTS FOR YOU", cost = 8, maxLevel = 1,
                apply = s => s.drones += 1 },
            new Gain { name = "DRONE POWER", icon = "icon_gain_drone_power", description = "DRONE SHOTS HIT +1 HARDER", cost = 7, maxLevel = 2,
                apply = s => s.droneDmg += 1, available = s => s.drones > 0 },
            new Gain { name = "DRONE RATE", icon = "icon_gain_drone_rate", description = "DRONE SHOOTS 33% FASTER", cost = 7, maxLevel = 2,
                apply = s => s.droneRate += 1, available = s => s.drones > 0 },
            new Gain { name = "MORE DRONES", icon = "icon_gain_drone_more", description = "+1 DRONE (UP TO 3)", cost = 9, maxLevel = 2,
                apply = s => s.drones += 1, available = s => s.drones > 0 },
            new Gain { name = "DRONE ORBIT", icon = "icon_gain_drone_orbit", description = "DRONES BLOCK ENEMY BULLETS", cost = 9, maxLevel = 1,
                apply = s => s.droneOrbit += 1, available = s => s.drones > 0 },
            new Gain { name = "CRITICAL", icon = "icon_gain_crit", description = "CHANCE FOR TRIPLE DAMAGE", cost = 6,
                apply = s => s.crit += 1 },
            new Gain { name = "SHOCKWAVE", icon = "icon_gain_dash_shockwave", description = "DASH END PUSHES AND HURTS NEARBY ENEMIES", cost = 9, maxLevel = 2,
                apply = s => s.shockwave += 1, available = s => s.dashLevel > 0 },
            new Gain { name = "DOUBLE DASH", icon = "icon_gain_dash_double", description = "DASH AGAIN RIGHT AFTER A DASH", cost = 9, maxLevel = 1,
                apply = s => s.doubleDash += 1, available = s => s.dashLevel > 0 },
            new Gain { name = "DASH CRIT", icon = "icon_gain_dash_crit", description = "SHOTS CRIT FOR 2 S AFTER A DASH", cost = 8, maxLevel = 1,
                apply = s => s.dashCrit += 1, available = s => s.dashLevel > 0 },
            new Gain { name = "DASH MINES", icon = "icon_gain_mines", description = "DASH DROPS MINES", cost = 7,
                apply = s => s.mines += 1, available = s => s.dashLevel > 0 },
            new Gain { name = "CRIT BLAST", icon = "icon_gain_crit_explode", description = "CRITICAL KILLS EXPLODE", cost = 9, maxLevel = 1,
                apply = s => s.critBlast += 1, available = s => s.crit > 0 },
            new Gain { name = "CRIT COINS", icon = "icon_gain_crit_coins", description = "CRITICAL KILLS DROP +1 COIN", cost = 6, maxLevel = 2,
                apply = s => s.critCoins += 1, available = s => s.crit > 0 },
            new Gain { name = "TRIPLE CRIT", icon = "icon_gain_crit_triple", description = "CRITICAL HITS DEAL +2 DAMAGE", cost = 8, maxLevel = 1,
                apply = s => s.tripleCrit += 1, available = s => s.crit > 0 },
            new Gain { name = "DEBT WEAPON", icon = "icon_gain_debt_weapon", description = "SHOTS HIT +1 HARDER WHILE IN DEBT (LV2: +1 MORE AT 30+)", cost = 8, maxLevel = 2,
                apply = s => s.debtWeapon += 1 },
            new Gain { name = "TIME REWIND", icon = "icon_gain_time_rewind", description = "ONCE PER WAVE A FATAL HIT IS UNDONE", cost = 10, maxLevel = 1,
                apply = s => s.rewind += 1 },
            new Gain { name = "PRICE LOCK", icon = "icon_gain_price_lock", description = "NEW PRICES CAN BE USED TWICE PER SHOP", cost = 4, maxLevel = 1,
                apply = s => s.priceLock += 1 },
            new Gain { name = "TIME FREEZE", icon = "icon_gain_freeze", description = "F: FREEZE ENEMIES ONCE PER WAVE", cost = 7,
                apply = s => s.freeze += 1 },
            new Gain { name = "SHIELD", icon = "icon_gain_shield", description = "BLOCKS A HIT EACH WAVE", cost = 7,
                apply = s => s.shieldMax += 1 },
        };

        public static readonly List<Price> Prices = new List<Price>
        {
            new Price { name = "FOG", icon = "icon_fog_eye", description = "YOU ONLY SEE NEAR YOURSELF",
                apply = s => s.fogStacks++, revert = s => s.fogStacks-- },
            new Price { name = "PIXELATION", icon = "icon_pixelation_grid", description = "WORSE PICTURE RESOLUTION",
                apply = s => s.pixelStacks++, revert = s => s.pixelStacks-- },
            new Price { name = "FLICKER", icon = "icon_flicker_flash", description = "PALETTE INVERTS EVERY FEW SECONDS",
                apply = s => s.flickerStacks++, revert = s => s.flickerStacks-- },
            new Price { name = "SLUGGISH AIM", icon = "icon_heavy_anchor", description = "AIM LAGS BEHIND THE MOUSE",
                apply = s => s.aimLagStacks++, revert = s => s.aimLagStacks-- },
            new Price { name = "ZOOM IN", icon = "icon_zoom", description = "CAMERA CLOSER, ENEMIES APPEAR LATE",
                apply = s => s.zoomStacks++, revert = s => s.zoomStacks--, available = s => s.zoomStacks < 3 },
            new Price { name = "FRAGILITY", icon = "icon_fragile", description = "-1 MAX HP",
                apply = s => s.fragileStacks++, revert = s => s.fragileStacks--, available = s => s.maxHp > 2 },
            new Price { name = "COSTLY LIFE", icon = "icon_costly", description = "HEALING GAINS COST DOUBLE",
                apply = s => s.costlyHealStacks++, revert = s => s.costlyHealStacks--, available = s => s.costlyHealStacks < 2 },
            new Price { name = "HUNGER", icon = "icon_hunger", description = "COINS VANISH FROM THE FLOOR",
                apply = s => s.hungerStacks++, revert = s => s.hungerStacks--, available = s => s.hungerStacks < 3 },
            new Price { name = "REVERSED CONTROLS", icon = "icon_reversed", description = "CONTROLS FLIP FOR SHORT MOMENTS",
                apply = s => s.invertStacks++, revert = s => s.invertStacks--, available = s => s.invertStacks < 3 },
            new Price { name = "SILENCE", icon = "icon_silence", description = "SOUND CUTS OUT FOR A MOMENT, NOW AND THEN",
                apply = s => s.silenceStacks++, revert = s => s.silenceStacks--, available = s => s.silenceStacks < 3 },
            new Price { name = "INERTIA", icon = "icon_inertia", description = "YOU SLIDE: SLOW TO START AND TO STOP",
                apply = s => s.inertiaStacks++, revert = s => s.inertiaStacks--, available = s => s.inertiaStacks < 3 },
            new Price { name = "INPUT DELAY", icon = "icon_input_delay", description = "MOVEMENT RESPONDS LATE",
                apply = s => s.inputDelayStacks++, revert = s => s.inputDelayStacks--, available = s => s.inputDelayStacks < 3 },
            new Price { name = "PRICEY SHOP", icon = "icon_pricey_shop", description = "EVERYTHING IN SHOPS COSTS +20%",
                apply = s => s.priceyShopStacks++, revert = s => s.priceyShopStacks--, available = s => s.priceyShopStacks < 3 },
            new Price { name = "HEAVY LEGS", icon = "icon_legs_boot", description = "SLOWER MOVEMENT",
                apply = s => s.legsStacks++, revert = s => s.legsStacks-- },
        };

        static Catalog()
        {
            void Sev(int v, params string[] names) { foreach (var n in names) Prices.Find(p => p.name == n).severity = v; }
            Sev(0, "FLICKER");
            Sev(1, "SILENCE", "ZOOM IN", "INPUT DELAY", "SLUGGISH AIM", "PIXELATION");
            Sev(2, "INERTIA", "COSTLY LIFE", "FOG", "HEAVY LEGS");
            Sev(3, "FRAGILITY", "HUNGER", "PRICEY SHOP");
            Sev(4, "REVERSED CONTROLS");
        }

        /// <summary>Mnoznik kosztu zysku zaleznie od dotkliwosci ceny: 1 = x1, 2 = x0.85, 3 = x0.7.</summary>
        public static float PriceDiscount(Price p) => 1 - p.severity * 0.1f;

        /// <summary>Nagroda za sprzedaz ceny: (5 + fala/2) * 0.8 / 1.0 / 1.3 zaleznie od dotkliwosci.</summary>
        public static int SellReward(float difficulty, Price p) =>
            Mathf.RoundToInt((5 + difficulty / 2) * (p.severity >= 3 ? 1.3f : p.severity == 2 ? 1f : 0.8f));

        /// <summary>Synergie zysk+cena: (zysk, cena, opis efektu). Efekty sa w RunStats / Drone / Player.</summary>
        private static readonly string[][] Synergies =
        {
            new[] { "CRITICAL", "FOG", "CRITS HIT x5 WHILE FOGGED" },
            new[] { "DRONE", "PIXELATION", "DRONE SHOOTS FASTER AND HARDER" },
            new[] { "SHIELD", "HEAVY LEGS", "SHIELD RECHARGES DURING A WAVE" },
        };

        /// <summary>Opis synergii dla oferty (z cena z tej karty albo z juz posiadana), inaczej null.</summary>
        public static string SynergyText(Offer o, RunStats stats)
        {
            foreach (var s in Synergies)
            {
                bool gainHere = o.gain != null && o.gain.name == s[0];
                bool priceHere = o.price.name == s[1];
                if (gainHere && (priceHere || stats.PriceCount(s[1]) > 0)) return s[2];
                if (priceHere && stats.Level(s[0]) > 0) return s[2];
            }
            return null;
        }

        public static int GainCost(Gain g, int level, float difficulty, RunStats stats)
        {
            // x1.3 (dochod czlowieka ~65% dochodu bota); drozeje z liczba posiadanych zyskow (+1 za poziom) i z fala (wave/3)
            int owned = 0;
            foreach (var kv in stats.levels) owned += kv.Value;
            // mnoznik ceny rosnie ze strefa (6 fal na strefe): dochod rosnie szybciej niz ceny, wiec strefy 2-3 drozeja
            float zoneMul = difficulty <= 6 ? 1.2f : difficulty <= 12 ? 1.4f : 1.5f;
            int cost = Mathf.RoundToInt((g.cost + (level - 1) * 3 + owned + difficulty / 3) * zoneMul);
            return g.heal ? cost * (1 + stats.costlyHealStacks) : cost;
        }

        public static List<Offer> Roll(int count, float difficulty, RunStats stats)
        {
            var gains = Gains.FindAll(g =>
                (g.available == null || g.available(stats)) &&
                (g.consumable || stats.Level(g.name) < g.maxLevel));
            var prices = Prices.FindAll(p => p.available == null || p.available(stats));
            var offers = new List<Offer>();
            var pool = new List<Gain>(gains);
            foreach (var cg in gains) if (cg.available != null) pool.Add(cg);   // zyski warunkowe: 2x wieksza szansa po spelnieniu warunku
            for (int i = 0; i < count && gains.Count > 0; i++)
            {
                var g = pool[UnityEngine.Random.Range(0, pool.Count)];
                gains.Remove(g);
                pool.RemoveAll(x => x == g);
                var p = prices[UnityEngine.Random.Range(0, prices.Count)];
                int level = g.consumable ? 1 : stats.Level(g.name) + 1;
                offers.Add(new Offer { gain = g, price = p, level = level, cost = GainCost(g, level, difficulty, stats), stacks = g.cost >= 9 && difficulty >= 7 ? 2 : 1 });
            }
            if (difficulty >= 2 && offers.Count >= 3 && UnityEngine.Random.value < 0.6f)
            {
                var sp = prices[UnityEngine.Random.Range(0, prices.Count)];
                offers[offers.Count - 1] = new Offer { gain = null, price = sp, sell = true, reward = SellReward(difficulty, sp) };
            }
            return offers;
        }
    }
}
