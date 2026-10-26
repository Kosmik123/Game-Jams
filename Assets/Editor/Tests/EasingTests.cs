using NUnit.Framework;
using ZeroHour.Tower;

public class EasingTests
{
    [Test]
    public void Follow_MovesTowardTheTarget_WithoutOvershoot()
    {
        float value = 0f;
        for (int i = 0; i < 60; i++)
        {
            float next = Easing.Follow(value, 1f, 12f, 1f / 60f);
            Assert.GreaterOrEqual(next, value);
            Assert.LessOrEqual(next, 1f);
            value = next;
        }
        Assert.AreEqual(1f, value, 1e-3f);
    }

    [Test]
    public void Follow_DoesNotDependOnTheFrameRate()
    {
        float oneStep = Easing.Follow(0f, 1f, 12f, 0.1f);
        float half = Easing.Follow(Easing.Follow(0f, 1f, 12f, 0.05f), 1f, 12f, 0.05f);
        Assert.AreEqual(oneStep, half, 1e-5f);
    }

    [Test]
    public void Follow_FirstFrameIsOnlyAPartOfTheJump()
    {
        // The bug: the platform used to jump the whole 0.45 in one frame.
        float step = Easing.Follow(0f, 0.45f, 12f, 1f / 60f) - 0f;
        Assert.Less(step, 0.45f * 0.25f);
    }
}
