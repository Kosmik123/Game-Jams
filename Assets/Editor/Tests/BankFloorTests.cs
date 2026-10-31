using System.Collections.Generic;
using NUnit.Framework;
using ZeroHour.Tower;

public class BankFloorTests
{
    [Test]
    public void Bank_IsNotInTheOriginalFloorList()
    {
        CollectionAssert.DoesNotContain(FloorType.All, FloorType.Bank);
        Assert.AreEqual(4, FloorType.All.Count);
    }

    [Test]
    public void PostJam_DrawsBanksLikeAnyOtherFloor()
    {
        IReadOnlyList<FloorType> pool = new PostJamRuleset().FloorPool;
        Assert.AreEqual(5, pool.Count);
        CollectionAssert.Contains(pool, FloorType.Bank);
        foreach (FloorType type in FloorType.All)
            CollectionAssert.Contains(pool, type);
        CollectionAssert.DoesNotContain(pool, FloorType.Gold);
    }

    [Test]
    public void Jam_NeverDrawsBanks()
    {
        CollectionAssert.DoesNotContain(new JamRuleset().FloorPool, FloorType.Bank);
    }

    [Test]
    public void Bank_HasNoStatsYet()
    {
        var stats = new PostJamCityStats();
        stats.Recalculate(new[] { FloorType.Bank, FloorType.Bank });
        Assert.AreEqual(0, stats.Population);
        Assert.AreEqual(0, stats.Jobs);
        Assert.AreEqual(0, stats.Energy);
        Assert.AreEqual(0, stats.Happiness);
        Assert.AreEqual(0f, stats.Income, 1e-4f);
    }

    [Test]
    public void FallSpeed_PostJamSpreadsPlusMinusTenPercent()
    {
        var postJam = new PostJamRuleset();
        float baseSpeed = postJam.FallSpeed(0);
        Assert.AreEqual(baseSpeed * 0.9f, postJam.NextFallSpeed(0, 0f), 1e-4f);
        Assert.AreEqual(baseSpeed, postJam.NextFallSpeed(0, 0.5f), 1e-4f);
        Assert.AreEqual(baseSpeed * 1.1f, postJam.NextFallSpeed(0, 1f), 1e-4f);
    }

    [Test]
    public void FallSpeed_JamStaysExact()
    {
        var jam = new JamRuleset();
        Assert.AreEqual(3f, jam.NextFallSpeed(0, 0f), 1e-4f);
        Assert.AreEqual(3f, jam.NextFallSpeed(0, 1f), 1e-4f);
    }
}
