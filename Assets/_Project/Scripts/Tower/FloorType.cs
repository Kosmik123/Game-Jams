using System.Collections.Generic;
using UnityEngine;

namespace ZeroHour.Tower
{
    public class FloorType
    {
        public string Name { get; }
        public Color Color { get; }

        private FloorType(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        public static IReadOnlyList<FloorType> All { get; } = new[]
        {
            new FloorType("Residential", new Color(0.93f, 0.55f, 0.35f)),
            new FloorType("Office", new Color(0.35f, 0.62f, 0.9f)),
            new FloorType("Power", new Color(0.98f, 0.82f, 0.25f)),
            new FloorType("Park", new Color(0.4f, 0.8f, 0.45f)),
        };

        public static FloorType GetRandom() => All[Random.Range(0, All.Count)];
    }
}
