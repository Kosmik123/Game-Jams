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
        public const int BaseHp = 5;

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

        // ---- ceny (stacki) ----
        public int fogStacks, pixelStacks, flickerStacks, aimLagStacks, legsStacks;
        public readonly Dictionary<string, int> priceCounts = new Dictionary<string, int>();
        public int zoomStacks, fragileStacks, costlyHealStacks, hungerStacks, invertStacks;

        public RunStats() => Recompute();

        public int PriceCount(string name) => priceCounts.TryGetValue(name, out var c) ? c : 0;
        public void AddPrice(string name) => priceCounts[name] = PriceCount(name) + 1;

        public int Level(string gainName) => levels.TryGetValue(gainName, out var l) ? l : 0;
        public bool HasDash => dashLevel > 0;

        public void Recompute()
        {
            maxHp = BaseHp - fragileStacks;
            fireRateMul = 1f;
            shotCount = 1;
            pierce = 0;
            dashLevel = 0;
            magnetRange = 1.8f;
            shieldMax = 0;
            ricochet = explosion = vampire = slowAura = drones = crit = mines = freeze = 0;

            foreach (var g in Catalog.Gains)
            {
                int lv = Level(g.name);
                for (int i = 0; i < lv; i++) g.apply?.Invoke(this);
            }
            maxHp = Mathf.Max(1, maxHp);
        }

        /// <summary>Poborca: usuwa losowy posiadany zysk (z poziomami). Zwraca jego nazwe albo null.</summary>
        public string StealRandomGain()
        {
            var owned = new List<string>();
            foreach (var kv in levels) if (kv.Value > 0) owned.Add(kv.Key);
            if (owned.Count == 0) return null;
            string name = owned[UnityEngine.Random.Range(0, owned.Count)];
            levels.Remove(name);
            Recompute();
            return name;
        }

        public float FireInterval => 0.35f / fireRateMul;
        public float MoveSpeed => 5f * Mathf.Pow(0.8f, legsStacks);
        public float CritChance => crit * 0.12f + 0.06f * Mathf.Min(crit, 1);
        public float CritMultiplier => fogStacks > 0 ? 5f : 3f;   // synergia: Mgla + Krytyk
        public float ExplosionRadius => 1.1f + 0.4f * explosion;
        public int ExplosionDamage => explosion >= 3 ? 2 : 1;
        public int VampireKills => vampire >= 3 ? 5 : vampire == 2 ? 8 : 12;
        public float SlowAuraRadius => 1.8f + 0.7f * slowAura;
        public float SlowAuraFactor => slowAura >= 3 ? 0.5f : slowAura == 2 ? 0.62f : 0.75f;
        public float DroneInterval => pixelStacks > 0 ? 0.75f : 1.1f;   // synergia: Pikselizacja + Pet-dron
        public int DroneDamage => pixelStacks > 0 ? 2 : 1;
        public float FreezeDuration => 1.5f + freeze * 1.0f;
        public float ShieldRegen => legsStacks > 0 ? 10f : 0f;           // synergia: Ciezkie nogi + Tarcza
        public float DashCooldown => dashLevel >= 3 ? 0.7f : dashLevel == 2 ? 0.95f : 1.2f;

        /// <summary>Promien widocznosci w jednostkach swiata; 0 = brak mgly.</summary>
        public float FogRadius => fogStacks <= 0 ? 0f : Mathf.Max(2.2f, 8.5f - 1.8f * (fogStacks - 1));

        /// <summary>Cena "Migotanie": co 8 s paleta odwraca sie na (1 + 0.5*stack) s.</summary>
        public float FlickerDuration => flickerStacks <= 0 ? 0f : 1f + 0.5f * flickerStacks;

        /// <summary>Cena "Zblizenie": mnoznik rozmiaru ortograficznego kamery.</summary>
        public float CameraZoom => zoomStacks <= 0 ? 1f : Mathf.Max(0.55f, 1f - 0.15f * zoomStacks);

        /// <summary>Cena "Glod": monety znikaja po tylu sekundach (0 = nigdy).</summary>
        public float CoinLifetime => hungerStacks <= 0 ? 0f : Mathf.Max(2.5f, 9f - 2.5f * (hungerStacks - 1));
    }

    public class Gain
    {
        public string name, description, icon;
        public int cost;
        public int maxLevel = 3;
        public Action<RunStats> apply;           // wywolywane raz na poziom (kumulatywnie)
        public Action<RunStats, Player> onBuy;   // natychmiastowy efekt przy zakupie
        public Func<RunStats, bool> available;   // dodatkowy warunek puli
        public bool consumable;                  // brak poziomow (np. leczenie)
        public bool heal;                        // koszt rosnie przez cene "Drogie zycie"
    }

    public class Price
    {
        public string name, description, icon;
        public Action<RunStats> apply;
        public Func<RunStats, bool> available;
    }

    public class Offer
    {
        public Gain gain;
        public Price price;
        public int cost;
        public int level;   // poziom zysku po zakupie
        public bool sell;   // oferta "Sprzedaj": bierzesz cene, dostajesz monety
        public int reward;
    }

    /// <summary>Katalog ofert. Dodanie zysku lub ceny = jedna pozycja na liscie.</summary>
    public static class Catalog
    {
        public static readonly List<Gain> Gains = new List<Gain>
        {
            new Gain { name = "FASTER SHOTS", icon = "icon_gain_firerate", description = "+35% FIRE RATE", cost = 5,
                apply = s => s.fireRateMul *= 1.35f },
            new Gain { name = "MORE HEALTH", heal = true, icon = "icon_heart", description = "+2 MAX HP AND HEAL 2", cost = 5,
                apply = s => s.maxHp += 2, onBuy = (s, p) => p.Heal(2) },
            new Gain { name = "TRIPLE SHOT", icon = "icon_gain_triple", description = "MORE BULLETS PER SHOT", cost = 8, maxLevel = 2,
                apply = s => s.shotCount += 2 },
            new Gain { name = "HEALING", heal = true, icon = "icon_gain_heal", description = "FULL HP", cost = 4, consumable = true,
                onBuy = (s, p) => p.Heal(99) },
            new Gain { name = "PIERCING", icon = "icon_gain_pierce", description = "BULLETS PIERCE +1 ENEMY", cost = 6,
                apply = s => s.pierce += 1 },
            new Gain { name = "DASH", icon = "icon_gain_dash", description = "SPACE: DASH, SHORTER COOLDOWN", cost = 7,
                apply = s => s.dashLevel += 1 },
            new Gain { name = "MAGNET", icon = "icon_gain_magnet", description = "PULLS COINS FROM FAR AWAY", cost = 4,
                apply = s => s.magnetRange += 2.5f },
            new Gain { name = "RICOCHET", description = "BULLETS BOUNCE OFF WALLS", cost = 6,
                apply = s => s.ricochet += 1 },
            new Gain { name = "EXPLOSION", description = "DEAD ENEMIES BLOW UP", cost = 8,
                apply = s => s.explosion += 1 },
            new Gain { name = "VAMPIRISM", description = "KILLS HEAL YOU NOW AND THEN", cost = 7,
                apply = s => s.vampire += 1 },
            new Gain { name = "SLOW AURA", description = "ENEMIES NEAR YOU MOVE SLOWER", cost = 6,
                apply = s => s.slowAura += 1 },
            new Gain { name = "DRONE", description = "A COMPANION SHOOTS FOR YOU", cost = 8,
                apply = s => s.drones += 1 },
            new Gain { name = "CRITICAL", description = "CHANCE FOR TRIPLE DAMAGE", cost = 6,
                apply = s => s.crit += 1 },
            new Gain { name = "DASH MINES", description = "DASH DROPS MINES", cost = 7,
                apply = s => s.mines += 1, available = s => s.dashLevel > 0 },
            new Gain { name = "TIME FREEZE", description = "F: FREEZE ENEMIES ONCE PER WAVE", cost = 9,
                apply = s => s.freeze += 1 },
            new Gain { name = "SHIELD", icon = "icon_gain_shield", description = "BLOCKS A HIT EACH WAVE", cost = 5,
                apply = s => s.shieldMax += 1 },
        };

        public static readonly List<Price> Prices = new List<Price>
        {
            new Price { name = "FOG", icon = "icon_fog_eye", description = "YOU ONLY SEE NEAR YOURSELF",
                apply = s => s.fogStacks++ },
            new Price { name = "PIXELATION", icon = "icon_pixelation_grid", description = "WORSE PICTURE RESOLUTION",
                apply = s => s.pixelStacks++ },
            new Price { name = "FLICKER", icon = "icon_flicker_flash", description = "PALETTE INVERTS EVERY FEW SECONDS",
                apply = s => s.flickerStacks++ },
            new Price { name = "SLUGGISH AIM", icon = "icon_heavy_anchor", description = "AIM LAGS BEHIND THE MOUSE",
                apply = s => s.aimLagStacks++ },
            new Price { name = "ZOOM IN", description = "CAMERA CLOSER, ENEMIES APPEAR LATE",
                apply = s => s.zoomStacks++, available = s => s.zoomStacks < 3 },
            new Price { name = "FRAGILITY", description = "-1 MAX HP",
                apply = s => s.fragileStacks++, available = s => s.maxHp > 2 },
            new Price { name = "COSTLY LIFE", description = "HEALING GAINS COST DOUBLE",
                apply = s => s.costlyHealStacks++, available = s => s.costlyHealStacks < 2 },
            new Price { name = "HUNGER", description = "COINS VANISH FROM THE FLOOR",
                apply = s => s.hungerStacks++, available = s => s.hungerStacks < 3 },
            new Price { name = "REVERSED CONTROLS", description = "CONTROLS FLIP FOR SHORT MOMENTS",
                apply = s => s.invertStacks++, available = s => s.invertStacks < 3 },
            new Price { name = "HEAVY LEGS", icon = "icon_legs_boot", description = "-20% MOVE SPEED",
                apply = s => s.legsStacks++ },
        };

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

        public static int GainCost(Gain g, int level, int wave, RunStats stats)
        {
            int cost = g.cost + (level - 1) * 3 + wave / 4;
            return g.heal ? cost * (1 + stats.costlyHealStacks) : cost;
        }

        public static List<Offer> Roll(int count, int wave, RunStats stats)
        {
            var gains = Gains.FindAll(g =>
                (g.available == null || g.available(stats)) &&
                (g.consumable || stats.Level(g.name) < g.maxLevel));
            var prices = Prices.FindAll(p => p.available == null || p.available(stats));
            var offers = new List<Offer>();
            for (int i = 0; i < count && gains.Count > 0; i++)
            {
                var g = gains[UnityEngine.Random.Range(0, gains.Count)];
                gains.Remove(g);
                var p = prices[UnityEngine.Random.Range(0, prices.Count)];
                int level = g.consumable ? 1 : stats.Level(g.name) + 1;
                offers.Add(new Offer { gain = g, price = p, level = level, cost = GainCost(g, level, wave, stats) });
            }
            if (wave >= 2 && offers.Count >= 3 && UnityEngine.Random.value < 0.6f)
            {
                var sp = prices[UnityEngine.Random.Range(0, prices.Count)];
                offers[offers.Count - 1] = new Offer { gain = null, price = sp, sell = true, reward = 5 + wave / 2 };
            }
            return offers;
        }
    }
}
