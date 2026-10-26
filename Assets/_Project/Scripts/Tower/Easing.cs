using UnityEngine;

namespace ZeroHour.Tower
{
    public static class Easing
    {
        // Moves current toward target exponentially; the result doesn't depend on the frame rate
        // (two half steps equal one full step). A higher speed settles faster.
        public static float Follow(float current, float target, float speed, float deltaTime) =>
            Mathf.Lerp(current, target, 1f - Mathf.Exp(-speed * deltaTime));
    }
}
