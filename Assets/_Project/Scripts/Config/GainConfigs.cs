using System;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>Wspolne pola kazdego zysku. Efekty (liczby) sa w klasach pochodnych, po jednej na zysk.</summary>
    [Serializable]
    public class GainConfig
    {
        [Tooltip("Cena bazowa w monetach.")]
        public int cost = 5;
        public int maxLevel = 3;
        [Tooltip("Waga w losowaniu sklepu: 1 = zwykle, 2 = dwa razy czesciej (zyski warunkowe).")]
        public int poolWeight = 1;
        [Tooltip("Bronie, przy ktorych zysk nie wchodzi do sklepu ani do losowania.")]
        public Weapon[] notWith = new Weapon[0];

        protected void Set(int cost, int maxLevel = 3, int poolWeight = 1)
        {
            this.cost = cost;
            this.maxLevel = maxLevel;
            this.poolWeight = poolWeight;
        }

        public bool OkFor(Weapon w) => Array.IndexOf(notWith, w) < 0;
    }

    [Serializable]
    public class FasterShotsGain : GainConfig
    {
        [Tooltip("Mnoznik szybkostrzelnosci za poziom (dziala tez na tick wiazki lasera).")]
        public float fireRateMul = 1.5f;
        public FasterShotsGain() { Set(5); }
    }

    [Serializable]
    public class MoreHealthGain : GainConfig
    {
        public int maxHp = 3;
        public int heal = 3;
        public MoreHealthGain() { Set(8); }
    }

    [Serializable]
    public class TripleShotGain : GainConfig
    {
        [Tooltip("Dodatkowe pociski za poziom.")]
        public int extraShots = 2;
        [Tooltip("Laser: przyrost szerokosci wiazki za poziom (x1 -> x1.8 -> x2.6).")]
        public float beamWidthPerLevel = 0.8f;
        public TripleShotGain() { Set(8, 2); }
    }

    [Serializable]
    public class HealingGain : GainConfig
    {
        [Tooltip("Ile HP leczy (99 = pelne).")]
        public int heal = 99;
        public HealingGain() { Set(3); }
    }

    [Serializable]
    public class PiercingGain : GainConfig
    {
        public int pierce = 1;
        public PiercingGain() { Set(6); notWith = new[] { Weapon.Laser }; }
    }

    [Serializable]
    public class DashGain : GainConfig
    {
        [Tooltip("Cooldown dasha na poziomie 1, 2, 3 (dalej ostatnia wartosc).")]
        public float[] cooldownByLevel = { 1.2f, 0.95f, 0.7f };
        public DashGain() { Set(7); }
    }

    [Serializable]
    public class MagnetGain : GainConfig
    {
        [Tooltip("Dodatkowy zasieg za poziom.")]
        public float rangePerLevel = 2.5f;
        public MagnetGain() { Set(4); }
    }

    [Serializable]
    public class RicochetGain : GainConfig
    {
        public int bounces = 1;
        public RicochetGain() { Set(6); notWith = new[] { Weapon.Laser }; }
    }

    [Serializable]
    public class ExplosionGain : GainConfig
    {
        public float baseRadius = 1.1f;
        public float radiusPerLevel = 0.4f;
        [Tooltip("Od tego poziomu wybuch zadaje BigDamage.")]
        public int bigDamageLevel = 3;
        public int damage = 1;
        public int bigDamage = 2;
        public ExplosionGain() { Set(8); }
    }

    [Serializable]
    public class VampirismGain : GainConfig
    {
        [Tooltip("Ile zabojstw potrzeba do leczenia na poziomie 1, 2, 3.")]
        public int[] killsByLevel = { 12, 8, 5 };
        public VampirismGain() { Set(7); }
    }

    [Serializable]
    public class SlowAuraGain : GainConfig
    {
        public float baseRadius = 1.8f;
        public float radiusPerLevel = 0.7f;
        [Tooltip("Mnoznik predkosci wrogow w aurze na poziomie 1, 2, 3.")]
        public float[] factorByLevel = { 0.75f, 0.62f, 0.5f };
        public SlowAuraGain() { Set(5); }
    }

    [Serializable]
    public class DroneGain : GainConfig
    {
        public float baseInterval = 1.1f;
        public int baseDamage = 1;
        [Tooltip("Synergia z PIXELATION: odstep strzalow drona.")]
        public float pixelationInterval = 0.75f;
        [Tooltip("Synergia z PIXELATION: obrazenia drona.")]
        public int pixelationDamage = 2;
        public DroneGain() { Set(8, 1); }
    }

    [Serializable]
    public class DronePowerGain : GainConfig
    {
        public int extraDamage = 1;
        public DronePowerGain() { Set(7, 2, 2); }
    }

    [Serializable]
    public class DroneRateGain : GainConfig
    {
        [Tooltip("Mnoznik odstepu strzalow drona za poziom.")]
        public float intervalMulPerLevel = 0.75f;
        public DroneRateGain() { Set(7, 2, 2); }
    }

    [Serializable]
    public class MoreDronesGain : GainConfig
    {
        public int extraDrones = 1;
        public MoreDronesGain() { Set(9, 2, 2); }
    }

    [Serializable]
    public class DroneOrbitGain : GainConfig
    {
        public DroneOrbitGain() { Set(9, 1, 2); }
    }

    [Serializable]
    public class CriticalGain : GainConfig
    {
        public float chancePerLevel = 0.12f;
        [Tooltip("Dodatkowa szansa od poziomu 1 (jednorazowo).")]
        public float firstLevelBonus = 0.06f;
        public float multiplier = 3f;
        [Tooltip("Synergia z FOG: mnoznik krytyka.")]
        public float fogMultiplier = 5f;
        public CriticalGain() { Set(6); }
    }

    [Serializable]
    public class ShockwaveGain : GainConfig
    {
        public float baseRadius = 2.2f;
        public float radiusPerLevel = 0.6f;
        public int damage = 1;
        [Tooltip("Obrazenia od poziomu 2.")]
        public int damageLevel2 = 2;
        [Tooltip("O ile jednostek odpycha wrogow.")]
        public float pushDistance = 1.5f;
        public ShockwaveGain() { Set(9, 2, 2); }
    }

    [Serializable]
    public class DoubleDashGain : GainConfig
    {
        [Tooltip("Okno na drugi dash (s).")]
        public float window = 0.8f;
        public DoubleDashGain() { Set(9, 1, 2); }
    }

    [Serializable]
    public class DashCritGain : GainConfig
    {
        [Tooltip("Czas krytykow po dashu (s).")]
        public float duration = 2f;
        public DashCritGain() { Set(8, 1, 2); }
    }

    [Serializable]
    public class DashMinesGain : GainConfig
    {
        public int minesPerLevel = 1;
        [Tooltip("Odstep miedzy minami w trakcie dasha (s).")]
        public float mineInterval = 0.045f;
        public DashMinesGain() { Set(7, 3, 2); }
    }

    [Serializable]
    public class CritBlastGain : GainConfig
    {
        public CritBlastGain() { Set(9, 1, 2); }
    }

    [Serializable]
    public class CritCoinsGain : GainConfig
    {
        public int extraCoinsPerLevel = 1;
        public CritCoinsGain() { Set(6, 2, 2); }
    }

    [Serializable]
    public class TripleCritGain : GainConfig
    {
        [Tooltip("Dodatek do mnoznika krytyka.")]
        public float extraMultiplier = 2f;
        public TripleCritGain() { Set(8, 1, 2); }
    }

    [Serializable]
    public class DebtWeaponGain : GainConfig
    {
        [Tooltip("Dodatkowe obrazenia przy jakimkolwiek dlugu.")]
        public int bonusDamage = 1;
        [Tooltip("Od tego dlugu poziom 2 daje jeszcze +1.")]
        public int level2DebtThreshold = 30;
        public DebtWeaponGain() { Set(8, 2); }
    }

    [Serializable]
    public class TimeRewindGain : GainConfig
    {
        public int hpAfterRewind = 2;
        public float invulnerableSeconds = 2f;
        public TimeRewindGain() { Set(10, 1); }
    }

    [Serializable]
    public class PriceLockGain : GainConfig
    {
        public PriceLockGain() { Set(4, 1); }
    }

    [Serializable]
    public class TimeFreezeGain : GainConfig
    {
        public float baseDuration = 1.5f;
        public float durationPerLevel = 1f;
        public TimeFreezeGain() { Set(7); }
    }

    [Serializable]
    public class ShieldGain : GainConfig
    {
        [Tooltip("Niezniszczalnosc po zablokowanym trafieniu (s).")]
        public float invulnerableSeconds = 0.6f;
        [Tooltip("Synergia z HEAVY LEGS: co ile sekund tarcza sie odnawia.")]
        public float heavyLegsRegenSeconds = 10f;
        public ShieldGain() { Set(7); }
    }

    /// <summary>Wszystkie zyski jako osobne struktury (kazda widoczna jako sekcja w inspektorze).</summary>
    [Serializable]
    public class GainsConfig
    {
        public FasterShotsGain fasterShots = new FasterShotsGain();
        public MoreHealthGain moreHealth = new MoreHealthGain();
        public TripleShotGain tripleShot = new TripleShotGain();
        public HealingGain healing = new HealingGain();
        public PiercingGain piercing = new PiercingGain();
        public DashGain dash = new DashGain();
        public MagnetGain magnet = new MagnetGain();
        public RicochetGain ricochet = new RicochetGain();
        public ExplosionGain explosion = new ExplosionGain();
        public VampirismGain vampirism = new VampirismGain();
        public SlowAuraGain slowAura = new SlowAuraGain();
        public DroneGain drone = new DroneGain();
        public DronePowerGain dronePower = new DronePowerGain();
        public DroneRateGain droneRate = new DroneRateGain();
        public MoreDronesGain moreDrones = new MoreDronesGain();
        public DroneOrbitGain droneOrbit = new DroneOrbitGain();
        public CriticalGain critical = new CriticalGain();
        public ShockwaveGain shockwave = new ShockwaveGain();
        public DoubleDashGain doubleDash = new DoubleDashGain();
        public DashCritGain dashCrit = new DashCritGain();
        public DashMinesGain dashMines = new DashMinesGain();
        public CritBlastGain critBlast = new CritBlastGain();
        public CritCoinsGain critCoins = new CritCoinsGain();
        public TripleCritGain tripleCrit = new TripleCritGain();
        public DebtWeaponGain debtWeapon = new DebtWeaponGain();
        public TimeRewindGain timeRewind = new TimeRewindGain();
        public PriceLockGain priceLock = new PriceLockGain();
        public TimeFreezeGain timeFreeze = new TimeFreezeGain();
        public ShieldGain shield = new ShieldGain();
    }
}
