using System.Collections.Generic;
using UnityEngine;

namespace ZeroHour.Tower
{
    public readonly struct FloorStats
    {
        public readonly float Population;
        public readonly float Jobs;
        public readonly float Energy;
        public readonly float Happiness;

        public FloorStats(float population, float jobs, float energy, float happiness)
        {
            Population = population;
            Jobs = jobs;
            Energy = energy;
            Happiness = happiness;
        }
    }

    // Effect of two floors directly on top of each other.
    public readonly struct NeighborEffect
    {
        public readonly int Happiness;
        public readonly float Income;

        public NeighborEffect(int happiness, float income)
        {
            Happiness = happiness;
            Income = income;
        }

        public bool IsNone => Happiness == 0 && Income == 0f;
    }

    // City economy computed from the floors in the tower. The base class is the jam economy
    // (all-or-nothing blackout); rulesets create a subclass to change the rules.
    public class CityStats
    {
        protected virtual float ResidentsLostPerUnhappiness => 5f;
        protected virtual float IncomePerWorker => 0.2f;

        private readonly List<FloorType> floorTypes = new List<FloorType>();
        private readonly List<float> floorQualities = new List<float>();
        private IReadOnlyList<float> qualities;
        private readonly List<bool> floorPowered = new List<bool>();

        public int Height { get; private set; }
        public float Population { get; private set; }
        public float Jobs { get; private set; }
        public float Energy { get; private set; }
        public float Happiness { get; private set; }
        public float Income { get; private set; }
        public float Money { get; private set; }

        // Everything earned from income so far, before upkeep and without one-off bonuses.
        public float TotalEarned { get; private set; }

        // The balance never goes above this; the jam economy has no limit.
        public virtual float MoneyCap => float.PositiveInfinity;

        // $ per second paid for keeping the tower up; the jam economy pays nothing.
        public virtual float Upkeep => 0f;

        public float NetIncome => Income - Upkeep;

        // Seconds until the balance reaches zero at the current net income; infinite when it isn't shrinking.
        public float Runway => NetIncome >= 0f ? float.PositiveInfinity : Mathf.Max(0f, Money) / -NetIncome;

        // Base income per second from neighbor pairs, before power and multipliers.
        public float NeighborIncome { get; private set; }

        // Share of the energy demand that is supplied, 0..1.
        public float PowerRatio { get; private set; }

        public bool IsPowered => PowerRatio >= 1f;

        // Red warning line for the HUD, or null when the power is fine.
        public virtual string PowerWarning => IsPowered ? null : "BLACKOUT!";

        public void Recalculate(IReadOnlyList<FallingFloor> floors)
        {
            floorTypes.Clear();
            floorQualities.Clear();
            foreach (FallingFloor floor in floors)
            {
                floorTypes.Add(floor.Type);
                floorQualities.Add(floor.Quality);
            }
            Recalculate(floorTypes, floorQualities);
        }

        // Floors from the bottom of the tower up, every one placed perfectly.
        public void Recalculate(IReadOnlyList<FloorType> floors) => Recalculate(floors, null);

        // qualities: how well each floor was placed (1 = perfectly); null counts every floor as perfect.
        public void Recalculate(IReadOnlyList<FloorType> floors, IReadOnlyList<float> qualities)
        {
            this.qualities = qualities;
            float population = 0f;
            float jobs = 0f;
            float supply = 0f;
            float demand = 0f;
            float happiness = 0f;
            for (int i = 0; i < floors.Count; i++)
            {
                FloorStats floor = StatsAt(floors, i);
                population += floor.Population;
                jobs += floor.Jobs;
                if (floor.Energy > 0)
                    supply += floor.Energy;
                else
                    demand -= floor.Energy;
                happiness += floor.Happiness;
            }

            // Each pair of floors directly on top of each other counts once.
            float neighborIncome = 0f;
            for (int i = 0; i + 1 < floors.Count; i++)
            {
                NeighborEffect effect = GetNeighborEffect(floors[i], floors[i + 1]);
                happiness += effect.Happiness;
                neighborIncome += effect.Income;
            }

            // Unhappy residents move out.
            if (happiness < 0)
                population = Mathf.Max(0f, population + happiness * ResidentsLostPerUnhappiness);

            Height = floors.Count;
            Population = population;
            Jobs = jobs;
            Energy = supply - demand;
            Happiness = happiness;
            NeighborIncome = neighborIncome;
            PowerRatio = CalculatePowerRatio(supply, demand);
            Income = CalculateIncome(population, jobs, happiness, PowerRatio);

            OnRecalculated(floors);
            Money = Mathf.Min(Money, MoneyCap);

            floorPowered.Clear();
            int consumers = 0;
            for (int i = 0; i < floors.Count; i++)
            {
                if (NeedsPower(StatsAt(floors, i)))
                    consumers++;
            }
            int poweredConsumers = CountPoweredConsumers(consumers);
            for (int i = 0; i < floors.Count; i++)
            {
                bool needsPower = NeedsPower(StatsAt(floors, i));
                floorPowered.Add(!needsPower || poweredConsumers-- > 0);
            }
        }

        private FloorStats StatsAt(IReadOnlyList<FloorType> floors, int index) =>
            ScaleStats(GetFloorStats(floors[index]), QualityAt(index));

        // How well the floor at this index (from the bottom) was placed, 0..1.
        protected float QualityAt(int index) =>
            qualities != null && index < qualities.Count ? qualities[index] : 1f;

        // The effects of a badly placed floor; the jam economy ignores placement.
        protected virtual FloorStats ScaleStats(FloorStats stats, float quality) => stats;

        // Whether the floor at this index (from the bottom) has power. FallingFloor keeps producers lit anyway.
        public bool IsFloorPowered(int index) => floorPowered[index];

        public void Tick(float deltaTime)
        {
            TotalEarned += Income * deltaTime;
            Money = Mathf.Min(Money + NetIncome * deltaTime, MoneyCap);
        }

        // One-off cash that doesn't come from income (the golden floor, the starting balance), up to the limit.
        public void AddBonusMoney(float amount) => Money = Mathf.Min(Money + amount, MoneyCap);

        // Called at the end of Recalculate, before the balance is cut to the limit.
        protected virtual void OnRecalculated(IReadOnlyList<FloorType> floors)
        {
        }

        // Perfect landings in a row; the jam economy doesn't count them.
        public int PerfectStreak { get; protected set; }

        // Combo level from the perfect streak (1 = no combo); it has no upper limit.
        public virtual int Combo => 1;

        // Income multiplier from the combo (1 = no bonus).
        public virtual float ComboIncomeMultiplier => 1f;

        public virtual void RegisterLanding(bool isPerfect)
        {
        }

        public virtual void RegisterCollapse()
        {
        }

        protected virtual FloorStats GetFloorStats(FloorType type) =>
            new FloorStats(type.Population, type.Jobs, type.Energy, type.Happiness);

        protected virtual float CalculatePowerRatio(float supply, float demand) => supply >= demand ? 1f : 0f;

        protected virtual float CalculateIncome(float population, float jobs, float happiness, float powerRatio) =>
            (Mathf.Min(population, jobs) * IncomePerWorker + NeighborIncome) * powerRatio;

        // The jam economy ignores the order of the floors.
        public virtual NeighborEffect GetNeighborEffect(FloorType below, FloorType above) => default;

        // Jam: in a blackout every floor goes dark (FallingFloor keeps power plants lit).
        protected virtual bool NeedsPower(FloorStats floor) => true;

        // How many of the consuming floors, counted from the bottom, get power.
        protected virtual int CountPoweredConsumers(int consumers) => IsPowered ? consumers : 0;
    }
}
