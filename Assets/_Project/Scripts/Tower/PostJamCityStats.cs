using System.Collections.Generic;
using UnityEngine;

namespace ZeroHour.Tower
{
    // Post-jam economy: proportional power instead of an all-or-nothing blackout,
    // and positive happiness boosts income.
    public class PostJamCityStats : CityStats
    {
        private const float HappinessIncomeBonus = 0.05f;
        private const float MaxHappinessMultiplier = 1.5f;
        private const int ParkNeighborJoy = 1;      // "view of the park"
        private const int PowerNeighborJoy = -1;    // "smog under the window"
        private const float CommuteIncome = 0.3f;   // "short commute"
        private const int MinComboStreak = 2;
        private const int MaxComboMultiplier = 5;

        // Floor numbers tuned so 2 residential + 2 offices + 1 power plant + 1 park balance out.
        private static readonly Dictionary<string, FloorStats> Floors = new Dictionary<string, FloorStats>
        {
            ["Residential"] = new FloorStats(population: 10, jobs: 0, energy: -1, happiness: -1),
            ["Office"] = new FloorStats(population: 0, jobs: 10, energy: -1, happiness: 0),
            ["Power"] = new FloorStats(population: 0, jobs: 0, energy: 4, happiness: -2),
            ["Park"] = new FloorStats(population: 0, jobs: 0, energy: 0, happiness: 4),
        };

        public override string PowerWarning => IsPowered ? null : $"Power {PowerRatio * 100f:0}%";

        // x2..x5 from the second perfect landing in a row.
        public override int ComboMultiplier => PerfectStreak >= MinComboStreak ? Mathf.Min(PerfectStreak, MaxComboMultiplier) : 1;

        public override void RegisterLanding(bool isPerfect) => PerfectStreak = isPerfect ? PerfectStreak + 1 : 0;

        public override void RegisterCollapse() => PerfectStreak = 0;

        // Residential floors care about their neighbors; the order of the pair doesn't matter.
        public override NeighborEffect GetNeighborEffect(FloorType below, FloorType above)
        {
            FloorType other;
            if (below.Name == "Residential")
                other = above;
            else if (above.Name == "Residential")
                other = below;
            else
                return default;

            switch (other.Name)
            {
                case "Park": return new NeighborEffect(ParkNeighborJoy, 0f);
                case "Power": return new NeighborEffect(PowerNeighborJoy, 0f);
                case "Office": return new NeighborEffect(0, CommuteIncome);
                default: return default;
            }
        }

        protected override FloorStats GetFloorStats(FloorType type) =>
            Floors.TryGetValue(type.Name, out FloorStats stats) ? stats : base.GetFloorStats(type);

        protected override float CalculatePowerRatio(int supply, int demand) =>
            demand == 0 ? 1f : Mathf.Min(1f, (float)supply / demand);

        protected override float CalculateIncome(int population, int jobs, int happiness, float powerRatio)
        {
            float happinessMultiplier = happiness > 0 ? Mathf.Min(1f + HappinessIncomeBonus * happiness, MaxHappinessMultiplier) : 1f;
            return base.CalculateIncome(population, jobs, happiness, powerRatio) * happinessMultiplier * ComboMultiplier;
        }

        protected override bool NeedsPower(FloorStats floor) => floor.Energy < 0;

        // Floors go dark from the top: the lowest consumers get the available power.
        // The epsilon keeps e.g. 4/6 * 6 from rounding down to 3.
        protected override int CountPoweredConsumers(int consumers) =>
            Mathf.FloorToInt(PowerRatio * consumers + 1e-4f);
    }
}
