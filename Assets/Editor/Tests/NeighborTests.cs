using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ZeroHour.Tower;

public class NeighborTests
{
    private static readonly FloorType R = FloorType.All.First(t => t.Name == "Residential");
    private static readonly FloorType O = FloorType.All.First(t => t.Name == "Office");
    private static readonly FloorType P = FloorType.All.First(t => t.Name == "Power");
    private static readonly FloorType Park = FloorType.All.First(t => t.Name == "Park");

    private static CityStats PostJam(params FloorType[] floors)
    {
        var stats = new PostJamRuleset().CreateEconomy();
        stats.Recalculate((IReadOnlyList<FloorType>)floors);
        return stats;
    }

    [Test]
    public void ResidentialNextToPark_GainsJoy()
    {
        Assert.AreEqual(-1 + 4 + 1, PostJam(R, Park).Happiness);
    }

    [Test]
    public void ResidentialNextToPower_LosesJoy()
    {
        var stats = PostJam(R, P);
        Assert.AreEqual(3, stats.Energy);
        Assert.AreEqual(-1 - 2 - 1, stats.Happiness);
    }

    [Test]
    public void ResidentialNextToOffice_AddsCommuteIncome()
    {
        // R-O pair (+0.3), the rest has no synergy; joy -1-2+8 = 5 -> x1.25, full power.
        var stats = PostJam(R, O, P, Park, Park);
        Assert.AreEqual(0.3f, stats.NeighborIncome, 1e-5f);
        Assert.AreEqual(1f, stats.PowerRatio);
        Assert.AreEqual((10 * 0.2f + 0.3f) * 1.25f, stats.Income, 1e-4f);
    }

    [Test]
    public void ParkResidentialPark_CountsBothPairs()
    {
        Assert.AreEqual(4 - 1 + 4 + 2, PostJam(Park, R, Park).Happiness);
    }

    [Test]
    public void Order_ChangesTheNumberOfPairs()
    {
        Assert.AreEqual(0.9f, PostJam(R, O, R, O).NeighborIncome, 1e-5f);
        Assert.AreEqual(0.3f, PostJam(R, R, O, O).NeighborIncome, 1e-5f);
    }

    [Test]
    public void PairOrder_DoesNotMatter()
    {
        var economy = new PostJamRuleset().CreateEconomy();
        Assert.AreEqual(economy.GetNeighborEffect(R, Park).Happiness, economy.GetNeighborEffect(Park, R).Happiness);
        Assert.IsTrue(economy.GetNeighborEffect(O, P).IsNone);
    }

    [Test]
    public void Jam_IgnoresNeighbors()
    {
        var stats = new JamRuleset().CreateEconomy();
        stats.Recalculate((IReadOnlyList<FloorType>)new[] { R, Park, R, P });
        Assert.AreEqual(-1 + 3 - 1 - 2, stats.Happiness);
        Assert.AreEqual(0f, stats.NeighborIncome);
    }
}
