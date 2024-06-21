using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace UniMakao
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private List<Player> playersInGame = new List<Player>();

        [SerializeField]
        private int aiPlayersCount = 2;
        [SerializeField]
        private Player aiPlayerPrototype;

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
            for (int i = 0; i < aiPlayersCount; i++)
            {
                var aiPlayer = Instantiate(aiPlayerPrototype);
                AddPlayer(aiPlayer);
            }
        }

        private void Singleton_OnClientConnectedCallback(ulong clientID)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientID, out var playerClient))
            {
                if (playerClient.PlayerObject.TryGetComponent<Player>(out var player))
                    AddPlayer(player);
            }
        }

        public void AddPlayer(Player player)
        {
            playersInGame.Add(player);
            player.OnReady += Player_OnReady;
        }

        private void Player_OnReady(Player readyPlayer)
        {
            readyPlayer.OnReady -= Player_OnReady;
            foreach (var player in playersInGame)
                if (player.IsReady == false)
                    return;

            StartGame();
        }

        private void StartGame()
        {
            Debug.Log("All Players Ready");
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton)
                NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
        }
    }
}
