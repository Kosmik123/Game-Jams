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
        // In the editor the address is empty, so debug is off and the config decides.
        Assert.IsFalse(new PostJamRuleset().IsDebug);
        Assert.IsTrue(new PostJamRuleset().LosesByBankruptcy);
    }

    [Test]
    public void Jam_HasNoDebugMode()
    {
        Assert.IsFalse(new JamRuleset().IsDebug);
    }
}
