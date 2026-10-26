using System.Collections.Generic;
using UnityEngine;

namespace ZeroHour.Tower
{
    public class FloorType
    {
        public string Name { get; }
        public Color Color { get; }
        public int Population { get; }
        public int Jobs { get; }
        public int Energy { get; }
        public int Happiness { get; }

        private FloorType(string name, Color color, int population, int jobs, int energy, int happiness)
        {
            Name = name;
            Color = color;
            Population = population;
            Jobs = jobs;
            Energy = energy;
            Happiness = happiness;
        }

        public static IReadOnlyList<FloorType> All { get; } = new[]
        {
            new FloorType("Residential", new Color(0.93f, 0.55f, 0.35f), 10, 0, -1, -1),
            new FloorType("Office", new Color(0.35f, 0.62f, 0.9f), 0, 10, -1, 0),
            new FloorType("Power", new Color(0.98f, 0.82f, 0.25f), 0, 0, 4, -2),
            new FloorType("Park", new Color(0.4f, 0.8f, 0.45f), 0, 0, 0, 3),
        };

        // The rare golden floor: no stats, never part of the random draw (see GoldSpawner).
        public static FloorType Gold { get; } = new FloorType("Gold", new Color(1f, 0.84f, 0.1f), 0, 0, 0, 0);

        public bool IsGold => this == Gold;

        public static FloorType GetRandom() => All[Random.Range(0, All.Count)];
    }
}
