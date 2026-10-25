using NUnit.Framework;
using UnityEngine;
using ZeroHour.Tower;

public class TowerSwayTests
{
    // Runs the sway long enough for the displayed instability to settle, then returns the
    // largest top offset seen over a few full swings.
    private static float MaxTopOffset(TowerSway sway, float instability, int height)
    {
        const float dt = 0.01f;
        float time = 0f;
        for (int i = 0; i < 300; i++, time += dt)
            sway.Advance(time, dt, instability, height);
        float max = 0f;
        for (int i = 0; i < 1500; i++, time += dt)
        {
            sway.Advance(time, dt, instability, height);
            max = Mathf.Max(max, Mathf.Abs(sway.Offset(height - 1, height)));
        }
        return max;
    }

    [TestCase(10, 0.27f)]
    [TestCase(30, 0.15f)]
    [TestCase(60, 0.09f)]
    public void PostJam_TopAmplitude_ShrinksWithHeight(int height, float expected)
    {
        Assert.AreEqual(expected, MaxTopOffset(new PostJamTowerSway(), 1f, height), 0.005f);
    }

    [Test]
    public void PostJam_BaseStandsStill_TopBends()
    {
        var sway = new PostJamTowerSway();
        MaxTopOffset(sway, 1f, 20);
        // Pick a moment of non-zero swing and compare floors.
        float top = sway.Offset(19, 20);
        float middle = sway.Offset(9, 20);
        float bottom = sway.Offset(0, 20);
        Assert.AreNotEqual(0f, top);
        Assert.AreEqual(Mathf.Pow(10f / 20f, 1.5f), middle / top, 1e-4f);
        Assert.AreEqual(Mathf.Pow(1f / 20f, 1.5f), bottom / top, 1e-4f);
    }

    [Test]
    public void PostJam_InstabilityChange_IsSmoothed()
    {
        var sway = new PostJamTowerSway();
        float previousTop = 0f;
        float maxStep = 0f;
        const float dt = 0.016f;
        for (int i = 0; i < 200; i++)
        {
            float instability = i < 100 ? 0f : 1f; // jump from calm to max
            sway.Advance(i * dt, dt, instability, 10);
            float top = sway.Offset(9, 10);
            maxStep = Mathf.Max(maxStep, Mathf.Abs(top - previousTop));
            previousTop = top;
        }
        // Without smoothing the jump would move the top by up to 0.27 in one frame.
        Assert.Less(maxStep, 0.03f);
    }

    [Test]
    public void PostJam_HeightChange_DoesNotJumpPhase()
    {
        var sway = new PostJamTowerSway();
        const float dt = 0.016f;
        for (int i = 0; i < 300; i++)
            sway.Advance(i * dt, dt, 1f, 10);
        float before = sway.Offset(9, 10) / (0.45f / (1f + 10f / 15f));
        sway.Advance(300 * dt, dt, 1f, 11);
        float after = sway.Offset(10, 11) / (0.45f / (1f + 11f / 15f));
        // sin(phase) moves by at most omega * dt per frame.
        Assert.Less(Mathf.Abs(after - before), 3f * dt + 1e-3f);
    }

    [Test]
    public void Jam_KeepsOriginalFormula()
    {
        var sway = new JamTowerSway();
        for (float time = 0f; time < 3f; time += 0.137f)
        {
            sway.Advance(time, 0.016f, 0.8f, 20);
            float raw = Mathf.Sin(time * 3f) * 0.8f;
            for (int i = 0; i < 20; i++)
                Assert.AreEqual(Mathf.Clamp(raw * 0.06f * (i + 1), -0.6f, 0.6f), sway.Offset(i, 20), 1e-6f);
        }
    }
}
