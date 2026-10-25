using NUnit.Framework;
using ZeroHour.Tower;

public class RelativeDragTests
{
    private const float HalfRange = 3f;

    [Test]
    public void Press_DoesNotMovePlatform()
    {
        var drag = new RelativeDrag(1f);
        Assert.AreEqual(0.5f, drag.Update(true, 1, -2.7f, 0.5f, HalfRange));
    }

    [Test]
    public void FingerMovesOneUnit_PlatformMovesOneUnit()
    {
        var drag = new RelativeDrag(1f);
        float x = drag.Update(true, 1, 2f, 0f, HalfRange);
        x = drag.Update(true, 1, 2.4f, x, HalfRange);
        x = drag.Update(true, 1, 3f, x, HalfRange);
        Assert.AreEqual(1f, x, 1e-5f);
    }

    [Test]
    public void Sensitivity_ScalesMovement()
    {
        var drag = new RelativeDrag(1.5f);
        float x = drag.Update(true, 1, 0f, 0f, HalfRange);
        x = drag.Update(true, 1, 1f, x, HalfRange);
        Assert.AreEqual(1.5f, x, 1e-5f);
    }

    [Test]
    public void AtEdge_StopsAndReversesImmediately()
    {
        var drag = new RelativeDrag(1f);
        float x = drag.Update(true, 1, 0f, 2.5f, HalfRange);
        x = drag.Update(true, 1, 2f, x, HalfRange);     // would be 4.5, clamped
        Assert.AreEqual(HalfRange, x, 1e-5f);
        x = drag.Update(true, 1, 1.8f, x, HalfRange);   // back by 0.2: no dead zone
        Assert.AreEqual(HalfRange - 0.2f, x, 1e-5f);
    }

    [Test]
    public void Release_KeepsPlatform_AndNewTouchIsNewReference()
    {
        var drag = new RelativeDrag(1f);
        float x = drag.Update(true, 1, 0f, 0f, HalfRange);
        x = drag.Update(true, 1, 1f, x, HalfRange);
        Assert.AreEqual(1f, drag.Update(false, 0, 0f, x, HalfRange), 1e-5f);
        x = drag.Update(true, 2, -2f, x, HalfRange);    // new touch far away: no jump
        Assert.AreEqual(1f, x, 1e-5f);
        x = drag.Update(true, 2, -1.5f, x, HalfRange);
        Assert.AreEqual(1.5f, x, 1e-5f);
    }

    [Test]
    public void FingerChange_WithoutRelease_IsNewReference()
    {
        var drag = new RelativeDrag(1f);
        float x = drag.Update(true, 1, 0f, 0f, HalfRange);
        x = drag.Update(true, 1, 0.5f, x, HalfRange);
        x = drag.Update(true, 7, 2.5f, x, HalfRange);   // another finger takes over
        Assert.AreEqual(0.5f, x, 1e-5f);
    }
}
