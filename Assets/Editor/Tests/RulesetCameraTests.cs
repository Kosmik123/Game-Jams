using NUnit.Framework;
using ZeroHour.Tower;

public class RulesetCameraTests
{
    [Test]
    public void Jam_KeepsTowerTopAtFortyPercent()
    {
        Assert.AreEqual(0.4f, new JamRuleset().TowerTopScreenFraction, 1e-4f);
    }

    [Test]
    public void PostJam_KeepsTowerTopLower_ForMoreSky()
    {
        Assert.AreEqual(0.25f, new PostJamRuleset().TowerTopScreenFraction, 1e-4f);
    }
}
