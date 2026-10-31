using System;
using UnityEngine;

namespace ZeroHour.Tower
{
    // Numbers of the post-jam version, editable in the Inspector. The asset lives at
    // Assets/_Project/Resources/PostJamConfig.asset (menu Tools/Create Post-Jam Config); the field
    // defaults are the shipped values, so the game also runs without the asset.
    public class PostJamConfig : ScriptableObject
    {
        public const string ResourceName = "PostJamConfig";

        [Serializable]
        public class FloorEntry
        {
            [Tooltip("FloorType name: Residential, Office, Power, Park, Bank.")]
            public string floorType;
            [Tooltip("Residents in units of 10 (a house = 1).")]
            public float population;
            [Tooltip("Jobs in units of 10 (an office = 1).")]
            public float jobs;
            public int energy;
            public int happiness;
        }

        [Header("Floor stats (energy: positive produces, negative consumes)")]
        public FloorEntry[] floors =
        {
            new FloorEntry { floorType = "Residential", population = 1, jobs = 0, energy = -1, happiness = 0 },
            new FloorEntry { floorType = "Office", population = 0, jobs = 1, energy = -1, happiness = -1 },
            new FloorEntry { floorType = "Power", population = 0, jobs = 0, energy = 4, happiness = -2 },
            new FloorEntry { floorType = "Park", population = 0, jobs = 0, energy = 0, happiness = 4 },
            new FloorEntry { floorType = "Bank", population = 0, jobs = 0, energy = 0, happiness = 0 },
        };

        [Header("Income")]
        [Tooltip("$ per second for every employed unit of residents (the smaller of population and jobs).")]
        [Min(0f)] public float incomePerWorker = 6f;
        [Tooltip("Population units that move out for every point of unhappiness below zero (0.5 = 5 residents).")]
        [Min(0f)] public float residentsLostPerUnhappiness = 0.5f;
        [Tooltip("Income bonus per point of happiness above zero (0.05 = +5%).")]
        [Min(0f)] public float happinessIncomeBonus = 0.05f;
        [Tooltip("Cap of the happiness income multiplier (1.5 = +50%).")]
        [Min(1f)] public float maxHappinessMultiplier = 1.5f;

        [Header("Treasury")]
        [Tooltip("Balance at the start of a round.")]
        public float startBalance = 100f;
        [Tooltip("The most money that can be kept without banks; income above the limit is lost.")]
        [Min(0f)] public float baseMoneyCap = 100f;
        [Tooltip("Extra limit for every bank in the tower.")]
        [Min(0f)] public float bankMoneyCap = 150f;
        [Tooltip("$ per second for every floor beyond the free ones (the gold floor never pays).")]
        [Min(0f)] public float upkeepPerFloor = 1f;
        [Tooltip("The first floors of the tower cost no upkeep.")]
        [Min(0)] public int freeFloors = 8;

        [Header("Neighbor pairs (a residential floor directly above or below)")]
        public int parkNeighborHappiness = 1;
        public int powerNeighborHappiness = -1;
        [Tooltip("$ per second for a residential floor next to an office.")]
        [Min(0f)] public float commuteIncome = 0.9f;

        [Header("Combo")]
        [Tooltip("Perfect landings in a row needed to start the combo.")]
        [Min(1)] public int minComboStreak = 2;
        [Tooltip("Income multiplier added per combo level above the first (0.1 = +10%).")]
        [Min(0f)] public float comboIncomePerLevel = 0.1f;

        [Header("Gold floor")]
        [Range(0f, 1f)] public float goldChance = 0.05f;
        [Min(0)] public int goldMinHeight = 3;
        [Tooltip("A gold floor is guaranteed after this many spawns without one.")]
        [Min(0)] public int goldPity = 25;
        [Min(0.1f)] public float goldFallSpeedFactor = 1.4f;
        public float goldTimeBonus = 3f;
        public float goldInstabilityRelief = 25f;
        public float goldMoneyBonus = 25f;

        [Header("Tempo")]
        [Min(0f)] public float baseFallSpeed = 3f;
        [Min(0f)] public float fallSpeedPerFloor = 0.15f;
        [Min(0f)] public float maxFallSpeed = 8f;
        [Tooltip("Each floor falls at its own speed within this share of the current one (0.1 = plus or minus 10%).")]
        [Range(0f, 0.9f)] public float fallSpeedSpread = 0.1f;
        [Min(0.05f)] public float baseSpawnInterval = 0.7f;
        [Min(0f)] public float spawnIntervalPerFloor = 0.01f;
        [Min(0.05f)] public float minSpawnInterval = 0.4f;
        [Tooltip("The gap between floors strays from the interval by this share (0.5 = from half to one and a half).")]
        [Range(0f, 0.95f)] public float spawnIntervalJitter = 0.5f;

        // Stats of a floor type from the table; false when the table has no row for it.
        public bool TryGetFloorStats(string floorType, out FloorStats stats)
        {
            if (floors != null)
            {
                foreach (FloorEntry entry in floors)
                {
                    if (entry != null && entry.floorType == floorType)
                    {
                        stats = new FloorStats(entry.population, entry.jobs, entry.energy, entry.happiness);
                        return true;
                    }
                }
            }
            stats = default;
            return false;
        }

        // The shipped values, ignoring any asset (tests, and the fallback when there is none).
        public static PostJamConfig CreateDefault()
        {
            var config = CreateInstance<PostJamConfig>();
            config.hideFlags = HideFlags.HideAndDontSave;
            return config;
        }

        // The asset from Resources, or the shipped values when it doesn't exist.
        public static PostJamConfig Load()
        {
            var config = Resources.Load<PostJamConfig>(ResourceName);
            return config != null ? config : CreateDefault();
        }
    }
}
