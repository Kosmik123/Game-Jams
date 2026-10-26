using NUnit.Framework;
using ZeroHour.Tower;

public class RoundTimeTests
{
    private readonly TowerRuleset postJam = new PostJamRuleset();
    private readonly TowerRuleset jam = new JamRuleset();

    [TestCase(1, 0.975f)]
    [TestCase(20, 0.5f)]
    [TestCase(40, 0.3f)]
    [TestCase(80, 0.3f)]
    public void FloorBonus_ShrinksWithHeight(int height, float expected)
    {
        Assert.AreEqual(expected, postJam.FloorTimeBonus(height, false), 1e-4f);
    }

    [Test]
    public void Perfect_AddsOneSecond()
    {
        Assert.AreEqual(postJam.FloorTimeBonus(20, false) + 1f, postJam.FloorTimeBonus(20, true), 1e-4f);
    }

    [Test]
    public void Collapse_TakesFourSeconds_NotBelowZero()
    {
        Assert.AreEqual(4f, postJam.CollapseTimePenalty, 1e-4f);
        Assert.AreEqual(6f, postJam.ApplyTimeChange(10f, -postJam.CollapseTimePenalty), 1e-4f);
        Assert.AreEqual(0f, postJam.ApplyTimeChange(3f, -postJam.CollapseTimePenalty), 1e-4f);
    }

    [Test]
    public void Clock_StartsAt25_AndIsCappedAt30()
    {
        Assert.AreEqual(25f, postJam.RoundDuration, 1e-4f);
        Assert.AreEqual(30f, postJam.ApplyTimeChange(29.5f, postJam.FloorTimeBonus(1, true)), 1e-4f);
    }

    [Test]
    public void Jam_HasNoTimeBonuses()
    {
        Assert.AreEqual(40f, jam.RoundDuration, 1e-4f);
        Assert.AreEqual(0f, jam.FloorTimeBonus(1, true));
        Assert.AreEqual(0f, jam.CollapseTimePenalty);
        Assert.AreEqual(0f, jam.MissedFloorTimePenalty);
    }

    [Test]
    public void PostJam_MissedFloor_CostsHalfASecond()
    {
        Assert.AreEqual(0.5f, postJam.MissedFloorTimePenalty, 1e-4f);
        Assert.AreEqual(9.5f, postJam.ApplyTimeChange(10f, -postJam.MissedFloorTimePenalty), 1e-4f);
        Assert.AreEqual(0f, postJam.ApplyTimeChange(0.3f, -postJam.MissedFloorTimePenalty), 1e-4f);
    }

    [Test]
    public void PostJam_SpawnGaps_AreIrregularButKeepTheMean()
    {
        // Height 0: base interval 0.7 s, jitter +-50% -> 0.35 s .. 1.05 s.
        Assert.AreEqual(0.35f, postJam.NextSpawnGap(0, 0f), 1e-4f);
        Assert.AreEqual(0.7f, postJam.NextSpawnGap(0, 0.5f), 1e-4f);
        Assert.AreEqual(1.05f, postJam.NextSpawnGap(0, 1f), 1e-4f);
        var rng = new System.Random(7);
        float sum = 0f;
        const int n = 20000;
        for (int i = 0; i < n; i++)
            sum += postJam.NextSpawnGap(0, (float)rng.NextDouble());
        Assert.AreEqual(0.7f, sum / n, 0.01f);
    }

    [Test]
    public void SpawnGap_NeverBelowMinimum_AndJamStaysRegular()
    {
        // Height 60: base interval 0.4 s -> 0.2 s at best, still above the 0.15 s floor.
        Assert.GreaterOrEqual(postJam.NextSpawnGap(60, 0f), TowerRuleset.MinSpawnGap);
        Assert.AreEqual(0.7f, jam.NextSpawnGap(0, 0f), 1e-4f);
        Assert.AreEqual(0.7f, jam.NextSpawnGap(0, 1f), 1e-4f);
    }
}
