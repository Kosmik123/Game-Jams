using UnityEngine;

namespace ZeroHour.Tower
{
    // Where the platform may go so that the roof of a leaning tower can reach every spawn column.
    public static class PlatformRange
    {
        // topOffset = roof X - platform X. The roof stays within +-playHalfWidth; the platform must stay
        // on screen (+-screenHalfWidth minus half its width). When the two can't both hold, the screen wins.
        public static void Get(float topOffset, float playHalfWidth, float screenHalfWidth, float platformHalfWidth,
            out float min, out float max)
        {
            float screenMin = -screenHalfWidth + platformHalfWidth;
            float screenMax = screenHalfWidth - platformHalfWidth;
            if (screenMin > screenMax)
                screenMin = screenMax = 0f;
            min = Mathf.Clamp(-playHalfWidth - topOffset, screenMin, screenMax);
            max = Mathf.Clamp(playHalfWidth - topOffset, screenMin, screenMax);
        }
    }
}
