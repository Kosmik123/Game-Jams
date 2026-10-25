using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ZeroHour.Tower;

public class ComboTests
{
    private static readonly FloorType R = FloorType.All.First(t => t.Name == "Residential");
    private static readonly FloorType O = FloorType.All.First(t => t.Name == "Office");
    private static readonly FloorType P = FloorType.All.First(t => t.Name == "Power");
    private static readonly FloorType Park = FloorType.All.First(t => t.Name == "Park");

    private static CityStats Land(CityStats stats, params bool[] perfect)
    {
        foreach (bool p in perfect)
            stats.RegisterLanding(p);
        return stats;
    }

    [Test]
    public void ThreePerfects_GiveX3_ThenImperfectResets()
    {
        var stats = Land(new PostJamRuleset().CreateEconomy(), true, true, true);
        Assert.AreEqual(3, stats.ComboMultiplier);
        stats.RegisterLanding(false);
        Assert.AreEqual(1, stats.ComboMultiplier);
    }

    [Test]
    public void SinglePerfect_IsNoComboYet()
    {
        Assert.AreEqual(1, Land(new PostJamRuleset().CreateEconomy(), true).ComboMultiplier);
        Assert.AreEqual(2, Land(new PostJamRuleset().CreateEconomy(), true, true).ComboMultiplier);
    }

    [Test]
    public void Multiplier_IsCappedAtFive()
    {
        var stats = Land(new PostJamRuleset().CreateEconomy(), true, true, true, true, true, true, true);
        Assert.AreEqual(7, stats.PerfectStreak);
        Assert.AreEqual(5, stats.ComboMultiplier);
    }

    [Test]
    public void Collapse_ResetsStreak()
    {
        var stats = Land(new PostJamRuleset().CreateEconomy(), true, true, true);
        stats.RegisterCollapse();
        Assert.AreEqual(0, stats.PerfectStreak);
        Assert.AreEqual(1, stats.ComboMultiplier);
    }

    [Test]
    public void Combo_MultipliesWithHappinessBonus()
    {
        var stats = Land(new PostJamRuleset().CreateEconomy(), true, true, true);
        stats.Recalculate((IReadOnlyList<FloorType>)new[] { R, O, P, Park, Park }); // 2.5 $/s without combo
        Assert.AreEqual(2.5f * 3f, stats.Income, 1e-4f);
    }

    [Test]
    public void Jam_HasNoCombo()
    {
        var stats = Land(new JamRuleset().CreateEconomy(), true, true, true);
        Assert.AreEqual(1, stats.ComboMultiplier);
        stats.Recalculate((IReadOnlyList<FloorType>)new[] { R, O, P, Park, Park });
        Assert.AreEqual(2f, stats.Income, 1e-4f);
    }
}
