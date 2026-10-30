using System;
using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>
    /// Parametry balansu w jednym assecie (Resources/GameConfig). Liczby sa tutaj, wzory w kodzie.
    /// Brak assetu = wartosci domyslne z kodu (te same, co w assecie wygenerowanym przez Jam/Create Game Config).
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Pay With Pixels/Game Config")]
    public class GameConfig : ScriptableObject
    {
        private static GameConfig instance;

        public static GameConfig Instance
        {
            get
            {
                if (instance == null) instance = Load();
                return instance;
            }
        }

        private static GameConfig Load()
        {
            var cfg = Resources.Load<GameConfig>("GameConfig");
            if (cfg == null) cfg = CreateInstance<GameConfig>();
            cfg.FillMissing();
            return cfg;
        }

        [Header("Misc")]
        [Tooltip("True: utrata fokusu karty pauzuje gre i wycisza dzwiek. False: gra dziala dalej.")]
        public bool shouldPauseOnDefocus;

        [Header("Run")]
        public RunConfig run = new RunConfig();

        [Header("Score")]
        public ScoreConfig score = new ScoreConfig();

        [Header("Player")]
        public PlayerConfig player = new PlayerConfig();

        [Header("Weapons")]
        public List<WeaponConfig> weapons = new List<WeaponConfig>();

        [Header("Enemies")]
        public EnemyScalingConfig enemyScaling = new EnemyScalingConfig();
        public List<EnemyConfig> enemies = new List<EnemyConfig>();
        public CollectorEnemyConfig collector = new CollectorEnemyConfig();

        [Header("Economy")]
        public EconomyConfig economy = new EconomyConfig();

        [Header("Gains")]
        public GainsConfig gains = new GainsConfig();

        [Header("Prices")]
        public PricesConfig prices = new PricesConfig();

        public WeaponConfig GetWeapon(Weapon type)
        {
            foreach (var w in weapons) if (w.type == type) return w;
            return weapons[0];
        }

        public CollectorEnemyConfig GetCollector() => collector;

        public EnemyConfig GetEnemy(EnemyType type)
        {
            foreach (var e in enemies) 
                if (e.type == type) 
                    return e;

            return null;
        }

        private void OnEnable() => FillMissing();

        [ContextMenu("Reset lists to defaults")]
        private void ResetLists()
        {
            weapons = WeaponConfig.Defaults();
            enemies = EnemyConfig.Defaults();
            collector = CollectorEnemyConfig.Default;
        }

        /// <summary>Uzupelnia brakujace wpisy list (nowa bron, nowy typ wroga) wartosciami domyslnymi.</summary>
        private void FillMissing()
        {
            if (weapons == null) weapons = new List<WeaponConfig>();
            if (enemies == null) enemies = new List<EnemyConfig>();
            foreach (var d in WeaponConfig.Defaults())
                if (!weapons.Exists(w => w.type == d.type)) weapons.Add(d);
            foreach (var d in EnemyConfig.Defaults())
                if (!enemies.Exists(e => e.type == d.type)) enemies.Add(d);
        }
    }

    [Serializable]
    public class RunConfig
    {
        [Tooltip("Liczba zwyklych fal w strefie (po nich boss strefy).")]
        public int wavesPerZone = 5;
        [Tooltip("Maksymalny dlug na kredyt.")]
        public int maxDebt = 20;
        [Tooltip("O ile kazdy zakup w runie obniza Difficulty (kupowanie oszukuje graczy na korzysc).")]
        public float difficultyPerPurchase = 0.15f;
        [Tooltip("Przyrost mnoznika wyniku za kazdy poziom Ascension.")]
        public float ascensionScoreStep = 0.25f;
    }

    [Serializable]
    public class ScoreConfig
    {
        public int pointsPerWave = 100;
        public int pointsPerKill = 2;
        [Tooltip("Mnoznik monet zarobionych w calym runie.")]
        public float earnedCoinsMul = 1f;
        [Tooltip("Mnoznik monet w kieszeni w chwili konca runu.")]
        public float leftoverCoinsMul = 1f;
        public int winBonus = 1000;
        [Tooltip("Wyniki powyzej tego progu to blad albo oszustwo i nie sa wysylane do tablicy.")]
        public int maxSaneScore = 200000;
    }

    [Serializable]
    public class PlayerConfig
    {
        public int baseHp = 6;
        public float moveSpeed = 5f;
        public float radius = 0.45f;
        [Tooltip("Czas niezniszczalnosci po otrzymaniu obrazen.")]
        public float hitInvulnerability = 1f;
        public float dashDuration = 0.15f;
        public float dashSpeed = 20f;
        [Tooltip("Bazowy zasieg magnesu na monety (MAGNET dodaje do niego).")]
        public float magnetBaseRange = 1.8f;
        [Tooltip("Odstep miedzy strzalami przy mnozniku szybkostrzelnosci x1.")]
        public float baseFireInterval = 0.35f;
    }

    [Serializable]
    public class WeaponConfig
    {
        public Weapon type;
        public float fireRateMul = 1f;
        public int shotCount = 1;
        public int damage = 1;
        public float bulletSpeed = 12f;
        public float bulletLife = 2.5f;
        [Tooltip("Kat miedzy pociskami (stopnie). Przy ConeDegrees > 0 liczony z stozka.")]
        public float spreadStep = 12f;
        [Tooltip("Strzelba: pelny stozek rozrzutu w stopniach (podzielony na pociski). 0 = uzywaj spreadStep.")]
        public float coneDegrees;
        [Tooltip("Losowe odchylenie kazdego pocisku w stopniach (+/-).")]
        public float spreadJitter;
        [Tooltip("Losowe odchylenie predkosci pocisku (+/- ulamek).")]
        public float speedJitter;
        public float baseCritChance;
        [Tooltip("Mnoznik promienia wybuchow EXPLOSION.")]
        public float explosionRadiusMul = 1f;
        [Tooltip("Jak szybko celowanie dogania mysz (wieksze = szybciej).")]
        public float aimTurnRate = 18f;

        [Header("Laser only")]
        public float beamTickInterval = 0.1f;
        public float beamHalfWidth = 0.12f;
        public float beamVisualWidth = 0.14f;
        public float beamMaxLength = 18f;
        public float beamChanceMin = 0.25f;
        public float beamChanceMax = 0.7f;
        [Tooltip("Czas ciaglego ognia do pelnego rozgrzania wiazki (s).")]
        public float heatUpTime = 1.5f;
        [Tooltip("Szybkosc stygniecia wiazki (1/s).")]
        public float heatCoolRate = 2.5f;

        public static List<WeaponConfig> Defaults() => new List<WeaponConfig>
        {
            new WeaponConfig { type = Weapon.Pistol, baseCritChance = 0.08f },
            new WeaponConfig { type = Weapon.Shotgun, fireRateMul = 0.6f, shotCount = 5, bulletSpeed = 11f, bulletLife = 0.55f,
                coneDegrees = 30f, spreadJitter = 2.5f, speedJitter = 0.1f, explosionRadiusMul = 1.25f },
            new WeaponConfig { type = Weapon.Laser, bulletSpeed = 24f, bulletLife = 1.4f, aimTurnRate = 9f },
        };
    }

    [Serializable]
    public class EnemyScalingConfig
    {
        [Tooltip("Predkosc wroga rosnie o ten ulamek za kazdy punkt Difficulty.")]
        public float speedPerDifficulty = 0.03f;
        [Tooltip("HP wrogow rosnie o ten ulamek co 4 fale runu.")]
        public float hpPerFourWaves = 0.1f;
        public float zone2Mul = 1.15f;
        public float zone3Mul = 1.2f;
        public float regularMaxSpeed = 0.8f * 5f * 0.92f;
        public float swarmMaxSpeed = 2.7f;
    }



    [Serializable]
    public class EnemyConfig
    {
        public EnemyType type;
        public float radius = 0.45f;
        public float hpBase = 1f;
        [Tooltip("HP rosnie o tyle za kazdy punkt Difficulty (sufit w dol).")]
        public float hpPerDifficulty;
        public float speed = 2f;
        public int contactDamage = 1;
        public float coins = 1;

        public static List<EnemyConfig> Defaults() => new List<EnemyConfig>
        {
            new EnemyConfig { type = EnemyType.Chaser, radius = 0.45f, hpBase = 1f, hpPerDifficulty = 1f / 3f, speed = 2.6f, contactDamage = 1, coins = 1 },
            new EnemyConfig { type = EnemyType.Shooter, radius = 0.45f, hpBase = 2f, hpPerDifficulty = 0.25f, speed = 2f, contactDamage = 1, coins = 1 },
            new EnemyConfig { type = EnemyType.Tank, radius = 0.7f, hpBase = 6f, hpPerDifficulty = 0.75f, speed = 1.2f, contactDamage = 2, coins = 3 },
            new EnemyConfig { type = EnemyType.Dasher, radius = 0.45f, hpBase = 2f, hpPerDifficulty = 0.2f, speed = 2.2f, contactDamage = 1, coins = 2 },
            new EnemyConfig { type = EnemyType.Siege, radius = 0.5f, hpBase = 4f, hpPerDifficulty = 0.25f, speed = 1.6f, contactDamage = 1, coins = 4 },
            new EnemyConfig { type = EnemyType.Swarm, radius = 0.3f, hpBase = 1f, hpPerDifficulty = 0f, speed = 2.8f, contactDamage = 1, coins = 0 },
            new EnemyConfig { type = EnemyType.Guard, radius = 0.5f, hpBase = 4f, hpPerDifficulty = 0.25f, speed = 1.7f, contactDamage = 1, coins = 3 },
            new EnemyConfig { type = EnemyType.Spawner, radius = 0.7f, hpBase = 8f, hpPerDifficulty = 0.5f, speed = 0.7f, contactDamage = 1, coins = 2 },
        };
    }

    [System.Serializable]
    public class CollectorEnemyConfig : EnemyConfig
    {
        [Header("Poborca only")]
        [Tooltip("Dodatkowe HP za numer strefy.")]
        public float hpPerZone;
        [Tooltip("Przyrost HP za kazde wczesniejsze spotkanie w runie (ulamek).")]
        public float hpPerEncounter;
        [Tooltip("Dodatkowe HP za kazda monete dlugu.")]
        public float hpPerDebt;

        public static CollectorEnemyConfig Default = new CollectorEnemyConfig
        {
            type = EnemyType.Collector,
            radius = 0.6f,
            hpBase = 30f,
            hpPerDifficulty = 1f,
            speed = 3.5f,
            contactDamage = 0,
            coins = 1,
            hpPerZone = 6f,
            hpPerEncounter = 0.12f,
            hpPerDebt = 1f
        };
    }

    [Serializable]
    public class EconomyConfig
    {
        [Header("Cena zysku (wzor w Catalog.GainCost / Game.CostOf)")]
        [Tooltip("Globalna skala cen zyskow w sklepie.")]
        public float gainCostMul = 1.15f;
        [Tooltip("Narzut ceny za kazdy wczesniejszy zakup w tej samej wizycie.")]
        public float purchaseSurcharge = 0.25f;
        [Tooltip("Dodatek do ceny za kazdy poziom zysku powyzej pierwszego.")]
        public float levelCostStep = 3f;
        [Tooltip("Dodatek do ceny za kazdy posiadany poziom zyskow.")]
        public float ownedGainCost = 1f;
        [Tooltip("Cena rosnie o Difficulty / ta wartosc.")]
        public float difficultyCostDivisor = 3f;
        [Tooltip("Do tej Difficulty obowiazuje Tier1Mul.")]
        public float tier1MaxDifficulty = 6f;
        public float tier2MaxDifficulty = 12f;
        public float tier1Mul = 1.2f;
        public float tier2Mul = 1.4f;
        public float tier3Mul = 1.5f;

        [Header("Oferty")]
        public int offersPerShop = 3;
        [Tooltip("Zysk o bazowym koszcie >= tej wartosci bierze 2 stacki ceny (od DoubleStackMinDifficulty).")]
        public int doubleStackMinCost = 9;
        public float doubleStackMinDifficulty = 7f;
        [Tooltip("Szansa na oferte Sprzedaj w sklepie.")]
        public float sellOfferChance = 0.6f;
        public float sellOfferMinDifficulty = 2f;

        [Header("Cena zysku vs dotkliwosc ceny")]
        [Tooltip("Znizka kosztu zysku za kazdy poziom dotkliwosci wzietej ceny.")]
        public float discountPerSeverity = 0.1f;

        [Header("Nagroda za sprzedaz ceny")]
        public float sellRewardBase = 5f;
        public float sellRewardDifficultyDivisor = 2f;
        public float sellRewardPerSeverity = 0.1f;
        [Tooltip("Losowy rozrzut nagrody (+/-).")]
        public int sellRewardSpread = 1;

        [Header("Reroll i targowanie")]
        public int rerollBase = 3;
        public int rerollStep = 2;
        public int haggleCost = 1;
        [Tooltip("Mnoznik Difficulty przy przeliczeniu nagrody po targowaniu.")]
        public float haggleDifficultyMul = 1.2f;
    }
}
