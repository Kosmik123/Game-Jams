using TMPro;
using UnityEngine;

namespace Bipolar.Pong
{
	public class UIController : MonoBehaviour
	{
		[SerializeField]
		private GameController gameController;

		[Header("UI Elements")]
		[SerializeField]
		private TMP_Text[] scoreDisplays;
		[SerializeField]
		private TMP_Text winnerDisplay;

		private void OnEnable()
		{
			gameController.OnGameStarted += GameController_OnGameStarted;
			gameController.OnPointScored += GameController_OnPointScored;
			gameController.OnPlayerWon += GameController_OnPlayerWon;
		}

		private void GameController_OnGameStarted()
		{
			winnerDisplay.gameObject.SetActive(false);
		}

		private void GameController_OnPointScored(int playerIndex)
		{
			int newScore = gameController.PlayerScores[playerIndex];
			scoreDisplays[playerIndex].SetText(newScore.ToString());
		}

		private void GameController_OnPlayerWon(int playerIndex)
		{
			winnerDisplay.gameObject.SetActive(true);
		}

		private void OnDisable()
		{
			gameController.OnGameStarted -= GameController_OnGameStarted;
			gameController.OnPointScored -= GameController_OnPointScored;
			gameController.OnPlayerWon -= GameController_OnPlayerWon;
		}
	}
}
