using UnityEngine;

namespace ZeroHour.Tower
{
    // Counts how long the balance has stayed below zero. Getting back to zero or above starts over.
    public class BankruptcyTimer
    {
        private readonly float graceSeconds;
        private bool isNegative;

        public BankruptcyTimer(float graceSeconds)
        {
            this.graceSeconds = graceSeconds;
        }

        // Seconds spent below zero in a row.
        public float Elapsed { get; private set; }

        // The balance is below zero right now.
        public bool IsCounting => isNegative;

        public float TimeLeft => Mathf.Max(0f, graceSeconds - Elapsed);

        public bool IsBankrupt => isNegative && Elapsed >= graceSeconds;

        public void Update(float money, float deltaTime)
        {
            isNegative = money < 0f;
            Elapsed = isNegative ? Elapsed + deltaTime : 0f;
        }
    }

    // Records when the score is the height of the tower.
    public static class RoundScore
    {
        // A taller tower wins; at the same height the one that earned more.
        public static bool IsNewBest(int height, int earned, int bestHeight, int bestEarned) =>
            height > bestHeight || (height == bestHeight && earned > bestEarned);
    }
}
