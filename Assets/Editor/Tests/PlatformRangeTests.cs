using NUnit.Framework;
using ZeroHour.Tower;

public class PlatformRangeTests
{
    private const float PlayHalf = 3f;       // playWidth = 6
    private const float WideScreen = 100f;   // screen limit out of the way
    private const float PlatformHalf = 0.8f;

    [Test]
    public void RoofOffsetPlusOne_ShiftsRangeLeft()
    {
        PlatformRange.Get(1f, PlayHalf, WideScreen, PlatformHalf, out float min, out float max);
        Assert.AreEqual(-4f, min, 1e-5f);
        Assert.AreEqual(2f, max, 1e-5f);
    }

    [Test]
    public void StraightTower_KeepsOldRange()
    {
        PlatformRange.Get(0f, PlayHalf, WideScreen, PlatformHalf, out float min, out float max);
        Assert.AreEqual(-3f, min, 1e-5f);
        Assert.AreEqual(3f, max, 1e-5f);
    }

    [Test]
    public void ScreenLimitsThePlatform()
    {
        // 540x720 window: visible half width 3.75, platform must keep 0.8 on screen.
        PlatformRange.Get(-1.5f, PlayHalf, 3.75f, PlatformHalf, out float min, out float max);
        Assert.AreEqual(-1.5f, min, 1e-5f);
        Assert.AreEqual(2.95f, max, 1e-5f);
    }

    [TestCase(20f, -2.95f)]
    [TestCase(-20f, 2.95f)]
    public void ExtremeOffset_ScreenWins_PlatformAtScreenEdge(float topOffset, float edge)
    {
        PlatformRange.Get(topOffset, PlayHalf, 3.75f, PlatformHalf, out float min, out float max);
        Assert.AreEqual(edge, min, 1e-5f);
        Assert.AreEqual(edge, max, 1e-5f);
    }
}
