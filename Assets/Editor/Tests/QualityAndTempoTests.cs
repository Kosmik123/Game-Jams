using System.Collections.Generic;
using NUnit.Framework;
using ZeroHour.Tower;

public class QualityAndTempoTests
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

    // ---- placement quality

    [Test]
    public void Quality_PerfectLandingIsOne()
    {
        Assert.AreEqual(1f, Landing.Quality(0.05f, true, 0.5f), 1e-5f);
    }

    [Test]
    public void Quality_ShrinksLinearlyToTheMinimumAtOneFloorWidth()
    {
        Assert.AreEqual(1f, Landing.Quality(0f, false, 0.5f), 1e-5f);
        Assert.AreEqual(0.75f, Landing.Quality(0.5f, false, 0.5f), 1e-5f);
        Assert.AreEqual(0.5f, Landing.Quality(1f, false, 0.5f), 1e-5f);
        Assert.AreEqual(0.5f, Landing.Quality(3f, false, 0.5f), 1e-5f, "never below the minimum");
    }

    [Test]
    public void Ruleset_JamIgnoresPlacement_PostJamUsesTheConfig()
    {
        Assert.AreEqual(1f, new JamRuleset().FloorQuality(1f, false), 1e-5f);
        PostJamConfig config = PostJamConfig.CreateDefault();
        config.minQuality = 0.2f;
        Assert.AreEqual(0.2f, new PostJamRuleset(config).FloorQuality(1f, false), 1e-5f);
    }

    [Test]
    public void BadlyPlacedFloors_DoLessGood()
    {
        PostJamCityStats stats = Economy();
        var floors = new[] { Type("Residential"), Type("Office") };
        stats.Recalculate(floors, new[] { 1f, 0.5f });
        Assert.AreEqual(0.5f, stats.Jobs, 1e-4f);
    }

    [Test]
    public void BadlyPlacedPark_GivesLessJoy_ButPlantKeepsItsSmog()
    {
        PostJamCityStats stats = Economy();
        stats.Recalculate(new[] { Type("Park") }, new[] { 0.5f });
        Assert.AreEqual(2f, stats.Happiness, 1e-4f);

        stats.Recalculate(new[] { Type("Power") }, new[] { 0.5f });
        Assert.AreEqual(2f, stats.Energy, 1e-4f, "half of the 4 energy");
        Assert.AreEqual(-2f, stats.Happiness, 1e-4f, "the smog is not reduced");
    }

    [Test]
    public void BadlyPlacedConsumers_StillUseAllTheirEnergy()
    {
        PostJamCityStats stats = Economy();
        stats.Recalculate(new[] { Type("Residential"), Type("Office") }, new[] { 0.5f, 0.5f });
        Assert.AreEqual(-2f, stats.Energy, 1e-4f);
    }

    [Test]
    public void BadlyPlacedBank_RaisesTheLimitLess()
    {
        PostJamCityStats stats = Economy();
        stats.Recalculate(new[] { Type("Bank"), Type("Bank") }, new[] { 1f, 0.5f });
        Assert.AreEqual(100f + 150f * 1.5f, stats.MoneyCap, 1e-3f);
    }

    [Test]
    public void WithoutQualities_EveryFloorIsPerfect()
    {
        PostJamCityStats stats = Economy();
        stats.Recalculate(new[] { Type("Residential"), Type("Office") });
        Assert.AreEqual(1f, stats.Jobs, 1e-4f);
    }

    [Test]
    public void Income_FallsWithTheQualityOfTheWorkplace()
    {
        PostJamConfig config = PostJamConfig.CreateDefault();
        config.freeFloors = 100;
        var floors = new[] { Type("Residential"), Type("Office"), Type("Power"), Type("Park") };
        PostJamCityStats perfect = Economy(config);
        perfect.Recalculate(floors);
        PostJamCityStats sloppy = Economy(config);
        sloppy.Recalculate(floors, new[] { 0.5f, 0.5f, 1f, 1f });
        Assert.Less(sloppy.Income, perfect.Income);
    }

    [Test]
    public void Jam_IgnoresQualities()
    {
        CityStats stats = new JamRuleset().CreateEconomy();
        var floors = new[] { Type("Residential"), Type("Office"), Type("Power") };
        stats.Recalculate(floors);
        float perfect = stats.Income;
        stats.Recalculate(floors, new[] { 0.5f, 0.5f, 0.5f });
        Assert.AreEqual(perfect, stats.Income, 1e-5f);
    }

    // ---- tempo and accuracy: grow with the square root of the highest tower, with no limit

    [Test]
    public void FallSpeed_GrowsWithTheSquareRootAndHasNoLimit()
    {
        var ruleset = new PostJamRuleset();
        Assert.AreEqual(3f, ruleset.FallSpeed(0), 1e-4f);
        Assert.AreEqual(3f + 0.8f * 4f, ruleset.FallSpeed(16), 1e-4f);
        Assert.AreEqual(3f + 0.8f * 100f, ruleset.FallSpeed(10000), 1e-3f);
        Assert.Greater(ruleset.FallSpeed(1000000), ruleset.FallSpeed(10000));
    }

    [Test]
    public void FallSpeed_RisesSlowerAndSlowerPerFloor()
    {
        var ruleset = new PostJamRuleset();
        float early = ruleset.FallSpeed(10) - ruleset.FallSpeed(0);
        float late = ruleset.FallSpeed(110) - ruleset.FallSpeed(100);
        Assert.Greater(early, late);
    }

    [Test]
    public void SpawnInterval_ShrinksWithHeight()
    {
        var ruleset = new PostJamRuleset();
        Assert.AreEqual(0.7f, ruleset.SpawnInterval(0), 1e-4f);
        Assert.Less(ruleset.SpawnInterval(100), ruleset.SpawnInterval(16));
        Assert.Less(ruleset.SpawnInterval(16), ruleset.SpawnInterval(0));
        Assert.GreaterOrEqual(ruleset.NextSpawnGap(100000, 0f), TowerRuleset.MinSpawnGap, "floors never overlap");
    }

    [Test]
    public void Accuracy_GetsHarderWithHeight()
    {
        var ruleset = new PostJamRuleset();
        Assert.AreEqual(1f, ruleset.PerfectWindowScale(0), 1e-5f);
        Assert.AreEqual(1f, ruleset.InstabilityScale(0), 1e-5f);
        Assert.AreEqual(0.5f, ruleset.PerfectWindowScale(100), 1e-4f, "sqrt(100) = 10 = the scale");
        Assert.AreEqual(1f + 8f / 8f, ruleset.InstabilityScale(64), 1e-4f);
        Assert.Less(ruleset.PerfectWindowScale(400), ruleset.PerfectWindowScale(100));
        Assert.Greater(ruleset.InstabilityScale(400), ruleset.InstabilityScale(100));
    }

    [Test]
    public void Jam_TempoAndAccuracyStayFixed()
    {
        var jam = new JamRuleset();
        Assert.AreEqual(3f, jam.FallSpeed(500), 1e-5f);
        Assert.AreEqual(0.7f, jam.SpawnInterval(500), 1e-5f);
        Assert.AreEqual(1f, jam.PerfectWindowScale(500), 1e-5f);
        Assert.AreEqual(1f, jam.InstabilityScale(500), 1e-5f);
    }
}
