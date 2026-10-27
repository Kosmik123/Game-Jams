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
        public string name, description;
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
            new Gain { name = "Szybsze strzały", description = "+35% szybkostrzelności", cost = 5,
                apply = (s, p) => s.fireRateMul *= 1.35f },
            new Gain { name = "Więcej życia", description = "+2 max HP i leczenie o 2", cost = 5,
                apply = (s, p) => { s.maxHp += 2; p.Heal(2); } },
            new Gain { name = "Potrójny strzał", description = "3 pociski naraz", cost = 8,
                apply = (s, p) => s.shotCount = 3 },
            new Gain { name = "Leczenie", description = "Pełne HP", cost = 4,
                apply = (s, p) => p.Heal(99) },
            new Gain { name = "Przebijanie", description = "Pociski przebijają +1 wroga", cost = 6,
                apply = (s, p) => s.pierce += 1 },
        };

        public static readonly List<Price> Prices = new List<Price>
        {
            new Price { name = "Mgła", description = "Widzisz tylko okolicę gracza",
                apply = s => s.fogStacks++ },
            new Price { name = "Pikselizacja", description = "Gorsza rozdzielczość obrazu",
                apply = s => s.pixelStacks++ },
            new Price { name = "Ciężkie nogi", description = "-20% prędkości ruchu",
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
