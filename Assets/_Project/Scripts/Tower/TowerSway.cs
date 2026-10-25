using UnityEngine;

namespace ZeroHour.Tower
{
    // Visual sway of the stacked floors; it doesn't affect the stability logic.
    public abstract class TowerSway
    {
        // Called once per frame before Offset. instability is 0..1.
        public abstract void Advance(float time, float deltaTime, float instability, int height);

        // Horizontal offset of the floor at this index (0 = bottom).
        public abstract float Offset(int index, int height);
    }

    // Jam formula: every floor swings in phase, more the higher it is, clamped hard.
    public class JamTowerSway : TowerSway
    {
        private const float Frequency = 3f;
        private const float OffsetPerFloor = 0.06f;
        private const float MaxWobble = 0.6f;

        private float sway;

        public override void Advance(float time, float deltaTime, float instability, int height) =>
            sway = Mathf.Sin(time * Frequency) * instability;

        public override float Offset(int index, int height) =>
            Mathf.Clamp(sway * OffsetPerFloor * (index + 1), -MaxWobble, MaxWobble);
    }

    // Tall towers sway less and slower, bending at the top while the base stands still.
    public class PostJamTowerSway : TowerSway
    {
        private const float MaxTopWobble = 0.45f;
        private const float HeightDamping = 15f;
        private const float BaseFrequency = 3f;
        private const float BendExponent = 1.5f;
        private const float InstabilitySmoothing = 2f;

        private float phase;
        private float displayedInstability;
        private float topAmplitude;

        public override void Advance(float time, float deltaTime, float instability, int height)
        {
            displayedInstability = Mathf.MoveTowards(displayedInstability, instability, InstabilitySmoothing * deltaTime);
            float heightFactor = 1f + height / HeightDamping;
            // Phase is accumulated, so a changing frequency never makes the tower jump.
            phase += BaseFrequency / Mathf.Sqrt(heightFactor) * deltaTime;
            topAmplitude = displayedInstability * MaxTopWobble / heightFactor;
        }

        public override float Offset(int index, int height)
        {
            if (height <= 0)
                return 0f;
            float bend = Mathf.Pow((index + 1f) / height, BendExponent);
            return topAmplitude * bend * Mathf.Sin(phase);
        }
    }
}
