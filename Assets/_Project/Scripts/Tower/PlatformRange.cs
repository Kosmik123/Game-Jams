namespace ZeroHour.Tower
{
    // Where the platform may go so that the roof of a leaning tower can reach every spawn column.
    public static class PlatformRange
    {
        // topOffset = roof X - platform X. Only the roof is limited to +-playHalfWidth; the platform may leave
        // the screen, since with the sinking tower it is usually below the screen anyway.
        public static void Get(float topOffset, float playHalfWidth, out float min, out float max)
        {
            min = -playHalfWidth - topOffset;
            max = playHalfWidth - topOffset;
        }
    }
}
