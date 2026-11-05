using System;
using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>
    /// Wszystkie modyfikatory biezacego runu. Zyski maja poziomy i sa przeliczane od zera (Recompute),
    /// dzieki czemu Poborca moze zabrac zysk. Ceny to liczniki "stackow". Liczby pochodza z GameConfig.
    /// </summary>
    public class RunStats
    {
        private static GameConfig C => GameConfig.Instance;
        private static GainsConfig G => GameConfig.Instance.gains;
        private static PricesConfig P => GameConfig.Instance.prices;
        private WeaponConfig W => C.GetWeapon(weapon);

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
        public float BulletSpeed => W.bulletSpeed;
        public float BulletLife => W.bulletLife;
        public float SpreadStep => W.coneDegrees > 0f ? W.coneDegrees / Mathf.Max(4, shotCount - 1) : W.spreadStep;   // strzelba: 5 srutow = stozek ok. 30 st.
        public bool LongBullets => weapon == Weapon.Laser;
        /// <summary>Laser: TRIPLE SHOT poszerza wiazke (poziomy 0/1/2 = x1, x1.8, x2.6) zamiast mnozyc pociski.</summary>
        public float BeamWidthMul => 1f + G.tripleShot.beamWidthPerLevel * Level("TRIPLE SHOT");

        public int PriceCount(string name) => priceCounts.TryGetValue(name, out var c) ? c : 0;
        public void AddPrice(string name) => priceCounts[name] = PriceCount(name) + 1;

        public int Level(string gainName) => levels.TryGetValue(gainName, out var l) ? l : 0;
        public bool HasDash => dashLevel > 0;

        public void Recompute()
        {
            maxHp = C.player.baseHp - fragileStacks * P.fragility.hpLossPerStack + bonusMaxHp;
            fireRateMul = W.fireRateMul;
            shotCount = W.shotCount;
            pierce = 0;
            dashLevel = 0;
            magnetRange = C.player.magnetBaseRange;
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
        public float FireInterval => C.player.baseFireInterval / (fireRateMul * tempFireMul);
        public float MoveSpeed => C.player.moveSpeed * PriceConfig.ByStack(P.heavyLegs.speedMulByStack, legsStacks, 1f);

        /// <summary>Wartosc z listy dla poziomu zysku (poziom 1 = pierwszy element, dalej ostatni; poziom 0 traktowany jak 1).</summary>
        private static T ByLevel<T>(T[] values, int level) => values[Mathf.Clamp(level, 1, values.Length) - 1];

        public float CritChance => crit * G.critical.chancePerLevel + G.critical.firstLevelBonus * Mathf.Min(crit, 1) + W.baseCritChance;   // pistolet: celny, krytyki
        public float CritMultiplier => (fogStacks > 0 ? G.critical.fogMultiplier : G.critical.multiplier) + G.tripleCrit.extraMultiplier * tripleCrit;   // synergia: Mgla + Krytyk
        public float ExplosionRadius => (G.explosion.baseRadius + G.explosion.radiusPerLevel * explosion) * W.explosionRadiusMul;   // strzelba: wieksze wybuchy
        public int ExplosionDamage => explosion >= G.explosion.bigDamageLevel ? G.explosion.bigDamage : G.explosion.damage;
        public int VampireKills => ByLevel(G.vampirism.killsByLevel, vampire);
        public float SlowAuraRadius => G.slowAura.baseRadius + G.slowAura.radiusPerLevel * slowAura;
        public float SlowAuraFactor => ByLevel(G.slowAura.factorByLevel, slowAura);
        public float DroneInterval => (pixelStacks > 0 ? G.drone.pixelationInterval : G.drone.baseInterval) * Mathf.Pow(G.droneRate.intervalMulPerLevel, droneRate);   // synergia: Pikselizacja + Pet-dron
        public int DroneDamage => (pixelStacks > 0 ? G.drone.pixelationDamage : G.drone.baseDamage) + droneDmg;
        public float FreezeDuration => G.timeFreeze.baseDuration + freeze * G.timeFreeze.durationPerLevel;
        public float ShieldRegen => legsStacks > 0 ? G.shield.heavyLegsRegenSeconds : 0f;           // synergia: Ciezkie nogi + Tarcza
        /// <summary>Dlug jako bron: bonus przy jakimkolwiek dlugu, +1 przy dlugu >= progu (poziom 2).</summary>
        public int DebtDamageBonus(int debt) => debtWeapon <= 0 || debt <= 0 ? 0 : G.debtWeapon.bonusDamage + (debtWeapon >= 2 && debt >= G.debtWeapon.level2DebtThreshold ? 1 : 0);
        public float ShockwaveRadius => G.shockwave.baseRadius + G.shockwave.radiusPerLevel * shockwave;
        public int ShockwaveDamage => shockwave >= 2 ? G.shockwave.damageLevel2 : G.shockwave.damage;
        public float InertiaTime => PriceConfig.ByStack(P.inertia.startTimeByStack, inertiaStacks, 0f);   // czas rozpedzania; hamowanie dluzej
        public float InertiaStopMul => P.inertia.stopTimeMul;
        public float InputDelay => PriceConfig.ByStack(P.inputDelay.delayByStack, inputDelayStacks, 0f);   // czas rozpedzania do pelnej predkosci
        public float DashCooldown => ByLevel(G.dash.cooldownByLevel, dashLevel);

        /// <summary>Promien widocznosci w jednostkach swiata; 0 = brak mgly.</summary>
        public float FogRadius => fogStacks <= 0 ? 0f : Mathf.Max(P.fog.minRadius,
            PriceConfig.ByStack(P.fog.visionRadiusByStack, fogStacks, 0f) - P.fog.radiusLossPerExtraStack * Mathf.Max(0, fogStacks - P.fog.visionRadiusByStack.Length));

        /// <summary>Cena "Migotanie": co okres paleta odwraca sie na ten czas (s).</summary>
        public float FlickerDuration => PriceConfig.ByStack(P.flicker.durationByStack, flickerStacks, 0f);

        /// <summary>Cena "Zblizenie": mnoznik rozmiaru ortograficznego kamery.</summary>
        public float CameraZoom => PriceConfig.ByStack(P.zoomIn.zoomByStack, zoomStacks, 1f);

        /// <summary>Cena "Glod": monety znikaja po tylu sekundach (0 = nigdy).</summary>
        public float CoinLifetime => PriceConfig.ByStack(P.hunger.coinLifetimeByStack, hungerStacks, 0f);
    }

    public class Gain
    {
        public string name, description, icon;
        public GainConfig cfg;                   // liczby z GameConfig (cena, poziomy, waga, wykluczenia bron)
        public Action<RunStats> apply;           // wywolywane raz na poziom (kumulatywnie)
        public Action<RunStats, Player> onBuy;   // natychmiastowy efekt przy zakupie
        public Func<RunStats, bool> available;   // dodatkowy warunek puli (zysk warunkowy)
        public bool consumable;                  // brak poziomow (np. leczenie)
        public bool heal;                        // koszt rosnie przez cene "Drogie zycie"

        public int cost => cfg.cost;
        public int maxLevel => cfg.maxLevel;
        public bool OkFor(Weapon w) => cfg.OkFor(w);
    }

    public class Price
    {
        public string name, description, icon;
        public PriceConfig cfg;
        public Action<RunStats> apply;
        public Action<RunStats> revert;   // REDEEM: zdejmuje 1 stack
        public Func<RunStats, bool> available;

        /// <summary>Dotkliwosc ceny (0 i wiecej): znizka kosztu zysku i wyzsza nagroda za sprzedaz.</summary>
        public float Severity => cfg.severity;
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

    /// <summary>Katalog ofert. Dodanie zysku lub ceny = jedna pozycja na liscie (liczby w GameConfig).</summary>
    public static class Catalog
    {
        private static GainsConfig G => GameConfig.Instance.gains;
        private static PricesConfig P => GameConfig.Instance.prices;
        private static EconomyConfig E => GameConfig.Instance.economy;

        public static readonly List<Gain> Gains = new List<Gain>
        {
            new Gain { name = "FASTER SHOTS", icon = "icon_gain_firerate", description = "+50% FIRE RATE", cfg = G.fasterShots,
                apply = s => s.fireRateMul *= G.fasterShots.fireRateMul },
            new Gain { name = "MORE HEALTH", heal = true, icon = "icon_heart", description = "+3 MAX HP AND HEAL 3", cfg = G.moreHealth,
                apply = s => s.maxHp += G.moreHealth.maxHp, onBuy = (s, p) => p.Heal(G.moreHealth.heal) },
            new Gain { name = "TRIPLE SHOT", icon = "icon_gain_triple", description = "MORE BULLETS PER SHOT", cfg = G.tripleShot,
                apply = s => s.shotCount += G.tripleShot.extraShots },
            new Gain { name = "HEALING", heal = true, icon = "icon_gain_heal", description = "FULL HP", cfg = G.healing, consumable = true,
                onBuy = (s, p) => p.Heal(G.healing.heal) },
            new Gain { name = "PIERCING", icon = "icon_gain_pierce", description = "BULLETS PIERCE +1 ENEMY", cfg = G.piercing,
                apply = s => s.pierce += G.piercing.pierce },
            new Gain { name = "DASH", icon = "icon_gain_dash", description = "SPACE: DASH, SHORTER COOLDOWN", cfg = G.dash,
                apply = s => s.dashLevel += 1 },
            new Gain { name = "MAGNET", icon = "icon_gain_magnet", description = "PULLS COINS FROM FAR AWAY", cfg = G.magnet,
                apply = s => s.magnetRange += G.magnet.rangePerLevel },
            new Gain { name = "RICOCHET", icon = "icon_gain_ricochet", description = "BULLETS BOUNCE OFF WALLS", cfg = G.ricochet,
                apply = s => s.ricochet += G.ricochet.bounces },
            new Gain { name = "EXPLOSION", icon = "icon_gain_explosion", description = "DEAD ENEMIES BLOW UP", cfg = G.explosion,
                apply = s => s.explosion += 1 },
            new Gain { name = "VAMPIRISM", icon = "icon_gain_vampire", description = "KILLS HEAL YOU NOW AND THEN", cfg = G.vampirism,
                apply = s => s.vampire += 1 },
            new Gain { name = "SLOW AURA", icon = "icon_gain_aura", description = "ENEMIES NEAR YOU MOVE SLOWER", cfg = G.slowAura,
                apply = s => s.slowAura += 1 },
            new Gain { name = "DRONE", icon = "icon_gain_drone", description = "A COMPANION SHOOTS FOR YOU", cfg = G.drone,
                apply = s => s.drones += 1 },
            new Gain { name = "DRONE POWER", icon = "icon_gain_drone_power", description = "DRONE SHOTS HIT +1 HARDER", cfg = G.dronePower,
                apply = s => s.droneDmg += G.dronePower.extraDamage, available = s => s.drones > 0 },
            new Gain { name = "DRONE RATE", icon = "icon_gain_drone_rate", description = "DRONE SHOOTS 33% FASTER", cfg = G.droneRate,
                apply = s => s.droneRate += 1, available = s => s.drones > 0 },
            new Gain { name = "MORE DRONES", icon = "icon_gain_drone_more", description = "+1 DRONE (UP TO 3)", cfg = G.moreDrones,
                apply = s => s.drones += G.moreDrones.extraDrones, available = s => s.drones > 0 },
            new Gain { name = "DRONE ORBIT", icon = "icon_gain_drone_orbit", description = "DRONES BLOCK ENEMY BULLETS", cfg = G.droneOrbit,
                apply = s => s.droneOrbit += 1, available = s => s.drones > 0 },
            new Gain { name = "CRITICAL", icon = "icon_gain_crit", description = "CHANCE FOR TRIPLE DAMAGE", cfg = G.critical,
                apply = s => s.crit += 1 },
            new Gain { name = "SHOCKWAVE", icon = "icon_gain_dash_shockwave", description = "DASH END PUSHES AND HURTS NEARBY ENEMIES", cfg = G.shockwave,
                apply = s => s.shockwave += 1, available = s => s.dashLevel > 0 },
            new Gain { name = "DOUBLE DASH", icon = "icon_gain_dash_double", description = "DASH AGAIN RIGHT AFTER A DASH", cfg = G.doubleDash,
                apply = s => s.doubleDash += 1, available = s => s.dashLevel > 0 },
            new Gain { name = "DASH CRIT", icon = "icon_gain_dash_crit", description = "SHOTS CRIT FOR 2 S AFTER A DASH", cfg = G.dashCrit,
                apply = s => s.dashCrit += 1, available = s => s.dashLevel > 0 },
            new Gain { name = "DASH MINES", icon = "icon_gain_mines", description = "DASH DROPS MINES", cfg = G.dashMines,
                apply = s => s.mines += G.dashMines.minesPerLevel, available = s => s.dashLevel > 0 },
            new Gain { name = "CRIT BLAST", icon = "icon_gain_crit_explode", description = "CRITICAL KILLS EXPLODE", cfg = G.critBlast,
                apply = s => s.critBlast += 1, available = s => s.crit > 0 },
            new Gain { name = "CRIT COINS", icon = "icon_gain_crit_coins", description = "CRITICAL KILLS DROP +1 COIN", cfg = G.critCoins,
                apply = s => s.critCoins += G.critCoins.extraCoinsPerLevel, available = s => s.crit > 0 },
            new Gain { name = "TRIPLE CRIT", icon = "icon_gain_crit_triple", description = "CRITICAL HITS DEAL +2 DAMAGE", cfg = G.tripleCrit,
                apply = s => s.tripleCrit += 1, available = s => s.crit > 0 },
            new Gain { name = "DEBT WEAPON", icon = "icon_gain_debt_weapon", description = "SHOTS HIT +1 HARDER WHILE IN DEBT (LV2: +1 MORE AT 30+)", cfg = G.debtWeapon,
                apply = s => s.debtWeapon += 1 },
            new Gain { name = "TIME REWIND", icon = "icon_gain_time_rewind", description = "ONCE PER WAVE A FATAL HIT IS UNDONE", cfg = G.timeRewind,
                apply = s => s.rewind += 1 },
            new Gain { name = "PRICE LOCK", icon = "icon_gain_price_lock", description = "NEW PRICES CAN BE USED TWICE PER SHOP", cfg = G.priceLock,
                apply = s => s.priceLock += 1 },
            new Gain { name = "TIME FREEZE", icon = "icon_gain_freeze", description = "F: FREEZE ENEMIES ONCE PER WAVE", cfg = G.timeFreeze,
                apply = s => s.freeze += 1 },
            new Gain { name = "SHIELD", icon = "icon_gain_shield", description = "BLOCKS A HIT EACH WAVE", cfg = G.shield,
                apply = s => s.shieldMax += 1 },
        };

        public static readonly List<Price> Prices = new List<Price>
        {
            new Price { name = "FOG", icon = "icon_fog_eye", description = "YOU ONLY SEE NEAR YOURSELF", cfg = P.fog,
                apply = s => s.fogStacks++, revert = s => s.fogStacks--, available = s => P.fog.CanAdd(s.fogStacks) },
            new Price { name = "PIXELATION", icon = "icon_pixelation_grid", description = "WORSE PICTURE RESOLUTION", cfg = P.pixelation,
                apply = s => s.pixelStacks++, revert = s => s.pixelStacks--, available = s => P.pixelation.CanAdd(s.pixelStacks) },
            new Price { name = "FLICKER", icon = "icon_flicker_flash", description = "PALETTE INVERTS EVERY FEW SECONDS", cfg = P.flicker,
                apply = s => s.flickerStacks++, revert = s => s.flickerStacks--, available = s => P.flicker.CanAdd(s.flickerStacks) },
            new Price { name = "SLUGGISH AIM", icon = "icon_heavy_anchor", description = "AIM LAGS BEHIND THE MOUSE", cfg = P.sluggishAim,
                apply = s => s.aimLagStacks++, revert = s => s.aimLagStacks--, available = s => P.sluggishAim.CanAdd(s.aimLagStacks) },
            new Price { name = "ZOOM IN", icon = "icon_zoom", description = "CAMERA CLOSER, ENEMIES APPEAR LATE", cfg = P.zoomIn,
                apply = s => s.zoomStacks++, revert = s => s.zoomStacks--, available = s => P.zoomIn.CanAdd(s.zoomStacks) },
            new Price { name = "FRAGILITY", icon = "icon_fragile", description = "-1 MAX HP", cfg = P.fragility,
                apply = s => s.fragileStacks++, revert = s => s.fragileStacks--, available = s => P.fragility.CanAdd(s.fragileStacks) && s.maxHp > P.fragility.minMaxHp },
            new Price { name = "COSTLY LIFE", icon = "icon_costly", description = "HEALING GAINS COST DOUBLE", cfg = P.costlyLife,
                apply = s => s.costlyHealStacks++, revert = s => s.costlyHealStacks--, available = s => P.costlyLife.CanAdd(s.costlyHealStacks) },
            new Price { name = "HUNGER", icon = "icon_hunger", description = "COINS VANISH FROM THE FLOOR", cfg = P.hunger,
                apply = s => s.hungerStacks++, revert = s => s.hungerStacks--, available = s => P.hunger.CanAdd(s.hungerStacks) },
            new Price { name = "REVERSED CONTROLS", icon = "icon_reversed", description = "CONTROLS FLIP FOR SHORT MOMENTS", cfg = P.reversedControls,
                apply = s => s.invertStacks++, revert = s => s.invertStacks--, available = s => P.reversedControls.CanAdd(s.invertStacks) },
            new Price { name = "SILENCE", icon = "icon_silence", description = "SOUND CUTS OUT FOR A MOMENT, NOW AND THEN", cfg = P.silence,
                apply = s => s.silenceStacks++, revert = s => s.silenceStacks--, available = s => P.silence.CanAdd(s.silenceStacks) },
            new Price { name = "INERTIA", icon = "icon_inertia", description = "YOU SLIDE: SLOW TO START AND TO STOP", cfg = P.inertia,
                apply = s => s.inertiaStacks++, revert = s => s.inertiaStacks--, available = s => P.inertia.CanAdd(s.inertiaStacks) },
            new Price { name = "INPUT DELAY", icon = "icon_input_delay", description = "MOVEMENT RESPONDS LATE", cfg = P.inputDelay,
                apply = s => s.inputDelayStacks++, revert = s => s.inputDelayStacks--, available = s => P.inputDelay.CanAdd(s.inputDelayStacks) },
            new Price { name = "PRICEY SHOP", icon = "icon_pricey_shop", description = "EVERYTHING IN SHOPS COSTS +20%", cfg = P.priceyShop,
                apply = s => s.priceyShopStacks++, revert = s => s.priceyShopStacks--, available = s => P.priceyShop.CanAdd(s.priceyShopStacks) },
            new Price { name = "HEAVY LEGS", icon = "icon_legs_boot", description = "SLOWER MOVEMENT", cfg = P.heavyLegs,
                apply = s => s.legsStacks++, revert = s => s.legsStacks--, available = s => P.heavyLegs.CanAdd(s.legsStacks) },
        };

        /// <summary>Mnoznik kosztu zysku zaleznie od dotkliwosci ceny: 1 - dotkliwosc * discountPerSeverity.</summary>
        public static float PriceDiscount(Price p) => 1 - p.Severity * E.discountPerSeverity;

        /// <summary>Nagroda za sprzedaz ceny: (baza + trudnosc / dzielnik) * (1 + dotkliwosc * bonus), plus losowy rozrzut.</summary>
        public static int SellReward(float difficulty, Price p) =>
            Mathf.RoundToInt((E.sellRewardBase + difficulty / E.sellRewardDifficultyDivisor) * (1 + p.Severity * E.sellRewardPerSeverity))
            + UnityEngine.Random.Range(-E.sellRewardSpread, E.sellRewardSpread + 1);

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
            // dochod czlowieka ~65% dochodu bota; cena drozeje z liczba posiadanych zyskow i z trudnoscia
            int owned = 0;
            foreach (var kv in stats.levels) owned += kv.Value;
            // mnoznik ceny rosnie ze strefa: dochod rosnie szybciej niz ceny, wiec strefy 2-3 drozeja
            float zoneMul = difficulty <= E.tier1MaxDifficulty ? E.tier1Mul : difficulty <= E.tier2MaxDifficulty ? E.tier2Mul : E.tier3Mul;
            int cost = Mathf.RoundToInt((g.cost + (level - 1) * E.levelCostStep + owned * E.ownedGainCost + difficulty / E.difficultyCostDivisor) * zoneMul);
            return g.heal ? cost * (1 + stats.costlyHealStacks * GameConfig.Instance.prices.costlyLife.extraMulPerStack) : cost;
        }

        public static List<Offer> Roll(int count, float difficulty, RunStats stats)
        {
            var gains = Gains.FindAll(g =>
                (g.available == null || g.available(stats)) && g.OkFor(stats.weapon) &&
                (g.consumable || stats.Level(g.name) < g.maxLevel));
            var prices = Prices.FindAll(p => p.available == null || p.available(stats));
            var offers = new List<Offer>();
            var pool = new List<Gain>(gains);
            foreach (var cg in gains) for (int w = 1; w < cg.cfg.poolWeight; w++) pool.Add(cg);   // waga wieksza od 1: wiecej wpisow w puli (zyski warunkowe 2x)
            for (int i = 0; i < count && gains.Count > 0; i++)
            {
                var g = pool[UnityEngine.Random.Range(0, pool.Count)];
                gains.Remove(g);
                pool.RemoveAll(x => x == g);
                var p = prices[UnityEngine.Random.Range(0, prices.Count)];
                int level = g.consumable ? 1 : stats.Level(g.name) + 1;
                offers.Add(new Offer { gain = g, price = p, level = level, cost = GainCost(g, level, difficulty, stats), stacks = g.cost >= E.doubleStackMinCost && difficulty >= E.doubleStackMinDifficulty ? 2 : 1 });
            }
            if (difficulty >= E.sellOfferMinDifficulty && offers.Count >= E.offersPerShop && UnityEngine.Random.value < E.sellOfferChance)
            {
                var sp = prices[UnityEngine.Random.Range(0, prices.Count)];
                offers[offers.Count - 1] = new Offer { gain = null, price = sp, sell = true, reward = SellReward(difficulty, sp) };
            }
            return offers;
        }
    }
}
