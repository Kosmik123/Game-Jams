using UnityEngine;

namespace UniMakao
{
    [System.Serializable]
    public class Card
    {
        [field: SerializeField]
        public byte Suit { get; private set; }
        [field: SerializeField]
        public byte Rank { get; private set; }

        public Card(byte suit, byte rank)
        {
            Suit = suit;
            Rank = rank;
        }
    }
}

