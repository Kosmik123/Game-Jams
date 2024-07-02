using System.Collections.Generic;
using UnityEngine;

namespace UniMakao
{
    public class Deck : MonoBehaviour
    {
        [SerializeField]
        private List<Card> cards;

        private void Awake()
        {
            for (byte suit = 0; suit < 4; suit++)
            {
                for (byte rank = 1; rank <= 4; rank++) 
                {
                    cards.Add(new Card(suit, rank));
                }
            }
            Shuffle();
        }

        [ContextMenu("Shuffle")]
        private void Shuffle()
        {
            for (int i = 0; i < cards.Count; i++)
            {
                int randomIndex = Random.Range(i, cards.Count);
                (cards[i], cards[randomIndex]) = (cards[randomIndex], cards[i]);
            }
        }
    }

}

