using UnityEngine;

namespace ZeroHour.Tower
{
    public static class Landing
    {
        // X of a caught floor relative to the platform. A landing closer to the previous floor than
        // perfectOffset (in floor widths) is perfect; with snapPerfect it is placed exactly on top of it.
        public static float PlaceX(float landedX, float previousX, float width, float perfectOffset, bool snapPerfect, out bool isPerfect)
        {
            isPerfect = Mathf.Abs(landedX - previousX) / width < perfectOffset;
            return isPerfect && snapPerfect ? previousX : landedX;
        }
    }
}
