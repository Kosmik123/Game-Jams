using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using ZeroHour.Tower;

public class GoldFloorTests
{
    private static FloorType Type(string name) => FloorType.All.First(name);

    [Test]
    public void GetRandom_NeverReturnsGold()
    {
        for (int i = 0; i < 2000; i++)
            Assert.IsFalse(FloorType.GetRandom().IsGold);
        CollectionAssert.DoesNotContain(FloorType.All, FloorType.Gold);
    }

    [Test]
    public void Pity_TwentyFiveDrySpawnsThenGold()
    {
        var spawner = new GoldSpawner(0.05f, 3, 25);
        for (int i = 0; i < 25; i++)
            Assert.IsFalse(spawner.Next(10, false, 0.99f), $"spawn {i + 1}");
        Assert.IsTrue(spawner.Next(10, false, 0.99f));
        // The counter restarts after a golden spawn.
        for (int i = 0; i < 25; i++)
            Assert.IsFalse(spawner.Next(10, false, 0.99f));
        Assert.IsTrue(spawner.Next(10, false, 0.99f));
    }

    [Test]
    public void LowRoll_GivesGold_OnlyWhenEligible()
    {
        var spawner = new GoldSpawner(0.05f, 3, 25);
        Assert.IsFalse(spawner.Next(2, false, 0f), "below the minimum height");
        Assert.IsTrue(spawner.Next(3, false, 0.04f));
        Assert.IsFalse(spawner.Next(3, false, 0.05f), "roll at the chance is a miss");
    }

    [Test]
    public void OnlyOneGoldInTheAir()
    {
        var spawner = new GoldSpawner(0.05f, 3, 25);
        for (int i = 0; i < 40; i++)
            Assert.IsFalse(spawner.Next(10, true, 0f));
        Assert.IsTrue(spawner.Next(10, false, 0f), "pity is still owed once the air is clear");
    }

    [Test]
    public void Jam_NeverSpawnsGold()
    {
        var ruleset = new JamRuleset();
        Assert.AreEqual(0f, ruleset.GoldChance);
        var spawner = new GoldSpawner(ruleset.GoldChance, ruleset.GoldMinHeight, ruleset.GoldPity);
        for (int i = 0; i < 200; i++)
            Assert.IsFalse(spawner.Next(50, false, 0f));
    }

    [Test]
    public void PostJam_Numbers()
    {
        var ruleset = new PostJamRuleset();
        Assert.AreEqual(0.05f, ruleset.GoldChance, 1e-5f);
        Assert.AreEqual(3, ruleset.GoldMinHeight);
        Assert.AreEqual(25, ruleset.GoldPity);
        Assert.AreEqual(1.4f, ruleset.GoldFallSpeedFactor, 1e-5f);
    }

    [Test]
    public void Catch_AddsThreeSeconds_WithinTheLimit()
    {
        var ruleset = new PostJamRuleset();
        Assert.AreEqual(3f, ruleset.GoldTimeBonus);
        Assert.AreEqual(20f, ruleset.ApplyTimeChange(17f, ruleset.GoldTimeBonus), 1e-4f);
        Assert.AreEqual(30f, ruleset.ApplyTimeChange(29f, ruleset.GoldTimeBonus), 1e-4f);
    }

    [TestCase(60f, 35f)]
    [TestCase(10f, 0f)]
    public void Catch_RelievesInstability_NotBelowZero(float before, float expected)
    {
        var go = new GameObject("tower");
        try
        {
            var tower = go.AddComponent<TowerController>();
            FieldInfo field = typeof(TowerController).GetField("instability", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(tower, before);
            tower.RelieveInstability(new PostJamRuleset().GoldInstabilityRelief);
            Assert.AreEqual(expected, (float)field.GetValue(tower), 1e-4f);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void Catch_PaysTwentyFiveDollars()
    {
        var stats = new PostJamRuleset().CreateEconomy();
        stats.AddBonusMoney(new PostJamRuleset().GoldMoneyBonus);
        Assert.AreEqual(25f, stats.Money, 1e-4f);
    }

    [Test]
    public void Gold_HasNoStatsAndNoSynergies()
    {
        var stats = new PostJamRuleset().CreateEconomy();
        var withGold = new[] { Type("Residential"), FloorType.Gold, Type("Park") };
        var without = new[] { Type("Residential"), Type("Park") };

        stats.Recalculate(withGold);
        Assert.AreEqual(3, stats.Height, "gold counts as a floor");
        int energy = stats.Energy, happiness = stats.Happiness, population = stats.Population;
        float neighborIncome = stats.NeighborIncome;
        Assert.AreEqual(0f, neighborIncome, "no pairs: gold separates the residential from the park");

        // Same stats as a tower without the gold floor, except that the pair R-Park would now exist.
        stats.Recalculate(without);
        Assert.AreEqual(energy, stats.Energy);
        Assert.AreEqual(population, stats.Population);
        Assert.AreEqual(happiness + 1, stats.Happiness, "R-Park pair gives +1 Joy; with gold in between it doesn't");

        Assert.IsTrue(stats.GetNeighborEffect(Type("Residential"), FloorType.Gold).IsNone);
        Assert.IsTrue(stats.GetNeighborEffect(FloorType.Gold, Type("Residential")).IsNone);
    }

    [Test]
    public void Gold_IsNotAPowerConsumer()
    {
        var stats = new PostJamRuleset().CreateEconomy();
        stats.Recalculate(new[] { FloorType.Gold, FloorType.Gold });
        Assert.AreEqual(1f, stats.PowerRatio);
        Assert.IsTrue(stats.IsFloorPowered(0));
        Assert.AreEqual(0, stats.Energy);
    }
}

internal static class FloorTypeListExtensions
{
    public static FloorType First(this System.Collections.Generic.IReadOnlyList<FloorType> list, string name)
    {
        foreach (FloorType type in list)
        {
            if (type.Name == name)
                return type;
        }
        return null;
    }
}
