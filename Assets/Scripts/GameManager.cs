using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
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
            if (networkManager.IsServer == false)
                return;

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

        public void SetIP(string ip)
        {
            var transport = (UnityTransport)networkManager.NetworkConfig.NetworkTransport;
            var connection = transport.ConnectionData;
            connection.Address = ip;
            transport.ConnectionData = connection;
        }        
        
        public void SetPort(string port)
        {
            var transport = (UnityTransport)networkManager.NetworkConfig.NetworkTransport;
            var connection = transport.ConnectionData;
            connection.Port = ushort.Parse(port);
            transport.ConnectionData = connection;
        }
    }
}
