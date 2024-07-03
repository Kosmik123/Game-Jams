using Bipolar;
using Unity.Netcode;
using UnityEngine;

namespace UniMakao
{
    [RequireComponent(typeof(Player))]
    public class HumanPlayer : NetworkBehaviour
    {
        private Player _player;
        public Player Player => this.GetRequired(ref _player);

        public override void OnNetworkSpawn()
        {
            gameObject.name = $"{NetworkManager.NetworkConfig.PlayerPrefab.name} ({NetworkObject.OwnerClientId})";
        }
    }
}

