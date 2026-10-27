using System;
using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>Wszystkie modyfikatory biezacego runu (zyski i ceny zmieniaja tylko to).</summary>
    public class RunStats
    {
        public int maxHp = 5;
        public float fireRateMul = 1f;
        public int shotCount = 1;     // 1 albo 3
        public int pierce = 0;
        public float moveSpeedMul = 1f;
        public int fogStacks;
        public int pixelStacks;
        public int flickerStacks;

        /// <summary>Cena "Migotanie": co 8 s paleta odwraca sie na (1 + 0.5*stack) s.</summary>
        public float FlickerDuration => flickerStacks <= 0 ? 0f : 1f + 0.5f * flickerStacks;

        public float FireInterval => 0.35f / fireRateMul;
        public float MoveSpeed => 5f * moveSpeedMul;

        /// <summary>Promien widocznosci w jednostkach swiata; 0 = brak mgly.</summary>
        public float FogRadius => fogStacks <= 0 ? 0f : Mathf.Max(2.2f, 8.5f - 1.8f * (fogStacks - 1));
    }

    public class Gain
    {
        public string name, description;
        public int cost;
        public Action<RunStats, Player> apply;
    }

    public class Price
    {
        public string name, description, icon;
        public Action<RunStats> apply;
    }

    public class Offer
    {
        public Gain gain;
        public Price price;
        public int cost;
    }

    /// <summary>Katalog ofert. Dodanie zysku lub ceny = jedna pozycja na liscie.</summary>
    public static class Catalog
    {
        public static readonly List<Gain> Gains = new List<Gain>
        {
            new Gain { name = "FASTER SHOTS", description = "+35% FIRE RATE", cost = 5,
                apply = (s, p) => s.fireRateMul *= 1.35f },
            new Gain { name = "MORE HEALTH", description = "+2 MAX HP AND HEAL 2", cost = 5,
                apply = (s, p) => { s.maxHp += 2; p.Heal(2); } },
            new Gain { name = "TRIPLE SHOT", description = "FIRE 3 BULLETS AT ONCE", cost = 8,
                apply = (s, p) => s.shotCount = 3 },
            new Gain { name = "HEALING", description = "FULL HP", cost = 4,
                apply = (s, p) => p.Heal(99) },
            new Gain { name = "PIERCING", description = "BULLETS PIERCE +1 ENEMY", cost = 6,
                apply = (s, p) => s.pierce += 1 },
        };

        public static readonly List<Price> Prices = new List<Price>
        {
            new Price { name = "FOG", icon = "icon_fog_eye", description = "YOU ONLY SEE NEAR YOURSELF",
                apply = s => s.fogStacks++ },
            new Price { name = "PIXELATION", icon = "icon_pixelation_grid", description = "WORSE PICTURE RESOLUTION",
                apply = s => s.pixelStacks++ },
            new Price { name = "FLICKER", icon = "icon_flicker_flash", description = "PALETTE INVERTS EVERY FEW SECONDS",
                apply = s => s.flickerStacks++ },
            new Price { name = "HEAVY LEGS", icon = "icon_legs_boot", description = "-20% MOVE SPEED",
                apply = s => s.moveSpeedMul *= 0.8f },
        };

        public static List<Offer> Roll(int count, int wave)
        {
            var gains = new List<Gain>(Gains);
            var offers = new List<Offer>();
            for (int i = 0; i < count && gains.Count > 0; i++)
            {
                var g = gains[UnityEngine.Random.Range(0, gains.Count)];
                gains.Remove(g);
                var p = Prices[UnityEngine.Random.Range(0, Prices.Count)];
                offers.Add(new Offer { gain = g, price = p, cost = g.cost + wave / 3 });
            }
            return offers;
        }
    }
}
