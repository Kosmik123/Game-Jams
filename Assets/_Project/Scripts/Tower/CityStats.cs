using System.Collections.Generic;
using UnityEngine;

namespace ZeroHour.Tower
{
    // City economy computed from the floors in the tower.
    public class CityStats
    {
        private const int ResidentsLostPerUnhappiness = 5;
        private const float IncomePerWorker = 0.2f;

        public int Height { get; private set; }
        public int Population { get; private set; }
        public int Jobs { get; private set; }
        public int Energy { get; private set; }
        public int Happiness { get; private set; }
        public float Income { get; private set; }
        public float Money { get; private set; }

        public bool IsPowered => Energy >= 0;

        public void Recalculate(IReadOnlyList<FallingFloor> floors)
        {
            int population = 0;
            int jobs = 0;
            int energy = 0;
            int happiness = 0;
            foreach (FallingFloor floor in floors)
            {
                population += floor.Type.Population;
                jobs += floor.Type.Jobs;
                energy += floor.Type.Energy;
                happiness += floor.Type.Happiness;
            }

            // Unhappy residents move out.
            if (happiness < 0)
                population = Mathf.Max(0, population + happiness * ResidentsLostPerUnhappiness);

            Height = floors.Count;
            Population = population;
            Jobs = jobs;
            Energy = energy;
            Happiness = happiness;
            Income = IsPowered ? Mathf.Min(population, jobs) * IncomePerWorker : 0f;
        }

        public void Tick(float deltaTime) => Money += Income * deltaTime;
    }
}
