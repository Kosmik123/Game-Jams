using UnityEngine;

public class VictoryUI : MonoBehaviour
{
    [SerializeField]
    private GameObject victoryPanel;
    [SerializeField]
    private GameObject lostPanel;

    private void OnEnable()
    {
        Game.OnGameWon += Game_OnGameWon;
        Game.OnGameLost += Game_OnGameLost;
    }

    private void Game_OnGameLost()
    {
        lostPanel.SetActive(true);
    }

    private void Game_OnGameWon()
    {
        victoryPanel.SetActive(true);
    }

    private void OnDisable()
    {
        Game.OnGameWon -= Game_OnGameWon;
        Game.OnGameLost -= Game_OnGameLost;
    }
}
