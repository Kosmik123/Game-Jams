using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ZeroHour.Tower;

public class CityStatsTests
{
    private static readonly FloorType R = FloorType.All.First(t => t.Name == "Residential");
    private static readonly FloorType O = FloorType.All.First(t => t.Name == "Office");
    private static readonly FloorType P = FloorType.All.First(t => t.Name == "Power");
    private static readonly FloorType Park = FloorType.All.First(t => t.Name == "Park");

    // These post-jam towers have one residential-office pair: +0.3 $/s commute income (see NeighborTests).
    private const float Commute = 0.3f;

    private static CityStats PostJam(params FloorType[] floors)
    {
        var stats = new PostJamRuleset().CreateEconomy();
        stats.Recalculate((IReadOnlyList<FloorType>)floors);
        return stats;
    }

    private static CityStats Jam(params FloorType[] floors)
    {
        var stats = new JamRuleset().CreateEconomy();
        stats.Recalculate((IReadOnlyList<FloorType>)floors);
        return stats;
    }

    [Test]
    public void PostJam_BalancedRecipe_BreaksEven()
    {
        var stats = PostJam(R, R, O, O, P, Park);
        Assert.AreEqual(0, stats.Energy);
        Assert.AreEqual(0, stats.Happiness);
        Assert.AreEqual(20, stats.Population);
        Assert.AreEqual(1f, stats.PowerRatio);
        Assert.AreEqual(4f + Commute, stats.Income, 1e-4f);
        Assert.IsNull(stats.PowerWarning);
    }

    [Test]
    public void PostJam_NoPowerPlant_NoIncome()
    {
        var stats = PostJam(R, R, R, O);
        Assert.AreEqual(0f, stats.PowerRatio);
        Assert.AreEqual(0f, stats.Income);
        Assert.AreEqual("Power 0%", stats.PowerWarning);
        for (int i = 0; i < 4; i++)
            Assert.IsFalse(stats.IsFloorPowered(i));
    }

    [Test]
    public void PostJam_PartialPower_ScalesIncomeAndDarkensFromTop()
    {
        var stats = PostJam(R, R, R, R, O, O, P);
        Assert.AreEqual(4f / 6f, stats.PowerRatio, 1e-4f);
        Assert.AreEqual(-6, stats.Happiness);
        Assert.AreEqual(10, stats.Population);
        Assert.AreEqual((10 * 0.2f + Commute) * 4f / 6f, stats.Income, 1e-4f);
        Assert.AreEqual("Power 67%", stats.PowerWarning);

        // 6 consumers, 4 powered: the four lowest are lit, the two offices on top are dark, the plant is lit.
        bool[] expected = { true, true, true, true, false, false, true };
        for (int i = 0; i < expected.Length; i++)
            Assert.AreEqual(expected[i], stats.IsFloorPowered(i), $"floor {i}");
    }

    [Test]
    public void PostJam_PositiveHappiness_BoostsIncome()
    {
        var stats = PostJam(R, O, P, Park, Park);
        Assert.AreEqual(5, stats.Happiness);
        Assert.AreEqual(1f, stats.PowerRatio);
        Assert.AreEqual((2f + Commute) * 1.25f, stats.Income, 1e-4f);
    }

    [Test]
    public void PostJam_HappinessBonus_IsCappedAt50Percent()
    {
        var stats = PostJam(R, O, P, P, Park, Park, Park, Park, Park, Park);
        Assert.Greater(stats.Happiness, 10);
        Assert.AreEqual((10 * 0.2f + Commute) * 1.5f, stats.Income, 1e-4f);
    }

    [Test]
    public void Jam_KeepsOriginalEconomy()
    {
        var balanced = Jam(R, R, O, O, P, Park);
        Assert.AreEqual(-1, balanced.Happiness, "jam park gives +3");
        Assert.AreEqual(15, balanced.Population);
        Assert.AreEqual(3f, balanced.Income, 1e-4f);
        Assert.IsTrue(balanced.IsPowered);

        var blackout = Jam(R, O, Park);
        Assert.IsFalse(blackout.IsPowered);
        Assert.AreEqual(0f, blackout.Income);
        Assert.AreEqual("BLACKOUT!", blackout.PowerWarning);
        for (int i = 0; i < 3; i++)
            Assert.IsFalse(blackout.IsFloorPowered(i), $"jam blackout darkens every floor ({i})");
    }

    [Test]
    public void Jam_HappyCity_NoIncomeBonus()
    {
        var stats = Jam(R, O, P, Park, Park);
        Assert.AreEqual(3, stats.Happiness);
        Assert.AreEqual(2f, stats.Income, 1e-4f);
    }
}
