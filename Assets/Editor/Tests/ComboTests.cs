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
    public void ThreePerfects_GiveCombo3_ThenImperfectResets()
    {
        var stats = Land(new PostJamRuleset().CreateEconomy(), true, true, true);
        Assert.AreEqual(3, stats.Combo);
        stats.RegisterLanding(false);
        Assert.AreEqual(1, stats.Combo);
    }

    [Test]
    public void SinglePerfect_IsNoComboYet()
    {
        Assert.AreEqual(1, Land(new PostJamRuleset().CreateEconomy(), true).Combo);
        Assert.AreEqual(2, Land(new PostJamRuleset().CreateEconomy(), true, true).Combo);
    }

    [Test]
    public void Combo_HasNoUpperLimit()
    {
        var stats = Land(new PostJamRuleset().CreateEconomy(), Enumerable.Repeat(true, 12).ToArray());
        Assert.AreEqual(12, stats.PerfectStreak);
        Assert.AreEqual(12, stats.Combo);
    }

    [Test]
    public void IncomeMultiplier_GrowsByTenthPerComboLevel()
    {
        Assert.AreEqual(1f, Land(new PostJamRuleset().CreateEconomy(), true).ComboIncomeMultiplier, 1e-4f);
        Assert.AreEqual(1.1f, Land(new PostJamRuleset().CreateEconomy(), true, true).ComboIncomeMultiplier, 1e-4f);
        Assert.AreEqual(1.2f, Land(new PostJamRuleset().CreateEconomy(), true, true, true).ComboIncomeMultiplier, 1e-4f);
        Assert.AreEqual(1.3f, Land(new PostJamRuleset().CreateEconomy(), true, true, true, true).ComboIncomeMultiplier, 1e-4f);
        Assert.AreEqual(2.1f, Land(new PostJamRuleset().CreateEconomy(), Enumerable.Repeat(true, 12).ToArray()).ComboIncomeMultiplier, 1e-4f);
    }

    [Test]
    public void Collapse_ResetsStreak()
    {
        var stats = Land(new PostJamRuleset().CreateEconomy(), true, true, true);
        stats.RegisterCollapse();
        Assert.AreEqual(0, stats.PerfectStreak);
        Assert.AreEqual(1, stats.Combo);
    }

    [Test]
    public void Combo_MultipliesWithHappinessBonus()
    {
        var stats = Land(new PostJamRuleset().CreateEconomy(), true, true, true);
        stats.Recalculate((IReadOnlyList<FloorType>)new[] { R, O, P, Park, Park }); // (2 + 0.3 commute) * 1.25 without combo
        Assert.AreEqual(2.3f * 1.25f * 1.2f, stats.Income, 1e-4f); // combo 3 -> x1.2
    }

    [Test]
    public void Jam_HasNoCombo()
    {
        var stats = Land(new JamRuleset().CreateEconomy(), true, true, true);
        Assert.AreEqual(1, stats.Combo);
        Assert.AreEqual(1f, stats.ComboIncomeMultiplier);
        stats.Recalculate((IReadOnlyList<FloorType>)new[] { R, O, P, Park, Park });
        Assert.AreEqual(2f, stats.Income, 1e-4f);
    }
}
