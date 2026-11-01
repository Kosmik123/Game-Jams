using NUnit.Framework;
using ZeroHour.Tower;

public class DebugModeTests
{
    [TestCase("https://kosmik123.itch.io/0h-game-jam-2026?debug=1", true)]
    [TestCase("http://127.0.0.1:8777/?debug=1", true)]
    [TestCase("http://127.0.0.1:8777/index.html?a=2&debug=1&b=3", true)]
    [TestCase("http://127.0.0.1:8777/?debug=1#top", true)]
    [TestCase("http://127.0.0.1:8777/", false)]
    [TestCase("http://127.0.0.1:8777/?debug=0", false)]
    [TestCase("http://127.0.0.1:8777/?debug=10", false)]
    [TestCase("http://127.0.0.1:8777/?nodebug=1", false)]
    [TestCase("http://127.0.0.1:8777/#debug=1", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public void FromUrl_RecognizesTheDebugSwitch(string url, bool expected)
    {
        Assert.AreEqual(expected, DebugMode.FromUrl(url));
    }

    [Test]
    public void PostJam_OutsideDebugKeepsTheLoss()
    {
        // In the editor the address is empty, so only the stored switch can turn debug on.
        bool stored = DebugMode.IsStored;
        DebugMode.SetStored(false);
        try
        {
            Assert.IsFalse(new PostJamRuleset().IsDebug);
            Assert.IsTrue(new PostJamRuleset().LosesByBankruptcy);
        }
        finally
        {
            DebugMode.SetStored(stored);
        }
    }

    [Test]
    public void StoredSwitch_TurnsDebugOnAndNeverLoses()
    {
        bool stored = DebugMode.IsStored;
        try
        {
            DebugMode.SetStored(true);
            Assert.IsTrue(DebugMode.IsEnabled);
            Assert.IsTrue(new PostJamRuleset().IsDebug);
            Assert.IsFalse(new PostJamRuleset().LosesByBankruptcy);
            Assert.IsFalse(new JamRuleset().IsDebug, "the jam version has no debug mode");
            DebugMode.SetStored(false);
            Assert.IsFalse(DebugMode.IsStored);
        }
        finally
        {
            DebugMode.SetStored(stored);
        }
    }

    [Test]
    public void Jam_HasNoDebugMode()
    {
        Assert.IsFalse(new JamRuleset().IsDebug);
    }
}
