using System.Collections.Generic;
using UnityEngine;

namespace ZeroHour.Tower
{
    // Post-jam economy: proportional power instead of an all-or-nothing blackout,
    // and positive happiness boosts income.
    public class PostJamCityStats : CityStats
    {
        // All numbers come from the config: the floor table, the income and happiness rules, the neighbor pairs
        // ("view of the park", "smog under the window", "short commute") and the combo (level 2 -> x1.1, 3 -> x1.2, ...).
        private readonly PostJamConfig config;

        public PostJamCityStats() : this(PostJamConfig.CreateDefault())
        {
        }

        private int bankCount;
        private int payingFloorCount;

        public PostJamCityStats(PostJamConfig config)
        {
            this.config = config;
            AddBonusMoney(config.startBalance);
        }

        // Banks raise the limit; the first floors and the gold floor cost no upkeep.
        public override float MoneyCap => config.baseMoneyCap + config.bankMoneyCap * bankCount;

        public override float Upkeep => config.upkeepPerFloor * Mathf.Max(0, payingFloorCount - config.freeFloors);

        protected override void OnRecalculated(IReadOnlyList<FloorType> floors)
        {
            bankCount = 0;
            payingFloorCount = 0;
            foreach (FloorType type in floors)
            {
                if (type == FloorType.Bank)
                    bankCount++;
                if (!type.IsGold)
                    payingFloorCount++;
            }
        }

        protected override float ResidentsLostPerUnhappiness => config.residentsLostPerUnhappiness;

        protected override float IncomePerWorker => config.incomePerWorker;

        public override string PowerWarning => IsPowered ? null : $"Power {PowerRatio * 100f:0}%";

        // Combo N from the Nth perfect landing in a row (from the second one on), without an upper limit.
        public override int Combo => PerfectStreak >= config.minComboStreak ? PerfectStreak : 1;

        public override float ComboIncomeMultiplier => 1f + config.comboIncomePerLevel * (Combo - 1);

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
                case "Park": return new NeighborEffect(config.parkNeighborHappiness, 0f);
                case "Power": return new NeighborEffect(config.powerNeighborHappiness, 0f);
                case "Office": return new NeighborEffect(0, config.commuteIncome);
                default: return default;
            }
        }

        protected override FloorStats GetFloorStats(FloorType type) =>
            config.TryGetFloorStats(type.Name, out FloorStats stats) ? stats : base.GetFloorStats(type);

        protected override float CalculatePowerRatio(int supply, int demand) =>
            demand == 0 ? 1f : Mathf.Min(1f, (float)supply / demand);

        protected override float CalculateIncome(float population, float jobs, int happiness, float powerRatio)
        {
            float happinessMultiplier = happiness > 0 ? Mathf.Min(1f + config.happinessIncomeBonus * happiness, config.maxHappinessMultiplier) : 1f;
            return base.CalculateIncome(population, jobs, happiness, powerRatio) * happinessMultiplier * ComboIncomeMultiplier;
        }

        protected override bool NeedsPower(FloorStats floor) => floor.Energy < 0;

        // Floors go dark from the top: the lowest consumers get the available power.
        // The epsilon keeps e.g. 4/6 * 6 from rounding down to 3.
        protected override int CountPoweredConsumers(int consumers) =>
            Mathf.FloorToInt(PowerRatio * consumers + 1e-4f);
    }
}
