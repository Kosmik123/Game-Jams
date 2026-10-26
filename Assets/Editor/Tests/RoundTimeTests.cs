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
}
