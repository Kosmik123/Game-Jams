using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace UniMakao
{
    public class CardGameManager : MonoBehaviour
    {
        [SerializeField]
        private List<Player> playersInGame = new List<Player>();

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
    }
}
