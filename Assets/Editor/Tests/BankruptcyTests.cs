using NUnit.Framework;
using ZeroHour.Tower;

public class BankruptcyTests
{
    [Test]
    public void Timer_CountsOnlyWhileTheBalanceIsNegative()
    {
        var timer = new BankruptcyTimer(8f);
        timer.Update(50f, 1f);
        Assert.IsFalse(timer.IsCounting);
        timer.Update(-1f, 1f);
        timer.Update(-2f, 2f);
        Assert.IsTrue(timer.IsCounting);
        Assert.AreEqual(3f, timer.Elapsed, 1e-4f);
        Assert.AreEqual(5f, timer.TimeLeft, 1e-4f);
        Assert.IsFalse(timer.IsBankrupt);
    }

    [Test]
    public void Timer_IsBankruptAfterTheGracePeriod()
    {
        var timer = new BankruptcyTimer(8f);
        timer.Update(-1f, 7.9f);
        Assert.IsFalse(timer.IsBankrupt);
        timer.Update(-1f, 0.2f);
        Assert.IsTrue(timer.IsBankrupt);
        Assert.AreEqual(0f, timer.TimeLeft, 1e-4f);
    }

    [Test]
    public void Timer_ResetsWhenTheBalanceRecovers()
    {
        var timer = new BankruptcyTimer(8f);
        timer.Update(-1f, 6f);
        timer.Update(0f, 0.1f);
        Assert.IsFalse(timer.IsCounting);
        Assert.AreEqual(0f, timer.Elapsed);
        timer.Update(-1f, 3f);
        Assert.AreEqual(3f, timer.Elapsed, 1e-4f);
        Assert.IsFalse(timer.IsBankrupt);
    }

    [Test]
    public void Timer_ZeroBalanceIsNotNegative()
    {
        var timer = new BankruptcyTimer(1f);
        timer.Update(0f, 100f);
        Assert.IsFalse(timer.IsBankrupt);
    }

    [Test]
    public void NewBest_TallerTowerWins()
    {
        Assert.IsTrue(RoundScore.IsNewBest(21, 0, 20, 9999));
        Assert.IsFalse(RoundScore.IsNewBest(19, 9999, 20, 0));
    }

    [Test]
    public void NewBest_SameHeightGoesToTheOneWhoEarnedMore()
    {
        Assert.IsTrue(RoundScore.IsNewBest(20, 501, 20, 500));
        Assert.IsFalse(RoundScore.IsNewBest(20, 500, 20, 500));
        Assert.IsFalse(RoundScore.IsNewBest(20, 499, 20, 500));
    }

    [Test]
    public void Jam_KeepsTheClockAndTheOldScore()
    {
        var jam = new JamRuleset();
        Assert.IsTrue(jam.UseRoundClock);
        Assert.IsFalse(jam.LosesByBankruptcy);
        Assert.IsFalse(jam.ScoreByMaxHeight);
        Assert.IsNull(jam.BestScoreKey);
    }

    [Test]
    public void PostJam_EndsByBankruptcyAndScoresByHeight()
    {
        var postJam = new PostJamRuleset();
        Assert.IsFalse(postJam.UseRoundClock);
        Assert.IsTrue(postJam.LosesByBankruptcy);
        Assert.AreEqual(8f, postJam.BankruptcyGraceSeconds, 1e-4f);
        Assert.IsTrue(postJam.ScoreByMaxHeight);
        Assert.AreEqual("BestHeight_PostJam", postJam.BestScoreKey);
    }

    [Test]
    public void PostJam_CanLoseSwitchTurnsBankruptcyOff()
    {
        PostJamConfig config = PostJamConfig.CreateDefault();
        config.canLose = false;
        config.bankruptcyGraceSeconds = 3f;
        var postJam = new PostJamRuleset(config);
        Assert.IsFalse(postJam.LosesByBankruptcy);
        Assert.AreEqual(3f, postJam.BankruptcyGraceSeconds, 1e-4f);
    }
}
