using NaughtyAttributes;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace UniMakao
{
    public class Player : NetworkBehaviour
    {
        public event System.Action<Player> OnReady;

        [SerializeField, ReadOnly]
        private bool isReady;
        public bool IsReady => isReady;

        [SerializeField]
        private List<Card> hand;

        [Button]
        private void Ready()
        {
            SetReadyServerRpc();
        }

        [ServerRpc]
        private void SetReadyServerRpc()
        {
            isReady = true;
            SetReadyClientRpc();
            if (IsServer)
                OnReady?.Invoke(this);
        }

        [ClientRpc]
        private void SetReadyClientRpc()
        {
            isReady = true;
        }
    }
}

