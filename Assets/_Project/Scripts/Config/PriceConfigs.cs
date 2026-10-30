using System;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>Wspolne pola kazdej ceny. Parametry efektu sa w klasach pochodnych, po jednej na cene.</summary>
    [Serializable]
    public class PriceConfig
    {
        [Tooltip("Dotkliwosc (0 i wiecej): znizka kosztu zysku i nagroda za sprzedaz rosna z nia.")]
        public int severity = 1;
        [Tooltip("Maksymalna liczba stackow (0 = bez limitu).")]
        public int maxStacks;

        protected void Set(int severity, int maxStacks = 0)
        {
            this.severity = severity;
            this.maxStacks = maxStacks;
        }

        public bool CanAdd(int stacks) => maxStacks <= 0 || stacks < maxStacks;

        /// <summary>Wartosc z krzywej po stackach: 1 stack = values[0], dalej ostatnia wartosc. 0 stackow = neutral.</summary>
        public static float ByStack(float[] values, int stacks, float neutral)
        {
            if (stacks <= 0 || values == null || values.Length == 0) return neutral;
            return values[Mathf.Min(stacks, values.Length) - 1];
        }
    }

    [Serializable]
    public class FogPrice : PriceConfig
    {
        [Tooltip("Promien widocznosci (jednostki swiata) przy 1, 2, 3 stackach.")]
        public float[] visionRadiusByStack = { 9f, 6.2f, 3.8f };
        [Tooltip("Dalsze zmniejszenie promienia za kazdy stack ponad dlugosc listy.")]
        public float radiusLossPerExtraStack = 0.6f;
        public float minRadius = 2.2f;
        public FogPrice() { Set(2); }
    }

    [Serializable]
    public class PixelationPrice : PriceConfig
    {
        public PixelationPrice() { Set(1); }
    }

    [Serializable]
    public class FlickerPrice : PriceConfig
    {
        [Tooltip("Czas odwrocenia palety (s) przy 1, 2, 3 stackach.")]
        public float[] durationByStack = { 0.8f, 1.7f, 2.8f };
        [Tooltip("Co ile sekund migotanie sie powtarza.")]
        public float period = 8f;
        public FlickerPrice() { Set(0); }
    }

    [Serializable]
    public class SluggishAimPrice : PriceConfig
    {
        [Tooltip("Spowolnienie obrotu celowania: tempo / (1 + ta wartosc * stacki).")]
        public float lagPerStack = 1.5f;
        public SluggishAimPrice() { Set(1); }
    }

    [Serializable]
    public class ZoomInPrice : PriceConfig
    {
        [Tooltip("Mnoznik rozmiaru kamery przy 1, 2, 3 stackach.")]
        public float[] zoomByStack = { 0.93f, 0.76f, 0.58f };
        public ZoomInPrice() { Set(1, 3); }
    }

    [Serializable]
    public class FragilityPrice : PriceConfig
    {
        public int hpLossPerStack = 1;
        [Tooltip("Cena nie wchodzi do puli, gdy max HP jest nizsze lub rowne tej wartosci.")]
        public int minMaxHp = 2;
        public FragilityPrice() { Set(3); }
    }

    [Serializable]
    public class CostlyLifePrice : PriceConfig
    {
        [Tooltip("O ile razy drozsze sa zyski leczace za kazdy stack (cena x (1 + stacki * ta wartosc)).")]
        public int extraMulPerStack = 1;
        public CostlyLifePrice() { Set(2, 2); }
    }

    [Serializable]
    public class HungerPrice : PriceConfig
    {
        [Tooltip("Po ilu sekundach moneta znika z podlogi przy 1, 2, 3 stackach.")]
        public float[] coinLifetimeByStack = { 10f, 6.5f, 3.5f };
        public HungerPrice() { Set(3, 3); }
    }

    [Serializable]
    public class ReversedControlsPrice : PriceConfig
    {
        [Tooltip("Czas normalnego sterowania miedzy odwroceniami (s), przy 0 stackow.")]
        public float period = 11f;
        public float periodReductionPerStack = 0.5f;
        [Tooltip("Czas odwrocenia sterowania (s).")]
        public float duration = 2f;
        public ReversedControlsPrice() { Set(4, 3); }
    }

    [Serializable]
    public class SilencePrice : PriceConfig
    {
        public float baseDuration = 1.5f;
        public float durationPerStack = 0.5f;
        public SilencePrice() { Set(1, 3); }
    }

    [Serializable]
    public class InertiaPrice : PriceConfig
    {
        [Tooltip("Czas rozpedzania (s) przy 1, 2, 3 stackach.")]
        public float[] startTimeByStack = { 0.10f, 0.22f, 0.40f };
        [Tooltip("Hamowanie trwa tyle razy dluzej niz rozpedzanie.")]
        public float stopTimeMul = 1.3f;
        public InertiaPrice() { Set(2, 3); }
    }

    [Serializable]
    public class InputDelayPrice : PriceConfig
    {
        [Tooltip("Czas rozpedzania (s) przy 1, 2, 3 stackach.")]
        public float[] delayByStack = { 0.15f, 0.3f, 0.5f };
        public InputDelayPrice() { Set(1, 3); }
    }

    [Serializable]
    public class PriceyShopPrice : PriceConfig
    {
        [Tooltip("O ile drozeja zyski w sklepie za kazdy stack (ulamek).")]
        public float costPerStack = 0.2f;
        public PriceyShopPrice() { Set(3, 3); }
    }

    [Serializable]
    public class HeavyLegsPrice : PriceConfig
    {
        [Tooltip("Mnoznik predkosci ruchu przy 1, 2, 3 stackach.")]
        public float[] speedMulByStack = { 0.92f, 0.76f, 0.58f };
        public HeavyLegsPrice() { Set(2); }
    }

    /// <summary>Wszystkie ceny jako osobne struktury (kazda widoczna jako sekcja w inspektorze).</summary>
    [Serializable]
    public class PricesConfig
    {
        public FogPrice fog = new FogPrice();
        public PixelationPrice pixelation = new PixelationPrice();
        public FlickerPrice flicker = new FlickerPrice();
        public SluggishAimPrice sluggishAim = new SluggishAimPrice();
        public ZoomInPrice zoomIn = new ZoomInPrice();
        public FragilityPrice fragility = new FragilityPrice();
        public CostlyLifePrice costlyLife = new CostlyLifePrice();
        public HungerPrice hunger = new HungerPrice();
        public ReversedControlsPrice reversedControls = new ReversedControlsPrice();
        public SilencePrice silence = new SilencePrice();
        public InertiaPrice inertia = new InertiaPrice();
        public InputDelayPrice inputDelay = new InputDelayPrice();
        public PriceyShopPrice priceyShop = new PriceyShopPrice();
        public HeavyLegsPrice heavyLegs = new HeavyLegsPrice();
    }
}
