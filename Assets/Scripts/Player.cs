using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace UniMakao
{
    public class Player : MonoBehaviour
    {
        public event System.Action<Player> OnReady;

        [SerializeField] 
        private bool isReady;
        public bool IsReady => isReady;

        [SerializeField]
        private List<Card> cards;

        [Button]
        private void Ready()
        {
            isReady = true;
            OnReady?.Invoke(this);
        }
    }

    public class Card
    {
        [SerializeField]
        private CardSuit suit;
        [SerializeField]
        private CardValue value;
    }
}
