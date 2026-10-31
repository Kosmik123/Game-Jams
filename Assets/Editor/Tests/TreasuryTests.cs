using System.Collections.Generic;
using NUnit.Framework;
using ZeroHour.Tower;

public class TreasuryTests
{
    private static FloorType Type(string name)
    {
        foreach (FloorType type in FloorType.WithBank)
        {
            if (type.Name == name)
                return type;
        }
        throw new KeyNotFoundException(name);
    }

    private static PostJamCityStats Economy(PostJamConfig config = null) => new PostJamCityStats(config ?? PostJamConfig.CreateDefault());

    private static FloorType[] Floors(int count, string name)
    {
        var floors = new FloorType[count];
        for (int i = 0; i < count; i++)
            floors[i] = Type(name);
        return floors;
    }

    [Test]
    public void Start_BalanceAndCapComeFromTheConfig()
    {
        PostJamConfig config = PostJamConfig.CreateDefault();
        config.startBalance = 70f;
        config.baseMoneyCap = 120f;
        PostJamCityStats stats = Economy(config);
        Assert.AreEqual(70f, stats.Money, 1e-4f);
        Assert.AreEqual(120f, stats.MoneyCap, 1e-4f);
    }

    [Test]
    public void Start_BalanceIsCutToTheCap()
    {
        PostJamConfig config = PostJamConfig.CreateDefault();
        config.startBalance = 500f;
        Assert.AreEqual(config.baseMoneyCap, Economy(config).Money, 1e-4f);
    }

    [Test]
    public void Upkeep_StartsAfterTheFreeFloors()
    {
        PostJamCityStats stats = Economy();
        stats.Recalculate(Floors(8, "Park"));
        Assert.AreEqual(0f, stats.Upkeep, 1e-4f);
        stats.Recalculate(Floors(11, "Park"));
        Assert.AreEqual(3f, stats.Upkeep, 1e-4f);
    }

    [Test]
    public void Upkeep_FollowsTheConfig()
    {
        PostJamConfig config = PostJamConfig.CreateDefault();
        config.upkeepPerFloor = 2.5f;
        config.freeFloors = 0;
        PostJamCityStats stats = Economy(config);
        stats.Recalculate(Floors(4, "Park"));
        Assert.AreEqual(10f, stats.Upkeep, 1e-4f);
    }

    [Test]
    public void GoldFloor_CostsNoUpkeep()
    {
        PostJamConfig config = PostJamConfig.CreateDefault();
        config.freeFloors = 0;
        PostJamCityStats stats = Economy(config);
        stats.Recalculate(new[] { Type("Park"), FloorType.Gold, Type("Park") });
        Assert.AreEqual(3, stats.Height);
        Assert.AreEqual(2f, stats.Upkeep, 1e-4f);
    }

    [Test]
    public void Banks_RaiseTheCap()
    {
        PostJamCityStats stats = Economy();
        stats.Recalculate(new[] { Type("Bank"), Type("Park"), Type("Bank") });
        Assert.AreEqual(100f + 2 * 150f, stats.MoneyCap, 1e-4f);
    }

    [Test]
    public void Bank_PaysUpkeepLikeAnyFloor()
    {
        PostJamConfig config = PostJamConfig.CreateDefault();
        config.freeFloors = 0;
        PostJamCityStats stats = Economy(config);
        stats.Recalculate(new[] { Type("Bank") });
        Assert.AreEqual(1f, stats.Upkeep, 1e-4f);
    }

    [Test]
    public void Tick_AddsNetIncomeAndStopsAtTheCap()
    {
        PostJamConfig config = PostJamConfig.CreateDefault();
        config.startBalance = 0f;
        config.freeFloors = 0;
        PostJamCityStats stats = Economy(config);
        // Residential + office + power + park: 1 worker x 6 + 0.9 commute = 6.9, +5% happiness; upkeep 4.
        stats.Recalculate(new[] { Type("Residential"), Type("Office"), Type("Power"), Type("Park") });
        float net = stats.Income - 4f;
        Assert.AreEqual(net, stats.NetIncome, 1e-4f);
        stats.Tick(2f);
        Assert.AreEqual(net * 2f, stats.Money, 1e-3f);
        stats.Tick(1000f);
        Assert.AreEqual(stats.MoneyCap, stats.Money, 1e-4f);
    }

    [Test]
    public void Tick_NegativeNet_DrainsTheBalanceBelowZero()
    {
        PostJamConfig config = PostJamConfig.CreateDefault();
        config.startBalance = 10f;
        config.freeFloors = 0;
        PostJamCityStats stats = Economy(config);
        stats.Recalculate(Floors(5, "Park")); // no income, upkeep 5
        stats.Tick(3f);
        Assert.AreEqual(-5f, stats.Money, 1e-3f);
    }

    [Test]
    public void Runway_IsTheTimeUntilTheBalanceHitsZero()
    {
        PostJamConfig config = PostJamConfig.CreateDefault();
        config.startBalance = 30f;
        config.freeFloors = 0;
        PostJamCityStats stats = Economy(config);
        stats.Recalculate(Floors(5, "Park"));
        Assert.AreEqual(6f, stats.Runway, 1e-3f);
        stats.Recalculate(new FloorType[0]);
        Assert.IsTrue(float.IsPositiveInfinity(stats.Runway));
    }

    [Test]
    public void LosingABank_CutsTheBalanceToTheNewCap()
    {
        PostJamConfig config = PostJamConfig.CreateDefault();
        PostJamCityStats stats = Economy(config);
        stats.Recalculate(new[] { Type("Bank") });
        stats.AddBonusMoney(500f);
        Assert.AreEqual(250f, stats.Money, 1e-4f);
        stats.Recalculate(new FloorType[0]); // the bank collapsed
        Assert.AreEqual(100f, stats.Money, 1e-4f);
    }

    [Test]
    public void TotalEarned_CountsIncomeOnly()
    {
        PostJamConfig config = PostJamConfig.CreateDefault();
        config.freeFloors = 0;
        PostJamCityStats stats = Economy(config);
        stats.Recalculate(new[] { Type("Residential"), Type("Office"), Type("Power"), Type("Park") });
        stats.AddBonusMoney(10f);
        stats.Tick(2f);
        Assert.AreEqual(stats.Income * 2f, stats.TotalEarned, 1e-3f);
    }

    [Test]
    public void Jam_HasNoCapAndNoUpkeep()
    {
        CityStats stats = new JamRuleset().CreateEconomy();
        stats.Recalculate(new[] { Type("Residential"), Type("Office"), Type("Power"), Type("Park"), Type("Park") });
        Assert.IsTrue(float.IsPositiveInfinity(stats.MoneyCap));
        Assert.AreEqual(0f, stats.Upkeep);
        stats.Tick(10f);
        Assert.AreEqual(stats.Income * 10f, stats.Money, 1e-3f);
        Assert.AreEqual(stats.Money, stats.TotalEarned, 1e-3f);
    }
}
