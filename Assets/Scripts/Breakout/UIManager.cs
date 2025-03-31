using UnityEngine;
using UnityEngine.UI;

namespace Bipolar.Breakout
{
	public class UIManager : MonoBehaviour
	{
		private GameController gameController;

		[SerializeField]
		private GameObject gameOverScreen;
		[SerializeField]
		private Button retryButton;

		private void Awake()
		{
			gameController = FindAnyObjectByType<GameController>();
		}

		private void Start()
		{
			gameOverScreen.SetActive(false);
		}

		private void OnEnable()
		{
			gameController.OnGameStarted += OnGameStarted;
			gameController.OnGameOver += OnGameOver;
			retryButton.onClick.AddListener(RetryGame);
		}

		private void OnGameStarted()
		{
			gameOverScreen.SetActive(false);
		}

		private void OnGameOver()
		{
			gameOverScreen.SetActive(true);
		}

		private void RetryGame()
		{
			gameController.RestartGame();
		}

		private void OnDisable()
		{
			gameController.OnGameStarted -= OnGameStarted;
            gameController.OnGameOver -= OnGameOver;
			retryButton.onClick.RemoveListener(RetryGame);
		}
	}
}
