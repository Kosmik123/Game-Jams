namespace ZeroHour.Tower
{
    // Decides which spawns are golden: a small chance per spawn, but only from a minimum tower height,
    // never two in the air at once, and guaranteed after a dry streak ("pity").
    public class GoldSpawner
    {
        private readonly float chance;
        private readonly int minHeight;
        private readonly int pity;
        private int spawnsSinceGold;

        public GoldSpawner(float chance, int minHeight, int pity)
        {
            this.chance = chance;
            this.minHeight = minHeight;
            this.pity = pity;
        }

        // roll is a random value in [0, 1).
        public bool Next(int towerHeight, bool goldInAir, float roll)
        {
            bool eligible = chance > 0f && towerHeight >= minHeight && !goldInAir;
            bool isGold = eligible && (spawnsSinceGold >= pity || roll < chance);
            spawnsSinceGold = isGold ? 0 : spawnsSinceGold + 1;
            return isGold;
        }
    }
}
