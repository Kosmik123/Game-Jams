using System.Collections.Generic;
using UnityEngine;

namespace ZeroHour.Tower
{
    public readonly struct FloorStats
    {
        public readonly int Population;
        public readonly int Jobs;
        public readonly int Energy;
        public readonly int Happiness;

        public FloorStats(int population, int jobs, int energy, int happiness)
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
        protected const int ResidentsLostPerUnhappiness = 5;
        protected const float IncomePerWorker = 0.2f;

        private readonly List<FloorType> floorTypes = new List<FloorType>();
        private readonly List<bool> floorPowered = new List<bool>();

        public int Height { get; private set; }
        public int Population { get; private set; }
        public int Jobs { get; private set; }
        public int Energy { get; private set; }
        public int Happiness { get; private set; }
        public float Income { get; private set; }
        public float Money { get; private set; }

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
            foreach (FallingFloor floor in floors)
                floorTypes.Add(floor.Type);
            Recalculate(floorTypes);
        }

        // Floors from the bottom of the tower up.
        public void Recalculate(IReadOnlyList<FloorType> floors)
        {
            int population = 0;
            int jobs = 0;
            int supply = 0;
            int demand = 0;
            int happiness = 0;
            foreach (FloorType type in floors)
            {
                FloorStats floor = GetFloorStats(type);
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
                population = Mathf.Max(0, population + happiness * ResidentsLostPerUnhappiness);

            Height = floors.Count;
            Population = population;
            Jobs = jobs;
            Energy = supply - demand;
            Happiness = happiness;
            NeighborIncome = neighborIncome;
            PowerRatio = CalculatePowerRatio(supply, demand);
            Income = CalculateIncome(population, jobs, happiness, PowerRatio);

            floorPowered.Clear();
            int consumers = 0;
            foreach (FloorType type in floors)
            {
                if (NeedsPower(GetFloorStats(type)))
                    consumers++;
            }
            int poweredConsumers = CountPoweredConsumers(consumers);
            foreach (FloorType type in floors)
            {
                bool needsPower = NeedsPower(GetFloorStats(type));
                floorPowered.Add(!needsPower || poweredConsumers-- > 0);
            }
        }

        // Whether the floor at this index (from the bottom) has power. FallingFloor keeps producers lit anyway.
        public bool IsFloorPowered(int index) => floorPowered[index];

        public void Tick(float deltaTime) => Money += Income * deltaTime;

        // Perfect landings in a row; the jam economy doesn't count them.
        public int PerfectStreak { get; protected set; }

        // Income multiplier from the perfect streak (1 = no combo).
        public virtual int ComboMultiplier => 1;

        public virtual void RegisterLanding(bool isPerfect)
        {
        }

        public virtual void RegisterCollapse()
        {
        }

        protected virtual FloorStats GetFloorStats(FloorType type) =>
            new FloorStats(type.Population, type.Jobs, type.Energy, type.Happiness);

        protected virtual float CalculatePowerRatio(int supply, int demand) => supply >= demand ? 1f : 0f;

        protected virtual float CalculateIncome(int population, int jobs, int happiness, float powerRatio) =>
            (Mathf.Min(population, jobs) * IncomePerWorker + NeighborIncome) * powerRatio;

        // The jam economy ignores the order of the floors.
        public virtual NeighborEffect GetNeighborEffect(FloorType below, FloorType above) => default;

        // Jam: in a blackout every floor goes dark (FallingFloor keeps power plants lit).
        protected virtual bool NeedsPower(FloorStats floor) => true;

        // How many of the consuming floors, counted from the bottom, get power.
        protected virtual int CountPoweredConsumers(int consumers) => IsPowered ? consumers : 0;
    }
}
