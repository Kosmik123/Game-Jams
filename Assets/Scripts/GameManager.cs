using Unity.Netcode;
using UnityEngine;

namespace UniMakao
{
    [RequireComponent(typeof(NetworkManager))]
    public class GameManager : MonoBehaviour
    {
        private NetworkManager networkManager;

        [SerializeField]
        private CardGameManager cardGameManager;

        [SerializeField]
        private Player aiPlayerPrototype;

        private void Awake()
        {
            networkManager = GetComponent<NetworkManager>();
        }

        private void OnEnable()
        {
            networkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        public void Host()
        {
            networkManager.StartHost();
        }

        public void Join()
        {
            networkManager.StartClient();
        }

        public void AddAIClient()
        {
            var aiPlayer = Instantiate(aiPlayerPrototype);
            aiPlayer.NetworkObject.Spawn();
            cardGameManager.AddPlayer(aiPlayer);
        }

        private void NetworkManager_OnClientConnectedCallback(ulong clientID)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientID, out var playerClient))
            {
                if (playerClient.PlayerObject.TryGetComponent<Player>(out var player))
                    cardGameManager.AddPlayer(player);
            }
        }

        private void OnDisable()
        {
            networkManager.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
        }
    }
}
