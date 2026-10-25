using NUnit.Framework;
using ZeroHour.Tower;

public class PlatformRangeTests
{
    private const float PlayHalf = 3f; // playWidth = 6

    [Test]
    public void RoofOffsetPlusOne_ShiftsRangeLeft()
    {
        PlatformRange.Get(1f, PlayHalf, out float min, out float max);
        Assert.AreEqual(-4f, min, 1e-5f);
        Assert.AreEqual(2f, max, 1e-5f);
    }

    [Test]
    public void StraightTower_KeepsOldRange()
    {
        PlatformRange.Get(0f, PlayHalf, out float min, out float max);
        Assert.AreEqual(-3f, min, 1e-5f);
        Assert.AreEqual(3f, max, 1e-5f);
    }

    [TestCase(20f, -23f, -17f)]
    [TestCase(-20f, 17f, 23f)]
    public void ExtremeOffset_RoofWins_NoScreenLimit(float topOffset, float expectedMin, float expectedMax)
    {
        PlatformRange.Get(topOffset, PlayHalf, out float min, out float max);
        Assert.AreEqual(expectedMin, min, 1e-5f);
        Assert.AreEqual(expectedMax, max, 1e-5f);
        // The roof reaches both edges of the play width.
        Assert.AreEqual(-PlayHalf, min + topOffset, 1e-5f);
        Assert.AreEqual(PlayHalf, max + topOffset, 1e-5f);
    }
}
