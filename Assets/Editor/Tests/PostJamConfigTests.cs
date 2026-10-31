using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ZeroHour.Tower;

public class PostJamConfigTests
{
    private static FloorType Type(string name)
    {
        foreach (FloorType type in FloorType.WithBank)
        {
            if (type.Name == name)
                return type;
        }
        throw new KeyNotFoundException(name);
    }

    private static PostJamConfig NewConfig() => PostJamConfig.CreateDefault();

    [Test]
    public void Defaults_HoldTheShippedFloorTable()
    {
        PostJamConfig config = NewConfig();
        Assert.IsTrue(config.TryGetFloorStats("Residential", out FloorStats residential));
        Assert.AreEqual(10, residential.Population);
        Assert.AreEqual(-1, residential.Energy);
        Assert.AreEqual(-1, residential.Happiness);
        Assert.IsTrue(config.TryGetFloorStats("Office", out FloorStats office));
        Assert.AreEqual(10, office.Jobs);
        Assert.IsTrue(config.TryGetFloorStats("Power", out FloorStats power));
        Assert.AreEqual(4, power.Energy);
        Assert.AreEqual(-2, power.Happiness);
        Assert.IsTrue(config.TryGetFloorStats("Park", out FloorStats park));
        Assert.AreEqual(4, park.Happiness);
        Assert.IsTrue(config.TryGetFloorStats("Bank", out FloorStats bank));
        Assert.AreEqual(0, bank.Population + bank.Jobs + bank.Energy + bank.Happiness);
        Assert.IsFalse(config.TryGetFloorStats("Nothing", out _));
    }

    [Test]
    public void Defaults_GiveTheOldBalancedBlock()
    {
        var stats = new PostJamCityStats(NewConfig());
        stats.Recalculate(new[] { Type("Residential"), Type("Office"), Type("Residential"), Type("Office"), Type("Power"), Type("Park") });
        Assert.AreEqual(0, stats.Energy);
        Assert.AreEqual(0, stats.Happiness);
        Assert.AreEqual(20, stats.Population);
        // 20 workers x 0.2 + three residential-office pairs x 0.3 commute
        Assert.AreEqual(4f + 0.9f, stats.Income, 1e-3f);
    }

    [Test]
    public void IncomePerWorker_ScalesTheIncome()
    {
        PostJamConfig config = NewConfig();
        config.incomePerWorker = 0.5f;
        var stats = new PostJamCityStats(config);
        stats.Recalculate(new[] { Type("Residential"), Type("Office"), Type("Power"), Type("Park") });
        // 10 workers x 0.5 + 0.3 commute, energy 4 vs 2 consumers, happiness -1 -2 +4 = +1 (+5%).
        Assert.AreEqual((10 * 0.5f + 0.3f) * 1.05f, stats.Income, 1e-3f);
    }

    [Test]
    public void FloorTable_ChangesTheStats()
    {
        PostJamConfig config = NewConfig();
        config.floors[0].population = 20; // Residential
        var stats = new PostJamCityStats(config);
        stats.Recalculate(new[] { Type("Residential"), Type("Office"), Type("Power"), Type("Park") });
        Assert.AreEqual(20, stats.Population);
    }

    [Test]
    public void MissingFloorRow_FallsBackToTheFloorType()
    {
        PostJamConfig config = NewConfig();
        config.floors = new PostJamConfig.FloorEntry[0];
        var stats = new PostJamCityStats(config);
        stats.Recalculate(new[] { Type("Office") });
        Assert.AreEqual(10, stats.Jobs); // FloorType.Office's own value
    }

    [Test]
    public void HappinessBonus_FollowsTheConfig()
    {
        PostJamConfig config = NewConfig();
        config.happinessIncomeBonus = 0.1f;
        config.maxHappinessMultiplier = 1.2f;
        var stats = new PostJamCityStats(config);
        stats.Recalculate(new[] { Type("Residential"), Type("Office"), Type("Power"), Type("Park"), Type("Park") });
        // happiness -1 -2 +8 = +5 -> x1.5 capped at x1.2
        Assert.AreEqual((10 * 0.2f + 0.3f) * 1.2f, stats.Income, 1e-3f);
    }

    [Test]
    public void Ruleset_ReadsTempoAndGoldFromTheConfig()
    {
        PostJamConfig config = NewConfig();
        config.baseFallSpeed = 5f;
        config.fallSpeedPerFloor = 0.5f;
        config.maxFallSpeed = 7f;
        config.goldChance = 0.5f;
        config.goldMoneyBonus = 99f;
        var ruleset = new PostJamRuleset(config);
        Assert.AreEqual(5f, ruleset.FallSpeed(0), 1e-4f);
        Assert.AreEqual(6f, ruleset.FallSpeed(2), 1e-4f);
        Assert.AreEqual(7f, ruleset.FallSpeed(100), 1e-4f);
        Assert.AreEqual(0.5f, ruleset.GoldChance, 1e-4f);
        Assert.AreEqual(99f, ruleset.GoldMoneyBonus, 1e-4f);
    }

    [Test]
    public void Ruleset_DefaultsKeepTheShippedTempo()
    {
        var ruleset = new PostJamRuleset();
        Assert.AreEqual(3f, ruleset.FallSpeed(0), 1e-4f);
        Assert.AreEqual(8f, ruleset.FallSpeed(1000), 1e-4f);
        Assert.AreEqual(0.7f, ruleset.SpawnInterval(0), 1e-4f);
        Assert.AreEqual(0.4f, ruleset.SpawnInterval(1000), 1e-4f);
        Assert.AreEqual(0.5f, ruleset.SpawnIntervalJitter, 1e-4f);
        Assert.AreEqual(0.1f, ruleset.FallSpeedJitter, 1e-4f);
    }

    [Test]
    public void Load_AlwaysReturnsAConfig()
    {
        Assert.IsNotNull(PostJamConfig.Load());
    }
}
