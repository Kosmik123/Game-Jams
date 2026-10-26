using NUnit.Framework;
using UnityEngine;
using ZeroHour.Tower;

public class SkyPaletteTests
{
    private static readonly Color Day = TowerGame.SkyColor;

    private static Color Hex(int rgb) =>
        new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f);

    private static Color Mid(Color a, Color b) => (a + b) / 2f;

    private static void AssertColor(Color expected, Color actual, string message = null)
    {
        Assert.AreEqual(expected.r, actual.r, 1e-4f, message);
        Assert.AreEqual(expected.g, actual.g, 1e-4f, message);
        Assert.AreEqual(expected.b, actual.b, 1e-4f, message);
    }

    [Test]
    public void Height0_IsTheDaySky_AtBothEnds()
    {
        SkyPalette.Evaluate(0f, Day, out Color top, out Color bottom);
        AssertColor(Day, top);
        AssertColor(Day, bottom);
    }

    [Test]
    public void Height5_IsHalfwayBetweenDayAndSunset()
    {
        SkyPalette.Evaluate(5f, Day, out Color top, out Color bottom);
        AssertColor(Mid(Day, Hex(0x4A6FA5)), top);
        AssertColor(Mid(Day, Hex(0xF4A261)), bottom);
    }

    [Test]
    public void Height10_IsSunset()
    {
        SkyPalette.Evaluate(10f, Day, out Color top, out Color bottom);
        AssertColor(Hex(0x4A6FA5), top);
        AssertColor(Hex(0xF4A261), bottom);
    }

    [Test]
    public void Height25_IsHalfwayBetweenDuskAndNight()
    {
        SkyPalette.Evaluate(25f, Day, out Color top, out Color bottom);
        AssertColor(Mid(Hex(0x2B2D6E), Hex(0x0A0F2C)), top);
        AssertColor(Mid(Hex(0xC2557A), Hex(0x1F2A5C)), bottom);
    }

    [TestCase(40f)]
    [TestCase(80f)]
    public void Height40AndUp_IsDeepNight(float height)
    {
        SkyPalette.Evaluate(height, Day, out Color top, out Color bottom);
        AssertColor(Hex(0x03051A), top);
        AssertColor(Hex(0x101A40), bottom);
    }

    [TestCase(0f, 0f)]
    [TestCase(20f, 0f)]
    [TestCase(26f, 0.5f)]
    [TestCase(32f, 1f)]
    [TestCase(80f, 1f)]
    public void StarAlpha_RisesBetween20And32(float height, float expected)
    {
        Assert.AreEqual(expected, SkyPalette.StarAlpha(height), 1e-4f);
    }

    [Test]
    public void Jam_KeepsThePlainSky()
    {
        Assert.IsFalse(new JamRuleset().DynamicSky);
        Assert.IsTrue(new PostJamRuleset().DynamicSky);
    }
}
