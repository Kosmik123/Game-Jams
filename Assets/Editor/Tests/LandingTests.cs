using NUnit.Framework;
using ZeroHour.Tower;

public class LandingTests
{
    private const float Width = 1f;
    private const float PerfectOffset = 0.12f;

    [Test]
    public void PerfectLanding_IsPlacedOnPreviousFloor()
    {
        float x = Landing.PlaceX(0.6f, 0.5f, Width, PerfectOffset, true, out bool isPerfect);
        Assert.IsTrue(isPerfect);
        Assert.AreEqual(0.5f, x, 1e-6f);
    }

    [Test]
    public void FirstFloor_PerfectLanding_IsCenteredOnPlatform()
    {
        float x = Landing.PlaceX(-0.1f, 0f, Width, PerfectOffset, true, out bool isPerfect);
        Assert.IsTrue(isPerfect);
        Assert.AreEqual(0f, x, 1e-6f);
    }

    [Test]
    public void ImperfectLanding_StaysWhereItLanded()
    {
        float x = Landing.PlaceX(0.7f, 0.5f, Width, PerfectOffset, true, out bool isPerfect);
        Assert.IsFalse(isPerfect);
        Assert.AreEqual(0.7f, x, 1e-6f);
    }

    [Test]
    public void WithoutSnap_PerfectLandingStaysWhereItLanded()
    {
        float x = Landing.PlaceX(0.6f, 0.5f, Width, PerfectOffset, false, out bool isPerfect);
        Assert.IsTrue(isPerfect);
        Assert.AreEqual(0.6f, x, 1e-6f);
    }
}
