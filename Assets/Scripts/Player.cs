using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace UniMakao
{
    public class Player : NetworkBehaviour
    {
        public event System.Action<Player> OnReady;

        [SerializeField] 
        private bool isReady;
        public bool IsReady => isReady;

        [SerializeField]
        private List<Card> hand;

#if NAUGHTY_ATTRIBUTES
        [NaughtyAttributes.Button]
#endif
        private void Ready()
        {
            isReady = true;
            OnReady?.Invoke(this);
        }
    }
}

